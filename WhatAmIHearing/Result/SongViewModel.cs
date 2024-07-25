using CommunityToolkit.Mvvm.Input;
using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Media.Imaging;
using WhatAmIHearing.Api.Shazam;

namespace WhatAmIHearing.Result;

internal class SongViewModel
{
   private const string _youTubeUrl = "https://www.youtube.com/results?search_query={0}";
   private const string _spotifyUrl = "https://open.spotify.com/search/{0}";

   public SongViewModel()
   {
   }

   public SongViewModel( DetectedTrackInfo songInfo )
   {
      CoverArtUrl = songInfo.ShareInfo.CoverArtUrl;
      Title = songInfo.Title;
      Subtitle = songInfo.Subtitle;
      ShazamUrl = songInfo.ShareInfo.ShazamUrl;
   }

   public string CoverArtUrl { get; init; }

   private BitmapImage _coverArt;
   public BitmapImage CoverArt
   {
      get
      {
         if ( _coverArt is null )
         {
            _coverArt = new BitmapImage();
            _coverArt.BeginInit();
            _coverArt.UriSource = new Uri( CoverArtUrl, UriKind.Absolute );
            _coverArt.EndInit();
         }

         return _coverArt;
      }
   }

   public string Title { get; init; }

   public string Subtitle { get; init; }

   public string ShazamUrl { get; init; }

   public string SearchText => $"{Title} - {Subtitle}";

   private RelayCommand _copyTitleToClipboard;
   public RelayCommand CopyTitleToClipboard => _copyTitleToClipboard ??= new RelayCommand( () =>
   {
      try
      {
         Clipboard.SetDataObject( new DataObject( DataFormats.UnicodeText, SearchText, true ), true );
      }
      catch { }
   } );

   private RelayCommand _findInYouTubeCommand;
   public RelayCommand FindInYouTubeCommand => _findInYouTubeCommand ??= new RelayCommand( () => OpenInBrowser( string.Format( _youTubeUrl, SearchText ) ) );

   private RelayCommand _findInSpotifyCommand;
   public RelayCommand FindInSpotifyCommand => _findInSpotifyCommand ??= new RelayCommand( () => OpenInBrowser( string.Format( _spotifyUrl, SearchText ) ) );

   private RelayCommand _openInShazamCommand;
   public RelayCommand OpenInShazamCommand => _openInShazamCommand ??= new RelayCommand( () => OpenInBrowser( ShazamUrl ) );

   private static void OpenInBrowser( string url ) => Process.Start( new ProcessStartInfo( url ) { UseShellExecute = true } );
}
