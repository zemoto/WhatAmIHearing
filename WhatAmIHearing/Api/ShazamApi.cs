using System.Text.Json;
using System.Threading.Tasks;

namespace WhatAmIHearing.Api
{
   internal static class ShazamApi
   {
      private const string DetectApiEndpoint = "https://shazam.p.rapidapi.com/songs/detect";

      public static async Task<string> DetectSongAsync( byte[] audioData )
      {
         using var httpClient = new ApiClient();
         var detectResponse = await httpClient.SendPostRequestAsync( DetectApiEndpoint, audioData ).ConfigureAwait( false );

         if ( !string.IsNullOrEmpty( detectResponse ) )
         {
            var parsedResponse = JsonSerializer.Deserialize<DetectSongResponse>( detectResponse );
            return parsedResponse?.Track?.Share?.SongUrl;
         }

         return string.Empty;
      }
   }
}
