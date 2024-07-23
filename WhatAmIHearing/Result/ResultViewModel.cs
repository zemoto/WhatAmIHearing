using CommunityToolkit.Mvvm.Input;
using System;
using System.Diagnostics;
using System.Windows.Media.Imaging;
using WhatAmIHearing.Api.Shazam;

namespace WhatAmIHearing.Result;
internal class ResultViewModel( DetectedTrackInfo detectedTrackInfo )
{
   private const string _youTubeUrl = "https://www.youtube.com/results?search_query={0}";
   private const string _spotifyUrl = "https://open.spotify.com/search/{0}";

   private BitmapImage _bitmapImage;
   public BitmapImage CoverArt
   {
      get
      {
         if ( _bitmapImage is null )
         {
            _bitmapImage = new BitmapImage();
            _bitmapImage.BeginInit();
            _bitmapImage.UriSource = new Uri( detectedTrackInfo.ShareInfo.CoverArtUrl, UriKind.Absolute );
            _bitmapImage.EndInit();
         }

         return _bitmapImage;
      }
   }

   public string Title => detectedTrackInfo.Title;

   public string Subtitle => detectedTrackInfo.Subtitle;

   private string SearchText => $"{Title} - {Subtitle}";

   private RelayCommand _findInYouTubeCommand;
   public RelayCommand FindInYouTubeCommand => _findInYouTubeCommand ??= new RelayCommand( () => OpenResultInBrowser( string.Format( _youTubeUrl, SearchText ) ) );

   private RelayCommand _findInSpotifyCommand;
   public RelayCommand FindInSpotifyCommand => _findInSpotifyCommand ??= new RelayCommand( () => OpenResultInBrowser( string.Format( _spotifyUrl, SearchText ) ) );

   private RelayCommand _openInShazamCommand;
   public RelayCommand OpenInShazamCommand => _openInShazamCommand ??= new RelayCommand( () => OpenResultInBrowser( detectedTrackInfo.ShareInfo.ShazamUrl ) );

   private static void OpenResultInBrowser( string url ) => Process.Start( new ProcessStartInfo( url ) { UseShellExecute = true } );
}
