using NAudio.Wave;

namespace WhatAmIHearing.Audio;

internal sealed class RecordingResult
{
   public byte[] RecordingData { get; }
   public double AudioDurationInSeconds { get; }
   public bool Cancelled { get; }

   public RecordingResult( byte[] recordedData, WaveFormat audioFormat, bool cancelled )
   {
      RecordingData = recordedData;
      AudioDurationInSeconds = Math.Round( (double)recordedData.Length / audioFormat.AverageBytesPerSecond, 2 );
      Cancelled = cancelled;
   }
}

internal sealed class RecordingProgressEventArgs( double progress, string statusText ) : EventArgs
{
   public double Progress { get; } = progress;
   public string StatusText { get; } = statusText;
}
