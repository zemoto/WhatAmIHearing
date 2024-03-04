using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;

namespace WhatAmIHearing;

internal sealed partial class MainWindow
{
   [DllImport( "user32.dll" )]
   [DefaultDllImportSearchPaths( DllImportSearchPath.System32 )]
   private static extern bool RegisterHotKey( IntPtr hWnd, int id, uint fsModifiers, uint vk );

   [DllImport( "user32.dll" )]
   [DefaultDllImportSearchPaths( DllImportSearchPath.System32 )]
   private static extern bool UnregisterHotKey( IntPtr hWnd, int id );

   private const int _recordingHotkeyId = 1;

   public event EventHandler RecordHotkeyPressed;

   private bool _recordHotkeyRegistered;
   private IntPtr _handle;

   public MainWindow( MainViewModel model )
   {
      DataContext = model;
      InitializeComponent();
   }

   public bool RegisterRecordHotkey()
   {
      if ( _recordHotkeyRegistered )
      {
         return true;
      }

      var helper = new WindowInteropHelper( this );
      helper.EnsureHandle();

      _handle = helper.Handle;
      _recordHotkeyRegistered = RegisterHotKey( _handle, _recordingHotkeyId, (uint)ModifierKeys.Shift, (uint)KeyInterop.VirtualKeyFromKey( Key.F2 ) );
      if ( _recordHotkeyRegistered )
      {
         var source = HwndSource.FromHwnd( _handle );
         source?.AddHook( WndProc );
      }

      return _recordHotkeyRegistered;
   }

   private void OnCloseClicked( object s, RoutedEventArgs e ) => Close();

   private IntPtr WndProc( IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled )
   {
      if ( msg == 0x0312/*WM_HOTKEY*/ )
      {
         RecordHotkeyPressed?.Invoke( this, EventArgs.Empty );
         handled = true;
      }

      return IntPtr.Zero;
   }

   protected override void OnClosed( EventArgs e )
   {
      if ( _recordHotkeyRegistered )
      {
         _ = UnregisterHotKey( _handle, _recordingHotkeyId );
      }

      base.OnClosed( e );
   }
}