using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.ObjectModel;
using WhatAmIHearing.Audio;
using WhatAmIHearing.Result;

namespace WhatAmIHearing;

internal sealed partial class MainViewModel( RecorderViewModel recorderVm, HistoryManager historyManager, Action<Hotkey> setHotkeyAction, Action<SongViewModel> deleteSongFromHistoryAction ) : ObservableObject
{
   public AppSettings Settings { get; } = AppSettings.Instance;
   public RecorderViewModel RecorderVm { get; } = recorderVm;

   [ObservableProperty]
   private string _hotkeyRegisterError;

   public ObservableCollection<SongViewModel> History => historyManager.History;

   [ObservableProperty]
   public SongViewModel _selectedSong;

   public RelayCommand<Hotkey> SetHotkeyCommand { get; } = new( setHotkeyAction );

   public RelayCommand<SongViewModel> DeleteSongFromHistoryCommand { get; } = new( deleteSongFromHistoryAction );
}
