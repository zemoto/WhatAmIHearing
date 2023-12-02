using System.Windows.Input;
using ZemotoCommon.UI;

namespace WhatAmIHearing;

internal enum AppState
{
   Stopped = 0,
   Recording = 1,
   Identifying = 2,
   Error = 3,
}

internal sealed class StateViewModel : ViewModelBase
{
   private AppState _state;
   public AppState State
   {
      get => _state;
      set => SetProperty( ref _state, value );
   }

   public ICommand ChangeStateCommand { get; init; }
}
