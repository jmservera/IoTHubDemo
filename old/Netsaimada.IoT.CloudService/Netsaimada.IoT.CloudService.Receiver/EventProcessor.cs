namespace Netsaimada.IoT.CloudService.Receiver
{
    using Microsoft.ServiceBus.Messaging;
    using Microsoft.WindowsAzure.Storage;
    using Netsaimada.IoT.Cloud.Models;
    using Netsaimada.IoT.CloudService.Receiver.Dal;
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Threading.Tasks;

    public class EventProcessor : IEventProcessor
    {
        public PartitionContext Context { get; private set; }
        public event EventHandler ProcessorClosed;
        public bool IsInitialized { get; private set; }
        public bool IsClosed { get; private set; }

        public bool IsReceivedMessageAfterClose { get; set; }
        Stopwatch checkpointStopWatch;

        BatchedStorage<MouseTelemetryData> _mouseTelemetries = new BatchedStorage<MouseTelemetryData>("mouseTelemetryLogs");
        BatchedStorage<ProcessorTelemetryData> _computerTelemetries = new BatchedStorage<ProcessorTelemetryData>("processorTelemetry");

        public EventProcessor()
        {
        }

        public Task CloseAsync(PartitionContext context, CloseReason reason)
        {
            Trace.TraceInformation("Processor Shuting Down.  Partition '{0}', Reason: '{1}'.", this.Context.Lease.PartitionId, reason.ToString());

            this.IsClosed = true;
            this.checkpointStopWatch.Stop();
            this.OnProcessorClosed();

            return context.CheckpointAsync();
        }

        protected virtual void OnProcessorClosed()
        {
            if (this.ProcessorClosed != null)
            {
                this.ProcessorClosed(this, EventArgs.Empty);
            }
        }

        public async Task OpenAsync(PartitionContext context)
        {
            Trace.TraceInformation("SimpleEventProcessor initialize.  Partition: '{0}', Offset: '{1}'", context.Lease.PartitionId, context.Lease.Offset);

            var t1 = _mouseTelemetries.OpenAsync();
            var t2 = _computerTelemetries.OpenAsync();
            await Task.WhenAll(t1, t2);

            this.Context = context;
            this.checkpointStopWatch = new Stopwatch();
            this.checkpointStopWatch.Start();
            this.IsInitialized = true;

            Trace.TraceInformation("SimpleEventProcessor initialized!!!  Partition: '{0}', Offset: '{1}'", context.Lease.PartitionId, context.Lease.Offset);
        }

        public async Task ProcessEventsAsync(PartitionContext context, IEnumerable<EventData> messages)
        {
            bool doCheckpoint = false;
            try
            {
                Trace.TraceInformation("Processing event hub data for {0} messages...", messages.Count());
                foreach (EventData eventData in messages)
                {
                    string macAddress = eventData.PartitionKey;

                    string data = System.Text.Encoding.UTF8.GetString(eventData.GetBytes());
                    try
                    {
                        //var json = JObject.Parse(data);
                        dynamic jsonData = Newtonsoft.Json.JsonConvert.DeserializeObject(data);

                        if (jsonData.date != null)
                        {
                            DateTime dateStamp = jsonData.date;

                            switch ((string)jsonData.eventType)
                            {
                                case "Mouse":
                                    {
                                        int x = jsonData.x;
                                        int y = jsonData.y;
                                        MouseTelemetryData telemetryData = new MouseTelemetryData(macAddress, dateStamp, x, y);
                                        _mouseTelemetries.Add(telemetryData);
                                        break;
                                    }
                                case "Processor":
                                    {
                                        float cpu = jsonData.cpu;
                                        float memory = jsonData.memory;
                                        _computerTelemetries.Add(new ProcessorTelemetryData(macAddress, dateStamp, cpu, memory));
                                        break;
                                    }
                                default:
                                    Trace.TraceInformation("Unknnown event {0}", data);
                                    break;
                            }
                        }
                        Trace.TraceInformation("Message received.  Partition: '{0}', Device: '{1}''",
                               this.Context.Lease.PartitionId, macAddress);
                    }
                    catch (Exception exx)
                    {
                        Trace.TraceError(exx.Message);
                    }
                }

                if (this.IsClosed)
                {
                    this.IsReceivedMessageAfterClose = true;
                }
                await Task.WhenAll(_mouseTelemetries.SaveAsync(), _computerTelemetries.SaveAsync());
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
                Trace.TraceError("Error in processing: {0}", exp.Message);
            }

            if (doCheckpoint)
            {
                await context.CheckpointAsync();
            }
        }
    }
}