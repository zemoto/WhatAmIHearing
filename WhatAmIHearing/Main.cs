using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Threading.Tasks;
using WhatAmIHearing.Api.Shazam;
using WhatAmIHearing.Api.Spotify;
using WhatAmIHearing.Audio;

namespace WhatAmIHearing;

internal sealed class Main : IDisposable
{
   private readonly MainViewModel _model;
   private readonly MainWindow _window;
   private readonly RecordingManager _recordingManager;
   private readonly SpotifyManager _spotifyManager = new();
   private readonly ShazamApi _shazamApi = new();

   public Main()
   {
      _recordingManager = new RecordingManager( ShazamSpecProvider.ShazamWaveFormat, ShazamSpecProvider.MaxBytes, async () => await ChangeStateAsync() );

      _spotifyManager.SignInComplete += OnSpotifySignInComplete;

      _model = new MainViewModel( _recordingManager.Model, _spotifyManager.Model );

      _window = new MainWindow( _model );
      _window.Closing += OnWindowClosing;
   }

   public void Dispose()
   {
      _recordingManager.Dispose();
      _spotifyManager.Dispose();
      _shazamApi.Dispose();
   }

   public void Start()
   {
      if ( _window.RegisterRecordHotkey() )
      {
         _window.RecordHotkeyPressed += OnRecordHotkey;
         _model.HotkeyStatusText = "Shift + F2";
      }
      else
      {
         _model.HotkeyStatusText = "Failed to register";
      }

      if ( AppSettings.Instance.KeepOpenInTray && AppSettings.Instance.OpenHidden )
      {
         HideWindow();
      }
      else
      {
         ShowAndForegroundMainWindow();
      }
   }

   public async Task ChangeStateAsync()
   {
      switch ( _model.RecorderVm.State )
      {
         case RecorderState.Stopped:
         {
            HandleRecordingResult( await _recordingManager.RecordAsync() );
            break;
         }
         case RecorderState.Recording:
         {
            _recordingManager.CancelRecording();
            break;
         }
         case RecorderState.Identifying:
         {
            _shazamApi.CancelRequests();
            break;
         }
         case RecorderState.Error:
         {
            _recordingManager.Reset();
            break;
         }
      }
   }

   private async void HandleRecordingResult( RecordingResult result )
   {
      if ( result.Cancelled )
      {
         _recordingManager.Reset();
         return;
      }

      _model.RecorderVm.RecordingProgress = 1;
      _model.RecorderVm.State = RecorderState.Identifying;

      _model.RecorderVm.RecorderStatusText = AppSettings.Instance.ProgressType switch
      {
         ProgressDisplayType.None => "Sending recorded audio to Shazam",
         ProgressDisplayType.Bytes => $"Sending {result.RecordingData.Length} bytes of audio to Shazam",
         ProgressDisplayType.Seconds => $"Sending {result.AudioDurationInSeconds} seconds of audio to Shazam",
         _ => throw new InvalidEnumArgumentException()
      };

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
         _model.RecorderVm.State = RecorderState.Error;
         _model.RecorderVm.RecordingProgress = 0;
         _model.RecorderVm.RecorderStatusText = "Shazam could not identify the audio";
         ShowAndForegroundMainWindow();
         return;
      }

      _recordingManager.Reset();
      _ = Process.Start( new ProcessStartInfo( detectedSong.Url ) { UseShellExecute = true } );

      if ( AppSettings.Instance.KeepOpenInTray && AppSettings.Instance.HideWindowAfterRecord )
      {
         HideWindow();
      }

      if ( AppSettings.Instance.AddSongsToSpotifyPlaylist )
      {
         await _spotifyManager.AddSongToOurPlaylistAsync( detectedSong.Title, detectedSong.Subtitle );
      }
   }

   private void OnSpotifySignInComplete( object sender, EventArgs e ) => ShowAndForegroundMainWindow();

   private void OnWindowClosing( object sender, CancelEventArgs e )
   {
      if ( AppSettings.Instance.KeepOpenInTray )
      {
         e.Cancel = true;
         HideWindow();
      }
   }

   public async void OnRecordHotkey( object sender, EventArgs e )
   {
      if ( _recordingManager.Model.State is RecorderState.Stopped )
      {
         ShowAndForegroundMainWindow();
      }

      await ChangeStateAsync();
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
