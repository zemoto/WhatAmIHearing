using System.Text.Json;
using System.Threading.Tasks;

namespace WhatAmIHearing.Api.Shazam
{
   internal static class ShazamApi
   {
      private const string DetectApiEndpoint = "https://shazam.p.rapidapi.com/songs/detect";

      public static async Task<DetectedTrackInfo> DetectSongAsync( byte[] audioData )
      {
         using var client = new ShazamApiClient();
         var detectResponse = await client.SendPostRequestAsync( DetectApiEndpoint, audioData ).ConfigureAwait( false );

         if ( !string.IsNullOrEmpty( detectResponse ) )
         {
            var parsedResponse = JsonSerializer.Deserialize<DetectSongResponse>( detectResponse );
            return parsedResponse?.Track;
         }

         return null;
      }
   }
}
