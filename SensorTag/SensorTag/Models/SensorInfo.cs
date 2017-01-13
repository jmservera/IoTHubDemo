using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SensorTag.Models
{
    /// <summary>
    /// A POCO object to send the information via JSON
    /// "{{\"guid\":\"{0}\", \"organization\":\"{1}\", \"displayname\": \"{2}\", \"location\": \"{3}\", \"measurename\": 
    /// \"{4}\", \"unitofmeasure\": \"{5}\", \"value\":{6}, \"timecreated\":\"{7}\" }}";
    /// </summary>
    public sealed class SensorInfo
    {
        [Newtonsoft.Json.JsonProperty(PropertyName = "guid")]
        public string Guid { get; set; }
        [Newtonsoft.Json.JsonProperty(PropertyName = "organization")]
        public string Organization { get; set; }
        [Newtonsoft.Json.JsonProperty(PropertyName = "displayname")]
        public string DisplayName { get; set; }
        [Newtonsoft.Json.JsonProperty(PropertyName = "location")]
        public string Location { get; set; }
        [Newtonsoft.Json.JsonProperty(PropertyName = "measurename")]
        public string MeasureName { get; set; }
        [Newtonsoft.Json.JsonProperty(PropertyName = "unitofmeasure")]
        public string UnitOfMeasure { get; set; }
        [Newtonsoft.Json.JsonProperty(PropertyName = "value")]
        public double Value { get; set; }
        [Newtonsoft.Json.JsonProperty(PropertyName = "timecreated")]
        public DateTime TimeCreated { get; set; }

    }
}
