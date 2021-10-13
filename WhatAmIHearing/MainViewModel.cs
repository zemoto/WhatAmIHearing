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
      private readonly Recorder _recorder;

      public MainViewModel()
      {
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

      private string _statusText;
      public string StatusText
      {
         get => _statusText;
         set => SetProperty( ref _statusText, value );
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
