using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace WhatAmIHearing;

internal enum AppState
{
   Stopped = 0,
   Recording = 1,
   Identifying = 2,
}

internal sealed partial class StateViewModel( Action changeStateAction ) : ObservableObject
{
   public void SetStatusText( string text, bool isError = false )
   {
      StatusText = text;
      ShowingErrorText = isError;
   }

   [ObservableProperty]
   public partial AppState State { get; set; }

   [ObservableProperty]
   public partial string StatusText { get; set; }

   [ObservableProperty]
   public partial bool ShowingErrorText { get; set; }

   public RelayCommand ChangeStateCommand { get; } = new( changeStateAction );
}
