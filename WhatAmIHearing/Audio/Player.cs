using NAudio.Wave;
using System.IO;

namespace WhatAmIHearing.Audio;

internal static class Player
{
   public static void PlayAudio( byte[] audioData, WaveFormat format )
   {
      var waveProvider = new RawSourceWaveStream( new MemoryStream( audioData ), format );
      var waveOut = new WaveOutEvent();

      waveOut.PlaybackStopped += ( s, e ) =>
      {
         waveProvider?.Dispose();
         waveOut?.Dispose();
      };

      waveOut.Init( waveProvider );
      waveOut.Play();
   }
}
