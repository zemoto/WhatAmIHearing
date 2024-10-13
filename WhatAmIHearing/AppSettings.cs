using CommunityToolkit.Mvvm.ComponentModel;
using System.Text.Json.Serialization;
using System.Windows.Input;
using ZemotoCommon;

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
   private static readonly SystemFile _configFile = new( "config.json" );

   private static AppSettings _instance;
   public static AppSettings Instance => _instance ??= Load();

   private static AppSettings Load() => _configFile.DeserializeContents<AppSettings>() ?? new AppSettings();

   public AppSettings() => _launchOnWindowsStartup = WindowsStartup.GetStartupWithWindows();

   public void Save() => _configFile.SerializeInto( this );

   [ObservableProperty]
   private string _selectedDevice = Constants.DefaultDeviceName;

   [ObservableProperty]
   private bool _keepOpenInTray = true;

   [ObservableProperty]
   [property: JsonIgnore] // Don't write to settings file. Value depends on reg key.
   private bool _launchOnWindowsStartup;
   partial void OnLaunchOnWindowsStartupChanged( bool oldValue, bool newValue )
   {
      if ( !WindowsStartup.SetStartupWithWindows( newValue ) )
      {
         _launchOnWindowsStartup = oldValue;
      }
   }

   [ObservableProperty]
   private bool _openHidden;

   [ObservableProperty]
   private bool _keepWindowTopmost;

   [ObservableProperty]
   private bool _putTitleOnClipboard;

   [ObservableProperty]
   private bool _openShazamOnResultFound;

   [ObservableProperty]
   private bool _hideWindowAfterRecord;

   [ObservableProperty]
   private ProgressDisplayType _progressType = ProgressDisplayType.Seconds;

   [ObservableProperty]
   private Hotkey _recordHotkey = new( Key.F2, ModifierKeys.Shift );

   [ObservableProperty]
   private double _historyHeight = 80;
}
