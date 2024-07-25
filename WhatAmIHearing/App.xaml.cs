using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using ZemotoCommon.UI;
using TrayIcon = System.Windows.Forms.NotifyIcon;

namespace WhatAmIHearing;

internal sealed partial class App : CommonApp
{
   private readonly Main _main;
   private readonly TrayIcon _trayIcon;

   public App()
      : base( Constants.InstanceName, listenForOtherInstances: true )
   {
      InitializeComponent();

      _main = new Main();

      _trayIcon = new TrayIcon { Icon = new System.Drawing.Icon( GetType(), "Icon.ico" ) };
      _trayIcon.MouseClick += OnTrayIconClicked;
      _trayIcon.ContextMenuStrip = new System.Windows.Forms.ContextMenuStrip();
      _ = _trayIcon.ContextMenuStrip.Items.Add( "Close", null, ( s, a ) => Shutdown() );
      _trayIcon.Visible = true;

      OverrideDefaultValues();
   }

   public override void Dispose()
   {
      _main?.Dispose();
      _trayIcon?.Dispose();
      base.Dispose();
   }

   protected override void OnStartup( StartupEventArgs e )
   {
      _main.Start();
      base.OnStartup( e );
   }

   protected override void OnExit( ExitEventArgs e )
   {
      AppSettings.Instance.Save();
      Api.ApiClient.StaticDispose();
      base.OnExit( e );
   }

   protected override void OnPingedByOtherProcess()
   {
      Dispatcher.Invoke( _main.ShowAndForegroundMainWindow );
      base.OnPingedByOtherProcess();
   }

   private void OnTrayIconClicked( object sender, System.Windows.Forms.MouseEventArgs e )
   {
      if ( e.Button is System.Windows.Forms.MouseButtons.Left )
      {
         _main.ShowAndForegroundMainWindow();
      }
   }

   private static void OverrideDefaultValues()
   {
      ToolTipService.ShowOnDisabledProperty.OverrideMetadata( typeof( FrameworkElement ), new FrameworkPropertyMetadata( true ) );
      ToolTipService.InitialShowDelayProperty.OverrideMetadata( typeof( FrameworkElement ), new FrameworkPropertyMetadata( 750 ) );
   }
}
