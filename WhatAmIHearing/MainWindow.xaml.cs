using System.Windows;

namespace WhatAmIHearing
{
   internal partial class MainWindow
   {
      public MainWindow()
      {
         InitializeComponent();
      }

      private void OnCloseClicked( object s, RoutedEventArgs e ) => Close();
   }
}