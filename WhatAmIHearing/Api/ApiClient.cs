using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace WhatAmIHearing.Api;

internal abstract class ApiClient : IDisposable
{
   private static readonly HttpClient _client = new();
   public static void StaticDispose() => _client.Dispose();

   protected abstract Dictionary<string, string> ApiHeaders { get; }

   private readonly List<CancellationTokenSource> _cancelTokenSources = new();
   private readonly object _cancelTokenLock = new();

   public void CancelRequests()
   {
      lock ( _cancelTokenLock )
      {
         _cancelTokenSources.ForEach( x => x.Cancel() );
      }
   }

   public void Dispose() => CancelRequests();

   public async Task<string> SendPostRequestAsync( string endpoint )
   {
      using var message = new HttpRequestMessage( HttpMethod.Post, endpoint );
      return await SendMessageAsync( message );
   }

   public async Task<string> SendPostRequestAsync( string endpoint, string body )
   {
      using var message = new HttpRequestMessage( HttpMethod.Post, endpoint ) { Content = new StringContent( body ) };
      return await SendMessageAsync( message );
   }

   public async Task<string> SendPostRequestAsync( string endpoint, byte[] data )
   {
      using var message = new HttpRequestMessage( HttpMethod.Post, endpoint ) { Content = new StringContent( Convert.ToBase64String( data ) ) };
      return await SendMessageAsync( message );
   }

   public async Task<string> SendPostRequestAsync( string endpoint, Dictionary<string,string> formUrlEncodedData )
   {
      using var message = new HttpRequestMessage( HttpMethod.Post, endpoint ) { Content = new FormUrlEncodedContent( formUrlEncodedData ) };
      return await SendMessageAsync( message );
   }

   public async Task<string> SendGetRequestAsync( string endpoint )
   {
      using var message = new HttpRequestMessage( HttpMethod.Get, endpoint );
      return await SendMessageAsync( message );
   }

   private async Task<string> SendMessageAsync( HttpRequestMessage message )
   {
      foreach ( var (key, value) in ApiHeaders )
      {
         message.Headers.Add( key, value );
      }

      CancellationTokenSource cancelTokenSource;
      lock ( _cancelTokenLock )
      {
         cancelTokenSource = new CancellationTokenSource();
         _cancelTokenSources.Add( cancelTokenSource );
      }

      try
      {
         var response = await _client.SendAsync( message, cancelTokenSource.Token );
         return response.IsSuccessStatusCode ? await response.Content.ReadAsStringAsync() : string.Empty;
      }
      finally
      {
         lock ( _cancelTokenLock )
         {
            cancelTokenSource.Dispose();
            _cancelTokenSources.Remove( cancelTokenSource );
         }
      }
   }
}
