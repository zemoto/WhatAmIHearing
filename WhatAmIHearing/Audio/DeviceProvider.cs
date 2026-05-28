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
   public ObservableCollection<DeviceListItem> Devices { get; } = [];

   public DeviceProvider()
   {
      _notificationTimer.Interval = TimeSpan.FromSeconds( 1 );
      _notificationTimer.Tick += OnTimerTick;

      _devicesListView = CollectionViewSource.GetDefaultView( Devices );

      UpdateDeviceList();
      _ = _deviceEnumerator.RegisterEndpointNotificationCallback( this );

      AppSettings.Instance.PropertyChanged += OnAppSettingsPropertyChanged;
   }

   public void Dispose()
   {
      _deviceList?.ForEach( x => x.Dispose() );

      _ = _deviceEnumerator.UnregisterEndpointNotificationCallback( this );
      _deviceEnumerator.Dispose();

      _notificationTimer.Stop();
      AppSettings.Instance.PropertyChanged -= OnAppSettingsPropertyChanged;
   }

   private void OnTimerTick( object sender, EventArgs e )
   {
      _notificationTimer.Stop();
      UpdateDeviceList();
   }

   private void OnAppSettingsPropertyChanged( object sender, PropertyChangedEventArgs e )
   {
      if ( e.PropertyName == nameof( AppSettings.DisplayInputDevices ) )
      {
         UpdateDeviceList();
      }
   }

   private void UpdateDeviceList()
   {
      var selectedDevice = AppSettings.Instance.SelectedDevice;

      _deviceList?.ForEach( x => x.Dispose() );
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

      // Reselect the previously selected device if it's still available, otherwise select the default output device
      if ( selectedDevice.Equals( Constants.DefaultOutputDeviceName, StringComparison.OrdinalIgnoreCase ) ||
         ( selectedDevice.Equals( Constants.DefaultInputDeviceName, StringComparison.OrdinalIgnoreCase ) && AppSettings.Instance.DisplayInputDevices ) ||
         _deviceList.Any( x => x.FriendlyName.Equals( selectedDevice, StringComparison.OrdinalIgnoreCase ) ) )
      {
         AppSettings.Instance.SelectedDevice = selectedDevice;
      }
      else
      {
         AppSettings.Instance.SelectedDevice = Constants.DefaultOutputDeviceName;
      }
   }

   // Returned MMDevice should be disposed by the caller
   public MMDevice GetSelectedDevice()
   {
      var selectedDevice = AppSettings.Instance.SelectedDevice;
      if ( _deviceList?.Any() != true || string.IsNullOrEmpty( selectedDevice ) )
      {
         return null;
      }
      else if ( selectedDevice.Equals( Constants.DefaultOutputDeviceName, StringComparison.OrdinalIgnoreCase ) )
      {
         return _deviceEnumerator.GetDefaultAudioEndpoint( DataFlow.Render, Role.Console );
      }
      else if ( selectedDevice.Equals( Constants.DefaultInputDeviceName, StringComparison.OrdinalIgnoreCase ) )
      {
         return _deviceEnumerator.GetDefaultAudioEndpoint( DataFlow.Capture, Role.Console );
      }
      else
      {
         // Make sure we return a copy of the device from the enumerator to ensure it does not get disposed when the device list is updated
         var device = _deviceList.FirstOrDefault( x => x.FriendlyName.Equals( selectedDevice, StringComparison.OrdinalIgnoreCase ) );
         return device is not null ? _deviceEnumerator.GetDevice( device.ID ) : null;
      }
   }

   private void ScheduleDeviceListUpdate() => _notificationTimer.Dispatcher.BeginInvoke( _notificationTimer.Start );

   public void OnDeviceStateChanged( string deviceId, DeviceState newState ) => ScheduleDeviceListUpdate();
   public void OnDeviceAdded( string pwstrDeviceId ) => ScheduleDeviceListUpdate();
   public void OnDeviceRemoved( string deviceId ) => ScheduleDeviceListUpdate();
   public void OnDefaultDeviceChanged( DataFlow flow, Role role, string defaultDeviceId ) { }
   public void OnPropertyValueChanged( string pwstrDeviceId, PropertyKey key ) { }
}