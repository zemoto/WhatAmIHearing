using CommunityToolkit.Mvvm.ComponentModel;
using System.Text.Json.Serialization;
using ZemotoCommon;

namespace WhatAmIHearing.Shazam;

internal sealed partial class ApiViewModel : ObservableObject
{
   private static readonly SystemFile _keyFile = new( "ShazamApiKey.json" );

   public static ApiViewModel Load() => _keyFile.DeserializeContents<ApiViewModel>() ?? new();

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

   [ObservableProperty]
   [NotifyPropertyChangedFor( nameof( QuotaUsed ) )]
   [NotifyPropertyChangedFor( nameof( HaveQuotaData ) )]
   [property: JsonIgnore]
   private int _quotaLimit;

   [ObservableProperty]
   [NotifyPropertyChangedFor( nameof( QuotaUsed ) )]
   [NotifyPropertyChangedFor( nameof( HaveQuotaData ) )]
   [property: JsonIgnore]
   private int _quotaRemaining;

   public int QuotaUsed => _quotaLimit - _quotaRemaining;

   public bool HaveQuotaData => _quotaLimit > 0 && _quotaRemaining >= 0;
}
