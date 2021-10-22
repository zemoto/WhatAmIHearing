using System;
using System.Collections.Generic;
using System.Text;

namespace WhatAmIHearing.Api.Spotify
{
   internal sealed class SpotifyApiClient : ApiClient
   {
      protected override Dictionary<string, string> ApiHeaders { get; } = new()
      {
         ["Authorization"] = $"Basic {ToBase64( $"{ApiConstants.SpotifyClientId}:{ApiConstants.SpotifyClientSecret}" )}"
      };

      private static string ToBase64( string value )
      {
         var bytes = Encoding.ASCII.GetBytes( value );
         return Convert.ToBase64String( bytes );
      }
   }
}
