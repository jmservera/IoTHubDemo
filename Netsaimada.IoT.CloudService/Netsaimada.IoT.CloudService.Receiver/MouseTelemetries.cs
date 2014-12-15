using Microsoft.WindowsAzure;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;
using Netsaimada.IoT.Cloud.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Netsaimada.IoT.CloudService.Receiver
{
    public class MouseTelemetries
    {
        static object _lock = new object();
        static CloudStorageAccount _storageAccount;
        string _telemetryLogsTableName = "mouseTelemetryLogs";
        TableBatchOperation _batch;


        public MouseTelemetries()
        {
            if (_storageAccount == null)
            {
                lock (_lock)
                {
                    if (_storageAccount == null)
                    {
                        _storageAccount = CloudStorageAccount.Parse(
                           CloudConfigurationManager.GetSetting("StorageConnectionString"));
                    }
                }
            }
        }

        public async Task OpenAsync()
        {
            try
            {
                CloudTableClient client = _storageAccount.CreateCloudTableClient();
                CloudTable table = client.GetTableReference(_telemetryLogsTableName);
                await table.CreateIfNotExistsAsync();
            }
            catch (Exception ex)
            {
                Trace.TraceError("{0} \t{1}", ex.Message, ex.StackTrace);
            }
        }        

        public void Add(MouseTelemetryData data)
        {
            lock (_lock)
            {
                if (_batch == null)
                {
                    _batch = new TableBatchOperation();
                }
                _batch.Add(TableOperation.Insert(data));
            }
        }

        public async Task SaveAsync()
        {
            TableBatchOperation batch;
            lock (_lock)
            {
                batch = _batch;
                _batch = null;
            }
            var tableClient = _storageAccount.CreateCloudTableClient();
            var table = tableClient.GetTableReference(_telemetryLogsTableName);
            var result = await table.ExecuteBatchAsync(batch);
        }
    }
}
