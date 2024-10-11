using System;
using System.Net;
using System.Text.Json;
using System.Threading.Tasks;

namespace WhatAmIHearing.Api.Shazam;

internal sealed class ShazamApi( ShazamApiSettings settings ) : IDisposable
{
   private const string DetectApiEndpoint = "https://shazam.p.rapidapi.com/songs/detect";

   private readonly ShazamApiClient _client = new( settings );

   public HttpStatusCode LastStatusCode => _client.LastStatusCode;

   public void Dispose() => _client.Dispose();

   public void CancelRequests() => _client.CancelRequests();

   public async Task<DetectedTrackInfo> DetectSongAsync( byte[] audioData )
   {
      var detectResponse = await _client.SendPostRequestAsync( DetectApiEndpoint, audioData );
      if ( string.IsNullOrEmpty( detectResponse ) )
      {
         return null;
      }

      var parsedResponse = JsonSerializer.Deserialize<DetectSongResponse>( detectResponse );
      return parsedResponse?.Track;
   }
}
