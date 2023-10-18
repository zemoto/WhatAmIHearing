using NAudio.CoreAudioApi;
using System;
using System.Collections.Generic;
using System.Linq;

namespace WhatAmIHearing.Audio;

internal sealed class DeviceProvider : IDisposable
{
   private const string DefaultDeviceName = "Default Input Device";

   private readonly MMDeviceEnumerator _deviceEnumerator = new();
   private readonly List<MMDevice> _deviceList;

   public DeviceProvider() => _deviceList = _deviceEnumerator.EnumerateAudioEndPoints( DataFlow.All, DeviceState.Active ).ToList();

   public void Dispose() => _deviceEnumerator.Dispose();

   public IReadOnlyCollection<string> GetDeviceNameList()
   {
      var deviceNameList = _deviceList.ConvertAll( x => x.FriendlyName );
      deviceNameList.Insert( 0, DefaultDeviceName );

      return deviceNameList;
   }

   public MMDevice GetSelectedDevice()
   {
      var selectedDevice = Properties.UserSettings.Default.SelectedDevice;
      return selectedDevice == DefaultDeviceName
         ? _deviceEnumerator.GetDefaultAudioEndpoint( DataFlow.Render, Role.Console )
         : _deviceList.First( x => x.FriendlyName == selectedDevice );
   }
}
