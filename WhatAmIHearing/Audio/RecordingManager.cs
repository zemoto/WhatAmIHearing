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

   public RecorderViewModel Model { get; }

   public RecordingManager( WaveFormat waveFormat, long maxBytesToRecord, Action changeStateAction )
   {
      _waveFormat = waveFormat;
      _maxBytesToRecord = maxBytesToRecord;

      Model = new RecorderViewModel( _deviceProvider ) { ChangeStateCommand = new RelayCommand( changeStateAction ) };
   }

   public void Dispose()
   {
      _deviceProvider.Dispose();
      _cancelTokenProvider.Dispose();
   }

   public async Task<RecordingResult> RecordAsync()
   {
      Model.State = RecorderState.Recording;

      using var recorder = new Recorder( _deviceProvider.GetSelectedDevice(), _waveFormat, (long)( Model.RecordPercent * _maxBytesToRecord ), _cancelTokenProvider.GetToken() );
      recorder.RecordingProgress += OnRecordingProgress;
      return await recorder.RecordAsync();
   }

   public void CancelRecording() => _cancelTokenProvider.Cancel();

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
