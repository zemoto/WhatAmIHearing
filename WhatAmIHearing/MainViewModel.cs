using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.ObjectModel;
using WhatAmIHearing.Audio;
using WhatAmIHearing.Result;

namespace WhatAmIHearing;

internal sealed partial class MainViewModel : ObservableObject
{
   public MainViewModel( RecorderViewModel recorderVm, ResultHistory history, Action<Hotkey> setHotkeyAction )
   {
      RecorderVm = recorderVm;
      History = history;
      SetHotkeyCommand = new RelayCommand<Hotkey>( setHotkeyAction );
      DeleteSongFromHistoryCommand = new RelayCommand<SongViewModel>( song => _ = History.Remove( song ) );
   }

   public AppSettings Settings { get; } = AppSettings.Instance;
   public RecorderViewModel RecorderVm { get; }
   public ObservableCollection<SongViewModel> History { get; }

   [ObservableProperty]
   private string _hotkeyRegisterError;

   [ObservableProperty]
   public SongViewModel _selectedSong;

   public RelayCommand<Hotkey> SetHotkeyCommand { get; }
   public RelayCommand<SongViewModel> DeleteSongFromHistoryCommand { get; }
}
