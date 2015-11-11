using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Netsaimada.IoT.Cloud.Models
{
    public class ProcessorTelemetryData : TableEntity
    {
        public ProcessorTelemetryData(string partition, DateTime date, double processor, double ram)
        {
            this.PartitionKey = partition;
            this.RowKey =date.ToString("o");
            Cpu = processor;
            Ram = ram;
        }
        public ProcessorTelemetryData() { }

        public double Cpu { get; set; }
        public double Ram { get; set; }
    }
}
