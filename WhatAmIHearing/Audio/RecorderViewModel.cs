using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.ObjectModel;

namespace WhatAmIHearing.Audio;

internal sealed partial class RecorderViewModel( StateViewModel stateVm, DeviceProvider deviceProvider ) : ObservableObject
{
   public AppSettings Settings { get; } = AppSettings.Instance;
   public StateViewModel StateVm { get; } = stateVm;
   public ObservableCollection<DeviceListItem> Devices => deviceProvider.Devices;

   [ObservableProperty]
   public partial double RecordingProgress { get; set; }

   [ObservableProperty]
   public partial double RecordPercent { get; set; } = 1.0;
}
