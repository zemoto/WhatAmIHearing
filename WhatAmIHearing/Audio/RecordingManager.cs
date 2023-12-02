using NAudio.Wave;
using System;
using ZemotoCommon.UI;

namespace WhatAmIHearing.Audio;

internal sealed class RecordingManager : IDisposable
{
   private readonly Recorder _recorder;
   private readonly DeviceProvider _deviceProvider = new();
   private readonly long _maxBytesToRecord;

   public event EventHandler<RecordingFinishedEventArgs> RecordingSuccess;
   public event EventHandler CancelRequested;

   public RecorderViewModel Model { get; }

   public RecordingManager( WaveFormat waveFormat, long maxBytesToRecord )
   {
      _recorder = new Recorder( waveFormat );
      _maxBytesToRecord = maxBytesToRecord;

      Model = new RecorderViewModel( _deviceProvider ) { RecordStopCommand = new RelayCommand( Record ) };

      _recorder.RecordingProgress += OnRecordingProgress;
      _recorder.RecordingFinished += OnRecordingFinished;
   }

   public void Dispose()
   {
      _recorder.Dispose();
      _deviceProvider.Dispose();
   }

   public void Record()
   {
      switch ( Model.State )
      {
         case RecorderState.Stopped:
         {
            Model.State = RecorderState.Recording;
            _recorder.StartRecording( _deviceProvider.GetSelectedDevice(), (long)( Model.RecordPercent * _maxBytesToRecord ) );
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
      if ( args.Cancelled )
      {
         Reset();
         return;
      }

      Model.RecordingProgress = 1;
      RecordingSuccess?.Invoke( this, args );
   }
}
