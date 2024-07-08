using NAudio.CoreAudioApi;
using System;
using System.Collections.Generic;
using System.Linq;

namespace WhatAmIHearing.Audio;

internal sealed class DeviceProvider : IDisposable
{
   private readonly MMDeviceEnumerator _deviceEnumerator = new();
   private readonly List<MMDevice> _deviceList;

   public DeviceProvider() => _deviceList = _deviceEnumerator.EnumerateAudioEndPoints( DataFlow.Render, DeviceState.Active ).ToList();

   public void Dispose() => _deviceEnumerator.Dispose();

   public IReadOnlyCollection<string> GetDeviceNameList()
   {
      var deviceNameList = _deviceList.ConvertAll( x => x.FriendlyName );
      deviceNameList.Insert( 0, Constants.DefaultDeviceName );

      return deviceNameList;
   }

   public MMDevice GetSelectedDevice()
   {
      var selectedDevice = AppSettings.Instance.SelectedDevice;
      return selectedDevice == Constants.DefaultDeviceName
         ? _deviceEnumerator.GetDefaultAudioEndpoint( DataFlow.Render, Role.Console )
         : _deviceList.First( x => x.FriendlyName == selectedDevice );
   }
}
