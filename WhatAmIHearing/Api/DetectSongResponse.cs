using System.Text.Json.Serialization;

namespace WhatAmIHearing.Api
{
   // Only keeping track of the parts we care about
   internal sealed class DetectSongResponse
   {
      [JsonPropertyName( "track" )]
      public DetectedTrackInfo Track { get; set; }
   }

   internal sealed class DetectedTrackInfo
   {
      [JsonPropertyName( "share" )]
      public DetectedTrackShareInfo Share { get; set; }
   }

   internal sealed class DetectedTrackShareInfo
   {
      [JsonPropertyName( "href" )]
      public string SongUrl { get; set; }
   }
}
