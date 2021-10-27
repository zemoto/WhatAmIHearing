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
      public List<SongSearchTrackItem> Items { get; set; }
   }

   internal class SongSearchTrackItem
   {
      [JsonPropertyName( "id" )]
      public string Id { get; set; }

      [JsonPropertyName( "name" )]
      public string Name { get; set; }

      [JsonPropertyName( "artists" )]
      public List<SongSearchTrackItemArtistInfo> Artists { get; set; }
   }

   internal class SongSearchTrackItemArtistInfo
   {
      [JsonPropertyName( "name" )]
      public string Name { get; set; }
   }
}
