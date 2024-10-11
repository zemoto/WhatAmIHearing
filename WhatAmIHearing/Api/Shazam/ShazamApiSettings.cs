using CommunityToolkit.Mvvm.ComponentModel;
using ZemotoCommon;

namespace WhatAmIHearing.Api.Shazam;

internal sealed partial class ShazamApiSettings : ObservableObject
{
   private static readonly SystemFile _keyFile = new( "ShazamApiKey.json" );

   public static ShazamApiSettings Load() => _keyFile.DeserializeContents<ShazamApiSettings>() ?? new();

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
   [NotifyPropertyChangedFor( nameof( CustomKeyIsValid ) )]
   private string _shazamApiKey;
   partial void OnShazamApiKeyChanged( string value ) => _keyChanged = true;

   // Just a random loose requirement so you can easily stop using a custom key
   public bool CustomKeyIsValid => _shazamApiKey is not null && _shazamApiKey.Length > 46 && _shazamApiKey.Length < 54 && !_shazamApiKey.Contains( ' ' );
}
