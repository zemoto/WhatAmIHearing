using System.Windows;
using System.Windows.Controls.Primitives;
using WhatAmIHearing.Audio;

namespace WhatAmIHearing.UI;

internal partial class RecorderControls
{
   private RecorderViewModel _model;

   public RecorderControls() => InitializeComponent();

   private void OnLoaded( object sender, RoutedEventArgs e ) => _model = (RecorderViewModel)DataContext;

   private void OnRecordPercentSliderDragStarted( object sender, DragStartedEventArgs e ) => SetRecordPercentStatusText();

   private void OnRecordPercentSliderDragDelta( object sender, DragDeltaEventArgs e ) => SetRecordPercentStatusText();

   private void OnRecordPercentSliderDragCompleted( object sender, DragCompletedEventArgs e ) => _model.RecorderStatusText = string.Empty;

   private void SetRecordPercentStatusText() => _model.RecorderStatusText = $"Record {(int)( _model.RecordPercent * 100)}% of allowed audio";
}
