using NAudio.CoreAudioApi;
using NAudio.Wave;
using System;
using System.IO;

namespace WhatAmIHearing.Audio;

internal sealed class Recorder : IDisposable
{
   private readonly WaveFormat _waveFormat;
   private readonly long _maxBytesToRecord;
   private long _bytesToRecord;

   private bool _cancelled;
   private WasapiLoopbackCapture _audioCapturer;
   private WaveFileWriter _audioWriter;
   private MemoryStream _recordedFileStream;

   public event EventHandler<RecordingProgressEventArgs> RecordingProgress;
   public event EventHandler<RecordingFinishedEventArgs> RecordingFinished;

   public Recorder( WaveFormat waveFormat, long maxBytesToRecord )
   {
      _waveFormat = waveFormat;
      _maxBytesToRecord = maxBytesToRecord;
   }

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

   public void StartRecording( MMDevice device, double percentToRecord )
   {
      _audioCapturer = new WasapiLoopbackCapture( device ) {  WaveFormat = _waveFormat };
      _audioCapturer.DataAvailable += OnDataCaptured;
      _audioCapturer.RecordingStopped += OnRecordingStopped;

      _recordedFileStream = new MemoryStream();
      _audioWriter = new WaveFileWriter( _recordedFileStream, _audioCapturer.WaveFormat );

      RecordingProgress.Invoke( this, new RecordingProgressEventArgs( 0, _bytesToRecord ) );

      _bytesToRecord = (int)( percentToRecord * _maxBytesToRecord );
      _audioCapturer.StartRecording();
   }

   public void CancelRecording()
   {
      _cancelled = true;
      _audioCapturer.StopRecording();
   }

   private void OnDataCaptured( object sender, WaveInEventArgs e )
   {
      if ( _audioWriter.Position + e.BytesRecorded >= _bytesToRecord )
      {
         _audioCapturer.StopRecording();
      }
      else
      {
         _audioWriter.Write( e.Buffer, 0, e.BytesRecorded );
         RecordingProgress.Invoke( this, new RecordingProgressEventArgs( _audioWriter.Position, _bytesToRecord ) );
      }
   }

   private void OnRecordingStopped( object sender, StoppedEventArgs e )
   {
      byte[] data = null;

      if ( !_cancelled )
      {
         _audioWriter.Flush();
         data = _recordedFileStream.ToArray();
      }

      RecordingFinished.Invoke( this, new RecordingFinishedEventArgs( data, _waveFormat ) );
      Dispose();
   }
}
