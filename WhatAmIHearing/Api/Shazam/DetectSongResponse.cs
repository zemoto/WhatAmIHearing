using System.Text.Json.Serialization;

namespace WhatAmIHearing.Api.Shazam;

// Only keeping track of the parts we care about
internal sealed class DetectSongResponse
{
   [JsonPropertyName( "track" )]
   public DetectedTrackInfo Track { get; set; }
}

internal sealed class DetectedTrackInfo
{
   public bool IsComplete => !string.IsNullOrEmpty( Title ) && !string.IsNullOrEmpty( Subtitle ) && !string.IsNullOrEmpty( Url );

   [JsonPropertyName( "title" )]
   public string Title { get; set; }

   [JsonPropertyName( "subtitle" )]
   public string Subtitle { get; set; }

   [JsonPropertyName( "url" )]
   public string Url { get; set; }

   [JsonPropertyName( "share" )]
   public DetectedTrackShareInfo ShareInfo { get; set; }
}

internal sealed class DetectedTrackShareInfo
{
   [JsonPropertyName( "image" )]
   public string CoverArtUrl { get; set; }

   [JsonPropertyName( "href" )]
   public string ShazamUrl { get; set; }
}
