using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using WhatAmIHearing.Api.Spotify;
using WhatAmIHearing.Audio;

namespace WhatAmIHearing;

internal sealed partial class MainViewModel( RecorderViewModel recorderVm, SpotifyViewModel spotifyVm, Action<Hotkey> setHotkeyAction ) : ObservableObject
{
   public AppSettings Settings { get; } = AppSettings.Instance;
   public RecorderViewModel RecorderVm { get; } = recorderVm;
   public SpotifyViewModel SpotifyVm { get; } = spotifyVm;

   [ObservableProperty]
   private string _hotkeyRegisterError;

   public RelayCommand<Hotkey> SetHotkeyCommand { get; } = new( setHotkeyAction );
}
