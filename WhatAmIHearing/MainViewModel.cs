using NAudio.CoreAudioApi;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using WhatAmIHearing.Api;
using WhatAmIHearing.Audio;
using ZemotoUI;

namespace WhatAmIHearing
{
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
         Recording = false;
         if ( args.Cancelled )
         {
            StatusReport.Reset();
            return;
         }

         StatusReport.Status.Text = $"Sending resampled {args.RecordedData.Length} bits to Shazam";
         StatusReport.Status.Progress = 100;

         var songInfoUrl = await ShazamApi.DetectSongAsync( args.RecordedData ).ConfigureAwait( true );
         StatusReport.Reset();

         DetectionFinished?.Invoke( this, !string.IsNullOrEmpty( songInfoUrl ) );
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

      public List<MMDevice> DeviceList { get; }
      public List<string> DeviceNameList { get; }

      public Properties.UserSettings Settings => Properties.UserSettings.Default;

      private bool _recording;
      public bool Recording
      {
         get => _recording;
         private set => SetProperty( ref _recording, value );
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
         if ( Recording )
         {
            _recorder.CancelRecording();
         }
         else
         {
            var selectedDevice = Settings.SelectedDevice == DefaultDeviceName
               ? _deviceEnumerator.GetDefaultAudioEndpoint( DataFlow.Render, Role.Console )
               : DeviceList.First( x => x.FriendlyName == Settings.SelectedDevice );

            Recording = true;
            _recorder.StartRecording( selectedDevice );
         }
      } );
   }
}
