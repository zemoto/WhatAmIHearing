using System.Windows;
using WhatAmIHearing.Utils;
using ZemotoCommon;
using TrayIcon = System.Windows.Forms.NotifyIcon;

namespace WhatAmIHearing
{
   public partial class App
   {
      private Main _main;
      private GlobalHotkeyHook _globalHotkeyHook;
      private TrayIcon _trayIcon;

      private const string InstanceName = "WhatAmIHearingInstance";
      private readonly SingleInstance _singleInstance = new( InstanceName, listenForOtherInstances: true );

      protected override void OnStartup( StartupEventArgs e )
      {
         if ( !_singleInstance.Claim() )
         {
            Shutdown();
            return;
         }

         _main = new Main();
         _globalHotkeyHook = new GlobalHotkeyHook();
         _trayIcon = new TrayIcon();

         _singleInstance.PingedByOtherProcess += ( s, a ) => Dispatcher.Invoke( _main.ShowAndForegroundMainWindow );

         _trayIcon.Icon = new System.Drawing.Icon( "Icon.ico" );
         _trayIcon.MouseClick += OnTrayIconClicked;
         _trayIcon.ContextMenuStrip = new System.Windows.Forms.ContextMenuStrip();
         _ = _trayIcon.ContextMenuStrip.Items.Add( "Close", null, ( s, a ) => Shutdown() );
         _trayIcon.Visible = true;

         bool hotkeyRegistered = _globalHotkeyHook.RegisterHotKey( ModifierKeys.Shift, System.Windows.Forms.Keys.F2 );
         if ( hotkeyRegistered )
         {
            _globalHotkeyHook.KeyPressed += _main.OnRecordHotkey;
         }

         _main.Start( hotkeyRegistered );
      }

      protected override void OnExit( ExitEventArgs e )
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
