using System;
using System.Threading.Tasks;
using ZemotoCommon.UI;

namespace WhatAmIHearing.Api.Spotify;

internal sealed class SpotifyManager : IDisposable
{
   private readonly SpotifyApi _api = new();
   private readonly Action _signInCompleteAction;

   public SpotifyViewModel Model { get; }

   public SpotifyManager( Action signInCompleteAction )
   {
      _signInCompleteAction = signInCompleteAction;
      Model = new SpotifyViewModel { SignInOutCommand = new RelayCommand( OnSpotifySignInOut ) };
   }

   public void Dispose() => _api.Dispose();

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
            _signInCompleteAction.Invoke();
         }
      }

      Model.NotifySignedInChanged();
   }

   public async Task AddSongToOurPlaylistAsync( string title, string subtitle )
   {
      if ( Model.SignedIn )
      {
         Model.Result = await _api.AddSongToOurPlaylistAsync( title, subtitle );
      }
   }
}
