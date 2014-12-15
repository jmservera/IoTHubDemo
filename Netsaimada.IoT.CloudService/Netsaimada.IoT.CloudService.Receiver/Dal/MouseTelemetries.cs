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

namespace Netsaimada.IoT.CloudService.Receiver.Dal
{
    public abstract class BatchedStorage<T> where T : class
    {
        protected static object _lock = new object();
        protected static CloudStorageAccount _storageAccount;
        protected TableBatchOperation _batch;
        string _tableName;

        public BatchedStorage(string tableName)
        {
            if(typeof(T).GetInterface(typeof(ITableEntity).FullName)==null)
            {
                throw new InvalidCastException(string.Format("{0} should implement interface ITableEntity", typeof(T).FullName));
            }
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
            _tableName=tableName;
        }
        public Task OpenAsync()
        {
            try
            {
                CloudTableClient client = _storageAccount.CreateCloudTableClient();
                CloudTable table = client.GetTableReference(_tableName);
                return table.CreateIfNotExistsAsync();
            }
            catch (Exception ex)
            {
                Trace.TraceError("{0} \t{1}", ex.Message, ex.StackTrace);
            }
            return Task.FromResult<object>(null);
        }

        public void Add(T data)
        {
            ITableEntity entity = data as ITableEntity;
            if (entity == null)
            {
                throw new ArgumentException("data should implement ITableEntity");
            }
            lock (_lock)
            {
                if (_batch == null)
                {
                    _batch = new TableBatchOperation();
                }
                _batch.Add(TableOperation.Insert(entity));
            }
        }
        public Task SaveAsync()
        {
            TableBatchOperation batch;
            lock (_lock)
            {
                batch = _batch;
                _batch = null;
            }
            if (batch!=null && batch.Count > 0)
            {
                var tableClient = _storageAccount.CreateCloudTableClient();
                var table = tableClient.GetTableReference(_tableName);
                return table.ExecuteBatchAsync(batch);
            }
            else
                return Task.FromResult<object>(null);
        }
    }
    public class MouseTelemetries : BatchedStorage<MouseTelemetryData>
    {
        const string _telemetryLogsTableName = "mouseTelemetryLogs";

        public MouseTelemetries():base(_telemetryLogsTableName)
        {
            
        }
    }
}
