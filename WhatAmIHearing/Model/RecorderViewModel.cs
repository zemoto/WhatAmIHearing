using System.Windows.Input;

namespace WhatAmIHearing.Model
{
   internal enum RecorderState
   {
      Stopped = 0,
      Recording = 1,
      SendingToShazam = 2
   }

   internal sealed class RecorderViewModel : ZemotoCommon.UI.ViewModelBase
   {
      private RecorderState _state;
      public RecorderState State
      {
         get => _state;
         set => SetProperty( ref _state, value );
      }

      private string _recorderStatusText;
      public string RecorderStatusText
      {
         get => _recorderStatusText;
         set => SetProperty( ref _recorderStatusText, value );
      }

      private int _recordingProgress;
      public int RecordingProgress
      {
         get => _recordingProgress;
         set => SetProperty( ref _recordingProgress, value );
      }

      public ICommand RecordStopCommand { get; set; }
   }
}
