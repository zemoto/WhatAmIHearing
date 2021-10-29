using System.Windows;
using System.Windows.Input;
using ZemotoUI;

namespace WhatAmIHearing.Api.Spotify
{
   internal sealed class SpotifyViewModel : ViewModelBase
   {
      public bool SignedIn => !string.IsNullOrEmpty( Properties.UserSettings.Default.SpotifyAccessToken );

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

      private ICommand _signInOutCommand;
      public ICommand SignInOutCommand => _signInOutCommand ??= new RelayCommand( async () =>
      {
         using ( var authenticator = new SpotifyAuthenticator() )
         {
            if ( SignedIn )
            {
               authenticator.SignOut();
            }
            else
            {
               await authenticator.SignInAsync().ConfigureAwait( true );
            }
         }

         OnPropertyChanged( nameof( SignedIn ) );
         Application.Current.MainWindow.Activate();
      } );
   }
}
