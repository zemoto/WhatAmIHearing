using System;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;
using ZemotoCommon.UI;

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
   private readonly IntPtr _handle;

   public MainWindow( MainViewModel model )
   {
      DataContext = model;
      DwmHelper.EnableDwmManagementOfWindow( this );
      InitializeComponent();

      var helper = new WindowInteropHelper( this );
      _ = helper.EnsureHandle();
      _handle = helper.Handle;

      var source = HwndSource.FromHwnd( _handle );
      source?.AddHook( WndProc );
   }

   public new void Hide()
   {
      ShowInTaskbar = false;
      base.Hide();
   }

   public bool RegisterRecordHotkey( Hotkey hotkey, out string error )
   {
      if ( !UnregisterRecordHotkey( out error ) )
      {
         return false;
      }

      if ( hotkey.IsNone() )
      {
         return true;
      }

      _recordHotkeyRegistered = RegisterHotKey( _handle, _recordingHotkeyId, (uint)hotkey.Modifiers, (uint)KeyInterop.VirtualKeyFromKey( hotkey.Key ) );
      if ( !_recordHotkeyRegistered )
      {
         error = "Failed to register hotkey";
      }
      return _recordHotkeyRegistered;
   }

   public void FocusCustomApiKeyTextBox() => CustomApiKeyTextBox.Focus();

   private bool UnregisterRecordHotkey( out string error )
   {
      error = string.Empty;
      if ( _recordHotkeyRegistered && !UnregisterHotKey( _handle, _recordingHotkeyId ) )
      {
         error = "Failed to unregister hotkey";
         return false;
      }

      _recordHotkeyRegistered = false;
      return true;
   }

   private void OnCloseClicked( object s, RoutedEventArgs e ) => Close();

   private IntPtr WndProc( IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled )
   {
      if ( msg == 0x0312/*WM_HOTKEY*/ )
      {
         if ( RecordHotkeyControl.IsKeyboardFocused )
         {
            // Take focus off the hotkey control if the user hits the already registered hotkey
            FocusManager.SetFocusedElement( this, this );
         }
         else
         {
            RecordHotkeyPressed?.Invoke( this, EventArgs.Empty );
         }

         handled = true;
      }

      return IntPtr.Zero;
   }

   protected override void OnClosing( CancelEventArgs e )
   {
      if ( AppSettings.Instance.KeepOpenInTray )
      {
         e.Cancel = true;
         Hide();
      }
      base.OnClosing( e );
   }

   protected override void OnClosed( EventArgs e )
   {
      _ = UnregisterRecordHotkey( out _ );
      base.OnClosed( e );
   }
}