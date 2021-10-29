namespace WhatAmIHearing.Api.Spotify
{
   internal enum AddToPlaylistResult
   {
      None = 0,
      FailedToAuthenticate = 1,
      CouldNotFindOrCreatePlaylist = 2,
      CouldNotFindSong = 3,
      SongAlreadyInPlaylist = 4,
      Failed = 5,
      Success = 6
   }
}
