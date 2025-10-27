using NAudio.CoreAudioApi;
using NAudio.CoreAudioApi.Interfaces;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
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

   private ICollectionView _devicesListView;
   public ObservableCollection<DeviceListItem> Devices = [];

   public DeviceProvider()
   {
      _notificationTimer.Interval = TimeSpan.FromSeconds( 1 );
      _notificationTimer.Tick += OnTimerTick;

      _devicesListView = CollectionViewSource.GetDefaultView( Devices );

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

      Devices.Clear();
      _devicesListView.GroupDescriptions.Clear();

      // Output devices
      var outputDevices = _deviceList.FindAll( x => x.DataFlow is DataFlow.Render );
      bool hasOutputDevices = outputDevices.Any();
      if ( hasOutputDevices )
      {
         Devices.Add( new DeviceListItem( Constants.DefaultOutputDeviceName, Constants.OutputDeviceCategoryName ) );

         foreach ( var device in outputDevices.ConvertAll( x => new DeviceListItem( x.FriendlyName, Constants.OutputDeviceCategoryName ) ) )
         {
            Devices.Add( device );
         }
      }

      // Input devices
      var inputDevices = _deviceList.FindAll( x => x.DataFlow is DataFlow.Capture );
      bool hasInputDevices = inputDevices.Any();
      if ( hasInputDevices )
      {
         Devices.Add( new DeviceListItem( Constants.DefaultInputDeviceName, Constants.InputDeviceCategoryName ) );

         foreach ( var device in inputDevices.ConvertAll( x => new DeviceListItem( x.FriendlyName, Constants.InputDeviceCategoryName ) ) )
         {
            Devices.Add( device );
         }
      }

      if ( hasInputDevices && hasOutputDevices )
      {
         _devicesListView.GroupDescriptions.Add( new PropertyGroupDescription( nameof( DeviceListItem.Category ) ) );
      }

      AppSettings.Instance.SelectedDevice = _deviceList.Any( x => x.FriendlyName.Equals( selectedDevice, StringComparison.OrdinalIgnoreCase ) ) ? selectedDevice : Constants.DefaultOutputDeviceName;
   }

   public MMDevice GetSelectedDevice()
   {
      if ( _deviceList?.Any() != true )
      {
         return null;
      }

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