using Microsoft.ServiceBus.Messaging;
using Microsoft.WindowsAzure.Storage.Table;
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

        CloudTableClient _tableClient;

        public async Task CloseAsync(PartitionContext context, CloseReason reason)
        {
            Console.WriteLine(string.Format("Processor Shuting Down.  Partition '{0}', Reason: '{1}'.", this.partitionContext.Lease.PartitionId, reason.ToString()));
            if (reason == CloseReason.Shutdown)
            {
                await context.CheckpointAsync();
            }
        }

        public Task OpenAsync(PartitionContext context)
        {
            Console.WriteLine(string.Format("SimpleEventProcessor initialize.  Partition: '{0}', Offset: '{1}'", context.Lease.PartitionId, context.Lease.Offset));
            this.partitionContext = context;
            this.checkpointStopWatch = new Stopwatch();
            this.checkpointStopWatch.Start();
            return Task.FromResult<object>(null);
        }

        public async Task ProcessEventsAsync(PartitionContext context, IEnumerable<EventData> messages)
        {
            try
             {
                 foreach (EventData eventData in messages)
                 {
                     Console.WriteLine("Processing event hub data...");
 
                     string key = eventData.PartitionKey;
 
                     string data = System.Text.Encoding.Unicode.GetString(eventData.GetBytes());
                     try
                     {
                         var json = JObject.Parse(data);
                         //string text = json["Message"].ToString();
                         //string agent = json["BrowserInfo"].ToString();
 
                         //if (queue != null)
                         //{
                             
                         //    await queue.SendAsync(new BrokeredMessage((agent + "##" + text)));
 
                         //    Trace.TraceInformation("Added to queue: " + agent);
                         //}
                     }
                     catch(Exception exx)
                     {
                         Console.WriteLine(exx.Message);
                     }
 
                     Console.WriteLine(string.Format("Message received.  Partition: '{0}', Device: '{1}'",
                         this.partitionContext.Lease.PartitionId, key));
                 }
 
                 //Call checkpoint every 5 minutes, so that worker can resume processing from the 1 minutes back if it restarts.
                 // Doing every ONE MINUTE now.
                 if (this.checkpointStopWatch.Elapsed > TimeSpan.FromMinutes(1))
                 {
                     await context.CheckpointAsync();
                     lock (this)
                     {
                         this.checkpointStopWatch.Reset();
                     }
                 }
             }
             catch (Exception exp)
             {
                 Console.WriteLine("Error in processing: " + exp.Message);
             }
        }
    }
}
