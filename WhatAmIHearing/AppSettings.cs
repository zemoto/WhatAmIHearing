using System;
using System.IO;
using System.Reflection;
using System.Text.Json;
using System.Windows.Input;
using ZemotoCommon.UI;

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

internal sealed class AppSettings : ViewModelBase
{
   private const string _configName = "config.json";
   private static readonly string _configFilePath = Path.Combine( Path.GetDirectoryName( Assembly.GetExecutingAssembly().Location ), _configName );

   private static AppSettings _instance;
   public static AppSettings Instance => _instance ??= Load();

   private static AppSettings Load()
   {
      if ( !File.Exists( _configFilePath ) )
      {
         return new();
      }

      var configString = File.ReadAllText( _configFilePath );
      return JsonSerializer.Deserialize<AppSettings>( configString );
   }

   public void Save()
   {
      var configJson = JsonSerializer.Serialize( this );
      File.WriteAllText( _configFilePath, configJson );
   }

   private string _selectedDevice = Constants.DefaultDeviceName;
   public string SelectedDevice
   {
      get => _selectedDevice;
      set => SetProperty( ref _selectedDevice, value );
   }

   private bool _keepOpenInTray = true;
   public bool KeepOpenInTray
   {
      get => _keepOpenInTray;
      set => SetProperty( ref _keepOpenInTray, value );
   }

   private bool _openHidden;
   public bool OpenHidden
   {
      get => _openHidden;
      set => SetProperty( ref _openHidden, value );
   }

   private bool _keepWindowTopmost;
   public bool KeepWindowTopmost
   {
      get => _keepWindowTopmost;
      set => SetProperty( ref _keepWindowTopmost, value );
   }

   private bool _hideWindowAfterRecord;
   public bool HideWindowAfterRecord
   {
      get => _hideWindowAfterRecord;
      set => SetProperty( ref _hideWindowAfterRecord, value );
   }

   private ProgressDisplayType _progressType = ProgressDisplayType.Seconds;
   public ProgressDisplayType ProgressType
   {
      get => _progressType;
      set => SetProperty( ref _progressType, value );
   }

   private string _spotifyAccessToken;
   public string SpotifyAccessToken
   {
      get => _spotifyAccessToken;
      set => SetProperty( ref _spotifyAccessToken, value );
   }

   private string _spotifyRefreshToken;
   public string SpotifyRefreshToken
   {
      get => _spotifyRefreshToken;
      set => SetProperty( ref _spotifyRefreshToken, value );
   }

   private DateTime _spotifyExpirationTimeUtc;
   public DateTime SpotifyExpirationTimeUtc
   {
      get => _spotifyExpirationTimeUtc;
      set => SetProperty( ref _spotifyExpirationTimeUtc, value );
   }

   private bool _addSongsToSpotifyPlaylist;
   public bool AddSongsToSpotifyPlaylist
   {
      get => _addSongsToSpotifyPlaylist;
      set => SetProperty( ref _addSongsToSpotifyPlaylist, value );
   }

   private Hotkey _recordHotkey = new( Key.F2, ModifierKeys.Shift );
   public Hotkey RecordHotkey
   {
      get => _recordHotkey;
      set => SetProperty( ref _recordHotkey, value );
   }
}
