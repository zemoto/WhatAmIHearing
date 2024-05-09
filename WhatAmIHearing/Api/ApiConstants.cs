using System.IO;
using System.Text.Json;

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
   public const string SpotifyClientId = "<Placeholder>";
   public const string SpotifyClientSecret = "<Placeholder>";
   private const string DefaultShazamApiKey = "<Placeholder>";
   private const string _keyFileName = "ShazamApiKey.json";
   public static string GetShazamApiKey()
   {
      if ( !File.Exists( _keyFileName ) )
      {
         return DefaultShazamApiKey;
      }

      var configString = File.ReadAllText( _keyFileName );
      var customKey = JsonSerializer.Deserialize<CustomShazamApiKey>( configString );
      return customKey.IsValidKey() ? customKey.ShazamApiKey : DefaultShazamApiKey;
   }
}
