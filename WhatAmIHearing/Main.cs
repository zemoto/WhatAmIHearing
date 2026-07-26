using System.ComponentModel;
using WhatAmIHearing.Audio;
using WhatAmIHearing.Properties;
using WhatAmIHearing.Result;
using WhatAmIHearing.Shazam;
using ZemotoCommon;

namespace WhatAmIHearing;

internal sealed class Main : IDisposable
{
   private readonly MainViewModel _model;
   private readonly StateViewModel _stateVm;
   private readonly MainWindow _window;
   private readonly RecordingManager _recordingManager;
   private readonly Api _api;

   public Main()
   {
      LegacyLoader.LoadLegacyDataIntoAppSettings();

      _api = new Api();

      _stateVm = new StateViewModel( ChangeStateAsync );
      _recordingManager = new RecordingManager( _stateVm );
      _model = new MainViewModel( _stateVm, _recordingManager.Model, SetHotkey );

      _window = new MainWindow( _model );
      _window.RecordHotkeyPressed += OnRecordHotkey;
   }

   public void Dispose()
   {
      _recordingManager.Dispose();
      _api.Dispose();
   }

   public void Start()
   {
      SetHotkey( _model.Settings.RecordHotkey );

      if ( AppSettings.Instance.KeepOpenInTray && AppSettings.Instance.OpenHidden )
      {
         _window.HideToTray();
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
            switch ( _model.Settings.StopBehavior )
            {
               case StopBehaviorType.Send:
                  _recordingManager.StopAndSendRecordedData();
                  break;
               default: // StopBehaviorType.Cancel
                  _recordingManager.CancelRecording();
                  break;
            }

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

   private async void HandleRecordingResult( RecordingResult? result )
   {
      if ( result is null )
      {
         _recordingManager.Reset();
         _stateVm.SetStatusText( Resources.ErrorInitiating, isError: true );
         return;
      }

      // If cancelled or recorded too little, discard the result and reset.
      if ( result.Cancelled || _model.RecorderVm.RecordingProgress < Constants.MinRecordingPercentForIdentification )
      {
         _recordingManager.Reset();
         return;
      }

      _model.RecorderVm.RecordingProgress = _model.RecorderVm.RecordPercent; // "100%" in the UI is whatever the target record percent is
      _stateVm.State = AppState.Identifying;

      _stateVm.SetStatusText( AppSettings.Instance.ProgressType switch
      {
         ProgressDisplayType.None => Resources.SendingWithNoUnits,
         ProgressDisplayType.Bytes => string.Format( Resources.SendingBytes, result.RecordingData.Length ),
         ProgressDisplayType.Seconds => string.Format( Resources.SendingSeconds, result.AudioDurationInSeconds ),
         _ => throw new InvalidEnumArgumentException()
      } );

      DetectedTrackInfo? detectedSong;
      try
      {
         detectedSong = await _api.DetectSongAsync( result.RecordingData ).ConfigureAwait( true );
      }
      catch ( OperationCanceledException )
      {
         _recordingManager.Reset();
         return;
      }
      catch
      {
         _recordingManager.Reset();
         _stateVm.SetStatusText( Resources.ErrorCommunicating, isError: true );
         ShowAndForegroundMainWindow();
         return;
      }

      if ( detectedSong?.IsComplete != true )
      {
         string errorMessage;
         if ( (int)_api.LastStatusCode is >= 200 and <= 299 )
         {
            errorMessage = Resources.FailedToIdentify;
         }
         else if ( _api.LastStatusCode is System.Net.HttpStatusCode.TooManyRequests )
         {
            errorMessage = _model.Settings.KeyData.UseDefaultKey
               ?  Resources.QuotaReachedUsingDefault
               :  Resources.QuotaReachedUsingCustom;
            _window.FocusCustomApiKeyTextBox();
         }
         else
         {
            errorMessage = _api.LastStatusCode is System.Net.HttpStatusCode.InternalServerError ? Resources.ErrorServerDown
                         : _api.LastStatusCode is System.Net.HttpStatusCode.Forbidden ? Resources.ErrorInvalidKey
                         : string.Format( Resources.ErrorUnknownServerError, (int)_api.LastStatusCode );
         }

         _recordingManager.Reset();
         _stateVm.SetStatusText( errorMessage, isError: true );
         ShowAndForegroundMainWindow();
         return;
      }

      _recordingManager.Reset();

      var songVm = new SongViewModel( detectedSong );

      var appSettings = AppSettings.Instance;
      appSettings.History.Insert( 0, songVm );
      _model.SelectedSong = songVm;

      if ( appSettings.PutTitleOnClipboard )
      {
         _model.SelectedSong.CopyTitleToClipboard.Execute( null );
      }

      if ( appSettings.OpenShazamOnResultFound )
      {
         UtilityMethods.OpenInBrowser( _model.SelectedSong.ShazamUrl );
      }

      if ( appSettings.KeepOpenInTray && appSettings.HideWindowAfterRecord )
      {
         _window.HideToTray();
      }
   }

   private void OnRecordHotkey()
   {
      if ( _stateVm.State is AppState.Stopped )
      {
         ShowAndForegroundMainWindow();
      }

      ChangeStateAsync();
   }
}
