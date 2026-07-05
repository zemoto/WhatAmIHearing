using CommunityToolkit.Mvvm.Input;
using System.Windows;
using System.Windows.Media.Imaging;
using WhatAmIHearing.Shazam;
using ZemotoCommon;

namespace WhatAmIHearing.Result;

internal sealed class SongViewModel
{
   private const string _youTubeUrl = "https://www.youtube.com/results?search_query={0}";
   private const string _spotifySearchUrl = "https://open.spotify.com/search/{0}";
   private const string _openInSpotifyProtocol = "spotify:search:{0}";

   public SongViewModel()
   {
   }

   public SongViewModel( DetectedTrackInfo songInfo )
   {
      CoverArtUrl = songInfo.ShareInfo?.CoverArtUrl ?? string.Empty;
      Title = songInfo.Title;
      Subtitle = songInfo.Subtitle;
      ShazamUrl = songInfo.ShareInfo?.ShazamUrl ?? string.Empty;
   }

   public string CoverArtUrl { get; init; } = string.Empty;

   public BitmapImage? CoverArt
   {
      get
      {
         if ( !string.IsNullOrEmpty( CoverArtUrl ) && field is null )
         {
            field = new BitmapImage();
            field.BeginInit();
            field.CacheOption = BitmapCacheOption.OnLoad;
            field.UriSource = new Uri( CoverArtUrl, UriKind.Absolute );
            field.EndInit();

            if ( field.IsDownloading )
            {
               field.DownloadCompleted += ( s, e ) => ( s as BitmapImage )?.Freeze();
            }
            else
            {
               field.Freeze();
            }
         }

         return field;
      }
   }

   public string Title { get; init; } = string.Empty;

   public string Subtitle { get; init; } = string.Empty;

   public string ShazamUrl { get; init; } = string.Empty;

   public string SearchText => $"{Title} - {Subtitle}";

   public RelayCommand CopyTitleToClipboard => field ??= new RelayCommand( () =>
   {
      try
      {
         Clipboard.SetDataObject( new DataObject( DataFormats.UnicodeText, SearchText, true ), true );
      }
      catch { }
   } );

   public RelayCommand FindInYouTubeCommand => field ??= new RelayCommand( () => UtilityMethods.OpenInBrowser( string.Format( _youTubeUrl, SearchText ) ) );

   public RelayCommand FindInSpotifyCommand => field ??= new RelayCommand( () =>
   {
      var url = AppSettings.Instance.OpenSpotifyLinksInApp ? _openInSpotifyProtocol : _spotifySearchUrl;
      UtilityMethods.OpenInBrowser( string.Format( url, SearchText ) );
   } );
}
