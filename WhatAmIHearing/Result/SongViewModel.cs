using CommunityToolkit.Mvvm.Input;
using System;
using System.Diagnostics;
using System.Windows.Media.Imaging;
using WhatAmIHearing.Api.Shazam;

namespace WhatAmIHearing.Result;

internal class SongViewModel
{
   private const string _youTubeUrl = "https://www.youtube.com/results?search_query={0}";
   private const string _spotifyUrl = "https://open.spotify.com/search/{0}";

   public SongViewModel( DetectedTrackInfo songInfo )
   {
      CoverArt = new BitmapImage();
      CoverArt.BeginInit();
      CoverArt.UriSource = new Uri( songInfo.ShareInfo.CoverArtUrl, UriKind.Absolute );
      CoverArt.EndInit();

      Title = songInfo.Title;
      Subtitle = songInfo.Subtitle;
      ShazamUrl = songInfo.ShareInfo.ShazamUrl;
   }

   public BitmapImage CoverArt { get; }

   public string Title { get; }

   public string Subtitle { get; }

   public string ShazamUrl { get; }

   private string SearchText => $"{Title} - {Subtitle}";

   private RelayCommand _findInYouTubeCommand;
   public RelayCommand FindInYouTubeCommand => _findInYouTubeCommand ??= new RelayCommand( () => OpenInBrowser( string.Format( _youTubeUrl, SearchText ) ) );

   private RelayCommand _findInSpotifyCommand;
   public RelayCommand FindInSpotifyCommand => _findInSpotifyCommand ??= new RelayCommand( () => OpenInBrowser( string.Format( _spotifyUrl, SearchText ) ) );

   private RelayCommand _openInShazamCommand;
   public RelayCommand OpenInShazamCommand => _openInShazamCommand ??= new RelayCommand( () => OpenInBrowser( ShazamUrl ) );

   private static void OpenInBrowser( string url ) => Process.Start( new ProcessStartInfo( url ) { UseShellExecute = true } );
}
