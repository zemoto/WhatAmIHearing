using NAudio.Wave;
using System;
using System.Threading.Tasks;
using ZemotoCommon;

namespace WhatAmIHearing.Audio;

internal sealed class RecordingManager : IDisposable
{
   private readonly DeviceProvider _deviceProvider = new();
   private readonly WaveFormat _waveFormat = new( rate: 44100, bits: 16, channels: 1 ); // Format required by Shazam API
   private readonly long _maxBytesToRecord = 500 * 1000; // 500KB. Max recording size according to Shazam API
   private readonly CancelTokenProvider _cancelTokenProvider = new();

   public RecorderViewModel Model { get; }

   public RecordingManager( StateViewModel stateVm ) => Model = new RecorderViewModel( stateVm, _deviceProvider );

   public void Dispose()
   {
      _deviceProvider.Dispose();
      _cancelTokenProvider.Dispose();
   }

   public async Task<RecordingResult> RecordAsync()
   {
      var selectedDevice = _deviceProvider.GetSelectedDevice();
      if ( selectedDevice is null )
      {
         return null;
      }

      Model.StateVm.State = AppState.Recording;
      using var recorder = new Recorder( selectedDevice, _waveFormat, (long)( Model.RecordPercent * _maxBytesToRecord ), _cancelTokenProvider.GetToken() );
      recorder.RecordingProgress += OnRecordingProgress;
      return await recorder.RecordAsync();
   }

   public void CancelRecording() => _cancelTokenProvider.Cancel();

   public void Reset()
   {
      Model.StateVm.State = AppState.Stopped;
      Model.StateVm.SetStatusText( string.Empty );
      Model.RecordingProgress = 0;
   }

   private void OnRecordingProgress( object sender, RecordingProgressEventArgs e )
   {
      Model.RecordingProgress = e.Progress * Model.RecordPercent; // The recorder does not take the record percent into account, so do it here
      Model.StateVm.SetStatusText( e.StatusText );
   }
}
