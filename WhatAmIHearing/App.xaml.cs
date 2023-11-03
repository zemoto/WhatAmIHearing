using System;
using System.Windows;
using ZemotoCommon;
using TrayIcon = System.Windows.Forms.NotifyIcon;

namespace WhatAmIHearing;

public sealed partial class App : IDisposable
{
   private readonly Main _main;
   private readonly GlobalHotkeyHook _globalHotkeyHook;
   private readonly TrayIcon _trayIcon;
   private readonly SingleInstance _singleInstance = new( Constants.InstanceName, listenForOtherInstances: true );

   public App()
   {
      if ( !_singleInstance.Claim() )
      {
         Shutdown();
         return;
      }

      InitializeComponent();

      _main = new Main();
      _globalHotkeyHook = new GlobalHotkeyHook();
      _trayIcon = new TrayIcon();

      _singleInstance.PingedByOtherProcess += ( s, a ) => Dispatcher.Invoke( _main.ShowAndForegroundMainWindow );

      _trayIcon.Icon = new System.Drawing.Icon( GetType(), "Icon.ico" );
      _trayIcon.MouseClick += OnTrayIconClicked;
      _trayIcon.ContextMenuStrip = new System.Windows.Forms.ContextMenuStrip();
      _ = _trayIcon.ContextMenuStrip.Items.Add( "Close", null, ( s, a ) => Shutdown() );
      _trayIcon.Visible = true;
   }

   public void Dispose()
   {
      _main.Dispose();
      _globalHotkeyHook.Dispose();
      _trayIcon.Dispose();
      _singleInstance.Dispose();
   }

   protected override void OnStartup( StartupEventArgs e )
   {
      bool hotkeyRegistered = _globalHotkeyHook.RegisterHotKey( ModifierKeys.Shift, System.Windows.Forms.Keys.F2 );
      if ( hotkeyRegistered )
      {
         _globalHotkeyHook.KeyPressed += _main.OnRecordHotkey;
      }

      _main.Start( hotkeyRegistered );
   }

   protected override void OnExit( ExitEventArgs e )
   {
      AppSettings.Instance.Save();
      Dispose();
   }

   private void OnTrayIconClicked( object sender, System.Windows.Forms.MouseEventArgs e )
   {
      if ( e.Button is System.Windows.Forms.MouseButtons.Left )
      {
         _main.ShowAndForegroundMainWindow();
      }
   }
}
