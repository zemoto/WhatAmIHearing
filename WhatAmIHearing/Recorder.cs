﻿using NAudio.CoreAudioApi;
using NAudio.Wave;
using System;
using System.IO;
using WhatAmIHearing.Api;

namespace WhatAmIHearing
{
   internal sealed class RecordingFinishedEventArgs : EventArgs
   {
      public byte[] RecordedData { get; }

      public RecordingFinishedEventArgs( byte[] recordedData ) => RecordedData = recordedData;
   }

   internal sealed class Recorder
   {
      private int _maxRecordingSize;
      private bool _cancelled;

      private WasapiLoopbackCapture _audioCapturer;
      private WaveFileWriter _audioWriter;
      private MemoryStream _recordedFileStream;

      public event EventHandler<RecordingFinishedEventArgs> RecordingStopped;

      public void StartRecording( MMDevice device )
      {
         _audioCapturer = new WasapiLoopbackCapture( device );
         _audioCapturer.DataAvailable += OnDataCaptured;
         _audioCapturer.RecordingStopped += OnRecordingStopped;

         _recordedFileStream = new MemoryStream();
         _audioWriter = new WaveFileWriter( _recordedFileStream, _audioCapturer.WaveFormat );
         _maxRecordingSize = ShazamSpecEnforcer.GetMaxRecordingSize( _audioCapturer.WaveFormat.AverageBytesPerSecond );

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
         }
      }

      private void OnRecordingStopped( object sender, StoppedEventArgs e )
      {
         byte[] data = null;

         if ( !_cancelled )
         {
            _audioWriter.Flush();
            data = ShazamSpecEnforcer.ResampleAudioToMatchSpec( _recordedFileStream );
         }

         RecordingStopped.Invoke( this, new RecordingFinishedEventArgs( data ) );
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
