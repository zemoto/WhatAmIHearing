using WhatAmIHearing.Api.Spotify;
using WhatAmIHearing.Audio;

namespace WhatAmIHearing;

internal sealed class MainViewModel( StateViewModel stateVm, RecorderViewModel recorderVm, SpotifyViewModel spotifyVm ) : ZemotoCommon.UI.ViewModelBase
{
   public AppSettings Settings { get; } = AppSettings.Instance;
   public StateViewModel StateVm { get; } = stateVm;
   public RecorderViewModel RecorderVm { get; } = recorderVm;
   public SpotifyViewModel SpotifyVm { get; } = spotifyVm;

   private string _hotkeyStatusText;
   public string HotkeyStatusText
   {
      get => _hotkeyStatusText;
      set => SetProperty( ref _hotkeyStatusText, value );
   }
}
