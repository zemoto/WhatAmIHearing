using System;
using System.Collections.Generic;
using System.Windows.Input;
using ZemotoCommon.UI;

namespace WhatAmIHearing.Audio;

internal enum RecorderState
{
   Stopped = 0,
   Recording = 1,
   Identifying = 2,
   Error = 3,
}

internal sealed class RecorderViewModel : ViewModelBase
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
      set => SetProperty( ref _recordingProgress, value * RecordPercent );
   }

   private double _recordPercent = 1.0;
   public double RecordPercent
   {
      get => _recordPercent;
      set => SetProperty( ref _recordPercent, Math.Clamp( value, 0.1, 1 ) );
   }

   public ICommand ChangeStateCommand { get; init; }
}
