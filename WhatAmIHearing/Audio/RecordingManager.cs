using NAudio.Wave;
using System;
using System.Threading.Tasks;
using ZemotoCommon;
using ZemotoCommon.UI;

namespace WhatAmIHearing.Audio;

internal sealed class RecordingManager : IDisposable
{
   private readonly DeviceProvider _deviceProvider = new();
   private readonly WaveFormat _waveFormat;
   private readonly long _maxBytesToRecord;
   private readonly CancelTokenProvider _cancelTokenProvider = new();

   public event EventHandler<RecordingResult> RecordingSuccess;
   public event EventHandler CancelRequested;

   public RecorderViewModel Model { get; }

   public RecordingManager( WaveFormat waveFormat, long maxBytesToRecord )
   {
      _waveFormat = waveFormat;
      _maxBytesToRecord = maxBytesToRecord;

      Model = new RecorderViewModel( _deviceProvider ) { ChangeStateCommand = new RelayCommand( async () => await ChangeStateAsync() ) };
   }

   public void Dispose()
   {
      _deviceProvider.Dispose();
      _cancelTokenProvider.Dispose();
   }

   public async Task ChangeStateAsync()
   {
      switch ( Model.State )
      {
         case RecorderState.Stopped:
         {
            Model.State = RecorderState.Recording;

            using var recorder = new Recorder( _deviceProvider.GetSelectedDevice(), _waveFormat, (long)( Model.RecordPercent * _maxBytesToRecord ), _cancelTokenProvider.GetToken() );
            recorder.RecordingProgress += OnRecordingProgress;
            var result = await recorder.RecordAsync();
            if ( result.Cancelled )
            {
               Reset();
            }
            else
            {
               Model.RecordingProgress = 1;
               RecordingSuccess?.Invoke( this, result );
            }
            break;
         }
         case RecorderState.Recording:
         {
            _cancelTokenProvider.Cancel();
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
}
