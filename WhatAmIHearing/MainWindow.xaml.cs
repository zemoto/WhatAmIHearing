using System.Windows;

namespace WhatAmIHearing
{
   public partial class MainWindow
   {
      public MainWindow()
      {
         InitializeComponent();
      }

      private void OnCloseClicked( object s, RoutedEventArgs e ) => Close();
      private void OnMinimizeClicked( object s, RoutedEventArgs e ) => WindowState = WindowState.Minimized;
   }
}