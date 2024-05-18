using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace WhatAmIHearing.Api.Spotify;

internal sealed partial class SpotifyViewModel : ObservableObject
{
   public void NotifySignedInChanged() => OnPropertyChanged( nameof( SignedIn ) );

   public bool SignedIn => !string.IsNullOrEmpty( AppSettings.Instance.SpotifyAccessToken );

   [ObservableProperty]
   [NotifyPropertyChangedFor( nameof( ResultText ) )]
   private AddToPlaylistResult _result;

   public string ResultText => _result switch
   {
      AddToPlaylistResult.FailedToAuthenticate => "Spotify authentication failed",
      AddToPlaylistResult.CouldNotFindOrCreatePlaylist => "Could not find or create private playlist",
      AddToPlaylistResult.CouldNotFindSong => "Could not find song on Spotify",
      AddToPlaylistResult.SongAlreadyInPlaylist => "Song already in playlist",
      AddToPlaylistResult.Failed => "Failed to add song to playlist",
      AddToPlaylistResult.Success => "Successfully added song to playlist",
      _ => string.Empty
   };

   public RelayCommand SignInOutCommand { get; set; }
}
