using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.IO;
using System.Text.Json;
using System.Windows.Input;

namespace WhatAmIHearing;

internal enum ProgressDisplayType
{
   None,
   Bytes,
   Seconds
}

internal readonly struct Hotkey( Key key, ModifierKeys modifiers )
{
   public Key Key { get; init; } = key;
   public ModifierKeys Modifiers { get; init; } = modifiers;

   public bool IsNone() => Key is Key.None;
}

internal sealed partial class AppSettings : ObservableObject
{
   private const string _configFileName = "config.json";

   private static AppSettings _instance;
   public static AppSettings Instance => _instance ??= Load();

   private static AppSettings Load()
   {
      if ( !File.Exists( _configFileName ) )
      {
         return new AppSettings();
      }

      var configString = File.ReadAllText( _configFileName );
      return JsonSerializer.Deserialize<AppSettings>( configString );
   }

   public void Save()
   {
      var configJson = JsonSerializer.Serialize( this );
      File.WriteAllText( _configFileName, configJson );
   }

   [ObservableProperty]
   private string _selectedDevice = Constants.DefaultDeviceName;

   [ObservableProperty]
   private bool _keepOpenInTray = true;

   [ObservableProperty]
   private bool _openHidden;

   [ObservableProperty]
   private bool _keepWindowTopmost;

   [ObservableProperty]
   private bool _hideWindowAfterRecord;

   [ObservableProperty]
   private ProgressDisplayType _progressType = ProgressDisplayType.Seconds;

   [ObservableProperty]
   private string _spotifyAccessToken;

   [ObservableProperty]
   private string _spotifyRefreshToken;

   [ObservableProperty]
   private DateTime _spotifyExpirationTimeUtc;

   [ObservableProperty]
   private bool _addSongsToSpotifyPlaylist;

   [ObservableProperty]
   private Hotkey _recordHotkey = new( Key.F2, ModifierKeys.Shift );
}
