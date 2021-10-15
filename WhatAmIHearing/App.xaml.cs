using System;
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
         _model.DetectionFinished += OnDetectionFinished;

         _window = new MainWindow( _model );
         _window.Closing += OnWindowClosing;

         _trayIcon.Icon = new System.Drawing.Icon( "Icon.ico" );
         _trayIcon.MouseClick += OnTrayIconClicked;
         _trayIcon.ContextMenuStrip = new System.Windows.Forms.ContextMenuStrip();
         _ = _trayIcon.ContextMenuStrip.Items.Add( "Close", null, ( s, a ) => Shutdown() );

         _globalHotkeyHook.KeyPressed += OnRecordHotkey;
      }

      private void OnStartup( object sender, StartupEventArgs e )
      {
         _model.HotkeyStatusText =
            _globalHotkeyHook.RegisterHotKey( ModifierKeys.Shift, System.Windows.Forms.Keys.F2 )
            ? "Shift + F2"
            : "Failed to register";

         _window.Show();
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
            HideWindow();
         }

         _windowShownFromHotkey = false;
      }

      private void OnWindowClosing( object sender, CancelEventArgs e )
      {
         if ( Settings.KeepOpenInTray )
         {
            e.Cancel = true;

            HideWindow();
         }
         else
         {
            Shutdown();
         }
      }

      private void OnTrayIconClicked( object sender, System.Windows.Forms.MouseEventArgs e )
      {
         if ( e.Button is not System.Windows.Forms.MouseButtons.Left )
         {
            return;
         }

         ShowAndForegroundMainWindow( false );
      }

      private void OnRecordHotkey( object sender, KeyPressedEventArgs e )
      {
         if ( !_model.Recording )
         {
            ShowAndForegroundMainWindow( true );
            _model.RecordStopCommand.Execute( null );
         }
      }

      private void HideWindow()
      {
         _trayIcon.Visible = true;
         _window.ShowInTaskbar = false;
         _window.WindowState = WindowState.Minimized;
         _window.Hide();
      }

      private void ShowAndForegroundMainWindow( bool showFromHotkey )
      {
         _windowShownFromHotkey = showFromHotkey;

         _trayIcon.Visible = false;

         _window.Show();
         _window.ShowInTaskbar = true;
         _window.WindowState = WindowState.Normal;
         _window.Activate();
      }
   }
}
