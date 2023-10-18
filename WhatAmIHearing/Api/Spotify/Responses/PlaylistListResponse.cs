using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace WhatAmIHearing.Api.Spotify.Responses;

internal sealed class PlaylistListResponse
{
   [JsonPropertyName( "items" )]
   public List<PlaylistInfo> Playlists { get; set; }
}

internal sealed class PlaylistInfo
{
   [JsonPropertyName( "name" )]
   public string Name { get; set; }

   [JsonPropertyName( "id" )]
   public string Id { get; set; }
}
