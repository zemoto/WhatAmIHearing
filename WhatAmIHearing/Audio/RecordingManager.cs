using NAudio.Wave;
using System;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows;
using WhatAmIHearing.Api;
using WhatAmIHearing.Api.Shazam;
using WhatAmIHearing.Model;
using ZemotoCommon;
using ZemotoCommon.UI;

namespace WhatAmIHearing.Audio;

internal sealed class RecordingManager : IDisposable
{
   private readonly Recorder _recorder;
   private readonly DeviceProvider _deviceProvider = new();

   public event EventHandler<DetectedTrackInfo> RecordingSuccess;

   public RecorderViewModel Model { get; }

   public RecordingManager( WaveFormat waveFormat, long numBytesToRecord )
   {
      _recorder = new Recorder( waveFormat, numBytesToRecord );
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
      if ( Model.State is RecorderState.Recording )
      {
         _recorder.CancelRecording();
      }
      else if ( Model.State is RecorderState.SendingToShazam )
      {
         ApiClient.CancelRequests();
      }
      else
      {
         Model.State = RecorderState.Recording;
         _recorder.StartRecording( _deviceProvider.GetSelectedDevice() );
      }
   }

   private void OnRecordingProgress( object sender, RecordingProgressEventArgs e )
   {
      Model.RecordingProgress = (int)( (double)e.BytesRecorded / e.MaxBytes * 100 );
      Model.RecorderStatusText = $"Recording: {e.BytesRecorded}/{e.MaxBytes} bytes";
   }

   private async void OnRecordingFinished( object sender, RecordingFinishedEventArgs args )
   {
      DetectedTrackInfo detectedSong = null;
      using var __ = new ScopeGuard( () => RecordingSuccess.Invoke( this, detectedSong ) );

      using ( new ScopeGuard( () =>
      {
         Model.State = RecorderState.Stopped;
         Model.RecorderStatusText = string.Empty;
         Model.RecordingProgress = 0;
      } ) )
      {
         if ( args.Cancelled )
         {
            return;
         }

         Model.State = RecorderState.SendingToShazam;
         Model.RecorderStatusText = $"Sending {args.RecordedData.Length} bytes to Shazam";
         Model.RecordingProgress = 100;
         try
         {
            detectedSong = await ShazamApi.DetectSongAsync( args.RecordedData ).ConfigureAwait( true );
         }
         catch ( TaskCanceledException )
         {
            return;
         }
      }

      if ( detectedSong?.IsComplete == true )
      {
         _ = Process.Start( new ProcessStartInfo( detectedSong.Url ) { UseShellExecute = true } );
      }
      else
      {
         var result = MessageBox.Show( Application.Current.MainWindow, "No song detected. Playback recorded sound?", "Detection Failed", MessageBoxButton.YesNo );
         if ( result == MessageBoxResult.Yes )
         {
            Player.PlayAudio( args.RecordedData, args.Format );
         }
      }
   }
}
