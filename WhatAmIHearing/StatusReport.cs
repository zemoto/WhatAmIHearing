using ZemotoUI;

namespace WhatAmIHearing
{
   internal sealed class StatusReport : ViewModelBase
   {
      private StatusReport() { }

      public static StatusReport Status { get; } = new StatusReport();

      private string _text;
      public string Text
      {
         get => _text;
         set => SetProperty( ref _text, value );
      }
   }
}
