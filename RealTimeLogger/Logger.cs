using Google.Cloud.Firestore;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RealTimeLogger
{
    public class Logger
    {
        private FirestoreDb _db;
        private string _serviceName;

        public Logger(string serviceName)
        {
            _serviceName = serviceName;
            string path = AppDomain.CurrentDomain.BaseDirectory + @"firebase-adminsdk.json";
            Environment.SetEnvironmentVariable("GOOGLE_APPLICATION_CREDENTIALS", path);
            _db = FirestoreDb.Create("rakib-test-project");
        }

        public async Task Log(Log log)
        {
            try
            {
                if (log.LogType == LogType.Success)
                    await SuccessLog(log);
                else
                    await ErrorLog(log);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task<CpuConfiguration> GetServiceCpuConfiguration()
        {
            try
            {
                DocumentReference docRef = _db.Collection("services").Document(_serviceName);
                DocumentSnapshot snapshot = await docRef.GetSnapshotAsync();
                if (snapshot.Exists)
                {
                    return snapshot.ConvertTo<CpuConfiguration>();
                }
                else
                {
                    return new CpuConfiguration();
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task UpdateServiceCpuConfiguration(LogType logType)
        {
            try
            {
                var cpuConfig = await GetServiceCpuConfiguration();

                Dictionary<string, object> config = new Dictionary<string, object>
                {
                    { "successCount", logType == LogType.Success ? cpuConfig.successCount + 1 : cpuConfig.successCount},
                    { "failedCount", logType == LogType.Error ? cpuConfig.failedCount + 1 : cpuConfig.failedCount},
                    { "lastUpdate", DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Utc)}
                };
                DocumentReference empRef = _db.Collection("services").Document(_serviceName);
                await empRef.UpdateAsync(config);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task SuccessLog(Log log)
        {
            CollectionReference logRef = _db.Collection("services").Document(_serviceName).Collection("all-log");

            Dictionary<string, object> successlog = new Dictionary<string, object>
                {
                    { "cpu", log.Cpu },
                    { "createdDate", DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Utc) },
                    { "logType", LogType.Success },
                    { "memory", log.Memory}
                };

            await logRef.AddAsync(successlog);
            await UpdateServiceCpuConfiguration(log.LogType);
        }

        public async Task ErrorLog(Log log)
        {
            CollectionReference logRef = _db.Collection("services").Document(_serviceName).Collection("all-log");

            Dictionary<string, object> errorlog = new Dictionary<string, object>
                {
                    { "cpu", log.Cpu },
                    { "createdDate", DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Utc) },
                    { "logType", LogType.Error },
                    { "disk", log.Disk},
                    { "memory", log.Memory},
                    { "failedDetails", log.FailedDetails}
                };

            await logRef.AddAsync(errorlog);
        }
    }

    public enum LogType
    {
        Success = 1,
        Error = 2
    }

    public class Log
    {
        public double Cpu { get; set; }
        public double Disk { get; set; }
        public double Memory { get; set; }
        public LogType LogType { get; set; }
        public string FailedDetails { get; set; }
    }

    [FirestoreData]
    public class CpuConfiguration
    {
        [FirestoreProperty]
        public int successCount { get; set; }

        [FirestoreProperty]
        public int failedCount { get; set; }
    }
}