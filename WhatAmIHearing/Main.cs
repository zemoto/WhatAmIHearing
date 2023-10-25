using System;
using System.ComponentModel;
using System.Diagnostics;
using WhatAmIHearing.Api.Shazam;
using WhatAmIHearing.Api.Spotify;
using WhatAmIHearing.Audio;
using WhatAmIHearing.Model;
using WhatAmIHearing.UI;

namespace WhatAmIHearing;

internal sealed class Main : IDisposable
{
   private readonly MainViewModel _model;
   private readonly MainWindow _window;
   private readonly RecordingManager _recordingManager = new( ShazamSpecProvider.ShazamWaveFormat, ShazamSpecProvider.MaxBytes );
   private readonly SpotifyManager _spotifyManager = new();
   private readonly SongDetector _songDetector = new();

   public Main()
   {
      _recordingManager.RecordingSuccess += OnRecordingSuccess;
      _spotifyManager.SignInComplete += OnSpotifySignInComplete;

      _model = new MainViewModel( _recordingManager.Model, _spotifyManager.Model );

      _window = new MainWindow( _model );
      _window.Closing += OnWindowClosing;
   }

   public void Dispose()
   {
      _recordingManager.Dispose();
      _spotifyManager.Dispose();
      _songDetector.Dispose();
   }

   public void Start( bool hotkeyRegistered )
   {
      _model.HotkeyStatusText = hotkeyRegistered ? "Shift + F2" : "Failed to register";

      if ( AppSettings.Instance.KeepOpenInTray && AppSettings.Instance.OpenHidden )
      {
         HideWindow();
      }
      else
      {
         ShowAndForegroundMainWindow();
      }
   }

   private async void OnRecordingSuccess( object sender, RecordingFinishedEventArgs args )
   {
      _model.RecorderVm.State = RecorderState.Identifying;
      _model.RecorderVm.RecorderStatusText = $"Sending {args.RecordingData.Length} bytes to Shazam";

      var detectedSong = await _songDetector.DetectSongAsync( args.RecordingData ).ConfigureAwait( true );
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

   public void OnRecordHotkey( object sender, KeyPressedEventArgs e )
   {
      if ( _recordingManager.Model.State is RecorderState.Stopped )
      {
         ShowAndForegroundMainWindow();
      }

      _recordingManager.Record();
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
