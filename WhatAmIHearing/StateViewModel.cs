using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;

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
   private AppState _state;

   private string _statusText;
   public string StatusText
   {
      get => _statusText;
      private set => SetProperty( ref _statusText, value );
   }

   private bool _showingErrorText;
   public bool ShowingErrorText
   {
      get => _showingErrorText;
      private set => SetProperty( ref _showingErrorText, value );
   }

   public RelayCommand ChangeStateCommand { get; } = new( changeStateAction );
}
