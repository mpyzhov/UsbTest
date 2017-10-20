using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace UsbTest
{
    class Program
    {
        static void Main(string[] args)
        {
            //if (args.Contains("reg"))
            //{
            //    RegistryHelper.SetupApplicationRunAtStartup(true);
            //    Console.WriteLine("Added to startup!");
            //}
            //else if (args.Contains("unreg"))
            //{
            //    RegistryHelper.SetupApplicationRunAtStartup(false);
            //    Console.WriteLine("Removed from startup!");
            //}

            int time = 500;

            if (args.Count() > 0)
            {
                int res = 0;
                if (int.TryParse(args[0], out res))
                {
                    time = res;
                }
            }

            while (true)
            {
                var devices = Devices.Enumerate().ToList();

                Console.WriteLine("Devices found {0}:", devices.Count);

                foreach (var device in devices)
                {
                    Console.WriteLine(device);
                }

                Console.WriteLine();

                foreach (var device in devices)
                {
                    Write("Test for " + device);
                    Factory(device);

                    Console.WriteLine();
                    
                }

                Thread.Sleep(time);
                Console.WriteLine();
            }
        }

        private static void Factory(SI_Device device)
        {
            switch(device.Pid)
            {
                case "0c08":
                case "0c09":
                case "0c0a":
                    DoAsetekV2(device);
                    break;
                case "0c12":
                case "0c13":
                case "0c14":
                case "0c15":
                case "0c16":
                    DoStarCoolers(device);
                    break;
                default:
                    DoDefault(device);
                    break;
            }
        }

        private static void DoDefault(SI_Device device)
        {
            WriteYellow(device.Pid + " hasn't command sequence. Will just open\\close device.");
            Do(device, d => { });
        }

        private static void DoAsetekV2(SI_Device device)
        {
            WriteOrange("Test #1. Device read values");
            Do(device, d=> 
                {
                    var data = new byte[] { 0x20 };

                    d.Write(data);
                    var res = d.Read();

                    WriteBytes("Write", data);
                    WriteBytes("Read", res);
                });

            WriteOrange("Test #2. External temp");
            Do(device, d =>
            {
                var data = new byte[] { 0x22, 0x01, 0x27, 0x00 };

                d.Write(data);
                var res = d.Read();

                WriteBytes("Write", data);
                WriteBytes("Read", res);
            });
        }

        private static void DoStarCoolers(SI_Device device)
        {
            WriteOrange("Test #1. Get temp");
            Do(device, d =>
                {
                    var data = new byte[] { 0xa9 };

                    d.Write(data);
                    var res = d.Read();

                    WriteBytes("Write", data);
                    WriteBytes("Read", res);
                });

            WriteOrange("Test #2. Get fan #1");
            Do(device, d =>
            {
                var data = new byte[] { 0x41, 0x00 };

                d.Write(data);
                var res = d.Read();

                WriteBytes("Write", data);
                WriteBytes("Read", res);
            });

            WriteOrange("Test #3. Get fan #2");
            Do(device, d =>
            {
                var data = new byte[] { 0x41, 0x01 };

                d.Write(data);
                var res = d.Read();

                WriteBytes("Write", data);
                WriteBytes("Read", res);
            });

            WriteOrange("Test #4. Get fan #3");
            Do(device, d =>
            {
                var data = new byte[] { 0x41, 0x02 };

                d.Write(data);
                var res = d.Read();

                WriteBytes("Write", data);
                WriteBytes("Read", res);
            });

            WriteOrange("Test #5. Get pump");
            Do(device, d =>
            {
                var data = new byte[] { 0x31 };

                d.Write(data);
                var res = d.Read();

                WriteBytes("Write", data);
                WriteBytes("Read", res);
            });

            WriteOrange("Test #6. Enable external temp");
            Do(device, d =>
            {
                var data = new byte[] { 0xa4, 0x01 };

                d.Write(data);
                var res = d.Read();

                WriteBytes("Write", data);
                WriteBytes("Read", res);
            });

            WriteOrange("Test #7. Set external temp");
            Do(device, d =>
            {
                var data = new byte[] { 0xa5, 0x24, 0x00 };

                d.Write(data);
                var res = d.Read();

                WriteBytes("Write", data);
                WriteBytes("Read", res);
            });
        }

        private static void Do(SI_Device device, Action<SI_Device> action)
        {
            try
            {
                device.EnsureOpened();
            }
            catch (Exception ex)
            {
                WriteError(device.Pid + " Open Error: " + ex.Message);
            }
            if (device.IsOpened())
            {
                WriteGreen(device.Pid + " Opened! Handle: " + device.DeviceHandle.DangerousGetHandle());

                action(device);
            }
            else
            {
                WriteYellow(device.Pid + " NOT opened!");
            }

            try
            {
                device.Close();
                WriteGreen(device.Pid + " Closed without errors");
            }
            catch (Exception ex)
            {
                WriteError(device.Pid + " Close Error: " + ex.Message);
            }
        }

        private static void WriteError(string msg)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine(msg);
            Console.ForegroundColor = ConsoleColor.Gray;
        }

        private static void WriteYellow(string msg)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine(msg);
            Console.ForegroundColor = ConsoleColor.Gray;
        }

        private static void WriteGreen(string msg)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine(msg);
            Console.ForegroundColor = ConsoleColor.Gray;
        }

        private static void WriteOrange(string msg)
        {
            Console.ForegroundColor = ConsoleColor.DarkYellow;
            Console.WriteLine(msg);
            Console.ForegroundColor = ConsoleColor.Gray;
        }

        private static void Write(string msg)
        {
            Console.WriteLine(msg);
        }

        private static void WriteBytes(string comment, byte[] data)
        {
            WriteGreen(comment + " " + string.Join(" ", data.Select(d => d.ToString("X2"))));
        }
    }
}
