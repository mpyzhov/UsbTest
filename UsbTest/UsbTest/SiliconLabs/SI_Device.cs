using Microsoft.Win32.SafeHandles;
using System;
using System.Linq;

namespace UsbTest
{
    public class SI_Device : IDisposable
    {
        private string deviceName;
        private string deviceVid;
        private string devicePid;
        private string deviceSerial;

        private SafeFileHandle deviceHandle;

        public SI_Device(string name, string vid, string pid, string serial)
        {
            deviceName = name;
            deviceVid = vid;
            devicePid = pid;
            deviceSerial = serial;
        }

        public SafeFileHandle DeviceHandle { get { return deviceHandle; } }

        public string Name { get { return deviceName; } }

        public string Vid
        {
            get { return deviceVid; }
        }

        public string Pid
        {
            get { return devicePid; }
        }

        public string Serial
        {
            get { return deviceSerial; }
        }

        public bool IsOpened()
        {
            return deviceHandle != null && !deviceHandle.IsInvalid && !deviceHandle.IsClosed;
        }

        private int GetIndex()
        {
            var thisDevice = Tuple.Create(Vid, Pid, Serial);
            int i = 0;
            foreach (var device in Devices.Enumerate().Select(d => Tuple.Create(d.Vid, d.Pid, d.Serial)))
            {
                if (thisDevice.Equals(device))
                {
                    return i;
                }

                i++;
            }

            return -1; // not found
        }

        public void EnsureOpened()
        {
            if (IsOpened())
            {
                return;
            }
            int index = GetIndex();
            if (index < 0)
            {
                throw new Exception("SI_DEVICE_NOT_FOUND");
            }

            IntPtr handle = IntPtr.Zero;
            var status = USBCommandHandler.SI_Open((uint)index, ref handle);
            if (status == USBCommandHandler.SI_SUCCESS)
            {
                deviceHandle = new SafeFileHandle(handle, true);
                Initialize();
            }
            else
            {
                Console.Write(status + " ");
            }
        }

        protected virtual void Initialize()
        {
        }

        public virtual bool Write(byte[] data)
        {
            if (data == null || data.Length == 0)
            {
                return false;
            }

            //EnsureOpened();

            uint bytesWritten = 0;
            USBCommandHandler.SI_FlushBuffers(deviceHandle.DangerousGetHandle(), 1, 1);
            var status = USBCommandHandler.SI_Write(deviceHandle.DangerousGetHandle(), data, (uint)data.Length, ref bytesWritten, IntPtr.Zero);
            return status == USBCommandHandler.SI_SUCCESS && bytesWritten == data.Length;
        }

        public virtual byte[] Read()
        {
            //EnsureOpened();
            byte[] buf = PrepareReadBuffer();
            uint bytesRead = 0;
            var status = USBCommandHandler.SI_Read(deviceHandle.DangerousGetHandle(), buf, (uint)buf.Length, ref bytesRead, IntPtr.Zero);

            if (status == USBCommandHandler.SI_SUCCESS && bytesRead != 0)
            {
                if (bytesRead != buf.Length)
                {
                    Array.Resize(ref buf, (int)bytesRead);
                }

                return buf;
            }

            throw new Exception(status.ToString());
        }

        protected virtual byte[] PrepareReadBuffer()
        {
            return new byte[512];
        }

        public void Close()
        {
            if (IsOpened())
            {
                try
                {
                    USBCommandHandler.SI_Close(deviceHandle.DangerousGetHandle());
                    deviceHandle.Close();
                    deviceHandle.Dispose();
                    deviceHandle = null;
                }
                catch (Exception ex)
                {
                    throw ex;
                }
            }
        }

        public void Dispose()
        {
            Close();
        }

        public override string ToString()
        {
            return string.Format("name={0}, vid={1}, pid={2}, serial={3};", Name, Vid, Pid, Serial);
        }
    }
}
