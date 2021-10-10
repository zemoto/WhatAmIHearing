using System.Windows;

namespace WhatAmIHearing
{
   public partial class App
   {
      protected override void OnStartup( StartupEventArgs e )
      {
         _ = new MainWindow().ShowDialog();
      }
   }
}
