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

   private readonly ObservableCollection<DeviceListItem> _devicesVms = [];
   public ListCollectionView DevicesListView { get; }

   public DeviceProvider()
   {
      _notificationTimer.Interval = TimeSpan.FromSeconds( 1 );
      _notificationTimer.Tick += OnTimerTick;

      DevicesListView = new ListCollectionView( _devicesVms );

      UpdateDeviceList();
      _ = _deviceEnumerator.RegisterEndpointNotificationCallback( this );

      AppSettings.Instance.PropertyChanged += ( sender, e ) =>
      {
         if ( e.PropertyName == nameof( AppSettings.DisplayInputDevices ) )
         {
            UpdateDeviceList();
         }
      };
   }

   public void Dispose() => _deviceEnumerator.Dispose();

   private void OnTimerTick( object sender, EventArgs e )
   {
      _notificationTimer.Stop();
      UpdateDeviceList();
   }

   private void UpdateDeviceList()
   {
      var selectedDevice = AppSettings.Instance.SelectedDevice;
      _deviceList = [.. _deviceEnumerator.EnumerateAudioEndPoints( AppSettings.Instance.DisplayInputDevices ? DataFlow.All : DataFlow.Render, DeviceState.Active )];

      _devicesVms.Clear();
      DevicesListView.GroupDescriptions.Clear();

      // Output devices
      _devicesVms.Add( new DeviceListItem( Constants.DefaultOutputDeviceName, Constants.OutputDeviceCategoryName ) );
      foreach ( var device in _deviceList.Where( x => x.DataFlow is DataFlow.Render ).Select( x => new DeviceListItem( x.FriendlyName, Constants.OutputDeviceCategoryName ) ) )
      {
         _devicesVms.Add( device );
      }

      // Input devices
      var inputDevices = _deviceList.Where( x => x.DataFlow is DataFlow.Capture ).ToList();
      if ( inputDevices.Any() )
      {
         DevicesListView.GroupDescriptions.Add( new PropertyGroupDescription( nameof( DeviceListItem.Category ) ) );

         _devicesVms.Add( new DeviceListItem( Constants.DefaultInputDeviceName, Constants.InputDeviceCategoryName ) );
         foreach ( var device in _deviceList.Where( x => x.DataFlow is DataFlow.Capture ).Select( x => new DeviceListItem( x.FriendlyName, Constants.InputDeviceCategoryName ) ) )
         {
            _devicesVms.Add( device );
         }
      }

      AppSettings.Instance.SelectedDevice = _deviceList.Any( x => x.FriendlyName.Equals( selectedDevice, StringComparison.OrdinalIgnoreCase ) ) ? selectedDevice : Constants.DefaultOutputDeviceName;
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