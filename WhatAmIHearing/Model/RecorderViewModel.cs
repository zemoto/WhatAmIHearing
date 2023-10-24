using System;
using System.Collections.Generic;
using System.Windows.Input;
using WhatAmIHearing.Audio;

namespace WhatAmIHearing.Model;

internal enum RecorderState
{
   Stopped = 0,
   Recording = 1,
   SendingToShazam = 2
}

internal sealed class RecorderViewModel : ZemotoCommon.UI.ViewModelBase
{
   public RecorderViewModel( DeviceProvider deviceProvider ) => DeviceNameList = deviceProvider.GetDeviceNameList();

   public AppSettings Settings { get; } = AppSettings.Instance;
   public IReadOnlyCollection<string> DeviceNameList { get; }

   private RecorderState _state;
   public RecorderState State
   {
      get => _state;
      set => SetProperty( ref _state, value );
   }

   private string _recorderStatusText;
   public string RecorderStatusText
   {
      get => _recorderStatusText;
      set => SetProperty( ref _recorderStatusText, value );
   }

   private double _recordingProgress;
   public double RecordingProgress
   {
      get => _recordingProgress;
      set => SetProperty( ref _recordingProgress, value );
   }

   private double _recordPercent = 1.0;
   public double RecordPercent
   {
      get => _recordPercent;
      set => SetProperty( ref _recordPercent, Math.Clamp( value, 0.1, 1 ) );
   }

   public ICommand RecordStopCommand { get; set; }
}
