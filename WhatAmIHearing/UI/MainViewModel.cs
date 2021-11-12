using System.Collections.Generic;
using System.Windows.Input;
using WhatAmIHearing.Api.Spotify;
using WhatAmIHearing.Audio;
using ZemotoUI;

namespace WhatAmIHearing.UI
{
   internal sealed class MainViewModel : ViewModelBase
   {
      public MainViewModel( DeviceProvider deviceProvider ) => DeviceNameList = deviceProvider.GetDeviceNameList();

      public List<string> DeviceNameList { get; }
      public Properties.UserSettings Settings { get; } = Properties.UserSettings.Default;
      public SpotifyViewModel SpotifyVm { get; } = new();
      public RecorderViewModel RecorderVm { get; } = new();

      private string _hotkeyStatusText;
      public string HotkeyStatusText
      {
         get => _hotkeyStatusText;
         set => SetProperty( ref _hotkeyStatusText, value );
      }

      public ICommand RecordStopCommand { get; set; }
   }
}
