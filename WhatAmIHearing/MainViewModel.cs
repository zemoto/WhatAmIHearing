using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using WhatAmIHearing.Audio;
using WhatAmIHearing.Result;

namespace WhatAmIHearing;

internal sealed partial class MainViewModel( RecorderViewModel recorderVm, Action<Hotkey> setHotkeyAction ) : ObservableObject
{
   public AppSettings Settings { get; } = AppSettings.Instance;
   public RecorderViewModel RecorderVm { get; } = recorderVm;

   [ObservableProperty]
   public SongViewModel _resultVm;

   [ObservableProperty]
   private string _hotkeyRegisterError;

   public RelayCommand<Hotkey> SetHotkeyCommand { get; } = new( setHotkeyAction );
}
