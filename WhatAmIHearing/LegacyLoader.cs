using System.Collections.ObjectModel;
using WhatAmIHearing.Result;
using ZemotoCommon;

namespace WhatAmIHearing;

internal sealed class LegacyLoader
{
   private static readonly SystemFile _legacyKeyFile = new( "ShazamApiKey.json" );
   private static readonly SystemFile _legacyHistoryFile = new( "history.json" );

   public static void LoadLegacyDataIntoAppSettings()
   {
      if ( !_legacyKeyFile.Exists() && !_legacyHistoryFile.Exists() )
      {
         return;
      }

      var appSettings = AppSettings.Instance;
      var keyData = _legacyKeyFile.DeserializeContents<ApiKeyData>();
      if ( keyData is not null )
      {
         _legacyKeyFile.MoveTo( _legacyKeyFile.FullPath + ".backup", false, out _ );
         appSettings.KeyData = keyData;
      }

      var history = _legacyHistoryFile.DeserializeContents<ObservableCollection<SongViewModel>>();
      if ( history is not null )
      {
         _legacyHistoryFile.MoveTo( _legacyHistoryFile.FullPath + ".backup", false, out _ );
         appSettings.History = history;
      }

      appSettings.Save();
   }
}
