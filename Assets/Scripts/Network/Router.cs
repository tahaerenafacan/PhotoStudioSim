using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class NetworkDevicesChangedEventArgs : EventArgs
{
    public readonly List<INetworkDevice> connectedDevices;
    public readonly INetworkDevice changedDevice;

    public NetworkDevicesChangedEventArgs(List<INetworkDevice> connectedDevices,  INetworkDevice changedDevice)
    {
        this.connectedDevices = connectedDevices;
        this.changedDevice = changedDevice;
    }
} 

public class Router : MonoBehaviour
{
    public static Router Instance { get; private set; }
    
    public event EventHandler<NetworkDevicesChangedEventArgs> OnNetworkDevicesChanged;
    
    private HashSet<INetworkDevice> connectedDevices =  new HashSet<INetworkDevice>();

    private void Awake()
    {
        if (Instance != null && Instance != this) 
        { 
            Destroy(gameObject); 
            return; 
        }
        Instance = this;
    }

    public void Connect(INetworkDevice device)
    {
        connectedDevices.Add(device);
        OnNetworkDevicesChanged?.Invoke(this, new NetworkDevicesChangedEventArgs(connectedDevices.ToList(), device));
    }

    public void Disconnect(INetworkDevice device)
    {
        if (!connectedDevices.Contains(device))
        {
            Debug.LogError($"{device.GetNetworkDeviceData().NetworkDeviceName} is not connected");
            return;
        }
        connectedDevices.Remove(device);
        OnNetworkDevicesChanged?.Invoke(this, new NetworkDevicesChangedEventArgs(connectedDevices.ToList(), device));
    }

    public List<INetworkDevice> GetDevicesByType(NetworkDeviceType deviceType)
    {
        return connectedDevices.Where(device => device.GetNetworkDeviceData().NetworkDeviceType == deviceType).ToList();
    }

    public T GetNetworkDeviceComponent<T>(INetworkDevice device) where T : Component
    {
        if (device is Component component)
            return component.GetComponent<T>();

        return null;
    }
}
