using System.Windows;
using System.Windows.Controls.Primitives;

namespace WhatAmIHearing.Audio;

internal partial class RecorderControls
{
   private RecorderViewModel _model;

   public RecorderControls() => InitializeComponent();

   private void OnLoaded( object sender, RoutedEventArgs e ) => _model = (RecorderViewModel)DataContext;

   private void OnRecordPercentSliderDragStarted( object sender, DragStartedEventArgs e ) => SetRecordPercentStatusText();

   private void OnRecordPercentSliderDragDelta( object sender, DragDeltaEventArgs e ) => SetRecordPercentStatusText();

   private void OnRecordPercentSliderDragCompleted( object sender, DragCompletedEventArgs e ) => _model.StateVm.StatusText = string.Empty;

   private void SetRecordPercentStatusText() => _model.StateVm.StatusText = $"Record {(int)( _model.RecordPercent * 100)}% of allowed audio";
}
