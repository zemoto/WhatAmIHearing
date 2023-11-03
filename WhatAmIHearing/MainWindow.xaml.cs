using System.Windows;

namespace WhatAmIHearing;

internal partial class MainWindow
{
   public MainWindow( MainViewModel model )
   {
      DataContext = model;
      InitializeComponent();
   }

   private void OnCloseClicked( object s, RoutedEventArgs e ) => Close();
}