using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.ObjectModel;
using WhatAmIHearing.Api.Shazam;
using WhatAmIHearing.Audio;
using WhatAmIHearing.Result;

namespace WhatAmIHearing;

internal sealed partial class MainViewModel : ObservableObject
{
   public MainViewModel( StateViewModel stateVm, RecorderViewModel recorderVm, ResultHistory history, ShazamApiSettings shazamSettings, Action<Hotkey> setHotkeyAction, Action<string> openHyperlinkAction )
   {
      StateVm = stateVm;
      RecorderVm = recorderVm;
      History = history;
      ShazamSettings = shazamSettings;
      SetHotkeyCommand = new RelayCommand<Hotkey>( setHotkeyAction );
      DeleteSongFromHistoryCommand = new RelayCommand<SongViewModel>( song => _ = History.Remove( song ) );
      OpenHyperlinkCommand = new RelayCommand<string>( openHyperlinkAction );
   }

   public AppSettings Settings { get; } = AppSettings.Instance;
   public StateViewModel StateVm { get; }
   public RecorderViewModel RecorderVm { get; }
   public ObservableCollection<SongViewModel> History { get; }
   public ShazamApiSettings ShazamSettings { get; }

   [ObservableProperty]
   private string _hotkeyRegisterError;

   [ObservableProperty]
   public SongViewModel _selectedSong;

   public RelayCommand<Hotkey> SetHotkeyCommand { get; }
   public RelayCommand<SongViewModel> DeleteSongFromHistoryCommand { get; }
   public RelayCommand<string> OpenHyperlinkCommand { get; }
}
