using ZemotoUI;

namespace WhatAmIHearing.Audio
{
   internal enum State
   {
      Stopped = 0,
      Recording = 1,
      SendingToShazam = 2
   }

   internal sealed class RecorderViewModel : ViewModelBase
   {
      private State _recorderState;
      public State RecorderState
      {
         get => _recorderState;
         set => SetProperty( ref _recorderState, value );
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
   }
}
