using UnityEngine;

[CreateAssetMenu(fileName = "NewNetworkDevice", menuName = "PSSGame/NetworkDevice")]
public class NetworkDeviceSO : ScriptableObject
{
    public string NetworkDeviceName;
    public NetworkDeviceType NetworkDeviceType;
}

public enum NetworkDeviceType
{
    Printer,
    Printer3D,
    Camera
}
