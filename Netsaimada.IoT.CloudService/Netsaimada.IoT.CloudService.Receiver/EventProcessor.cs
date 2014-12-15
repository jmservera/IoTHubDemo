using Microsoft.ServiceBus.Messaging;
using Microsoft.WindowsAzure;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;
using Netsaimada.IoT.Cloud.Models;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Netsaimada.IoT.CloudService.Receiver
{
    public class EventProcessor:IEventProcessor
    {
        PartitionContext partitionContext;
        Stopwatch checkpointStopWatch;

        CloudStorageAccount _storageAccount;
        string _telemetryLogsTableName = "telemetryLogs";

        public async Task CloseAsync(PartitionContext context, CloseReason reason)
        {
            Console.WriteLine(string.Format("Processor Shuting Down.  Partition '{0}', Reason: '{1}'.", this.partitionContext.Lease.PartitionId, reason.ToString()));
            if (reason == CloseReason.Shutdown)
            {
                await context.CheckpointAsync();
            }
        }

        public async Task OpenAsync(PartitionContext context)
        {
            Console.WriteLine(string.Format("SimpleEventProcessor initialize.  Partition: '{0}', Offset: '{1}'", context.Lease.PartitionId, context.Lease.Offset));

            _storageAccount = CloudStorageAccount.Parse(
               CloudConfigurationManager.GetSetting("StorageConnectionString"));
            CloudTableClient client = _storageAccount.CreateCloudTableClient();

            CloudTable table = client.GetTableReference(_telemetryLogsTableName);
            await table.CreateIfNotExistsAsync();

            this.partitionContext = context;
            this.checkpointStopWatch = new Stopwatch();
            this.checkpointStopWatch.Start();
            Console.WriteLine(string.Format("SimpleEventProcessor initialized!!!  Partition: '{0}', Offset: '{1}'", context.Lease.PartitionId, context.Lease.Offset));
        }

        public async Task ProcessEventsAsync(PartitionContext context, IEnumerable<EventData> messages)
        {
            bool doCheckpoint = false;
            try
             {
                 Trace.TraceInformation("Processing event hub data for {0} messages...", messages.Count());
                 var batch = new TableBatchOperation();
                 foreach (EventData eventData in messages)
                 {
                     string key = eventData.PartitionKey;
 
                     string data = System.Text.Encoding.UTF8.GetString(eventData.GetBytes());
                     try
                     {
                         var json = JObject.Parse(data);
                         dynamic jsonData = Newtonsoft.Json.JsonConvert.DeserializeObject(data);

                         if (jsonData.date != null)
                         {
                             int x = jsonData.x;
                             int y = jsonData.y;
                             DateTime d = jsonData.date;
                             MouseTelemetryData telemetryData = new MouseTelemetryData(key, d, x,y);
                             batch.Add(TableOperation.Insert(telemetryData));

                         }
                         //string text = json["Message"].ToString();
                         //string agent = json["BrowserInfo"].ToString();
 
                         //if (queue != null)
                         //{
                             
                         //    await queue.SendAsync(new BrokeredMessage((agent + "##" + text)));
 
                         //    Trace.TraceInformation("Added to queue: " + agent);
                         //}
                         Trace.TraceInformation("Message received.  Partition: '{0}', Device: '{1}'",
                                this.partitionContext.Lease.PartitionId, key);         
                     }
                     catch(Exception exx)
                     {
                         Trace.TraceError(exx.Message);
                     }
 

                 }
                 var tableClient = _storageAccount.CreateCloudTableClient();
                 var table = tableClient.GetTableReference(_telemetryLogsTableName);
                 var result =await table.ExecuteBatchAsync(batch);
                //if(result.)
                 await context.CheckpointAsync();

                 //Call checkpoint every 5 minutes, so that worker can resume processing from the 1 minutes back if it restarts.
                 // Doing every ONE MINUTE now.
                 //if (this.checkpointStopWatch.Elapsed > TimeSpan.FromMinutes(1))
                 //{
                 //    await context.CheckpointAsync();
                 //    lock (this)
                 //    {
                 //        this.checkpointStopWatch.Reset();
                 //    }
                 //}
             }
            catch (StorageException sex)
            {
                if (sex.RequestInformation.HttpStatusCode == 409)
                {
                    
                    //ignore repeated 
                    doCheckpoint = true;
                }
            }
             catch (Exception exp)
             {
                 Console.WriteLine("Error in processing: " + exp.Message);
             }
            if (doCheckpoint)
            {
                await context.CheckpointAsync();
            }
        }
    }
}
