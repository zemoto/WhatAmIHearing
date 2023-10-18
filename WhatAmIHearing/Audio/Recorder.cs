using NAudio.CoreAudioApi;
using NAudio.Wave;
using System;
using System.IO;
using WhatAmIHearing.Api.Shazam;

namespace WhatAmIHearing.Audio;

internal sealed class Recorder : IDisposable
{
   private long _maxBytes;
   private bool _cancelled;
   private WasapiLoopbackCapture _audioCapturer;
   private WaveFileWriter _audioWriter;
   private MemoryStream _recordedFileStream;

   public event EventHandler<RecordingProgressEventArgs> RecordingProgress;
   public event EventHandler<RecordingFinishedEventArgs> RecordingFinished;

   public void Dispose()
   {
      _cancelled = false;

      _audioWriter?.Dispose();
      _audioWriter = null;

      _recordedFileStream?.Dispose();
      _recordedFileStream = null;

      if ( _audioCapturer is not null )
      {
         _audioCapturer.Dispose();
         _audioCapturer.DataAvailable -= OnDataCaptured;
         _audioCapturer.RecordingStopped -= OnRecordingStopped;
         _audioCapturer = null;
      }
   }

   public void StartRecording( MMDevice device )
   {
      _audioCapturer = new WasapiLoopbackCapture( device );
      _audioCapturer.DataAvailable += OnDataCaptured;
      _audioCapturer.RecordingStopped += OnRecordingStopped;

      _recordedFileStream = new MemoryStream();
      _audioWriter = new WaveFileWriter( _recordedFileStream, _audioCapturer.WaveFormat );
      _maxBytes = ShazamSpecEnforcer.GetMaxRecordingSize( _audioCapturer.WaveFormat.AverageBytesPerSecond );

      RecordingProgress.Invoke( this, new RecordingProgressEventArgs( 0, _maxBytes ) );

      _audioCapturer.StartRecording();
   }

   public void CancelRecording()
   {
      _cancelled = true;
      _audioCapturer.StopRecording();
   }

   private void OnDataCaptured( object sender, WaveInEventArgs e )
   {
      if ( _audioWriter.Position + e.BytesRecorded >= _maxBytes )
      {
         _audioCapturer.StopRecording();
      }
      else
      {
         _audioWriter.Write( e.Buffer, 0, e.BytesRecorded );
         RecordingProgress.Invoke( this, new RecordingProgressEventArgs( _audioWriter.Position, _maxBytes ) );
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

      RecordingFinished.Invoke( this, new RecordingFinishedEventArgs( data, format ) );
      Dispose();
   }
}
