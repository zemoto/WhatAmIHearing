using CommunityToolkit.Mvvm.Input;
using System.Text;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

namespace WhatAmIHearing;

internal sealed partial class HotkeyControl
{
   public static readonly DependencyProperty HotkeyProperty = DependencyProperty.Register( nameof( Hotkey ), typeof( Key ), typeof( HotkeyControl ), new PropertyMetadata( Key.None, OnHotkeyChanged ) );
   public Key Hotkey
   {
      get => (Key)GetValue( HotkeyProperty );
      set => SetValue( HotkeyProperty, value );
   }
   public static readonly DependencyProperty HotkeyModifiersProperty = DependencyProperty.Register( nameof( HotkeyModifiers ), typeof( ModifierKeys ), typeof( HotkeyControl ), new PropertyMetadata( ModifierKeys.None, OnHotkeyChanged ) );
   public ModifierKeys HotkeyModifiers
   {
      get => (ModifierKeys)GetValue( HotkeyModifiersProperty );
      set => SetValue( HotkeyModifiersProperty, value );
   }
   private static void OnHotkeyChanged( DependencyObject d, DependencyPropertyChangedEventArgs e ) => ( (HotkeyControl)d ).DisplayHotkey();

   public static readonly DependencyProperty ErrorMessageProperty = DependencyProperty.Register( nameof( ErrorMessage ), typeof( string ), typeof( HotkeyControl ), new PropertyMetadata( string.Empty ) );
   public string ErrorMessage
   {
      get => (string)GetValue( ErrorMessageProperty );
      set => SetValue( ErrorMessageProperty, value );
   }

   public static readonly DependencyProperty SetHotkeyCommandProperty = DependencyProperty.Register( nameof( SetHotkeyCommand ), typeof( RelayCommand<Hotkey> ), typeof( HotkeyControl ), new PropertyMetadata( null ) );
   public RelayCommand<Hotkey> SetHotkeyCommand
   {
      get => (RelayCommand<Hotkey>)GetValue( SetHotkeyCommandProperty );
      set => SetValue( SetHotkeyCommandProperty, value );
   }

   public HotkeyControl() => InitializeComponent();

   private void OnPreviewMouseDownAnywhereInWindow( object sender, MouseButtonEventArgs e )
   {
      if ( Keyboard.FocusedElement.Equals( this ) && !IsMouseOver )
      {
         ClearFocus();
      }
   }

   private void DisplayHotkey()
   {
      if ( !string.IsNullOrEmpty( ErrorMessage ) )
      {
         Foreground = Brushes.Red;
         Text = ErrorMessage;
         return;
      }

      if ( Hotkey is Key.None )
      {
         Foreground = Brushes.Gray;
         Text = "No hotkey set";
         return;
      }

      var sb = new StringBuilder();
      if ( HotkeyModifiers.HasFlag( ModifierKeys.Control ) )
      {
         sb.Append( "Ctrl + " );
      }
      if ( HotkeyModifiers.HasFlag( ModifierKeys.Alt ) )
      {
         sb.Append( "Alt + " );
      }
      if ( HotkeyModifiers.HasFlag( ModifierKeys.Shift ) )
      {
         sb.Append( "Shift + " );
      }

      sb.Append( Hotkey );

      Foreground = Brushes.Black;
      Text = sb.ToString();
   }

   private void OnLoaded( object sender, RoutedEventArgs e )
   {
      var parentWindow = Window.GetWindow( this );
      if ( parentWindow is not null )
      {
         parentWindow.PreviewMouseDown += OnPreviewMouseDownAnywhereInWindow;
      }

      DisplayHotkey();
   }

   private void OnGotKeyboardFocus( object sender, RoutedEventArgs e )
   {
      Foreground = Brushes.Gray;
      Text = "Enter hotkey (ESC to clear)";
   }

   private void OnLostKeyboardFocus( object sender, KeyboardFocusChangedEventArgs e )
   {
      DisplayHotkey();
      ClearFocus();
   }

   private void OnKeyDown( object sender, KeyEventArgs e )
   {
      e.Handled = true;

      if ( e.Key is Key.Escape )
      {
         InvokeSetHotkeyCommand( new Hotkey( Key.None, ModifierKeys.None ) );
      }
      else if ( e.KeyboardDevice.Modifiers is not ModifierKeys.None && ( ( e.Key >= Key.A && e.Key <= Key.Z ) || ( e.Key >= Key.F1 && e.Key <= Key.F12 ) ) )
      {
         InvokeSetHotkeyCommand( new Hotkey( e.Key, e.KeyboardDevice.Modifiers ) );
      }
   }

   private void InvokeSetHotkeyCommand( Hotkey hotkey )
   {
      if ( SetHotkeyCommand?.CanExecute( hotkey ) == true )
      {
         SetHotkeyCommand.Execute( hotkey );
         ClearFocus();
      }
   }

   private void ClearFocus() => FocusManager.SetFocusedElement( FocusManager.GetFocusScope( this ), Application.Current.MainWindow );
}