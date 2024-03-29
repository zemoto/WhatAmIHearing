﻿using ZemotoCommon.UI;

namespace WhatAmIHearing.Api.Spotify;

internal sealed class SpotifyViewModel : ViewModelBase
{
   public void NotifySignedInChanged() => OnPropertyChanged( nameof( SignedIn ) );

   public bool SignedIn => !string.IsNullOrEmpty( AppSettings.Instance.SpotifyAccessToken );

   private AddToPlaylistResult _result;
   public AddToPlaylistResult Result
   {
      get => _result;
      set
      {
         if ( SetProperty( ref _result, value ) )
         {
            OnPropertyChanged( nameof( ResultText ) );
         }
      }
   }

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
