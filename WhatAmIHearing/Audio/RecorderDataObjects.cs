using NAudio.Wave;
using System;

namespace WhatAmIHearing.Audio;

internal sealed class RecordingResult
{
   public byte[] RecordingData { get; }
   public double AudioDurationInSeconds { get; }
   public bool Cancelled => RecordingData is null;

   public RecordingResult( byte[] recordedData, WaveFormat audioFormat )
   {
      RecordingData = recordedData;
      if ( recordedData is not null )
      {
         AudioDurationInSeconds = Math.Round( (double)recordedData.Length / audioFormat.AverageBytesPerSecond, 2 );
      }
   }
}

internal sealed class RecordingProgressEventArgs : EventArgs
{
   public double Progress { get; }
   public string StatusText { get; }

   public RecordingProgressEventArgs( double progress, string statusText )
   {
      Progress = progress;
      StatusText = statusText;
   }
}
