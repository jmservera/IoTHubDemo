using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Threading;
using Microsoft.ServiceBus;
using Microsoft.ServiceBus.Messaging;
using Microsoft.WindowsAzure;
using Microsoft.WindowsAzure.ServiceRuntime;
using System.Threading.Tasks;

namespace Netsaimada.IoT.CloudService.Receiver
{
    public class WorkerRole : RoleEntryPoint
    {
        // The name of your queue
        const string QueueName = "ProcessingQueue";

        private readonly CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
        private readonly ManualResetEvent runCompleteEvent = new ManualResetEvent(false);
        private EventProcessorHost _host;
        EventHubClient client;

        // QueueClient is thread-safe. Recommended that you cache 
        // rather than recreating it on every request
        //QueueClient Client;
        ManualResetEvent CompletedEvent = new ManualResetEvent(false);

        public override void Run()
        {
            Trace.TraceInformation("WorkerRole1 is running");

            try
            {
                this.RunAsync(this.cancellationTokenSource.Token).Wait();
            }
            finally
            {
                this.runCompleteEvent.Set();
            }
        }

        public override bool OnStart()
        {
            // Set the maximum number of concurrent connections 
            ServicePointManager.DefaultConnectionLimit = 12;

            // Create the queue if it does not exist already
            string serviceBusConnectionString = CloudConfigurationManager.GetSetting("Microsoft.ServiceBus.ConnectionString");
            string storageConnectionString = CloudConfigurationManager.GetSetting("Microsoft.WindowsAzure.Storage.ConnectionString");
            
            var namespaceManager = NamespaceManager.CreateFromConnectionString(serviceBusConnectionString);
              
                          Trace.TraceInformation("WorkerRole1 has been started");

             string eventHubName = "telemetry";
 
              client = EventHubClient.Create(eventHubName);
             Trace.TraceInformation("Consumer group is: " + client.GetDefaultConsumerGroup().GroupName);
 
             _host = new EventProcessorHost("Worker"+new Guid().ToString(), eventHubName, client.GetDefaultConsumerGroup().GroupName,
                 serviceBusConnectionString, storageConnectionString);
 
             Trace.TraceInformation("Created event processor host...");

            return base.OnStart();
        }

        public override void OnStop()
        {
            // Close the connection to Service Bus Queue
            client.Close();
            CompletedEvent.Set();
            base.OnStop();
        }

        private async Task RunAsync(CancellationToken cancellationToken)
        {
            await _host.RegisterEventProcessorAsync<EventProcessor>();

            // TODO: Replace the following with your own logic.
            while (!cancellationToken.IsCancellationRequested)
            {
                //Trace.TraceInformation("Working");
                await Task.Delay(1000);
            }
        }
    }
}