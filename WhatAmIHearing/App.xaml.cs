using System;
using System.ComponentModel;
using System.Windows;

namespace WhatAmIHearing
{
   public partial class App
   {
      private readonly System.Windows.Forms.NotifyIcon _trayIcon = new() { Visible = false };
      private readonly MainWindow _window = new();

      private Properties.UserSettings Settings => WhatAmIHearing.Properties.UserSettings.Default;

      public App()
      {
         _window.Closing += OnWindowClosing;

         _trayIcon.Icon = new System.Drawing.Icon( "Icon.ico" );
         _trayIcon.MouseClick += OnTrayIconClicked;

         _trayIcon.ContextMenuStrip = new System.Windows.Forms.ContextMenuStrip();
         _ = _trayIcon.ContextMenuStrip.Items.Add( "Close", null, OnTrayIconMenuClose );
      }

      private void OnStartup( object sender, StartupEventArgs e )
      {
         _ = _window.ShowDialog();

         Settings.Save();
         _trayIcon.Dispose();
      }

      private void OnWindowClosing( object sender, CancelEventArgs e )
      {
         if ( Settings.KeepOpenInTray )
         {
            e.Cancel = true;

            _trayIcon.Visible = true;
            _window.ShowInTaskbar = false;
            _window.WindowState = WindowState.Minimized;
         }
      }

      private void OnTrayIconClicked( object sender, System.Windows.Forms.MouseEventArgs e )
      {
         if ( e.Button is not System.Windows.Forms.MouseButtons.Left )
         {
            return;
         }

         _trayIcon.Visible = false;
         _window.ShowInTaskbar = true;
         _window.WindowState = WindowState.Normal;
         _window.Show();
      }

      private void OnTrayIconMenuClose( object sender, EventArgs e )
      {
         _window.Closing -= OnWindowClosing;
         _window.Close();
      }
   }
}
