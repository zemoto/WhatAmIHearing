using NAudio.CoreAudioApi;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using WhatAmIHearing.Api;
using WhatAmIHearing.Api.Shazam;
using WhatAmIHearing.Api.Spotify;
using WhatAmIHearing.Audio;
using WhatAmIHearing.UI;
using WhatAmIHearing.Utils;
using ZemotoUI;
using ZemotoUtils;

namespace WhatAmIHearing
{
   internal sealed class Main
   {
      private const string DefaultDeviceName = "Default Input Device";

      private readonly MainViewModel _model;
      private readonly MainWindow _window;
      private readonly Recorder _recorder = new();
      private readonly MMDeviceEnumerator _deviceEnumerator = new();
      private readonly List<MMDevice> _deviceList;

      private bool _windowShownFromHotkey;

      private Properties.UserSettings Settings => Properties.UserSettings.Default;

      public Main()
      {
         _deviceList = _deviceEnumerator.EnumerateAudioEndPoints( DataFlow.All, DeviceState.Active ).ToList();
         _model = new MainViewModel( _deviceList, DefaultDeviceName ) { RecordStopCommand = new RelayCommand( OnRecord ) };
         _model.SpotifyVm.SignInOutCommand = new RelayCommand( OnSpotifySignInOut );

         _window = new MainWindow( _model );
         _window.Closing += OnWindowClosing;

         _recorder.RecordingFinished += OnRecordingStopped;
      }

      public void Start( bool hotkeyRegistered )
      {
         _model.HotkeyStatusText = hotkeyRegistered ? "Shift + F2" : "Failed to register";

         if ( Settings.OpenHidden )
         {
            HideWindow();
         }
         else
         {
            ShowAndForegroundMainWindow();
         }
      }

      private async void OnRecordingStopped( object sender, RecordingFinishedEventArgs args )
      {
         DetectedTrackInfo detectedSong = null;
         using ( new ScopeGuard( () =>
            {
               if ( detectedSong?.IsComplete == true && _windowShownFromHotkey )
               {
                  HideWindow();
               }

               _windowShownFromHotkey = false;
            } ) )
         {
            using ( new ScopeGuard( () =>
               {
                  _model.RecorderState = State.Stopped;
                  StatusReport.Reset();
               } ) )
            {
               if ( args.Cancelled )
               {
                  return;
               }

               StatusReport.Status.Text = $"Sending resampled {args.RecordedData.Length} bits to Shazam";
               StatusReport.Status.Progress = 100;

               _model.RecorderState = State.SendingToShazam;
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

               if ( _model.SpotifyVm.SignedIn && Settings.AddSongsToSpotifyPlaylist )
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
      }

      private void OnRecord()
      {
         if ( _model.RecorderState is State.Recording )
         {
            _recorder.CancelRecording();
         }
         else if ( _model.RecorderState is State.SendingToShazam )
         {
            ApiClient.CancelRequests();
         }
         else
         {
            var selectedDevice = Settings.SelectedDevice == DefaultDeviceName
               ? _deviceEnumerator.GetDefaultAudioEndpoint( DataFlow.Render, Role.Console )
               : _deviceList.First( x => x.FriendlyName == Settings.SelectedDevice );

            _model.RecorderState = State.Recording;
            _recorder.StartRecording( selectedDevice );
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
         if ( Settings.KeepOpenInTray )
         {
            e.Cancel = true;
            HideWindow();
         }
      }

      public void OnRecordHotkey( object sender, KeyPressedEventArgs e )
      {
         if ( _model.RecorderState is State.Stopped )
         {
            if ( !_window.IsVisible )
            {
               _windowShownFromHotkey = true;
            }

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
