using CommunityToolkit.Mvvm.ComponentModel;
using ZemotoCommon;

namespace WhatAmIHearing.Shazam;

internal sealed partial class ApiViewModel : ObservableObject
{
   private static readonly SystemFile _keyFile = new( "ShazamApiKey.json" );

   public static ApiViewModel Load() => _keyFile.DeserializeContents<ApiViewModel>() ?? new();

   public void Save()
   {
      if ( !UseDefaultKey )
      {
         _keyFile.SerializeInto( this );
      }
   }

   public const string DefaultShazamApiKey = "<Placeholder>";

   [ObservableProperty]
   [NotifyPropertyChangedFor( nameof( UseDefaultKey ) )]
   [NotifyPropertyChangedFor( nameof( CanDisplayQuotaData ) )]
   private string _shazamApiKey;
   partial void OnShazamApiKeyChanged( string value )
   {
      QuotaLimit = 0;
      QuotaRemaining = 0;
   }

   public bool UseDefaultKey => string.IsNullOrWhiteSpace( _shazamApiKey );

   [ObservableProperty]
   [NotifyPropertyChangedFor( nameof( QuotaUsed ) )]
   [NotifyPropertyChangedFor( nameof( CanDisplayQuotaData ) )]
   private int _quotaLimit;

   [ObservableProperty]
   [NotifyPropertyChangedFor( nameof( QuotaUsed ) )]
   [NotifyPropertyChangedFor( nameof( CanDisplayQuotaData ) )]
   private int _quotaRemaining;

   public int QuotaUsed => _quotaLimit - _quotaRemaining;

   public bool CanDisplayQuotaData => !UseDefaultKey && _quotaLimit > 0 && _quotaRemaining >= 0;
}
