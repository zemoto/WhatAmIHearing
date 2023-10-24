using System;
using System.Collections.Generic;
using System.Text;

namespace WhatAmIHearing.Api.Spotify;

internal sealed class SpotifyApiClient : ApiClient
{
   protected override Dictionary<string, string> ApiHeaders { get; }

   public SpotifyApiClient( bool isAuthenticated = true )
   {
      string authHeader;
      if ( isAuthenticated )
      {
         authHeader = $"Bearer {AppSettings.Instance.SpotifyAccessToken}";
      }
      else
      {
         var bytes = Encoding.ASCII.GetBytes( $"{ApiConstants.SpotifyClientId}:{ApiConstants.SpotifyClientSecret}" );
         authHeader = $"Basic {Convert.ToBase64String( bytes )}";
      }

      ApiHeaders = new() { ["Authorization"] = authHeader };
   }
}
