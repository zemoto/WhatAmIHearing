using System;
using System.Collections.Generic;
using System.Text;

namespace WhatAmIHearing.Api.Spotify
{
   internal sealed class SpotifyApiClient : ApiClient
   {
      protected override Dictionary<string, string> ApiHeaders
      {
         get
         {
            string authHeader;
            var accessToken = Properties.UserSettings.Default.SpotifyAccessToken;
            if ( !string.IsNullOrEmpty( accessToken ) )
            {
               authHeader = $"Bearer {accessToken}";
            }
            else
            {
               var bytes = Encoding.ASCII.GetBytes( $"{ApiConstants.SpotifyClientId}:{ApiConstants.SpotifyClientSecret}" );
               authHeader = $"Basic {Convert.ToBase64String( bytes )}";
            }

            return new() { ["Authorization"] = authHeader };
         }
      }
   }
}
