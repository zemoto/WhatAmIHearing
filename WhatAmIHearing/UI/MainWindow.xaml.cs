using System.Windows;

namespace WhatAmIHearing.UI;

internal partial class MainWindow
{
   public MainWindow( MainViewModel model )
   {
      DataContext = model;
      InitializeComponent();
   }

   private void OnCloseClicked( object s, RoutedEventArgs e ) => Close();
}