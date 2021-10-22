using NAudio.CoreAudioApi;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using WhatAmIHearing.Api;
using WhatAmIHearing.Api.Shazam;
using WhatAmIHearing.Audio;
using ZemotoUI;
using ZemotoUtils;

namespace WhatAmIHearing
{
   internal enum State
   {
      Stopped = 0,
      Recording = 1,
      SendingToShazam = 2
   }

   internal sealed class MainViewModel : ViewModelBase
   {
      private const string DefaultDeviceName = "Default Input Device";

      private readonly Recorder _recorder = new();
      private readonly MMDeviceEnumerator _deviceEnumerator = new();

      public event EventHandler<bool> DetectionFinished;

      public MainViewModel()
      {
         DeviceList = _deviceEnumerator.EnumerateAudioEndPoints( DataFlow.All, DeviceState.Active ).ToList();
         DeviceNameList = DeviceList.ConvertAll( x => x.FriendlyName );
         DeviceNameList.Insert( 0, DefaultDeviceName );

         if ( string.IsNullOrEmpty( Settings.SelectedDevice ) )
         {
            Settings.SelectedDevice = DefaultDeviceName;
         }

         _recorder.RecordingFinished += OnRecordingStopped;
      }

      private async void OnRecordingStopped( object sender, RecordingFinishedEventArgs args )
      {
         string songInfoUrl = string.Empty;
         using ( new ScopeGuard( () => DetectionFinished?.Invoke( this, !string.IsNullOrEmpty( songInfoUrl ) ) ) )
         {
            using ( new ScopeGuard( Reset ) )
            {
               if ( args.Cancelled )
               {
                  return;
               }

               StatusReport.Status.Text = $"Sending resampled {args.RecordedData.Length} bits to Shazam";
               StatusReport.Status.Progress = 100;

               RecorderState = State.SendingToShazam;
               try
               {
                  songInfoUrl = await ShazamApi.DetectSongAsync( args.RecordedData ).ConfigureAwait( true );
               }
               catch ( TaskCanceledException )
               {
                  return;
               }
            }

            if ( !string.IsNullOrEmpty( songInfoUrl ) )
            {
               _ = Process.Start( new ProcessStartInfo( songInfoUrl ) { UseShellExecute = true } );
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

      private void Reset()
      {
         RecorderState = State.Stopped;
         StatusReport.Reset();
      }

      public List<MMDevice> DeviceList { get; }
      public List<string> DeviceNameList { get; }

      public Properties.UserSettings Settings => Properties.UserSettings.Default;

      private State _recorderState;
      public State RecorderState
      {
         get => _recorderState;
         private set => SetProperty( ref _recorderState, value );
      }

      private string _hotkeyStatusText;
      public string HotkeyStatusText
      {
         get => _hotkeyStatusText;
         set => SetProperty( ref _hotkeyStatusText, value );
      }

      private ICommand _recordCommand;
      public ICommand RecordStopCommand => _recordCommand ??= new RelayCommand( () =>
      {
         if ( RecorderState is State.Recording )
         {
            _recorder.CancelRecording();
         }
         else if ( RecorderState is State.SendingToShazam )
         {
            ApiClient.CancelRequests();
         }
         else
         {
            var selectedDevice = Settings.SelectedDevice == DefaultDeviceName
               ? _deviceEnumerator.GetDefaultAudioEndpoint( DataFlow.Render, Role.Console )
               : DeviceList.First( x => x.FriendlyName == Settings.SelectedDevice );

            RecorderState = State.Recording;
            _recorder.StartRecording( selectedDevice );
         }
      } );
   }
}
