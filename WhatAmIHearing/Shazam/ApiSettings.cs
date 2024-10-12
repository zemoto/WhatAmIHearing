using CommunityToolkit.Mvvm.ComponentModel;
using ZemotoCommon;

namespace WhatAmIHearing.Shazam;

internal sealed partial class ApiSettings : ObservableObject
{
   private static readonly SystemFile _keyFile = new( "ShazamApiKey.json" );

   public static ApiSettings Load() => _keyFile.DeserializeContents<ApiSettings>() ?? new();

   private bool _keyChanged;

   public void Save()
   {
      if ( _keyChanged )
      {
         _keyFile.SerializeInto( this );
      }
   }

   public const string DefaultShazamApiKey = "<Placeholder>";

   [ObservableProperty]
   [NotifyPropertyChangedFor( nameof( UseDefaultKey ) )]
   private string _shazamApiKey;
   partial void OnShazamApiKeyChanged( string value ) => _keyChanged = true;

   public bool UseDefaultKey => string.IsNullOrWhiteSpace( _shazamApiKey );
}
