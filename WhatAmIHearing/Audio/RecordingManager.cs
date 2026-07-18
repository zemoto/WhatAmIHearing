using NAudio.Wave;
using ZemotoCommon;

namespace WhatAmIHearing.Audio;

internal sealed class RecordingManager : IDisposable
{
   private const long _maxBytesToRecord = 500 * 1000; // 500KB. Max recording size according to Shazam API

   private readonly DeviceProvider _deviceProvider = new();
   private readonly WaveFormat _waveFormat = new( rate: 44100, bits: 16, channels: 1 ); // Format required by Shazam API
   private readonly CancelTokenProvider _cancelTokenProvider = new();

   private Recorder? _currentRecorder;

   public RecorderViewModel Model { get; }

   public RecordingManager( StateViewModel stateVm ) => Model = new RecorderViewModel( stateVm, _deviceProvider );

   public void Dispose()
   {
      _deviceProvider.Dispose();
      _cancelTokenProvider.Dispose();
   }

   public async Task<RecordingResult?> RecordAsync()
   {
      try
      {
         using var selectedDevice = _deviceProvider.GetSelectedDevice();
         if ( selectedDevice is null )
         {
            return null;
         }

         Model.StateVm.State = AppState.Recording;

         _currentRecorder = new Recorder( selectedDevice, _waveFormat, (long)( Model.RecordPercent * _maxBytesToRecord ), _cancelTokenProvider.GetToken() );
         _currentRecorder.RecordingProgress += OnRecordingProgress;

         return await _currentRecorder.RecordAsync();
      }
      catch
      {
         return null;
      }
      finally
      {
         if ( _currentRecorder is not null )
         {
            _currentRecorder.RecordingProgress -= OnRecordingProgress;
            _currentRecorder.Dispose();
            _currentRecorder = null;
         }
      }
   }

   public void CancelRecording() => _cancelTokenProvider.Cancel();

   public void StopAndSendRecordedData() => _currentRecorder?.Stop();

   public void Reset()
   {
      Model.StateVm.State = AppState.Stopped;
      Model.StateVm.SetStatusText( string.Empty );
      Model.RecordingProgress = 0;
   }

   private void OnRecordingProgress( object? sender, RecordingProgressEventArgs e )
   {
      Model.RecordingProgress = e.Progress * Model.RecordPercent; // The recorder does not take the record percent into account, so do it here
      Model.StateVm.SetStatusText( e.StatusText );
   }
}
