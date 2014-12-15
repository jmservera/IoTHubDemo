using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Netsaimada.IoT.Cloud.Models
{
    public class MouseTelemetryData:TableEntity
    {
        public MouseTelemetryData(string partition,  DateTime date,int x, int y)
        {
            this.PartitionKey = partition;
            this.RowKey =date.ToString("o");
            X = x;
            Y = y;
        }

        public MouseTelemetryData() { }

        public int X { get; set; }
        public int Y { get; set; }
    }
}
