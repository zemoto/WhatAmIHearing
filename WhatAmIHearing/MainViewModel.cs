using WhatAmIHearing.Api.Spotify;
using WhatAmIHearing.Audio;

namespace WhatAmIHearing;

internal sealed class MainViewModel : ZemotoCommon.UI.ViewModelBase
{
   public MainViewModel( StateViewModel stateVm, RecorderViewModel recorderVm, SpotifyViewModel spotifyVm )
   {
      StateVm = stateVm;
      RecorderVm = recorderVm;
      SpotifyVm = spotifyVm;
   }

   public AppSettings Settings { get; } = AppSettings.Instance;
   public StateViewModel StateVm { get; }
   public RecorderViewModel RecorderVm { get; }
   public SpotifyViewModel SpotifyVm { get; }

   private string _hotkeyStatusText;
   public string HotkeyStatusText
   {
      get => _hotkeyStatusText;
      set => SetProperty( ref _hotkeyStatusText, value );
   }
}
