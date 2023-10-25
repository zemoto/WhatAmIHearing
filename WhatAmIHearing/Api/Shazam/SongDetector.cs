using System;
using System.Threading.Tasks;

namespace WhatAmIHearing.Api.Shazam;

internal sealed class SongDetector : IDisposable
{
   private readonly ShazamApi _api = new();

   public void Dispose() => _api.Dispose();

   public async Task<DetectedTrackInfo> DetectSongAsync( byte[] recordingData )
   {
      DetectedTrackInfo detectedSong = null;
      try
      {
         detectedSong = await _api.DetectSongAsync( recordingData );
      }
      catch ( TaskCanceledException )
      {
      }

      return detectedSong;
   }
}
