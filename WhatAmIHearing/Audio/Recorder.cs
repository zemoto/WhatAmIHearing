using NAudio.CoreAudioApi;
using NAudio.Wave;
using System;
using System.ComponentModel;
using System.IO;

namespace WhatAmIHearing.Audio;

internal sealed class Recorder : IDisposable
{
   private readonly WasapiLoopbackCapture _audioCapturer;
   private readonly WaveFormat _waveFormat;
   private readonly long _bytesToRecord;
   private readonly double _secondsOfAudioToRecord;
   private readonly WaveFileWriter _audioWriter;
   private readonly MemoryStream _recordedFileStream;

   private bool _cancelled;

   public event EventHandler<RecordingProgressEventArgs> RecordingProgress;
   public event EventHandler<RecordingFinishedEventArgs> RecordingFinished;

   public Recorder( MMDevice device, WaveFormat waveFormat, long bytesToRecord )
   {
      _audioCapturer = new WasapiLoopbackCapture( device ) { WaveFormat = waveFormat };
      _audioCapturer.DataAvailable += OnDataCaptured;
      _audioCapturer.RecordingStopped += OnRecordingStopped;

      _waveFormat = waveFormat;
      _bytesToRecord = bytesToRecord;

      _recordedFileStream = new MemoryStream();
      _audioWriter = new WaveFileWriter( _recordedFileStream, _waveFormat );

      _secondsOfAudioToRecord = Math.Round( (double)_bytesToRecord / _waveFormat.AverageBytesPerSecond, 2 );
   }

   public void Dispose()
   {
      _audioWriter.Dispose();
      _recordedFileStream.Dispose();

      _audioCapturer.Dispose();
      _audioCapturer.DataAvailable -= OnDataCaptured;
      _audioCapturer.RecordingStopped -= OnRecordingStopped;
   }

   public void StartRecording()
   {
      RecordingProgress.Invoke( this, new RecordingProgressEventArgs( 0, GetStatusText( 0 ) ) );
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
         RecordingProgress.Invoke( this, new RecordingProgressEventArgs( (double)_audioWriter.Position / _bytesToRecord, GetStatusText( _audioWriter.Position ) ) );
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
   }

   private string GetStatusText( long bytesRecorded )
   {
      switch ( AppSettings.Instance.ProgressType )
      {
         case ProgressDisplayType.None:
            return string.Empty;
         case ProgressDisplayType.Bytes:
            return $"Recording: {bytesRecorded}/{_bytesToRecord} bytes";
         case ProgressDisplayType.Seconds:
            var secondsRecorded = Math.Round( (double)bytesRecorded / _waveFormat.AverageBytesPerSecond, 2 );
            return $"Recording: {secondsRecorded}/{_secondsOfAudioToRecord} seconds";
         default:
            throw new InvalidEnumArgumentException();
      }
   }
}
