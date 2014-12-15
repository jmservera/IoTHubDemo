using Netsaimada.IoT.Cloud.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Netsaimada.IoT.CloudService.Receiver.Dal
{
    public class ComputerTelemetries:BatchedStorage<ProcessorTelemetryData>
    {
        public ComputerTelemetries():base("processorTelemetry")
        {

        }
    }
}
