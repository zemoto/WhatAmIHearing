using System.Collections.Generic;
using WhatAmIHearing.Audio;

namespace WhatAmIHearing.Model
{
   internal sealed class MainViewModel : ZemotoCommon.UI.ViewModelBase
   {
      public MainViewModel( RecorderViewModel recorderVm, SpotifyViewModel spotifyVm, DeviceProvider deviceProvider )
      {
         RecorderVm = recorderVm;
         SpotifyVm = spotifyVm;
         DeviceNameList = deviceProvider.GetDeviceNameList();
      }

      public Properties.UserSettings Settings { get; } = Properties.UserSettings.Default;
      public RecorderViewModel RecorderVm { get; }
      public SpotifyViewModel SpotifyVm { get; }
      public List<string> DeviceNameList { get; }

      private string _hotkeyStatusText;
      public string HotkeyStatusText
      {
         get => _hotkeyStatusText;
         set => SetProperty( ref _hotkeyStatusText, value );
      }
   }
}
