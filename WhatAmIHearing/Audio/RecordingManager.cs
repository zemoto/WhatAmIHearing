using NAudio.Wave;
using System;
using ZemotoCommon.UI;

namespace WhatAmIHearing.Audio;

internal sealed class RecordingManager : IDisposable
{
   private readonly Recorder _recorder;
   private readonly DeviceProvider _deviceProvider = new();

   public event EventHandler<RecordingFinishedEventArgs> RecordingSuccess;
   public event EventHandler CancelRequested;

   public RecorderViewModel Model { get; }

   public RecordingManager( WaveFormat waveFormat, long maxBytesToRecord )
   {
      _recorder = new Recorder( waveFormat, maxBytesToRecord );
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
            _recorder.StartRecording( _deviceProvider.GetSelectedDevice(), Model.RecordPercent );
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
      Model.RecordingProgress = (double)e.BytesRecorded / _recorder.MaxBytesToRecord;
      Model.RecorderStatusText = $"Recording: {e.BytesRecorded}/{e.BytesToRecord} bytes";
   }

   private void OnRecordingFinished( object sender, RecordingFinishedEventArgs args )
   {
      if ( args.Cancelled )
      {
         Reset();
         return;
      }

      Model.RecordingProgress = Model.RecordPercent;
      RecordingSuccess?.Invoke( this, args );
   }
}
