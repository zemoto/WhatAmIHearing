using System.Windows.Input;
using ZemotoCommon.UI;

namespace WhatAmIHearing;

internal enum AppState
{
   Stopped = 0,
   Recording = 1,
   Identifying = 2,
}

internal sealed class StateViewModel : ViewModelBase
{
   public void SetStatusText( string text, bool isError = false )
   {
      StatusText = text;
      ShowingErrorText = isError;
   }

   private AppState _state;
   public AppState State
   {
      get => _state;
      set => SetProperty( ref _state, value );
   }

   private string _statusText;
   public string StatusText
   {
      get => _statusText;
      private set => SetProperty( ref _statusText, value );
   }

   private bool _ShowingErrorText;
   public bool ShowingErrorText
   {
      get => _ShowingErrorText;
      private set => SetProperty( ref _ShowingErrorText, value );
   }

   public ICommand ChangeStateCommand { get; init; }
}
