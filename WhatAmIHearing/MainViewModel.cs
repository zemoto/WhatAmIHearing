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
            CanStartRecording = true;
            var result = ShazamApi.DetectSong( args.RecordedData );
            if ( !string.IsNullOrEmpty( result ) )
            {
               Process.Start( new ProcessStartInfo( result ) { UseShellExecute = true } );
            }
            else
            {
               MessageBox.Show( "No song detected" );
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

      private bool _canStartRecording = true;
      public bool CanStartRecording
      {
         get => _canStartRecording;
         set => SetProperty( ref _canStartRecording, value );
      }

      private ICommand _recordCommand;
      public ICommand RecordCommand => _recordCommand ??= new RelayCommand( () =>
      {
         CanStartRecording = false;
         _recorder.StartRecording( _selectedDevice );
      } );
   }
}
