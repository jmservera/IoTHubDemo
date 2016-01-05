using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DhtView
{
    /// <summary>
    /// A POCO object to send the information via JSON
    /// "{{\"guid\":\"{0}\", \"organization\":\"{1}\", \"displayname\": \"{2}\", \"location\": \"{3}\", \"measurename\": 
    /// \"{4}\", \"unitofmeasure\": \"{5}\", \"value\":{6}, \"timecreated\":\"{7}\" }}";
    /// </summary>
    public sealed class SensorInfo
    {
        public string Guid { get; set; }
        public string Organization { get; set; }
        public string Displayname { get; set; }
        public string Location { get; set; }
        public string Measurename { get; set; }
        public string Unitofmeasure { get; set; }
        public double Value { get; set; }
        public DateTime Timecreated { get; set; }

    }
}
