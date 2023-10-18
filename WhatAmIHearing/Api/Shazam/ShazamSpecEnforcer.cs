using NAudio.Wave;
using NAudio.Wave.SampleProviders;
using System.IO;

namespace WhatAmIHearing.Api.Shazam;

internal static class ShazamSpecEnforcer
{
   private const int RequiredSampleRate = 44100;
   private const int RequiredBytesPerSample = 2;
   private const int MaxAudioDataSize = 500 * 1000; // 500KB

   public static long GetMaxRecordingSize( int recordingDeviceBytesPerSecond )
   {
      const int requiredBytesPerSecond = RequiredBytesPerSample * RequiredSampleRate;
      return (long)( (double)recordingDeviceBytesPerSecond / requiredBytesPerSecond * MaxAudioDataSize );
   }

   public static byte[] ResampleAudioToMatchSpec( MemoryStream waveFileStream, out WaveFormat format )
   {
      waveFileStream.Position = 0;
      using var reader = new WaveFileReader( waveFileStream );

      // Resample to required 1 channel 16 bit PCM 44100 sample rate
      var resampledWave = new WdlResamplingSampleProvider( reader.ToSampleProvider(), RequiredSampleRate ).ToMono().ToWaveProvider16();

      int bytesRead;
      using var resampledStream = new MemoryStream();
      var buffer = new byte[resampledWave.WaveFormat.AverageBytesPerSecond];
      while ( ( bytesRead = resampledWave.Read( buffer, 0, buffer.Length ) ) > 0 )
      {
         resampledStream.Write( buffer, 0, bytesRead );
      }

      format = resampledWave.WaveFormat;
      return resampledStream.ToArray();
   }
}
