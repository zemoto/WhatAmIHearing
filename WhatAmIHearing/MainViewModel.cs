using NAudio.CoreAudioApi;
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

      public MainViewModel()
      {
         DeviceList = _deviceEnumerator.EnumerateAudioEndPoints( DataFlow.All, DeviceState.Active ).ToList();
         DeviceNameList = DeviceList.ConvertAll( x => x.FriendlyName );
         DeviceNameList.Insert( 0, DefaultDeviceName );

         var lastSelectedDevice = Properties.UserSettings.Default.SelectedDevice;
         SelectedDeviceName = string.IsNullOrEmpty( lastSelectedDevice ) ? DefaultDeviceName : lastSelectedDevice;

         _recorder.RecordingFinished += OnRecordingStopped;
      }

      private async void OnRecordingStopped( object sender, RecordingFinishedEventArgs args )
      {
         Recording = false;
         if ( args.RecordedData == null )
         {
            StatusReport.Reset();
            return;
         }

         StatusReport.Status.Text = $"Sending resampled {args.RecordedData.Length} bits to Shazam";
         StatusReport.Status.Progress = 100;

         var songInfoUrl = await ShazamApi.DetectSongAsync( args.RecordedData ).ConfigureAwait( true );
         StatusReport.Reset();

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

      private string _selectedDeviceName;
      public string SelectedDeviceName
      {
         get => _selectedDeviceName;
         set => SetProperty( ref _selectedDeviceName, value );
      }

      private bool _recording;
      public bool Recording
      {
         get => _recording;
         private set => SetProperty( ref _recording, value );
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
            var selectedDevice = SelectedDeviceName == DefaultDeviceName
               ? _deviceEnumerator.GetDefaultAudioEndpoint( DataFlow.Render, Role.Console )
               : DeviceList.First( x => x.FriendlyName == SelectedDeviceName );

            Properties.UserSettings.Default.SelectedDevice = SelectedDeviceName;

            Recording = true;
            _recorder.StartRecording( selectedDevice );
         }
      } );
   }
}
