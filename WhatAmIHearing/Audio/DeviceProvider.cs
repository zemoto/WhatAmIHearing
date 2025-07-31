using NAudio.CoreAudioApi;
using NAudio.CoreAudioApi.Interfaces;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Data;
using System.Windows.Threading;

namespace WhatAmIHearing.Audio;

internal sealed class DeviceListItem( string name, string category )
{
   public string Name { get; } = name;
   public string Category { get; } = category;
}

internal sealed class DeviceProvider : IDisposable, IMMNotificationClient
{
   private readonly DispatcherTimer _notificationTimer = new();
   private readonly MMDeviceEnumerator _deviceEnumerator = new();
   private List<MMDevice> _deviceList;

   private readonly ObservableCollection<DeviceListItem> _devices = [];
   public ListCollectionView Devices { get; }

   public DeviceProvider()
   {
      _notificationTimer.Interval = TimeSpan.FromSeconds( 1 );
      _notificationTimer.Tick += OnTimerTick;

      Devices = new ListCollectionView( _devices );
      Devices.GroupDescriptions.Add( new PropertyGroupDescription( nameof( DeviceListItem.Category ) ) );

      UpdateDeviceList();
      _ = _deviceEnumerator.RegisterEndpointNotificationCallback( this );
   }

   public void Dispose() => _deviceEnumerator.Dispose();

   private void OnTimerTick( object sender, EventArgs e )
   {
      _notificationTimer.Stop();

      var selectedDevice = AppSettings.Instance.SelectedDevice;
      UpdateDeviceList();
      AppSettings.Instance.SelectedDevice = _deviceList.Any( x => x.FriendlyName.Equals( selectedDevice, StringComparison.OrdinalIgnoreCase ) ) ? selectedDevice : Constants.DefaultOutputDeviceName;
   }

   private void UpdateDeviceList()
   {
      _deviceList = [.. _deviceEnumerator.EnumerateAudioEndPoints( DataFlow.All, DeviceState.Active )];

      _devices.Clear();

      // Output devices
      _devices.Add( new DeviceListItem( Constants.DefaultOutputDeviceName, Constants.OutputDeviceCategoryName ) );
      foreach ( var device in _deviceList.Where( x => x.DataFlow is DataFlow.Render ).Select( x => new DeviceListItem( x.FriendlyName, Constants.OutputDeviceCategoryName ) ) )
      {
         _devices.Add( device );
      }

      // Input devices
      _devices.Add( new DeviceListItem( Constants.DefaultInputDeviceName, Constants.InputDeviceCategoryName ) );
      foreach ( var device in _deviceList.Where( x => x.DataFlow is DataFlow.Capture ).Select( x => new DeviceListItem( x.FriendlyName, Constants.InputDeviceCategoryName ) ) )
      {
         _devices.Add( device );
      }
   }

   public MMDevice GetSelectedDevice()
   {
      var selectedDevice = AppSettings.Instance.SelectedDevice;
      return string.IsNullOrEmpty( selectedDevice ) ? null
           : selectedDevice.Equals( Constants.DefaultOutputDeviceName, StringComparison.OrdinalIgnoreCase ) ? _deviceEnumerator.GetDefaultAudioEndpoint( DataFlow.Render, Role.Console )
           : selectedDevice.Equals( Constants.DefaultInputDeviceName, StringComparison.OrdinalIgnoreCase ) ? _deviceEnumerator.GetDefaultAudioEndpoint( DataFlow.Capture, Role.Console )
           : _deviceList.FirstOrDefault( x => x.FriendlyName.Equals( selectedDevice, StringComparison.OrdinalIgnoreCase ) );
   }

   public void OnDeviceStateChanged( string deviceId, DeviceState newState ) => _notificationTimer.Start();
   public void OnDeviceAdded( string pwstrDeviceId ) => _notificationTimer.Start();
   public void OnDeviceRemoved( string deviceId ) => _notificationTimer.Start();
   public void OnDefaultDeviceChanged( DataFlow flow, Role role, string defaultDeviceId ) { }
   public void OnPropertyValueChanged( string pwstrDeviceId, PropertyKey key ) { }
}