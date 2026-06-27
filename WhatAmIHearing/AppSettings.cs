using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.ObjectModel;
using System.Text.Json.Serialization;
using System.Windows.Input;
using WhatAmIHearing.Result;
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

   public static AppSettings Instance => field ??= _configFile.DeserializeContents<AppSettings>() ?? new AppSettings();

   public AppSettings() => LaunchOnWindowsStartup = WindowsStartup.GetStartupWithWindows();

   public void Save() => _configFile.SerializeInto( this );

   [ObservableProperty]
   public partial string SelectedDevice { get; set; } = Constants.DefaultOutputDeviceName;

   [ObservableProperty]
   public partial bool KeepOpenInTray { get; set; } = true;

   [ObservableProperty]
   [JsonIgnore] // Don't write to settings file. Value depends on reg key.
   public partial bool LaunchOnWindowsStartup { get; set; }
   partial void OnLaunchOnWindowsStartupChanged( bool oldValue, bool newValue )
   {
      if ( !WindowsStartup.SetStartupWithWindows( newValue ) )
      {
         LaunchOnWindowsStartup = oldValue;
      }
   }

   [ObservableProperty]
   public partial bool OpenHidden { get; set; }

   [ObservableProperty]
   public partial bool KeepWindowTopmost { get; set; }

   [ObservableProperty]
   public partial bool DisplayInputDevices { get; set; }

   [ObservableProperty]
   public partial bool PutTitleOnClipboard { get; set; }

   [ObservableProperty]
   public partial bool OpenShazamOnResultFound { get; set; }

   [ObservableProperty]
   public partial bool OpenSpotifyLinksInApp { get; set; } = true;

   [ObservableProperty]
   public partial bool HideWindowAfterRecord { get; set; }

   [ObservableProperty]
   public partial ProgressDisplayType ProgressType { get; set; } = ProgressDisplayType.Seconds;

   [ObservableProperty]
   public partial Hotkey RecordHotkey { get; set; } = new( Key.F2, ModifierKeys.Shift );

   [ObservableProperty]
   public partial double HistoryHeight { get; set; } = 80;

   [ObservableProperty]
   public partial ApiKeyData KeyData { get; set; } = new();

   [ObservableProperty]
   public partial ObservableCollection<SongViewModel> History { get; set; } = [];
}

internal sealed partial class ApiKeyData : ObservableObject
{
   public const string DefaultShazamApiKey = "<Placeholder>";

   [ObservableProperty]
   [NotifyPropertyChangedFor( nameof( UseDefaultKey ) )]
   [NotifyPropertyChangedFor( nameof( CanDisplayQuotaData ) )]
   public partial string ShazamApiKey { get; set; }

   partial void OnShazamApiKeyChanged( string value )
   {
      QuotaLimit = 0;
      QuotaUsed = 0;
   }

   public bool UseDefaultKey => string.IsNullOrWhiteSpace( ShazamApiKey );

   [ObservableProperty]
   [NotifyPropertyChangedFor( nameof( CanDisplayQuotaData ) )]
   public partial int QuotaLimit { get; set; } = -1;

   [ObservableProperty]
   [NotifyPropertyChangedFor( nameof( CanDisplayQuotaData ) )]
   public partial int QuotaUsed { get; set; } = -1;

   public bool CanDisplayQuotaData => !UseDefaultKey && QuotaLimit > 0 && QuotaUsed >= 0;
}
