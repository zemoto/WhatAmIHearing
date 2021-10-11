using NAudio.CoreAudioApi;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;
using ZemotoUI;

namespace WhatAmIHearing
{
   internal enum RecordingState
   {
      Stopped = 0,
      Recording = 1
   }

   internal sealed class MainViewModel : ViewModelBase
   {
      private readonly RecordingManager _recordingManager = new RecordingManager();

      public MainViewModel() => _recordingManager.RecordingStateChanged += ( s, a ) => State = a.NewState;

      public IEnumerable<MMDevice> DeviceList { get; } = new MMDeviceEnumerator().EnumerateAudioEndPoints( DataFlow.All, DeviceState.Active );

      private MMDevice _selectedDevice;
      public MMDevice SelectedDevice
      {
         get => _selectedDevice ??= DeviceList.FirstOrDefault();
         set => SetProperty( ref _selectedDevice, value );
      }

      private RecordingState _state;
      public RecordingState State
      {
         get => _state;
         private set => SetProperty( ref _state, value );
      }

      private ICommand _recordCommand;
      public ICommand RecordCommand => _recordCommand ??= new RelayCommand( () => _recordingManager.StartRecording( _selectedDevice ) );

      private ICommand _stopCommand;
      public ICommand StopCommand => _stopCommand ??= new RelayCommand( _recordingManager.StopRecording );
   }
}
