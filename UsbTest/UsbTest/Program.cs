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
            if (args.Contains("reg"))
            {
                RegistryHelper.SetupApplicationRunAtStartup(true);
                Console.WriteLine("Added to startup!");
            }
            else if (args.Contains("unreg"))
            {
                RegistryHelper.SetupApplicationRunAtStartup(false);
                Console.WriteLine("Removed from startup!");
            }

            var devices = Devices.Enumerate().ToList();

            Console.WriteLine("Count {0}:", devices.Count);

            foreach (var device in devices)
            {
                Console.WriteLine(device);
            }

            Console.WriteLine();

            int attempt = 1;

            while (true)
            {
                Console.WriteLine("Attempt = {0}", attempt++);
                foreach (var device in devices)
                {
                    try
                    {
                        device.EnsureOpened();
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("EnsureOpened Error: " + ex.Message);
                    }
                    if (device.IsOpened())
                    {
                        Console.WriteLine(device.Pid + " is opened! " + device.DeviceHandle.DangerousGetHandle());
                    }
                    else
                        Console.WriteLine(device.Pid + " is NOT opened!");

                    try
                    {
                        device.Close();
                    }
                    catch(Exception ex)
                    {
                        Console.WriteLine("Close Error: " + ex.Message);
                    }
                }

                Console.WriteLine();
                Thread.Sleep(500);
            }
        }
    }
}
