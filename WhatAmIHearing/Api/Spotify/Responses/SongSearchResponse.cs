using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace WhatAmIHearing.Api.Spotify.Responses
{
   internal sealed class SongSearchResponse
   {
      [JsonPropertyName( "tracks" )]
      public SongSearchTrackInfo Tracks { get; set; }
   }

   internal sealed class SongSearchTrackInfo
   {
      [JsonPropertyName( "items" )]
      public List<TrackItem> Items { get; set; }
   }
}
