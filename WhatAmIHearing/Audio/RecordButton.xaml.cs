using System.Windows;

namespace WhatAmIHearing.Audio;

internal sealed partial class RecordButton
{
   public static readonly DependencyProperty StateProperty = DependencyProperty.Register( nameof( State ), typeof( AppState ), typeof( RecordButton ), new PropertyMetadata( AppState.Stopped, OnStateChanged ) );
   private static void OnStateChanged( DependencyObject d, DependencyPropertyChangedEventArgs e ) => ( (RecordButton)d ).OnStateChanged();
   private void OnStateChanged()
   {
      switch ( State )
      {
         case AppState.Stopped:
            VisualStateManager.GoToElementState( this, StoppedState.Name, true );
            break;
         case AppState.Recording:
            VisualStateManager.GoToElementState( this, RecordingState.Name, true );
            break;
         case AppState.Identifying:
            VisualStateManager.GoToElementState( this, IdentifyingState.Name, true );
            break;
      }
   }

   public AppState State
   {
      get => (AppState)GetValue( StateProperty );
      set => SetValue( StateProperty, value );
   }

   public RecordButton() => InitializeComponent();

   public override void OnApplyTemplate() => VisualStateManager.GoToElementState( this, StoppedState.Name, false );
}
