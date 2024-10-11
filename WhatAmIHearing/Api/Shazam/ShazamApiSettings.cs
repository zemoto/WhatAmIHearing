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
   private string _shazamApiKey;
   partial void OnShazamApiKeyChanged( string value ) => _keyChanged = true;
}
