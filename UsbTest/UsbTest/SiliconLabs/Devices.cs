using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UsbTest
{
    public class Devices
    {
        public static IEnumerable<SI_Device> Enumerate()
        {
            uint numDevices = 0;
            USBCommandHandler.SI_GetNumDevices(ref numDevices);

            if (numDevices == 0)
            {
                yield break;
            }

            for (uint i = 0; i < numDevices; i++)
            {
                string pid = USBCommandHandler.SI_GetPID(i);
                string vid = USBCommandHandler.SI_GetVID(i);

                string deviceName = USBCommandHandler.SI_GetDeviceName(i);
                string serial = USBCommandHandler.SI_GetDeviceSerialNumber(i);
                yield return new SI_Device(deviceName, vid, pid, serial);
            }
        }
    }
}
