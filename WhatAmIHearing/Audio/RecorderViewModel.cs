using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.Generic;

namespace WhatAmIHearing.Audio;

internal sealed partial class RecorderViewModel( StateViewModel stateVm, DeviceProvider deviceProvider ) : ObservableObject
{
   public AppSettings Settings { get; } = AppSettings.Instance;
   public StateViewModel StateVm { get; } = stateVm;
   public IReadOnlyCollection<string> DeviceNameList { get; } = deviceProvider.GetDeviceNameList();

   [ObservableProperty]
   private double _recordingProgress;

   [ObservableProperty]
   private double _recordPercent = 1.0;
}
