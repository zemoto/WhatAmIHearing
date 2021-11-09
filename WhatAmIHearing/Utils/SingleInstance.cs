using System;
using System.IO.Pipes;
using System.Threading;

namespace WhatAmIHearing.Utils
{
   internal static class SingleInstance
   {
      private const string SingleInstanceName = "WhatAmIHearingInstance";
      private static readonly Mutex SingleInstanceMutex = new( true, SingleInstanceName );

      public static event EventHandler PingedBySecondProcess;

      public static bool Claim()
      {
         if ( !SingleInstanceMutex.WaitOne( TimeSpan.Zero ) )
         {
            PingSingleInstance();
            return false;
         }

         ListenForOtherProcesses();
         return true;
      }

      private static void PingSingleInstance()
      {
         // The act of connecting indicates to the single instance that another process tried to run
         using var client = new NamedPipeClientStream( ".", SingleInstanceName, PipeDirection.Out );
         try
         {
            client.Connect( 0 );
         }
         catch
         {
            // ignore
         }
      }

      private static void ListenForOtherProcesses()
      {
         var server = new NamedPipeServerStream( SingleInstanceName, PipeDirection.In, 1, PipeTransmissionMode.Message, PipeOptions.Asynchronous );
         _ = server.BeginWaitForConnection( OnPipeConnection, server );
      }

      private static void OnPipeConnection( IAsyncResult ar )
      {
         using ( var server = (NamedPipeServerStream)ar.AsyncState )
         {
            try
            {
               server.EndWaitForConnection( ar );
            }
            catch
            {
               // ignore
            }
         }

         PingedBySecondProcess?.Invoke( null, EventArgs.Empty );

         ListenForOtherProcesses();
      }
   }
}
