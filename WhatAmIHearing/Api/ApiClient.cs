﻿using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace WhatAmIHearing.Api
{
   internal abstract class ApiClient : IDisposable
   {
      protected abstract Dictionary<string, string> ApiHeaders { get; }

      private static readonly List<ApiClient> _apiClients = new();

      public static void CancelRequests() => _apiClients.ForEach( x => x._cancelToken.Cancel() );

      private readonly HttpClient _client = new();
      private readonly CancellationTokenSource _cancelToken = new();

      protected ApiClient() => _apiClients.Add( this );

      public void Dispose()
      {
         _client?.Dispose();
         _cancelToken?.Dispose();
         _apiClients.Remove( this );
      }

      public async Task<string> SendPostRequestAsync( string endpoint, byte[] data )
      {
         var message = new HttpRequestMessage( HttpMethod.Post, endpoint ) { Content = new StringContent( Convert.ToBase64String( data ) ) };

         foreach( var ( key, value ) in ApiHeaders )
         {
            message.Headers.Add( key, value );
         }

         var response = await _client.SendAsync( message, _cancelToken.Token ).ConfigureAwait( false );

         return response.IsSuccessStatusCode ? await response.Content.ReadAsStringAsync().ConfigureAwait( false ) : string.Empty;
      }
   }
}
