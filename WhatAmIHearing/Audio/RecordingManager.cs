using NAudio.Wave;
using System;
using ZemotoCommon.UI;

namespace WhatAmIHearing.Audio;

internal sealed class RecordingManager : IDisposable
{
   private readonly DeviceProvider _deviceProvider = new();
   private readonly WaveFormat _waveFormat;
   private readonly long _maxBytesToRecord;

   private Recorder _recorder;

   public event EventHandler<RecordingFinishedEventArgs> RecordingSuccess;
   public event EventHandler CancelRequested;

   public RecorderViewModel Model { get; }

   public RecordingManager( WaveFormat waveFormat, long maxBytesToRecord )
   {
      _waveFormat = waveFormat;
      _maxBytesToRecord = maxBytesToRecord;

      Model = new RecorderViewModel( _deviceProvider ) { RecordStopCommand = new RelayCommand( Record ) };
   }

   public void Dispose() => _deviceProvider.Dispose();

   public void Record()
   {
      switch ( Model.State )
      {
         case RecorderState.Stopped:
         {
            Model.State = RecorderState.Recording;
            _recorder = new Recorder( _deviceProvider.GetSelectedDevice(), _waveFormat, (long)( Model.RecordPercent * _maxBytesToRecord ) );
            _recorder.RecordingProgress += OnRecordingProgress;
            _recorder.RecordingFinished += OnRecordingFinished;
            _recorder.StartRecording();
            break;
         }
         case RecorderState.Recording:
         {
            _recorder.CancelRecording();
            break;
         }
         case RecorderState.Identifying:
         {
            CancelRequested?.Invoke( this, EventArgs.Empty );
            break;
         }
         case RecorderState.Error:
         {
            Reset();
            break;
         }
      }
   }

   public void Reset()
   {
      Model.State = RecorderState.Stopped;
      Model.RecorderStatusText = string.Empty;
      Model.RecordingProgress = 0;
   }

   private void OnRecordingProgress( object sender, RecordingProgressEventArgs e )
   {
      Model.RecordingProgress = e.Progress;
      Model.RecorderStatusText = e.StatusText;
   }

   private void OnRecordingFinished( object sender, RecordingFinishedEventArgs args )
   {
      _recorder.Dispose();
      _recorder = null;

      if ( args.Cancelled )
      {
         Reset();
         return;
      }

      Model.RecordingProgress = 1;
      RecordingSuccess?.Invoke( this, args );
   }
}
