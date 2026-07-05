using System.Text.Json.Serialization;

namespace WhatAmIHearing.Shazam;

// Only keeping track of the parts we care about
internal sealed class DetectSongResponse
{
   [JsonPropertyName( "track" )]
   public DetectedTrackInfo? Track { get; set; }
}

internal sealed class DetectedTrackInfo
{
   public bool IsComplete => !string.IsNullOrEmpty( Title ) && !string.IsNullOrEmpty( Subtitle ) && !string.IsNullOrEmpty( Url );

   [JsonPropertyName( "title" )]
   public string Title { get; set; } = string.Empty;

   [JsonPropertyName( "subtitle" )]
   public string Subtitle { get; set; } = string.Empty;

   [JsonPropertyName( "url" )]
   public string Url { get; set; } = string.Empty;

   [JsonPropertyName( "share" )]
   public DetectedTrackShareInfo? ShareInfo { get; set; }
}

internal sealed class DetectedTrackShareInfo
{
   [JsonPropertyName( "image" )]
   public string CoverArtUrl { get; set; } = string.Empty;

   [JsonPropertyName( "href" )]
   public string ShazamUrl { get; set; } = string.Empty;
}
