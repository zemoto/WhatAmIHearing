using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

namespace WhatAmIHearing.Api
{
   internal sealed class ApiClient : IDisposable
   {
      private static readonly Dictionary<string, string> ApiHeaders = new Dictionary<string, string>
      {
         ["x-rapidapi-host"] = "shazam.p.rapidapi.com",
         ["x-rapidapi-key"] = ApiConstants.ApiKey
      };

      private readonly HttpClient _client = new HttpClient();

      public void Dispose() => _client?.Dispose();

      public async Task<string> SendPostRequestAsync( string endpoint, byte[] data )
      {
         var message = new HttpRequestMessage( HttpMethod.Post, endpoint ) { Content = new StringContent( Convert.ToBase64String( data ) ) };

         foreach( var ( key, value ) in ApiHeaders )
         {
            message.Headers.Add( key, value );
         }

         var response = await _client.SendAsync( message ).ConfigureAwait( false );

         return response.IsSuccessStatusCode ? await response.Content.ReadAsStringAsync().ConfigureAwait( false ) : string.Empty;
      }
   }
}
