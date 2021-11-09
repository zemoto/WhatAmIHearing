using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace WhatAmIHearing.Api.Spotify.Responses
{
   internal sealed class PlaylistTrackListResponse
   {
      [JsonPropertyName( "items" )]
      public List<PlaylistItem> Items { get; set; }
   }

   internal sealed class PlaylistItem
   {
      [JsonPropertyName( "track" )]
      public TrackItem Track { get; set; }
   }
}
