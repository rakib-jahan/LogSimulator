using RealTimeLogger;
using System;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace TestService
{
    class Program
    {
        static async Task Main(string[] args)
        {
            string serviceName = ConfigurationManager.AppSettings["ServiceName"].ToString();
            Logger logger = new Logger(serviceName);

            Random rnd = new Random();
            var random = rnd.Next(1, 5);

            if (random % 2 == 0)
            {
                await logger.Log(new Log()
                {
                    LogType = LogType.Success,
                    Cpu = GetCPUCounter(),
                    Memory = GetMemoryCounter(),
                });
            }
            else
            {
                await logger.Log(new Log()
                {
                    LogType = LogType.Error,
                    Cpu = GetCPUCounter(),
                    Memory = GetMemoryCounter(),
                    Disk = GetDiskCounter(),
                    FailedDetails = GetException(random)
                });
            }
        }

        private static string GetException(int random)
        {
            switch (random)
            {
                case 1:
                    return "Null Reference Exception Occuered";
                case 2:
                    return "Invalid Cast Exception Occuered";
                case 3:
                    return "Out Of Memory Exception Occuered";
                case 4:
                    return "Stack Overflow Exception Occuered";
                case 5:
                    return "Unknown error Occuered";
                default:
                    break;
            }

            return string.Empty;
        }

        public static double GetCPUCounter()
        {
            PerformanceCounter cpuCounter = new PerformanceCounter("Processor", "% Processor Time", "_Total");
            float firstValue = cpuCounter.NextValue();
            Thread.Sleep(1000);
            float secondValue = cpuCounter.NextValue();
            return Math.Round(secondValue / 100 * 3.6, 2);
        }

        public static double GetMemoryCounter()
        {
            PerformanceCounter cpuCounter = new PerformanceCounter("Memory", "% Committed Bytes In Use");
            float firstValue = cpuCounter.NextValue();
            Thread.Sleep(1000);
            float secondValue = cpuCounter.NextValue();
            return Math.Round(secondValue / 100 * 16, 2);
        }

        public static double GetDiskCounter()
        {
            DriveInfo dDrive = new DriveInfo("C");
            float freeSpacePerc = 0;
            if (dDrive.IsReady)
            {
                freeSpacePerc = ((dDrive.TotalSize / 1024) / 1024) / 1024 - ((dDrive.AvailableFreeSpace / 1024) / 1024) / 1024;
            }
            return freeSpacePerc;
        }
    }
}