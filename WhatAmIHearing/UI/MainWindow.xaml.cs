using System.Windows;
using WhatAmIHearing.Model;

namespace WhatAmIHearing.UI
{
   internal partial class MainWindow
   {
      public MainWindow( MainViewModel model )
      {
         DataContext = model;
         InitializeComponent();
      }

      private void OnCloseClicked( object s, RoutedEventArgs e ) => Close();
   }
}