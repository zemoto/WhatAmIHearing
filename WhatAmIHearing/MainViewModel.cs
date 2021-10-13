using NAudio.CoreAudioApi;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using WhatAmIHearing.Api;
using ZemotoUI;

namespace WhatAmIHearing
{
   internal sealed class MainViewModel : ViewModelBase, IStatusTextDisplayer
   {
      private const string DefaultDeviceName = "Default Input Device";

      private readonly Recorder _recorder;
      private readonly MMDeviceEnumerator _deviceEnumerator = new MMDeviceEnumerator();

      public MainViewModel()
      {
         DeviceList = _deviceEnumerator.EnumerateAudioEndPoints( DataFlow.All, DeviceState.Active ).ToList();
         DeviceNameList = DeviceList.ConvertAll( x => x.FriendlyName );
         DeviceNameList.Insert( 0, DefaultDeviceName );

         var lastSelectedDevice = Properties.UserSettings.Default.SelectedDevice;
         SelectedDeviceName = string.IsNullOrEmpty( lastSelectedDevice ) ? DefaultDeviceName : lastSelectedDevice;

         _recorder = new Recorder( this );
         _recorder.RecordingStopped += async ( s, args ) =>
         {
            Recording = false;
            if ( args.RecordedData != null )
            {
               StatusText = $"Sending resampled {args.RecordedData.Length} bits to Shazam";
               var result = await ShazamApi.DetectSongAsync( args.RecordedData ).ConfigureAwait( true );
               if ( !string.IsNullOrEmpty( result ) )
               {
                  _ = Process.Start( new ProcessStartInfo( result ) { UseShellExecute = true } );
               }
               else
               {
                  var msgBoxResult = MessageBox.Show( "No song detected. Playback recorded sound?", "Detection Failed", MessageBoxButton.YesNo );
                  if ( msgBoxResult == MessageBoxResult.Yes )
                  {
                     var player = new AudioPlayer( args.RecordedData, args.Format );
                     player.PlayAudio();
                  }
               }
            }

            StatusText = string.Empty;
         };
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

      private string _statusText;
      public string StatusText
      {
         get => _statusText;
         set => SetProperty( ref _statusText, value );
      }

      private ICommand _recordCommand;
      public ICommand RecordCommand => _recordCommand ??= new RelayCommand( () =>
      {
         var selectedDevice = SelectedDeviceName == DefaultDeviceName
            ? _deviceEnumerator.GetDefaultAudioEndpoint( DataFlow.Render, Role.Console )
            : DeviceList.First( x => x.FriendlyName == SelectedDeviceName );

         Properties.UserSettings.Default.SelectedDevice = SelectedDeviceName;
         Properties.UserSettings.Default.Save();

         Recording = true;
         _recorder.StartRecording( selectedDevice );
      } );

      private ICommand _stopCommand;
      public ICommand StopCommand => _stopCommand ??= new RelayCommand( _recorder.CancelRecording );
   }
}
