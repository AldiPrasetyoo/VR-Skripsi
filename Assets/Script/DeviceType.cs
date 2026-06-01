using UnityEngine;

namespace HPhysic
{
    public enum DeviceType
    {
        NanoStation,
        POE,
        Switch,
        Laptop,
        Cable
    }

    public class DeviceIdentity : MonoBehaviour
    {
        public DeviceType deviceType;
    }
}