using NAudio.Wave;
using System.IO;

namespace WhatAmIHearing
{
   internal class AudioPlayer
   {
      private readonly byte[] _audioData;
      private readonly WaveFormat _format;

      private RawSourceWaveStream _waveProvider;
      private WaveOutEvent _waveOut;

      public AudioPlayer( byte[] audioData, WaveFormat format )
      {
         _audioData = audioData;
         _format = format;
      }

      public void PlayAudio()
      {
         if ( _waveOut != null )
         {
            return;
         }

         _waveProvider = new RawSourceWaveStream( new MemoryStream( _audioData ), _format );
         _waveOut = new WaveOutEvent();
         _waveOut.PlaybackStopped += OnPlaybackStopped;
         _waveOut.Init( _waveProvider );
         _waveOut.Play();
      }

      private void OnPlaybackStopped( object sender, StoppedEventArgs e )
      {
         _waveProvider?.Dispose();
         _waveProvider = null;

         _waveOut?.Dispose();
         _waveOut = null;
      }
   }
}
