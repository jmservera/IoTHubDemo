using Microsoft.ServiceBus.Messaging;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Netsaimada.IoT.CloudService.Receiver
{
    public class EventProcessorFactory : IEventProcessorFactory
    {
        private readonly ConcurrentDictionary<string, EventProcessor> eventProcessors = new ConcurrentDictionary<string, EventProcessor>();

        public EventProcessorFactory(string hostname)
        {
            this.HostName = hostname;
        }

        public string HostName { get; private set; }

        public int ActiveProcesors
        {
            get
            {
                return this.eventProcessors.Count;
            }
        }


        public IEventProcessor CreateEventProcessor(PartitionContext context)
        {
            var processor = new EventProcessor();
            processor.ProcessorClosed += this.ProcessorOnProcessorClosed;
            this.eventProcessors.TryAdd(context.Lease.PartitionId, processor);
            return processor;
        }

        public Task WaitForAllProcessorsInitialized(TimeSpan timeout)
        {
            return this.WaitForAllProcessorsCondition(p => p.IsInitialized, timeout);
        }

        public Task WaitForAllProcessorsClosed(TimeSpan timeout)
        {
            return this.WaitForAllProcessorsCondition(p => p.IsClosed, timeout);
        }

        public async Task WaitForAllProcessorsCondition(Func<EventProcessor, bool> predicate, TimeSpan timeout)
        {
            TimeSpan sleepInterval = TimeSpan.FromSeconds(2);
            while (!this.eventProcessors.Values.All(predicate))
            {
                if (timeout > TimeSpan.Zero)
                {
                    timeout = timeout.Subtract(sleepInterval);
                }
                else
                {
                    throw new TimeoutException("Condition not satisfied within expected timeout.");
                }

                await Task.Delay(sleepInterval);
            }
        }

        private void ProcessorOnProcessorClosed(object sender, EventArgs eventArgs)
        {
            var processor = sender as EventProcessor;
            if (processor != null)
            {
                this.eventProcessors.TryRemove(processor.Context.Lease.PartitionId, out processor);
            }
        }
    }
}
