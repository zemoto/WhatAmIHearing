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

namespace WhatAmIHearing.Api.Spotify
{
   internal sealed class SpotifyAuthenticator : IDisposable
   {
      private static readonly IPEndPoint _loopbackIpAddress = new( IPAddress.Loopback, 1378 );
      private const string RedirectUrl = "http://127.0.0.1:1378";
      private static readonly string AuthUrl = $"https://accounts.spotify.com/authorize?client_id={ApiConstants.SpotifyClientId}&response_type=code&redirect_uri={RedirectUrl}&scope=playlist-read-private,playlist-modify-private&show_dialog=true";
      private const string TokenExchangeEndpoint = "https://accounts.spotify.com/api/token";

      private readonly AutoResetEvent _authDoneEvent = new( false );
      private Thread _authenticateThread;

      private Properties.UserSettings Settings => Properties.UserSettings.Default;

      private bool IsAuthenticated() => !string.IsNullOrEmpty( Settings.SpotifyAccessToken ) &&
                                        !string.IsNullOrEmpty( Settings.SpotifyRefreshToken ) &&
                                        DateTime.UtcNow < Settings.SpotifyExpirationTimeUtc;

      public async Task<bool> EnsureAuthenticationIsValid()
      {
         if ( !string.IsNullOrEmpty( Settings.SpotifyAccessToken ) && !string.IsNullOrEmpty( Settings.SpotifyRefreshToken ) )
         {
            if ( DateTime.UtcNow + TimeSpan.FromMinutes( 1 ) > Settings.SpotifyExpirationTimeUtc )
            {
               await RefreshAuthenticationAsync().ConfigureAwait( false );
            }
         }

         return IsAuthenticated();
      }

      public async Task<bool> AuthenticateAsync()
      {
         if ( !IsAuthenticated() )
         {
            await AuthenticateWithBrowserAsync().ConfigureAwait( false );
         }

         return IsAuthenticated();
      }

      private async Task RefreshAuthenticationAsync()
      {
         var data = new Dictionary<string, string>
         {
            ["grant_type"] = "refresh_token",
            ["refresh_token"] = Settings.SpotifyRefreshToken
         };

         await ExchangeTokensAsync( data ).ConfigureAwait( false );
      }

      private async Task AuthenticateWithBrowserAsync()
      {
         _ = Process.Start( new ProcessStartInfo( AuthUrl ) { UseShellExecute = true } );

         if ( _authenticateThread is null )
         {
            _authenticateThread = new Thread( ListenForAuthentication ) { IsBackground = true };
            _authenticateThread.Start();
         }

         await Task.Run( _authenticateThread.Join ).ConfigureAwait( false );
      }

      private async void ListenForAuthentication()
      {
         var listener = new Socket( _loopbackIpAddress.Address.AddressFamily, SocketType.Stream, ProtocolType.IP );
         listener.Bind( _loopbackIpAddress );
         listener.Listen( 10 );

         var state = new StateObject { Listener = listener };
         listener.BeginAccept( AcceptCallback, state );

         _authDoneEvent.WaitOne( TimeSpan.FromMinutes( 2 ) );

         if ( !string.IsNullOrEmpty( state.ParsedAuthParams ) && state.ParsedAuthParams.Contains( "code" ) )
         {
            var values = HttpUtility.ParseQueryString( state.ParsedAuthParams );
            var authCode = values["/?code"];

            await GetTokensFromAuthCodeAsync( authCode ).ConfigureAwait( false );
         }

         _authenticateThread = null;
      }

      private async Task GetTokensFromAuthCodeAsync( string authCode )
      {
         var data = new Dictionary<string, string>
         {
            ["grant_type"] = "authorization_code",
            ["code"] = authCode,
            ["redirect_uri"] = RedirectUrl
         };

         await ExchangeTokensAsync( data ).ConfigureAwait( false );
      }

      private async Task ExchangeTokensAsync( Dictionary<string,string> requestData )
      {
         using var client = new SpotifyApiClient( false );
         var responseJson = await client.SendPostRequestAsync( TokenExchangeEndpoint, requestData ).ConfigureAwait( false );

         if ( !string.IsNullOrEmpty( responseJson ) )
         {
            using var json = JsonDocument.Parse( responseJson );
            if ( json.RootElement.TryGetProperty( "access_token", out var jsonAccessToken ) )
            {
               Settings.SpotifyAccessToken = jsonAccessToken.ToString();

               if ( json.RootElement.TryGetProperty( "refresh_token", out var jsonRefreshToken ) )
               {
                  Settings.SpotifyRefreshToken = jsonRefreshToken.ToString();
               }

               if ( json.RootElement.TryGetProperty( "expires_in", out var jsonExpiresIn ) )
               {
                  Settings.SpotifyExpirationTimeUtc = DateTime.UtcNow + TimeSpan.FromSeconds( jsonExpiresIn.GetInt32() );
               }

               Settings.Save();
               return;
            }
         }

         // In case of any failure clear out what tokens we have
         Settings.SpotifyAccessToken = string.Empty;
         Settings.SpotifyRefreshToken = string.Empty;
         Settings.SpotifyExpirationTimeUtc = default;
         Settings.Save();
      }

      private void AcceptCallback( IAsyncResult ar )
      {
         var state = (StateObject)ar.AsyncState;
         try
         {
            state.Receiver = state.Listener.EndAccept( ar );
            state.Receiver.BeginReceive( state.Buffer, 0, StateObject.BufferSize, SocketFlags.None, ReceiveCallback, state );
            CleanupSocket( state.Listener );
         }
         catch
         {
            _authDoneEvent.Set();
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
               if ( lineItems[0].Equals( "GET", StringComparison.InvariantCultureIgnoreCase ) )
               {
                  state.ParsedAuthParams = lineItems[1];
               }

               const string DoneMessage = "Authentication done, you can close this page";
               var bytes = Encoding.UTF8.GetBytes( DoneMessage );
               state.Receiver.BeginSend( bytes, 0, bytes.Length, SocketFlags.None, SendCallback, state );
            }
         }
         catch { }
      }

      private void SendCallback( IAsyncResult ar )
      {
         var state = (StateObject)ar.AsyncState;
         try
         {
            state.Receiver.EndSend( ar );
         }
         catch { }

         CleanupSocket( state.Receiver );
         _authDoneEvent.Set();
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
}
