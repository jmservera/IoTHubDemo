using Microsoft.ServiceBus.Messaging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Netsaimada.IoT.CloudService.StatsViewer.Models
{
    public class QueueViewModel
    {
        public EventHubRuntimeInformation RuntimeInfo { get; set; }
    }
}