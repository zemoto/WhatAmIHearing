using NAudio.Wave;

namespace WhatAmIHearing.Api.Shazam;

internal static class ShazamSpecProvider
{
   private const int _sampleRate = 44100;
   private const int _channelCount = 1;
   private const int _bitsPerChannel = 16;

   public static WaveFormat ShazamWaveFormat { get; } = new( _sampleRate, _bitsPerChannel, _channelCount );

   public static long MaxBytes { get; } = 500 * 1000; // 500KB
}
