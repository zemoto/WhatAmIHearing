using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using System.Web;

namespace WhatAmIHearing.Api.Spotify;

internal sealed class SpotifyAuthenticator : IDisposable
{
   private static readonly IPEndPoint _loopbackIpAddress = new( IPAddress.Loopback, 1378 );
   private const string RedirectUrl = "http://127.0.0.1:1378";
   private const string AuthUrl = $"https://accounts.spotify.com/authorize?client_id={ApiConstants.SpotifyClientId}&response_type=code&redirect_uri={RedirectUrl}&scope=playlist-read-private,playlist-modify-private&show_dialog=true";
   private const string TokenExchangeEndpoint = "https://accounts.spotify.com/api/token";

   private readonly AutoResetEvent _authDoneEvent = new( false );
   private readonly AppSettings _settings = AppSettings.Instance;

   public async Task<bool> EnsureAuthenticationIsValid()
   {
      if ( !string.IsNullOrEmpty( _settings.SpotifyAccessToken ) &&
           !string.IsNullOrEmpty( _settings.SpotifyRefreshToken ) &&
           DateTime.UtcNow + TimeSpan.FromMinutes( 1 ) > _settings.SpotifyExpirationTimeUtc )
      {
         await RefreshAuthenticationAsync();
      }

      return !string.IsNullOrEmpty( _settings.SpotifyAccessToken ) &&
             !string.IsNullOrEmpty( _settings.SpotifyRefreshToken ) &&
             DateTime.UtcNow < _settings.SpotifyExpirationTimeUtc;
   }

   public async Task<bool> SignInAsync() => !string.IsNullOrEmpty( _settings.SpotifyAccessToken ) || await AuthenticateWithBrowserAsync();

   public void SignOut()
   {
      _settings.SpotifyAccessToken = string.Empty;
      _settings.SpotifyRefreshToken = string.Empty;
      _settings.SpotifyExpirationTimeUtc = default;
      _settings.Save();
   }

   private async Task RefreshAuthenticationAsync()
   {
      var data = new Dictionary<string, string>
      {
         ["grant_type"] = "refresh_token",
         ["refresh_token"] = _settings.SpotifyRefreshToken
      };

      _ = await ExchangeTokensAsync( data );
   }

   private async Task<bool> AuthenticateWithBrowserAsync()
   {
      _ = _authDoneEvent.Reset();
      _ = Process.Start( new ProcessStartInfo( AuthUrl ) { UseShellExecute = true } );
      return await ListenForAuthentication();
   }

   private async Task<bool> ListenForAuthentication()
   {
      var listener = new Socket( _loopbackIpAddress.Address.AddressFamily, SocketType.Stream, ProtocolType.IP );
      listener.Bind( _loopbackIpAddress );
      listener.Listen( 10 );

      var state = new StateObject { Listener = listener };
      _ = listener.BeginAccept( AcceptCallback, state );

      _ = await Task.Run( () => _authDoneEvent.WaitOne( TimeSpan.FromMinutes( 2 ) ) );

      if ( !string.IsNullOrEmpty( state.ParsedAuthParams ) && state.ParsedAuthParams.Contains( "code", StringComparison.OrdinalIgnoreCase ) )
      {
         var values = HttpUtility.ParseQueryString( state.ParsedAuthParams );
         var authCode = values["/?code"];

         return await GetTokensFromAuthCodeAsync( authCode );
      }

      return false;
   }

   private async Task<bool> GetTokensFromAuthCodeAsync( string authCode )
   {
      var data = new Dictionary<string, string>
      {
         ["grant_type"] = "authorization_code",
         ["code"] = authCode,
         ["redirect_uri"] = RedirectUrl
      };

      return await ExchangeTokensAsync( data );
   }

   private async Task<bool> ExchangeTokensAsync( Dictionary<string,string> requestData )
   {
      using var client = new SpotifyApiClient( false );
      var responseJson = await client.SendPostRequestAsync( TokenExchangeEndpoint, requestData );

      if ( !string.IsNullOrEmpty( responseJson ) )
      {
         using var json = JsonDocument.Parse( responseJson );
         if ( json.RootElement.TryGetProperty( "access_token", out var jsonAccessToken ) )
         {
            _settings.SpotifyAccessToken = jsonAccessToken.ToString();

            if ( json.RootElement.TryGetProperty( "refresh_token", out var jsonRefreshToken ) )
            {
               _settings.SpotifyRefreshToken = jsonRefreshToken.ToString();
            }

            if ( json.RootElement.TryGetProperty( "expires_in", out var jsonExpiresIn ) )
            {
               _settings.SpotifyExpirationTimeUtc = DateTime.UtcNow + TimeSpan.FromSeconds( jsonExpiresIn.GetInt32() );
            }

            _settings.Save();
            return true;
         }
      }

      SignOut(); // Clear any saved credentials on failure
      return false;
   }

   private void AcceptCallback( IAsyncResult ar )
   {
      var state = (StateObject)ar.AsyncState;
      try
      {
         state.Receiver = state.Listener.EndAccept( ar );
         _ = state.Receiver.BeginReceive( state.Buffer, 0, StateObject.BufferSize, SocketFlags.None, ReceiveCallback, state );
         CleanupSocket( state.Listener );
      }
      catch
      {
         _ = _authDoneEvent.Set();
      }
   }

   private void ReceiveCallback( IAsyncResult ar )
   {
      var state = (StateObject)ar.AsyncState;
      try
      {
         var numBytesReceived = state.Receiver.EndReceive( ar );
         var stringBuffer = Encoding.UTF8.GetString( state.Buffer, 0, numBytesReceived );
         var lines = stringBuffer.Split( Environment.NewLine, StringSplitOptions.RemoveEmptyEntries );
         if ( lines.Length > 1 )
         {
            var lineItems = lines[0].Split( ' ', StringSplitOptions.RemoveEmptyEntries );
            if ( lineItems[0].Equals( "GET", StringComparison.OrdinalIgnoreCase ) )
            {
               state.ParsedAuthParams = lineItems[1];
            }

            const string DoneMessage = "Authentication done, you can close this page";
            var bytes = Encoding.UTF8.GetBytes( DoneMessage );
            _ = state.Receiver.BeginSend( bytes, 0, bytes.Length, SocketFlags.None, SendCallback, state );
         }
      }
      catch { }
   }

   private void SendCallback( IAsyncResult ar )
   {
      var state = (StateObject)ar.AsyncState;
      try
      {
         _ = state.Receiver.EndSend( ar );
      }
      catch { }

      CleanupSocket( state.Receiver );
      _ = _authDoneEvent.Set();
   }

   private static void CleanupSocket( Socket socket )
   {
      if ( socket.Connected )
      {
         socket.Shutdown( SocketShutdown.Both );
      }

      socket.Close();
   }

   public void Dispose() => _authDoneEvent?.Dispose();

   private sealed class StateObject
   {
      public const int BufferSize = 1024;
      public byte[] Buffer { get; } = new byte[1024];
      public Socket Listener;
      public Socket Receiver;
      public string ParsedAuthParams;
   }
}
