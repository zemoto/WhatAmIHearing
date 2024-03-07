using WhatAmIHearing.Api.Spotify;
using WhatAmIHearing.Audio;

namespace WhatAmIHearing;

internal sealed class MainViewModel( RecorderViewModel recorderVm, SpotifyViewModel spotifyVm ) : ZemotoCommon.UI.ViewModelBase
{
   public AppSettings Settings { get; } = AppSettings.Instance;
   public RecorderViewModel RecorderVm { get; } = recorderVm;
   public SpotifyViewModel SpotifyVm { get; } = spotifyVm;

   private string _hotkeyStatusText;
   public string HotkeyStatusText
   {
      get => _hotkeyStatusText;
      set => SetProperty( ref _hotkeyStatusText, value );
   }
}
