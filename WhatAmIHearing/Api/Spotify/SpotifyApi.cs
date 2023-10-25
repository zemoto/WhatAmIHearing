using System;
using System.Globalization;
using System.Linq;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;
using WhatAmIHearing.Api.Spotify.Responses;

namespace WhatAmIHearing.Api.Spotify;

internal sealed class SpotifyApi : IDisposable
{
   private const string SongSearchEndpoint = "https://api.spotify.com/v1/search";
   private const string UserPlaylistsEndpoint = "https://api.spotify.com/v1/me/playlists";
   private const string PlaylistTracksEndpoint = "https://api.spotify.com/v1/playlists/{0}/tracks?";

   private const string OurPlaylistName = "What Did I Hear?";

   private readonly SpotifyApiClient _client = new();

   public void Dispose() => _client.Dispose();

   public async Task<AddToPlaylistResult> AddSongToOurPlaylistAsync( string title, string artist )
   {
      using ( var authenticator = new SpotifyAuthenticator() )
      {
         if ( !await authenticator.EnsureAuthenticationIsValid() )
         {
            return AddToPlaylistResult.FailedToAuthenticate;
         }
      }

      var ourPlaylistId = await GetOurPlaylistIdAsync();
      if ( string.IsNullOrEmpty( ourPlaylistId ) )
      {
         ourPlaylistId = await CreateOurPlaylistAsync();
         if ( string.IsNullOrEmpty( ourPlaylistId ) )
         {
            return AddToPlaylistResult.CouldNotFindOrCreatePlaylist;
         }
      }

      var songId = await GetSongIdAsync( title, artist );
      if ( string.IsNullOrEmpty( songId ) )
      {
         return AddToPlaylistResult.CouldNotFindSong;
      }

      if ( await GetIsSongInPlaylistAsync( songId, ourPlaylistId ) )
      {
         return AddToPlaylistResult.SongAlreadyInPlaylist;
      }

      var playlistTracksEndpoint = string.Format( CultureInfo.InvariantCulture, PlaylistTracksEndpoint, ourPlaylistId );
      var endpointBuilder = new UriBuilder( playlistTracksEndpoint );
      var query = HttpUtility.ParseQueryString( endpointBuilder.Query );
      query["uris"] = $"spotify:track:{songId}";
      endpointBuilder.Query = query.ToString();

      var result = await _client.SendPostRequestAsync( endpointBuilder.ToString() );
      return !string.IsNullOrEmpty( result ) ? AddToPlaylistResult.Success : AddToPlaylistResult.Failed;
   }

   private static readonly Regex SongQualifierRegex = new( @".*(\s\(.*\))" );
   private async Task<string> GetSongIdAsync( string title, string artist )
   {
      var result = SongQualifierRegex.Match( title );
      if ( result.Groups.Count > 0 )
      {
         var qualifier = result.Groups[^1].Value;
         title = title[..title.LastIndexOf( qualifier, StringComparison.OrdinalIgnoreCase )];
      }

      var endpointBuilder = new UriBuilder( SongSearchEndpoint );
      var query = HttpUtility.ParseQueryString( endpointBuilder.Query );
      query["q"] = title;
      query["type"] = "track";
      query["market"] = "US";
      endpointBuilder.Query = query.ToString();

      var response = await _client.SendGetRequestAsync( endpointBuilder.ToString() );
      if ( string.IsNullOrEmpty( response ) )
      {
         return string.Empty;
      }

      var songSearchResult = JsonSerializer.Deserialize<SongSearchResponse>( response );
      var trackResult = songSearchResult?.Tracks?.Items?.FirstOrDefault();
      return trackResult?.Artists is not null && trackResult.Artists.Any( x => x.Name.Equals( artist, StringComparison.OrdinalIgnoreCase ) )
         ? trackResult.Id
         : string.Empty;
   }

   private async Task<string> GetOurPlaylistIdAsync()
   {
      var response = await _client.SendGetRequestAsync( UserPlaylistsEndpoint );
      if ( string.IsNullOrEmpty( response ) )
      {
         return string.Empty;
      }

      var playlistList = JsonSerializer.Deserialize<PlaylistListResponse>( response );
      if ( ( playlistList?.Playlists?.Any() ) != true )
      {
         return string.Empty;
      }

      var ourPlaylist = playlistList.Playlists.Find( x => x.Name == OurPlaylistName );
      return ourPlaylist is null ? string.Empty : ourPlaylist.Id;
   }

   private async Task<string> CreateOurPlaylistAsync()
   {
      const string createPlaylistBodyTemplate = "{{ \"name\": \"{0}\", \"description\": \"{1}\", \"public\": \"false\" }}";
      const string description = "Playlist where songs identified by the WhatAmIHearingApp are added";
      var createPlaylistData = string.Format( CultureInfo.InvariantCulture, createPlaylistBodyTemplate, OurPlaylistName, description );

      var response = await _client.SendPostRequestAsync( UserPlaylistsEndpoint, createPlaylistData );
      if ( string.IsNullOrEmpty( response ) )
      {
         return string.Empty;
      }

      using var json = JsonDocument.Parse( response );
      return json.RootElement.TryGetProperty( "id", out var jsonId ) ? jsonId.ToString() : string.Empty;
   }

   private async Task<bool> GetIsSongInPlaylistAsync( string songId, string ourPlaylistId )
   {
      var playlistTracksEndpoint = string.Format( CultureInfo.InvariantCulture, PlaylistTracksEndpoint, ourPlaylistId );
      var endpointBuilder = new UriBuilder( playlistTracksEndpoint );
      var query = HttpUtility.ParseQueryString( endpointBuilder.Query );
      query["market"] = "US";
      query["fields"] = "items(track(id))";
      endpointBuilder.Query = query.ToString();

      var response = await _client.SendGetRequestAsync( endpointBuilder.ToString() );
      if ( string.IsNullOrEmpty( response ) )
      {
         return false;
      }

      var parsedResponse = JsonSerializer.Deserialize<PlaylistTrackListResponse>( response );
      return parsedResponse?.Items?.Any( x => x?.Track?.Id == songId ) == true;
   }
}
