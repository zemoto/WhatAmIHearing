using NAudio.CoreAudioApi;
using NAudio.CoreAudioApi.Interfaces;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Threading;

namespace WhatAmIHearing.Audio;

internal sealed class DeviceProvider : IDisposable, IMMNotificationClient
{
   private readonly DispatcherTimer _notificationTimer = new();
   private readonly MMDeviceEnumerator _deviceEnumerator = new();
   private List<MMDevice> _deviceList;

   public ObservableCollection<string> DeviceNames { get; } = [];

   public DeviceProvider()
   {
      _notificationTimer.Interval = TimeSpan.FromSeconds( 1 );
      _notificationTimer.Tick += OnTimerTick;

      UpdateDeviceList();
      _ = _deviceEnumerator.RegisterEndpointNotificationCallback( this );
   }

   public void Dispose() => _deviceEnumerator.Dispose();

   private void OnTimerTick( object sender, EventArgs e )
   {
      _notificationTimer.Stop();

      var selectedDevice = AppSettings.Instance.SelectedDevice;
      UpdateDeviceList();
      AppSettings.Instance.SelectedDevice = DeviceNames.Contains( selectedDevice ) ? selectedDevice : Constants.DefaultDeviceName;
   }

   private void UpdateDeviceList()
   {
      _deviceList = [.. _deviceEnumerator.EnumerateAudioEndPoints( DataFlow.Render, DeviceState.Active )];

      DeviceNames.Clear();
      DeviceNames.Add( Constants.DefaultDeviceName );
      _deviceList.ForEach( x => DeviceNames.Add( x.FriendlyName ) );
   }

   public MMDevice GetSelectedDevice()
   {
      var selectedDevice = AppSettings.Instance.SelectedDevice;
      return selectedDevice == Constants.DefaultDeviceName
         ? _deviceEnumerator.GetDefaultAudioEndpoint( DataFlow.Render, Role.Console )
         : _deviceList.First( x => x.FriendlyName == selectedDevice );
   }

   public void OnDeviceStateChanged( string deviceId, DeviceState newState ) => _notificationTimer.Start();
   public void OnDeviceAdded( string pwstrDeviceId ) => _notificationTimer.Start();
   public void OnDeviceRemoved( string deviceId ) => _notificationTimer.Start();
   public void OnDefaultDeviceChanged( DataFlow flow, Role role, string defaultDeviceId ) { }
   public void OnPropertyValueChanged( string pwstrDeviceId, PropertyKey key ) { }
}