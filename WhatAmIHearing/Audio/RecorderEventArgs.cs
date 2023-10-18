using System;
using NAudio.Wave;

namespace WhatAmIHearing.Audio;

internal sealed class RecordingFinishedEventArgs : EventArgs
{
   public byte[] RecordedData { get; }
   public WaveFormat Format { get; }
   public bool Cancelled => RecordedData is null;

   public RecordingFinishedEventArgs( byte[] recordedData, WaveFormat format )
   {
      RecordedData = recordedData;
      Format = format;
   }
}

internal sealed class RecordingProgressEventArgs : EventArgs
{
   public long BytesRecorded { get; }
   public long MaxBytes { get; }

   public RecordingProgressEventArgs( long bytesRecorded, long maxBytes )
   {
      BytesRecorded = bytesRecorded;
      MaxBytes = maxBytes;
   }
}
