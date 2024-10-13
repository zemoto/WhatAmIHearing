using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Threading.Tasks;
using WhatAmIHearing.Audio;
using WhatAmIHearing.Result;
using WhatAmIHearing.Shazam;

namespace WhatAmIHearing;

internal sealed class Main : IDisposable
{
   private readonly MainViewModel _model;
   private readonly StateViewModel _stateVm;
   private readonly MainWindow _window;
   private readonly RecordingManager _recordingManager;
   private readonly ApiViewModel _apiVm;
   private readonly Api _api;
   private readonly ResultHistory _history = ResultHistory.Load();

   public Main()
   {
      _apiVm = ApiViewModel.Load();
      _api = new Api( _apiVm );

      _stateVm = new StateViewModel( ChangeStateAsync );
      _recordingManager = new RecordingManager( _stateVm );
      _model = new MainViewModel( _stateVm, _recordingManager.Model, _history, _apiVm, SetHotkey, OpenHyperlink );

      _window = new MainWindow( _model );
      _window.RecordHotkeyPressed += OnRecordHotkey;
   }

   public void Dispose()
   {
      _apiVm.Save();
      _history.Save();
      _recordingManager.Dispose();
      _api.Dispose();
   }

   public void Start()
   {
      SetHotkey( _model.Settings.RecordHotkey );

      if ( AppSettings.Instance.KeepOpenInTray && AppSettings.Instance.OpenHidden )
      {
         _window.Hide();
      }
      else
      {
         ShowAndForegroundMainWindow();
      }
   }

   public void ShowAndForegroundMainWindow()
   {
      _window.ShowInTaskbar = true;
      _window.Show();
      _ = _window.Activate();
   }

   private async void ChangeStateAsync()
   {
      switch ( _stateVm.State )
      {
         case AppState.Stopped:
         {
            _model.SelectedSong = null;
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
            _api.CancelRequests();
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

   private void OpenHyperlink( string hyperlinkUri )
   {
      var startInfo = new ProcessStartInfo( hyperlinkUri )
      {
         UseShellExecute = true
      };

      _ = Process.Start( startInfo );
   }

   private async void HandleRecordingResult( RecordingResult result )
   {
      if ( result.Cancelled )
      {
         _recordingManager.Reset();
         return;
      }

      _model.RecorderVm.RecordingProgress = _model.RecorderVm.RecordPercent; // "100%" in the UI is whatever the target record percent is
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
         detectedSong = await _api.DetectSongAsync( result.RecordingData ).ConfigureAwait( true );
      }
      catch ( TaskCanceledException )
      {
         _recordingManager.Reset();
         return;
      }

      if ( detectedSong?.IsComplete != true )
      {
         string errorMessage;
         if ( _api.LastStatusCode is System.Net.HttpStatusCode.TooManyRequests )
         {
            errorMessage = "Max API quota reached; custom API key required";
            _window.FocusCustomApiKeyTextBox();
         }
         else
         {
            errorMessage = _api.LastStatusCode is System.Net.HttpStatusCode.Forbidden ? "API Key is invalid" : "Shazam could not identify the audio";
         }

         _stateVm.State = AppState.Stopped;
         _stateVm.SetStatusText( errorMessage, isError: true );
         _model.RecorderVm.RecordingProgress = 0;
         ShowAndForegroundMainWindow();
         return;
      }

      _recordingManager.Reset();

      var songVm = new SongViewModel( detectedSong );
      _history.Insert( 0, songVm );
      _model.SelectedSong = songVm;

      if ( AppSettings.Instance.PutTitleOnClipboard )
      {
         _model.SelectedSong.CopyTitleToClipboard.Execute( null );
      }

      if ( AppSettings.Instance.OpenShazamOnResultFound )
      {
         _model.SelectedSong.OpenInShazamCommand.Execute( null );

         if ( AppSettings.Instance.KeepOpenInTray && AppSettings.Instance.HideWindowAfterRecord )
         {
            _window.Hide();
         }
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
}
