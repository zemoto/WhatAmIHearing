using ZemotoCommon;

namespace WhatAmIHearing;

internal sealed class LegacyLoader
{
   private static readonly SystemFile _legacyKeyFile = new( "ShazamApiKey.json" );

   public static void LoadLegacyDataIntoAppSettings()
   {
      if ( !_legacyKeyFile.Exists() )
      {
         return;
      }

      var keyData = _legacyKeyFile.DeserializeContents<ApiKeyData>();
      if ( keyData is not null )
      {
         _legacyKeyFile.Delete();

         var appSettings = AppSettings.Instance;
         appSettings.KeyData = keyData;
         appSettings.Save();
      }
   }
}
