using System;
using WhatAmIHearing.Api.Spotify;
using WhatAmIHearing.Audio;
using ZemotoCommon.UI;

namespace WhatAmIHearing;

internal sealed class MainViewModel( RecorderViewModel recorderVm, SpotifyViewModel spotifyVm, Action<Hotkey> setHotkeyAction ) : ViewModelBase
{
   public AppSettings Settings { get; } = AppSettings.Instance;
   public RecorderViewModel RecorderVm { get; } = recorderVm;
   public SpotifyViewModel SpotifyVm { get; } = spotifyVm;

   private string _hotkeyRegisterError;
   public string HotkeyRegisterError
   {
      get => _hotkeyRegisterError;
      set => SetProperty( ref _hotkeyRegisterError, value );
   }

   public RelayCommand<Hotkey> SetHotkeyCommand { get; } = new( setHotkeyAction );
}
