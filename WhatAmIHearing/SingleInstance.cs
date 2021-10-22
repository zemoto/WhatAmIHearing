using System;
using System.Threading;
using System.Windows;

namespace WhatAmIHearing
{
   internal static class SingleInstance
   {
      private static readonly Mutex SingleInstanceMutex = new( true, "{F90BECD9-8759-4A8C-A8B4-A228EB11F315}" );

      public static bool Claim()
      {
         if ( SingleInstanceMutex.WaitOne( TimeSpan.Zero ) )
         {
            return true;
         }

         MessageBox.Show( "WhatAmIHearing is already running", "Already Running", MessageBoxButton.OK, MessageBoxImage.Information );
         return false;
      }
   }
}
