// Based on https://stackoverflow.com/a/27309185
using System;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace WhatAmIHearing
{
   [Flags]
   internal enum ModifierKeys : uint
   {
      None = 0,
      Alt = 1,
      Control = 1 << 1,
      Shift = 1 << 2,
      Win = 1 << 3
   }

   internal sealed class KeyPressedEventArgs : EventArgs
   {
      public ModifierKeys Modifier { get; }
      public Keys Key { get; }

      internal KeyPressedEventArgs( ModifierKeys modifier, Keys key )
      {
         Modifier = modifier;
         Key = key;
      }
   }

   internal sealed class GlobalHotkeyHook : IDisposable
   {
      [DllImport( "user32.dll" )]
      private static extern bool RegisterHotKey( IntPtr hWnd, int id, uint fsModifiers, uint vk );
      [DllImport( "user32.dll" )]
      private static extern bool UnregisterHotKey( IntPtr hWnd, int id );

      private sealed class MessageReceivingWindow : NativeWindow, IDisposable
      {
         private const int WM_HOTKEY = 0x0312;

         public MessageReceivingWindow() => CreateHandle( new CreateParams() );

         public void Dispose() => DestroyHandle();

         protected override void WndProc( ref Message m )
         {
            base.WndProc( ref m );

            if ( m.Msg == WM_HOTKEY )
            {
               Keys key = (Keys)( ( (int)m.LParam >> 16 ) & 0xFFFF );
               ModifierKeys modifier = (ModifierKeys)( (int)m.LParam & 0xFFFF );

               KeyPressed?.Invoke( this, new KeyPressedEventArgs( modifier, key ) );
            }
         }

         public event EventHandler<KeyPressedEventArgs> KeyPressed;
      }

      private readonly MessageReceivingWindow _window = new ();
      private int _currentHotkeyId;

      public bool RegisterHotKey( ModifierKeys modifier, Keys key )
      {
         _currentHotkeyId++;
         return RegisterHotKey( _window.Handle, _currentHotkeyId, (uint)modifier, (uint)key );
      }

      public event EventHandler<KeyPressedEventArgs> KeyPressed
      {
         add => _window.KeyPressed += value;
         remove => _window.KeyPressed -= value;
      }

      public void Dispose()
      {
         for ( int i = _currentHotkeyId; i > 0; i-- )
         {
            _ = UnregisterHotKey( _window.Handle, i );
         }

         _window.Dispose();
      }
   }
}
