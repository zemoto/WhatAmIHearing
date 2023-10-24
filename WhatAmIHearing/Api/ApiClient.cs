﻿using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace WhatAmIHearing.Api;

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
      _ = _apiClients.Remove( this );
   }

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

      var response = await _client.SendAsync( message, _cancelToken.Token );

      return response.IsSuccessStatusCode ? await response.Content.ReadAsStringAsync() : string.Empty;
   }
}
