using NAudio.CoreAudioApi;
using NAudio.Wave;
using NAudio.Wave.SampleProviders;
using System;
using System.IO;

namespace WhatAmIHearing
{
   internal sealed class RecordingFinishedEventArgs : EventArgs
   {
      public byte[] RecordedData { get; }

      public RecordingFinishedEventArgs( byte[] recordedData ) => RecordedData = recordedData;
   }

   internal sealed class Recorder
   {
      private const double OutputBytesPerSecond = 2 * 44100; // Bytes per sample * sample rate
      private const int MaxOutputSize = 500 * 1000; // 500KB
      private int _maxRecordingSize;

      private WasapiLoopbackCapture _audioCapturer;
      private WaveFileWriter _audioWriter;
      private MemoryStream _recordedFileStream;

      public event EventHandler<RecordingFinishedEventArgs> RecordingStopped;

      public void StartRecording( MMDevice device )
      {
         _audioCapturer = new WasapiLoopbackCapture( device );

         _maxRecordingSize = (int)( _audioCapturer.WaveFormat.AverageBytesPerSecond / OutputBytesPerSecond * MaxOutputSize );

         _audioCapturer.DataAvailable += OnDataCaptured;
         _audioCapturer.RecordingStopped += OnRecordingStopped;

         _recordedFileStream = new MemoryStream();
         _audioWriter = new WaveFileWriter( _recordedFileStream, _audioCapturer.WaveFormat );
         _audioCapturer.StartRecording();
      }

      public void StopRecording() => _audioCapturer.StopRecording();

      private void OnDataCaptured( object sender, WaveInEventArgs e )
      {
         if ( _audioWriter.Position + e.BytesRecorded >= _maxRecordingSize )
         {
            StopRecording();
         }
         else
         {
            _audioWriter.Write( e.Buffer, 0, e.BytesRecorded );
         }
      }

      private void OnRecordingStopped( object sender, StoppedEventArgs e )
      {
         _audioWriter.Flush();
         var resampledData = GetResampledData();
         RecordingStopped.Invoke( this, new RecordingFinishedEventArgs( resampledData ) );

         Cleanup();
      }

      private byte[] GetResampledData()
      {
         _recordedFileStream.Position = 0;
         using var reader = new WaveFileReader( _recordedFileStream );
         var sampleProvider = new WaveToSampleProvider( reader );

         var resampledWave =
            new StereoToMonoProvider16(                                      // To 1 channel
            new SampleToWaveProvider16(                                      // To 16 bits per sample
               new WdlResamplingSampleProvider( sampleProvider, 44100 ) ) ); // to 44100 sample rate

         using var resampledStream = new MemoryStream();
         var bytes = new byte[resampledWave.WaveFormat.AverageBytesPerSecond];
         int bytesRead;
         while ( ( bytesRead = resampledWave.Read( bytes, 0, bytes.Length ) ) > 0 )
         {
            resampledStream.Write( bytes, 0, bytesRead );
         }

         return resampledStream.ToArray();
      }

      private void Cleanup()
      {
         _audioWriter?.Dispose();
         _audioWriter = null;
         _recordedFileStream = null;

         _audioCapturer?.Dispose();
         _audioCapturer.DataAvailable -= OnDataCaptured;
         _audioCapturer.RecordingStopped -= OnRecordingStopped;
         _audioCapturer = null;
      }
   }
}
