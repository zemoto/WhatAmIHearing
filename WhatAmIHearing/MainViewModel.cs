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
   internal sealed class MainViewModel : ViewModelBase
   {
      private readonly Recorder _recorder = new Recorder();

      public MainViewModel()
      {
         _recorder.RecordingStopped += ( s, args ) =>
         {
            Recording = false;
            if ( args.RecordedData == null )
            {
               return;
            }

            var result = ShazamApi.DetectSong( args.RecordedData );
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
         };
      }

      public IEnumerable<MMDevice> DeviceList { get; } = new MMDeviceEnumerator().EnumerateAudioEndPoints( DataFlow.All, DeviceState.Active );

      private MMDevice _selectedDevice;
      public MMDevice SelectedDevice
      {
         get => _selectedDevice ??= DeviceList.FirstOrDefault();
         set => SetProperty( ref _selectedDevice, value );
      }

      private bool _recording;
      public bool Recording
      {
         get => _recording;
         private set => SetProperty( ref _recording, value );
      }

      private ICommand _recordCommand;
      public ICommand RecordCommand => _recordCommand ??= new RelayCommand( () =>
      {
         Recording = true;
         _recorder.StartRecording( _selectedDevice );
      } );

      private ICommand _stopCommand;
      public ICommand StopCommand => _stopCommand ??= new RelayCommand( _recorder.CancelRecording );
   }
}
