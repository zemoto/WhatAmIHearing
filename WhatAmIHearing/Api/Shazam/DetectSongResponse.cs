using System.Text.Json.Serialization;

namespace WhatAmIHearing.Api.Shazam
{
   // Only keeping track of the parts we care about
   internal sealed class DetectSongResponse
   {
      [JsonPropertyName( "track" )]
      public DetectedTrackInfo Track { get; set; }
   }

   internal sealed class DetectedTrackInfo
   {
      [JsonPropertyName( "url" )]
      public string Url { get; set; }
   }
}
