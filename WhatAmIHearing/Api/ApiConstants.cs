using ZemotoCommon;

namespace WhatAmIHearing.Api;

internal sealed class CustomShazamApiKey
{
   public string ShazamApiKey { get; set; }

   // Just a random loose requirement so you can easily stop using a custom key
   public bool IsValidKey()
   {
      return ShazamApiKey.Length > 46 && ShazamApiKey.Length < 54 && !ShazamApiKey.Contains( ' ' );
   }
}

internal static class ApiConstants
{
   private const string DefaultShazamApiKey = "<Placeholder>";

   private static readonly SystemFile _keyFile = new( "ShazamApiKey.json" );

   public static string GetShazamApiKey()
   {
      var customKey = _keyFile.DeserializeContents<CustomShazamApiKey>();
      return customKey?.IsValidKey() == true ? customKey.ShazamApiKey : DefaultShazamApiKey;
   }
}
