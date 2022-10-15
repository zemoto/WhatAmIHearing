using System;
using System.Threading.Tasks;
using WhatAmIHearing.Model;
using ZemotoCommon.UI;

namespace WhatAmIHearing.Api.Spotify
{
   internal sealed class SpotifyManager
   {
      public SpotifyViewModel Model { get; }

      public event EventHandler SignInComplete;

      public SpotifyManager() => Model = new SpotifyViewModel { SignInOutCommand = new RelayCommand( OnSpotifySignInOut ) };

      private async void OnSpotifySignInOut()
      {
         using ( var authenticator = new SpotifyAuthenticator() )
         {
            if ( Model.SignedIn )
            {
               authenticator.SignOut();
            }
            else if ( await authenticator.SignInAsync().ConfigureAwait( true ) )
            {
               SignInComplete.Invoke( this, EventArgs.Empty );
            }
         }

         Model.NotifySignedInChanged();
      }

      public async Task AddSongToOurPlaylistAsync( string title, string subtitle )
      {
         if ( Model.SignedIn )
         {
            Model.Result = await SpotifyApi.AddSongToOurPlaylistAsync( title, subtitle ).ConfigureAwait( false );
         }
      }
   }
}
