using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace WhatAmIHearing.Api.Spotify.Responses
{
   internal class TrackItem
   {
      [JsonPropertyName( "id" )]
      public string Id { get; set; }

      [JsonPropertyName( "name" )]
      public string Name { get; set; }

      [JsonPropertyName( "artists" )]
      public List<TrackItemArtistInfo> Artists { get; set; }
   }

   internal class TrackItemArtistInfo
   {
      [JsonPropertyName( "name" )]
      public string Name { get; set; }
   }
}
