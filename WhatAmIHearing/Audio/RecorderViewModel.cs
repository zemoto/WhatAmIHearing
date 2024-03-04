using System;
using System.Collections.Generic;
using ZemotoCommon.UI;

namespace WhatAmIHearing.Audio;

internal sealed class RecorderViewModel( StateViewModel stateVm, DeviceProvider deviceProvider ) : ViewModelBase
{
   public AppSettings Settings { get; } = AppSettings.Instance;
   public StateViewModel StateVm { get; } = stateVm;
   public IReadOnlyCollection<string> DeviceNameList { get; } = deviceProvider.GetDeviceNameList();

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
}
