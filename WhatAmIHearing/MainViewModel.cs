using WhatAmIHearing.Api.Spotify;
using WhatAmIHearing.Audio;

namespace WhatAmIHearing;

internal sealed class MainViewModel : ZemotoCommon.UI.ViewModelBase
{
   public MainViewModel( RecorderViewModel recorderVm, SpotifyViewModel spotifyVm )
   {
      RecorderVm = recorderVm;
      SpotifyVm = spotifyVm;
   }

   public AppSettings Settings { get; } = AppSettings.Instance;
   public RecorderViewModel RecorderVm { get; }
   public SpotifyViewModel SpotifyVm { get; }

   private string _hotkeyStatusText;
   public string HotkeyStatusText
   {
      get => _hotkeyStatusText;
      set => SetProperty( ref _hotkeyStatusText, value );
   }
}
