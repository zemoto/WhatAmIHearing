using System.Windows;
using WhatAmIHearing.Utils;

namespace WhatAmIHearing
{
   public partial class App
   {
      private readonly Main _main = new();
      private readonly GlobalHotkeyHook _globalHotkeyHook = new();
      private readonly System.Windows.Forms.NotifyIcon _trayIcon = new();

      private void OnStartup( object sender, StartupEventArgs e )
      {
         if ( !SingleInstance.Claim() )
         {
            Shutdown();
            return;
         }

         SingleInstance.PingedBySecondProcess += ( s, a ) => Dispatcher.Invoke( _main.ShowAndForegroundMainWindow );

         _globalHotkeyHook.KeyPressed += _main.OnRecordHotkey;

         _trayIcon.Visible = true;
         _trayIcon.Icon = new System.Drawing.Icon( "Icon.ico" );
         _trayIcon.MouseClick += OnTrayIconClicked;
         _trayIcon.ContextMenuStrip = new System.Windows.Forms.ContextMenuStrip();
         _ = _trayIcon.ContextMenuStrip.Items.Add( "Close", null, ( s, a ) => Shutdown() );

         bool hotkeyRegistered = _globalHotkeyHook.RegisterHotKey( ModifierKeys.Shift, System.Windows.Forms.Keys.F2 );
         _main.Start( hotkeyRegistered );
      }

      private void OnExit( object sender, ExitEventArgs e )
      {
         WhatAmIHearing.Properties.UserSettings.Default.Save();
         _trayIcon.Dispose();
         _globalHotkeyHook.Dispose();
      }

      private void OnTrayIconClicked( object sender, System.Windows.Forms.MouseEventArgs e )
      {
         if ( e.Button is System.Windows.Forms.MouseButtons.Left )
         {
            _main.ShowAndForegroundMainWindow();
         }
      }
   }
}
