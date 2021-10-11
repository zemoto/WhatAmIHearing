using NAudio.CoreAudioApi;
using NAudio.Wave;
using System;
using System.IO;
using ZemotoUtils;

namespace WhatAmIHearing
{
   internal sealed class RecordingStateChangedEventArgs : EventArgs
   {
      public RecordingState NewState { get; }

      public RecordingStateChangedEventArgs( RecordingState state ) => NewState = state;
   }

   internal sealed class RecordingManager
   {
      private static readonly string _outputFilePath = Path.Combine( Path.GetTempPath(), "recorded.wav" );

      private WasapiLoopbackCapture _audioCapturer;
      private WaveFileWriter _audioWriter;

      public event EventHandler<RecordingStateChangedEventArgs> RecordingStateChanged;

      public void StartRecording( MMDevice device )
      {
         _audioCapturer = new WasapiLoopbackCapture( device );

         _audioCapturer.DataAvailable += OnDataCaptured;
         _audioCapturer.RecordingStopped += OnRecordingStopped;

         FileUtils.SafeDeleteFile( _outputFilePath );
         _audioWriter = new WaveFileWriter( _outputFilePath, _audioCapturer.WaveFormat );
         _audioCapturer.StartRecording();

         RecordingStateChanged?.Invoke( this, new RecordingStateChangedEventArgs( RecordingState.Recording ) );
      }

      public void StopRecording() => _audioCapturer.StopRecording();

      private void OnDataCaptured( object sender, WaveInEventArgs e ) => _audioWriter.Write( e.Buffer, 0, e.BytesRecorded );

      private void OnRecordingStopped( object sender, StoppedEventArgs e )
      {
         _audioWriter?.Dispose();
         _audioWriter = null;

         _audioCapturer?.Dispose();
         _audioCapturer = null;

         RecordingStateChanged?.Invoke( this, new RecordingStateChangedEventArgs( RecordingState.Stopped ) );
      }
   }
}
