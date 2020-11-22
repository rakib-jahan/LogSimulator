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
            Processor processor = new Processor();
            
            try
            {
                processor.Process();

                await logger.Log(new Log()
                {
                    LogType = LogType.Success,
                    Cpu = GetCPUCounter(),
                    Memory = GetMemoryCounter(),
                });
            }
            catch (System.Exception ex)
            {
                await logger.Log(new Log()
                {
                    LogType = LogType.Error,
                    Cpu = GetCPUCounter(),
                    Memory = GetMemoryCounter(),
                    Disk = GetDiskCounter(),
                    FailedDetails = ex.Message
                });

                throw ex;
            }
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

    public class Processor
    {
        public void Process()
        {
        }
    }
}