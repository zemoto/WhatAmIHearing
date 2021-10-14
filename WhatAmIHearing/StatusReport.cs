using ZemotoUI;

namespace WhatAmIHearing
{
   internal sealed class StatusReport : ViewModelBase
   {
      private StatusReport() { }

      public static StatusReport Status { get; } = new StatusReport();

      public static void Reset()
      {
         Status.Text = string.Empty;
         Status.Progress = 0;
      }

      private string _text;
      public string Text
      {
         get => _text;
         set => SetProperty( ref _text, value );
      }

      private int _progress;
      public int Progress
      {
         get => _progress;
         set => SetProperty( ref _progress, value );
      }
   }
}
