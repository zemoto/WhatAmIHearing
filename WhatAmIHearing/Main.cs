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
   private readonly RecordingManager _recordingManager = new( ShazamSpecProvider.ShazamWaveFormat, ShazamSpecProvider.MaxBytes );
   private readonly SpotifyManager _spotifyManager = new();
   private readonly ShazamApi _shazamApi = new();

   public Main()
   {
      _recordingManager.RecordingSuccess += OnRecordingSuccess;
      _recordingManager.CancelRequested += OnCancelRequested;
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

   private async void OnRecordingSuccess( object sender, RecordingResult args )
   {
      _model.RecorderVm.State = RecorderState.Identifying;

      _model.RecorderVm.RecorderStatusText = AppSettings.Instance.ProgressType switch
      {
         ProgressDisplayType.None => "Sending recorded audio to Shazam",
         ProgressDisplayType.Bytes => $"Sending {args.RecordingData.Length} bytes of audio to Shazam",
         ProgressDisplayType.Seconds => $"Sending {args.AudioDurationInSeconds} seconds of audio to Shazam",
         _ => throw new InvalidEnumArgumentException()
      };

      DetectedTrackInfo detectedSong;
      try
      {
         detectedSong = await _shazamApi.DetectSongAsync( args.RecordingData ).ConfigureAwait( true );
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

   private void OnCancelRequested( object sender, EventArgs e ) => _shazamApi.CancelRequests();

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

      await _recordingManager.ChangeStateAsync();
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
