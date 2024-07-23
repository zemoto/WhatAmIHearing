using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Threading.Tasks;
using WhatAmIHearing.Api.Shazam;
using WhatAmIHearing.Api.Spotify;
using WhatAmIHearing.Audio;
using WhatAmIHearing.Result;

namespace WhatAmIHearing;

internal sealed class Main : IDisposable
{
   private readonly MainViewModel _model;
   private readonly StateViewModel _stateVm;
   private readonly MainWindow _window;
   private readonly RecordingManager _recordingManager;
   private readonly SpotifyManager _spotifyManager;
   private readonly ShazamApi _shazamApi = new();

   public Main()
   {
      _stateVm = new StateViewModel( ChangeStateAsync );
      _recordingManager = new RecordingManager( _stateVm, ShazamSpecProvider.ShazamWaveFormat, ShazamSpecProvider.MaxBytes );
      _spotifyManager = new SpotifyManager( ShowAndForegroundMainWindow );
      _model = new MainViewModel( _recordingManager.Model, _spotifyManager.Model, SetHotkey );

      _window = new MainWindow( _model );
      _window.Closing += OnWindowClosing;
      _window.RecordHotkeyPressed += OnRecordHotkey;
   }

   public void Dispose()
   {
      _recordingManager.Dispose();
      _spotifyManager.Dispose();
      _shazamApi.Dispose();
   }

   public void Start()
   {
      SetHotkey( _model.Settings.RecordHotkey );

      if ( AppSettings.Instance.KeepOpenInTray && AppSettings.Instance.OpenHidden )
      {
         HideWindow();
      }
      else
      {
         ShowAndForegroundMainWindow();
      }
   }

   private async void ChangeStateAsync()
   {
      switch ( _stateVm.State )
      {
         case AppState.Stopped:
         {
            HandleRecordingResult( await _recordingManager.RecordAsync().ConfigureAwait( true ) );
            break;
         }
         case AppState.Recording:
         {
            _recordingManager.CancelRecording();
            break;
         }
         case AppState.Identifying:
         {
            _shazamApi.CancelRequests();
            break;
         }
      }
   }

   private void SetHotkey( Hotkey hotkey )
   {
      if ( _window.RegisterRecordHotkey( hotkey, out var error ) )
      {
         _model.Settings.RecordHotkey = hotkey;
      }

      _model.HotkeyRegisterError = error;
   }

   private async void HandleRecordingResult( RecordingResult result )
   {
      if ( result.Cancelled )
      {
         _recordingManager.Reset();
         return;
      }

      _model.RecorderVm.RecordingProgress = 1;
      _stateVm.State = AppState.Identifying;

      _stateVm.SetStatusText( AppSettings.Instance.ProgressType switch
      {
         ProgressDisplayType.None => "Sending recorded audio to Shazam",
         ProgressDisplayType.Bytes => $"Sending {result.RecordingData.Length} bytes of audio to Shazam",
         ProgressDisplayType.Seconds => $"Sending {result.AudioDurationInSeconds} seconds of audio to Shazam",
         _ => throw new InvalidEnumArgumentException()
      } );

      DetectedTrackInfo detectedSong;
      try
      {
         detectedSong = await _shazamApi.DetectSongAsync( result.RecordingData ).ConfigureAwait( true );
      }
      catch ( TaskCanceledException )
      {
         _recordingManager.Reset();
         return;
      }

      if ( detectedSong?.IsComplete != true )
      {
         _stateVm.State = AppState.Stopped;
         _stateVm.SetStatusText( "Shazam could not identify the audio", isError: true );
         _model.RecorderVm.RecordingProgress = 0;
         ShowAndForegroundMainWindow();
         return;
      }

      _recordingManager.Reset();
      _model.ResultVm = new ResultViewModel( detectedSong );
      _model.ResultsIsExpanded = true;

      if ( AppSettings.Instance.KeepOpenInTray && AppSettings.Instance.HideWindowAfterRecord )
      {
         HideWindow();
      }

      if ( AppSettings.Instance.AddSongsToSpotifyPlaylist )
      {
         await _spotifyManager.AddSongToOurPlaylistAsync( detectedSong.Title, detectedSong.Subtitle );
      }
   }

   private void OnWindowClosing( object sender, CancelEventArgs e )
   {
      if ( AppSettings.Instance.KeepOpenInTray )
      {
         e.Cancel = true;
         HideWindow();
      }
   }

   private void OnRecordHotkey( object sender, EventArgs e )
   {
      if ( _stateVm.State is AppState.Stopped )
      {
         ShowAndForegroundMainWindow();
      }

      ChangeStateAsync();
   }

   private void HideWindow()
   {
      _window.ShowInTaskbar = false;
      _window.Hide();
   }

   public void ShowAndForegroundMainWindow()
   {
      _window.ShowInTaskbar = true;
      _window.Show();
      _ = _window.Activate();
   }
}
