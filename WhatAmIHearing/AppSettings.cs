using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.IO;
using System.Text.Json.Serialization;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using WhatAmIHearing.Properties;
using WhatAmIHearing.Result;
using ZemotoCommon;

namespace WhatAmIHearing;

internal enum ProgressDisplayType
{
   None,
   Bytes,
   Seconds
}

internal enum StopBehaviorType
{
   [LocalizedDescription( "CancelStopBehavior", typeof( Resources ) )]
   Cancel,
   [LocalizedDescription( "SendStopBehavior", typeof( Resources ) )]
   Send,
}

internal readonly struct Hotkey( Key key, ModifierKeys modifiers )
{
   public Key Key { get; init; } = key;
   public ModifierKeys Modifiers { get; init; } = modifiers;

   public bool IsNone() => Key is Key.None;
}

internal sealed partial class AppSettings : ObservableObject
{
   private const string _settingsFileName = "config.json";
   private static readonly string _appDataFolderPath = Path.Combine( Environment.GetFolderPath( Environment.SpecialFolder.ApplicationData ), Constants.AppName );
   private static readonly string _localFolderPath = AppContext.BaseDirectory;

   private static SystemFile _configFile;

   static AppSettings()
   {
      bool? saveSettingsInAppData;
      _configFile = new( Path.Combine( _appDataFolderPath, _settingsFileName ) );
      if ( _configFile.Exists() )
      {
         saveSettingsInAppData = true;
      }
      else
      {
         _configFile = new( Path.Combine( _localFolderPath, _settingsFileName ) );
         saveSettingsInAppData = false;
      }

      Instance = _configFile.DeserializeContents<AppSettings>() ?? new AppSettings();
      Instance.SaveSettingsInAppData = saveSettingsInAppData;
      Instance._passiveSavingEnabled = true;
   }

   public static AppSettings Instance { get; }

   private readonly DispatcherTimer _passiveSaveTimer;
   private bool _passiveSavingEnabled = false;

   public AppSettings()
   {
      LaunchOnWindowsStartup = WindowsStartup.GetStartupWithWindows();

      _passiveSaveTimer = new DispatcherTimer( DispatcherPriority.Normal, Application.Current.Dispatcher ) { Interval = TimeSpan.FromSeconds( 3 ) };
      _passiveSaveTimer.Tick += OnSaveTimerTick;

      KeyData.ApiKeyChanged += OnApiKeyChanged;
      History.CollectionChanged += OnHistoryCollectionChanged;
   }

   public void Save()
   {
      _passiveSaveTimer.Stop();
      _ = _configFile.SerializeInto( this );
   }

   private void PassiveSave()
   {
      if ( _passiveSavingEnabled )
      {
         _passiveSaveTimer.Start();
      }
   }

   private void OnSaveTimerTick( object? sender, EventArgs e ) => Save();

   [ObservableProperty]
   public partial string SelectedDevice { get; set; } = Resources.DefaultOutputDeviceName;

   [ObservableProperty]
   public partial bool KeepOpenInTray { get; set; } = true;

   [JsonIgnore] // Don't write to settings file. Value depends whether there is a settings file in AppData or not.
   public bool? SaveSettingsInAppData
   {
      get;
      set
      {
         if ( !field.HasValue || value is null )
         {
            field = value;
            return;
         }

         if ( field.Value != value.Value )
         {
            var newFolder = value.Value ? _appDataFolderPath : _localFolderPath;
            UtilityMethods.CreateDirectory( newFolder );
            if ( _configFile.MoveTo( Path.Combine( newFolder, _settingsFileName ), overwrite: true, out var newConfigFile ) )
            {
               _configFile = newConfigFile;
               field = value;
               OnPropertyChanged( nameof( SaveSettingsInAppData ) );
            }
            else
            {
               _ = MessageBox.Show( Resources.FailedToMoveSettingsFileErrorText, Resources.Error, MessageBoxButton.OK, MessageBoxImage.Error );
            }
         }
      }
   }

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
   public partial StopBehaviorType StopBehavior { get; set; } = StopBehaviorType.Cancel;

   [ObservableProperty]
   public partial Hotkey RecordHotkey { get; set; } = new( Key.F2, ModifierKeys.Shift );

   [ObservableProperty]
   public partial double HistoryHeight { get; set; } = 80;

   [ObservableProperty]
   public partial ApiKeyData KeyData { get; set; } = new();
   partial void OnKeyDataChanged( ApiKeyData oldValue, ApiKeyData newValue )
   {
      oldValue?.ApiKeyChanged -= OnApiKeyChanged;
      newValue?.ApiKeyChanged += OnApiKeyChanged;
   }
   private void OnApiKeyChanged() => PassiveSave();

   [ObservableProperty]
   public partial ObservableCollection<SongViewModel> History { get; set; } = [];
   partial void OnHistoryChanged( ObservableCollection<SongViewModel> oldValue, ObservableCollection<SongViewModel> newValue )
   {
      oldValue?.CollectionChanged -= OnHistoryCollectionChanged;
      newValue?.CollectionChanged += OnHistoryCollectionChanged;
   }
   private void OnHistoryCollectionChanged( object? sender, NotifyCollectionChangedEventArgs e ) => PassiveSave();
}

internal sealed partial class ApiKeyData : ObservableObject
{
   public const string DefaultShazamApiKey = "<Placeholder>";

   public event Action? ApiKeyChanged;

   [ObservableProperty]
   [NotifyPropertyChangedFor( nameof( UseDefaultKey ) )]
   [NotifyPropertyChangedFor( nameof( CanDisplayQuotaData ) )]
   public partial string ShazamApiKey { get; set; } = string.Empty;
   partial void OnShazamApiKeyChanged( string value )
   {
      QuotaLimit = 0;
      QuotaUsed = 0;
      ApiKeyChanged?.Invoke();
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
