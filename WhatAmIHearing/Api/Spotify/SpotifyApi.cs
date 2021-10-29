using System;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using System.Web;
using WhatAmIHearing.Api.Spotify.Responses;

namespace WhatAmIHearing.Api.Spotify
{
   internal static class SpotifyApi
   {
      private const string SongSearchEndpoint = "https://api.spotify.com/v1/search";
      private const string UserPlaylistsEndpoint = "https://api.spotify.com/v1/me/playlists";
      private const string PlaylistTracksEndpoint = "https://api.spotify.com/v1/playlists/{0}/tracks?";

      private const string OurPlaylistName = "What Did I Hear?";

      public static async Task<bool> AddSongToOurPlaylistAsync( string title, string artist )
      {
         using ( var authenticator = new SpotifyAuthenticator() )
         {
            if ( !await authenticator.EnsureAuthenticationIsValid().ConfigureAwait( false ) )
            {
               return false;
            }
         }

         using var client = new SpotifyApiClient();
         var ourPlaylistId = await GetOurPlaylistIdAsync( client ).ConfigureAwait( false );
         if ( string.IsNullOrEmpty( ourPlaylistId ) )
         {
            ourPlaylistId = await CreateOurPlaylistAsync( client ).ConfigureAwait( false );
            if ( string.IsNullOrEmpty( ourPlaylistId ) )
            {
               return false;
            }
         }

         var songId = await GetSongIdAsync( client, title, artist ).ConfigureAwait( false );
         if ( string.IsNullOrEmpty( songId ) )
         {
            return false;
         }

         if ( await GetIsSongInPlaylistAsync( client, songId, ourPlaylistId ).ConfigureAwait( false ) )
         {
            return true;
         }

         var playlistTracksEndpoint = string.Format( PlaylistTracksEndpoint, ourPlaylistId );
         var endpointBuilder = new UriBuilder( playlistTracksEndpoint );
         var query = HttpUtility.ParseQueryString( endpointBuilder.Query );
         query["uris"] = $"spotify:track:{songId}";
         endpointBuilder.Query = query.ToString();

         var result = await client.SendPostRequestAsync( endpointBuilder.ToString() ).ConfigureAwait( false );
         return !string.IsNullOrEmpty( result );
      }

      private static async Task<string> GetSongIdAsync( ApiClient client, string title, string artist )
      {
         var endpointBuilder = new UriBuilder( SongSearchEndpoint );
         var query = HttpUtility.ParseQueryString( endpointBuilder.Query );
         query["q"] = title;
         query["type"] = "track";
         query["market"] = "US";
         endpointBuilder.Query = query.ToString();

         var response = await client.SendGetRequestAsync( endpointBuilder.ToString() ).ConfigureAwait( false );

         if ( !string.IsNullOrEmpty( response ) )
         {
            var songSearchResult = JsonSerializer.Deserialize<SongSearchResponse>( response );
            var trackResult = songSearchResult?.Tracks?.Items?.FirstOrDefault();
            if ( trackResult?.Artists is not null && trackResult.Artists.Any( x => x.Name.Equals( artist, StringComparison.InvariantCultureIgnoreCase ) ) )
            {
               return trackResult.Id;
            }
         }

         return string.Empty;
      }

      private static async Task<string> GetOurPlaylistIdAsync( ApiClient client )
      {
         var response = await client.SendGetRequestAsync( UserPlaylistsEndpoint ).ConfigureAwait( false );

         if ( !string.IsNullOrEmpty( response ) )
         {
            var playlistList = JsonSerializer.Deserialize<PlaylistListResponse>( response );
            if ( playlistList?.Playlists?.Any() == true )
            {
               var ourPlaylist = playlistList.Playlists.Find( x => x.Name == OurPlaylistName );
               if ( ourPlaylist is not null )
               {
                  return ourPlaylist.Id;
               }
            }
         }

         return string.Empty;
      }

      private static async Task<string> CreateOurPlaylistAsync( ApiClient client )
      {
         const string createPlaylistBodyTemplate = "{{ \"name\": \"{0}\", \"description\": \"{1}\", \"public\": \"false\" }}";
         const string description = "Playlist where songs identified by the WhatAmIHearingApp are added";
         var createPlaylistData = string.Format( createPlaylistBodyTemplate, OurPlaylistName, description );

         var response = await client.SendPostRequestAsync( UserPlaylistsEndpoint, createPlaylistData ).ConfigureAwait( false );
         if ( !string.IsNullOrEmpty( response ) )
         {
            using var json = JsonDocument.Parse( response );
            if ( json.RootElement.TryGetProperty( "id", out var jsonId ) )
            {
               return jsonId.ToString();
            }
         }

         return string.Empty;
      }

      private static async Task<bool> GetIsSongInPlaylistAsync( ApiClient client, string songId, string ourPlaylistId )
      {
         var playlistTracksEndpoint = string.Format( PlaylistTracksEndpoint, ourPlaylistId );
         var endpointBuilder = new UriBuilder( playlistTracksEndpoint );
         var query = HttpUtility.ParseQueryString( endpointBuilder.Query );
         query["market"] = "US";
         query["fields"] = "items(track(id))";
         endpointBuilder.Query = query.ToString();

         var response = await client.SendGetRequestAsync( endpointBuilder.ToString() ).ConfigureAwait( false );
         if ( !string.IsNullOrEmpty( response ) )
         {
            var parsedResponse = JsonSerializer.Deserialize<PlaylistTrackListResponse>( response );
            if ( parsedResponse?.Items?.Any( x => x?.Track?.Id == songId ) == true )
            {
               return true;
            }
         }

         return false;
      }
   }
}
