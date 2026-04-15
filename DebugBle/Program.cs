using System;
using System.Threading;

namespace DebugBle
{
    class Program
    {
        static string targetDeviceMac = "40:06:a0:66:7b:dd";
        static string deviceId = null;
        static bool found = false;

        static void Main(string[] args)
        {
            Console.WriteLine("Starting BLE scan...");

            // ---------------- START SCAN ----------------
            var scan = BLE.ScanDevices();

            scan.Found = (id, name) =>
            {
                Console.WriteLine($"Found: {name} -> {id}");

                if (id != null && id.ToLower().Contains(targetDeviceMac))
                {
                    deviceId = id;
                    found = true;
                    Console.WriteLine("TARGET DEVICE FOUND");
                }
            };

            scan.Finished = () =>
            {
                Console.WriteLine("SCAN FINISHED");
            };

            // ---------------- WAIT FOR DEVICE ----------------
            while (!found)
                Thread.Sleep(200);

            scan.Cancel();

            if (string.IsNullOrEmpty(deviceId))
            {
                Console.WriteLine("Device not found.");
                return;
            }

            Console.WriteLine("\nSelected device:");
            Console.WriteLine(deviceId);

            Console.WriteLine("\nWaiting 2 seconds before reading...");
            Thread.Sleep(2000);

            // ---------------- TEST READ LOOP ----------------
            Console.WriteLine("\nStarting FFF5 read test (every 500ms)\n");

            while (true)
            {
                try
                {
                    BLE.TestReadFFF5(deviceId);
                }
                catch (Exception e)
                {
                    Console.WriteLine("ERROR: " + e.Message);
                }

                Thread.Sleep(500);
            }
        }
    }
}