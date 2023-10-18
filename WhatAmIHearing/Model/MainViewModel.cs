namespace WhatAmIHearing.Model;

internal sealed class MainViewModel : ZemotoCommon.UI.ViewModelBase
{
   public MainViewModel( RecorderViewModel recorderVm, SpotifyViewModel spotifyVm )
   {
      RecorderVm = recorderVm;
      SpotifyVm = spotifyVm;
   }

   public Properties.UserSettings Settings { get; } = Properties.UserSettings.Default;
   public RecorderViewModel RecorderVm { get; }
   public SpotifyViewModel SpotifyVm { get; }

   private string _hotkeyStatusText;
   public string HotkeyStatusText
   {
      get => _hotkeyStatusText;
      set => SetProperty( ref _hotkeyStatusText, value );
   }
}
