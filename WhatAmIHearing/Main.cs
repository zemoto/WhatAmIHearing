using System.ComponentModel;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows;
using WhatAmIHearing.Api;
using WhatAmIHearing.Api.Shazam;
using WhatAmIHearing.Api.Spotify;
using WhatAmIHearing.Audio;
using WhatAmIHearing.Model;
using WhatAmIHearing.UI;
using WhatAmIHearing.Utils;
using ZemotoCommon;
using ZemotoCommon.UI;

namespace WhatAmIHearing
{
   internal sealed class Main
   {
      private readonly MainViewModel _model;
      private readonly MainWindow _window;
      private readonly Recorder _recorder = new();
      private readonly DeviceProvider _deviceProvider = new();
      private readonly Properties.UserSettings _settings = Properties.UserSettings.Default;

      public Main()
      {
         _model = new MainViewModel( _deviceProvider ) { RecordStopCommand = new RelayCommand( OnRecord ) };
         _model.SpotifyVm.SignInOutCommand = new RelayCommand( OnSpotifySignInOut );

         _window = new MainWindow( _model );
         _window.Closing += OnWindowClosing;

         _recorder.RecordingProgress += OnRecordingProgress;
         _recorder.RecordingFinished += OnRecordingStopped;
      }

      public void Start( bool hotkeyRegistered )
      {
         _model.HotkeyStatusText = hotkeyRegistered ? "Shift + F2" : "Failed to register";

         if ( _settings.KeepOpenInTray && _settings.OpenHidden )
         {
            HideWindow();
         }
         else
         {
            ShowAndForegroundMainWindow();
         }
      }

      private void OnRecordingProgress( object sender, RecordingProgressEventArgs e )
      {
         _model.RecorderVm.RecordingProgress = (int)( (double)e.BytesRecorded / e.MaxBytes * 100 );
         _model.RecorderVm.RecorderStatusText = $"Recording: {e.BytesRecorded}/{e.MaxBytes} bits";
      }

      private async void OnRecordingStopped( object sender, RecordingFinishedEventArgs args )
      {
         DetectedTrackInfo detectedSong = null;
         using var __ = new ScopeGuard( () =>
         {
            if ( detectedSong?.IsComplete == true && _settings.KeepOpenInTray && _settings.HideWindowAfterRecord )
            {
               HideWindow();
            }
         } );

         using ( new ScopeGuard( () =>
            {
               _model.RecorderVm.State = RecorderState.Stopped;
               _model.RecorderVm.RecorderStatusText = string.Empty;
               _model.RecorderVm.RecordingProgress = 0;
            } ) )
         {
            if ( args.Cancelled )
            {
               return;
            }

            _model.RecorderVm.State = RecorderState.SendingToShazam;
            _model.RecorderVm.RecorderStatusText = $"Sending resampled {args.RecordedData.Length} bits to Shazam";
            _model.RecorderVm.RecordingProgress = 100;
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

            if ( _model.SpotifyVm.SignedIn && _settings.AddSongsToSpotifyPlaylist )
            {
               _model.SpotifyVm.Result = await SpotifyApi.AddSongToOurPlaylistAsync( detectedSong.Title, detectedSong.Subtitle ).ConfigureAwait( false );
            }
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

      private void OnRecord()
      {
         if ( _model.RecorderVm.State is RecorderState.Recording )
         {
            _recorder.CancelRecording();
         }
         else if ( _model.RecorderVm.State is RecorderState.SendingToShazam )
         {
            ApiClient.CancelRequests();
         }
         else
         {
            _model.RecorderVm.State = RecorderState.Recording;
            _recorder.StartRecording( _deviceProvider.GetSelectedDevice() );
         }
      }

      private async void OnSpotifySignInOut()
      {
         using ( var authenticator = new SpotifyAuthenticator() )
         {
            if ( _model.SpotifyVm.SignedIn )
            {
               authenticator.SignOut();
            }
            else
            {
               await authenticator.SignInAsync().ConfigureAwait( true );
               ShowAndForegroundMainWindow();
            }
         }

         _model.SpotifyVm.NotifySignedInChanged();
      }

      private void OnWindowClosing( object sender, CancelEventArgs e )
      {
         if ( _settings.KeepOpenInTray )
         {
            e.Cancel = true;
            HideWindow();
         }
      }

      public void OnRecordHotkey( object sender, KeyPressedEventArgs e )
      {
         if ( _model.RecorderVm.State is RecorderState.Stopped )
         {
            ShowAndForegroundMainWindow();
         }

         OnRecord();
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
}
