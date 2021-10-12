namespace WhatAmIHearing.Api
{
   internal static class ShazamApi
   {
      private const string DetectApiEndpoint = "https://shazam.p.rapidapi.com/songs/detect";

      public static string DetectSong( byte[] audioData )
      {
         using var httpClient = new ApiClient();
         var detectResponse = httpClient.SendPostRequest( DetectApiEndpoint, audioData );

         if ( !string.IsNullOrEmpty( detectResponse ) )
         {
            // TODO
         }

         return string.Empty;
      }
   }
}
