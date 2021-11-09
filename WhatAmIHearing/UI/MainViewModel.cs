using System.Collections.Generic;
using System.Windows.Input;
using WhatAmIHearing.Api.Spotify;
using WhatAmIHearing.Audio;
using ZemotoUI;

namespace WhatAmIHearing.UI
{
   internal enum State
   {
      Stopped = 0,
      Recording = 1,
      SendingToShazam = 2
   }

   internal sealed class MainViewModel : ViewModelBase
   {
      public MainViewModel( DeviceProvider deviceProvider ) => DeviceNameList = deviceProvider.GetDeviceNameList();

      public List<string> DeviceNameList { get; }
      public Properties.UserSettings Settings { get; } = Properties.UserSettings.Default;
      public SpotifyViewModel SpotifyVm { get; } = new();

      private State _recorderState;
      public State RecorderState
      {
         get => _recorderState;
         set => SetProperty( ref _recorderState, value );
      }

      private string _hotkeyStatusText;
      public string HotkeyStatusText
      {
         get => _hotkeyStatusText;
         set => SetProperty( ref _hotkeyStatusText, value );
      }

      public bool KeepWindowTopmost
      {
         get => Settings.KeepWindowTopmost;
         set
         {
            if ( Settings.KeepWindowTopmost != value )
            {
               Settings.KeepWindowTopmost = value;
               OnPropertyChanged( nameof( KeepWindowTopmost ) );
            }
         }
      }

      public ICommand RecordStopCommand { get; set; }
   }
}
