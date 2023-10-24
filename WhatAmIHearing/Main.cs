using System;
using System.ComponentModel;
using WhatAmIHearing.Api.Shazam;
using WhatAmIHearing.Api.Spotify;
using WhatAmIHearing.Audio;
using WhatAmIHearing.Model;
using WhatAmIHearing.UI;
using WhatAmIHearing.Utils;

namespace WhatAmIHearing;

internal sealed class Main : IDisposable
{
   private readonly MainViewModel _model;
   private readonly MainWindow _window;
   private readonly RecordingManager _recordingManager = new( ShazamSpecProvider.ShazamWaveFormat, ShazamSpecProvider.MaxBytes );
   private readonly SpotifyManager _spotifyManager = new();

   public Main()
   {
      _recordingManager.RecordingSuccess += OnRecordingDone;
      _spotifyManager.SignInComplete += OnSpotifySignInComplete;

      _model = new MainViewModel( _recordingManager.Model, _spotifyManager.Model );

      _window = new MainWindow( _model );
      _window.Closing += OnWindowClosing;
   }

   public void Dispose() => _recordingManager.Dispose();

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

   private async void OnRecordingDone( object sender, DetectedTrackInfo detectedSong )
   {
      if ( detectedSong is null )
      {
         return;
      }

      if ( AppSettings.Instance.KeepOpenInTray && AppSettings.Instance.HideWindowAfterRecord )
      {
         HideWindow();
      }

      if ( AppSettings.Instance.AddSongsToSpotifyPlaylist )
      {
         await _spotifyManager.AddSongToOurPlaylistAsync( detectedSong.Title, detectedSong.Subtitle );
      }
   }

   private void OnSpotifySignInComplete( object sender, System.EventArgs e ) => ShowAndForegroundMainWindow();

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
