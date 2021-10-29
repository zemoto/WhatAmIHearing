using System.ComponentModel;
using System.Windows;

namespace WhatAmIHearing
{
   public partial class App
   {
      private readonly System.Windows.Forms.NotifyIcon _trayIcon = new() { Visible = false };
      private readonly GlobalHotkeyHook _globalHotkeyHook = new();
      private readonly MainViewModel _model = new();
      private readonly MainWindow _window;

      private bool _windowShownFromHotkey;

      private Properties.UserSettings Settings => WhatAmIHearing.Properties.UserSettings.Default;

      public App()
      {
         _window = new MainWindow( _model );
         _window.Closing += OnWindowClosing;

         _model.DetectionFinished += OnDetectionFinished;

         _trayIcon.Icon = new System.Drawing.Icon( "Icon.ico" );
         _trayIcon.MouseClick += OnTrayIconClicked;
         _trayIcon.ContextMenuStrip = new System.Windows.Forms.ContextMenuStrip();
         _ = _trayIcon.ContextMenuStrip.Items.Add( "Close", null, ( s, a ) => Shutdown() );

         _globalHotkeyHook.KeyPressed += OnRecordHotkey;
      }

      private void OnStartup( object sender, StartupEventArgs e )
      {
         if ( !SingleInstance.Claim() )
         {
            Shutdown();
            return;
         }

         SingleInstance.PingedBySecondProcess += ( s, a ) => Dispatcher.Invoke( ShowAndForegroundMainWindow );

         _model.HotkeyStatusText =
            _globalHotkeyHook.RegisterHotKey( ModifierKeys.Shift, System.Windows.Forms.Keys.F2 )
            ? "Shift + F2"
            : "Failed to register";

         if ( Settings.OpenHidden )
         {
            HideWindowAndShowTrayIcon();
         }
         else
         {
            ShowAndForegroundMainWindow();
         }
      }

      private void OnExit( object sender, ExitEventArgs e )
      {
         Settings.Save();
         _globalHotkeyHook.Dispose();
         _trayIcon.Dispose();
      }

      private void OnDetectionFinished( object sender, bool detectionSuccess )
      {
         if ( detectionSuccess && _windowShownFromHotkey )
         {
            HideWindowAndShowTrayIcon();
         }

         _windowShownFromHotkey = false;
      }

      private void OnWindowClosing( object sender, CancelEventArgs e )
      {
         if ( Settings.KeepOpenInTray )
         {
            e.Cancel = true;
            HideWindowAndShowTrayIcon();
         }
         else
         {
            Shutdown();
         }
      }

      private void OnTrayIconClicked( object sender, System.Windows.Forms.MouseEventArgs e )
      {
         if ( e.Button is System.Windows.Forms.MouseButtons.Left )
         {
            ShowAndForegroundMainWindow();
         }
      }

      private void OnRecordHotkey( object sender, KeyPressedEventArgs e )
      {
         if ( _model.RecorderState is State.Stopped )
         {
            if ( _trayIcon.Visible )
            {
               _windowShownFromHotkey = true;
            }

            ShowAndForegroundMainWindow();
            _model.RecordStopCommand.Execute( null );
         }
      }

      private void HideWindowAndShowTrayIcon()
      {
         _trayIcon.Visible = true;
         _window.ShowInTaskbar = false;
         _window.Hide();
      }

      private void ShowAndForegroundMainWindow()
      {
         _trayIcon.Visible = false;
         _window.ShowInTaskbar = true;
         _window.Show();
         _window.Activate();
      }
   }
}
