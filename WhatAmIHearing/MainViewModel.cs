using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Win32;
using WhatAmIHearing.Audio;
using WhatAmIHearing.Result;

namespace WhatAmIHearing;

internal sealed partial class MainViewModel : ObservableObject
{
   public MainViewModel( StateViewModel stateVm, RecorderViewModel recorderVm, Action<Hotkey> setHotkeyAction )
   {
      StateVm = stateVm;
      RecorderVm = recorderVm;
      SetHotkeyCommand = new RelayCommand<Hotkey>( setHotkeyAction );
      DeleteSongFromHistoryCommand = new RelayCommand<SongViewModel>( song => _ = Settings.History.Remove( song! ) );

      using RegistryKey? key = Registry.ClassesRoot.OpenSubKey( "spotify" );
      CanOpenInSpotify = key?.GetValue( "" ) is not null;
      if ( !CanOpenInSpotify )
      {
         Settings.OpenSpotifyLinksInApp = false;
      }
   }

   public AppSettings Settings { get; } = AppSettings.Instance;
   public StateViewModel StateVm { get; }
   public RecorderViewModel RecorderVm { get; }

   [ObservableProperty]
   public partial string HotkeyRegisterError { get; set; } = string.Empty;

   [ObservableProperty]
   public partial SongViewModel? SelectedSong { get; set; }

   public bool CanOpenInSpotify { get; }

   public RelayCommand<Hotkey> SetHotkeyCommand { get; }
   public RelayCommand<SongViewModel> DeleteSongFromHistoryCommand { get; }
}
