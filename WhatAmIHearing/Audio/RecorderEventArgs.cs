using System;

namespace WhatAmIHearing.Audio;

internal sealed class RecordingFinishedEventArgs : EventArgs
{
   public byte[] RecordingData { get; }
   public bool Cancelled => RecordingData is null;

   public RecordingFinishedEventArgs( byte[] recordedData ) => RecordingData = recordedData;
}

internal sealed class RecordingProgressEventArgs : EventArgs
{
   public long BytesRecorded { get; }
   public long BytesToRecord { get; }

   public RecordingProgressEventArgs( long bytesRecorded, long bytesToRecord )
   {
      BytesRecorded = bytesRecorded;
      BytesToRecord = bytesToRecord;
   }
}
