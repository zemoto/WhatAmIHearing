using NAudio.CoreAudioApi;
using NAudio.Wave;
using System;
using System.IO;
using WhatAmIHearing.Api;

namespace WhatAmIHearing
{
   internal sealed class RecordingFinishedEventArgs : EventArgs
   {
      public byte[] RecordedData { get; }
      public WaveFormat Format { get; }

      public RecordingFinishedEventArgs( byte[] recordedData, WaveFormat format )
      {
         RecordedData = recordedData;
         Format = format;
      }
   }

   internal sealed class Recorder
   {
      private readonly IStatusTextDisplayer _statusText;

      private int _maxRecordingSize;
      private bool _cancelled;

      private WasapiLoopbackCapture _audioCapturer;
      private WaveFileWriter _audioWriter;
      private MemoryStream _recordedFileStream;

      public event EventHandler<RecordingFinishedEventArgs> RecordingStopped;

      public Recorder( IStatusTextDisplayer statusText ) => _statusText = statusText;

      public void StartRecording( MMDevice device )
      {
         _audioCapturer = new WasapiLoopbackCapture( device );
         _audioCapturer.DataAvailable += OnDataCaptured;
         _audioCapturer.RecordingStopped += OnRecordingStopped;

         _recordedFileStream = new MemoryStream();
         _audioWriter = new WaveFileWriter( _recordedFileStream, _audioCapturer.WaveFormat );
         _maxRecordingSize = ShazamSpecEnforcer.GetMaxRecordingSize( _audioCapturer.WaveFormat.AverageBytesPerSecond );

         _statusText.StatusText = $"Recording: 0/{_maxRecordingSize} bits";

         _audioCapturer.StartRecording();
      }

      public void CancelRecording()
      {
         _cancelled = true;
         _audioCapturer.StopRecording();
      }

      private void OnDataCaptured( object sender, WaveInEventArgs e )
      {
         if ( _audioWriter.Position + e.BytesRecorded >= _maxRecordingSize )
         {
            _audioCapturer.StopRecording();
         }
         else
         {
            _audioWriter.Write( e.Buffer, 0, e.BytesRecorded );
            _statusText.StatusText = $"Recording: {_audioWriter.Position}/{_maxRecordingSize} bits";
         }
      }

      private void OnRecordingStopped( object sender, StoppedEventArgs e )
      {
         WaveFormat format = null;
         byte[] data = null;

         if ( !_cancelled )
         {
            _audioWriter.Flush();
            data = ShazamSpecEnforcer.ResampleAudioToMatchSpec( _recordedFileStream, out format );
         }

         RecordingStopped.Invoke( this, new RecordingFinishedEventArgs( data, format ) );
         Cleanup();
      }

      private void Cleanup()
      {
         _cancelled = false;

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
