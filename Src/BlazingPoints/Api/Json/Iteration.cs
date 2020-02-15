using Newtonsoft.Json;
using System;

namespace BlazingPoints.Api.Json
{
    public class Iteration
    {
        [JsonIgnore]
        public int count { get; set; }

        [JsonProperty("value")]
        public ValueI[] valueI { get; set; }
    }

    public class ValueI
    {
        public string id { get; set; }
        
        public string name { get; set; }
        
        [JsonIgnore] 
        public string path { get; set; }
        
        public Attributes attributes { get; set; }

        [JsonIgnore] 
        public string url { get; set; }
    }

    public class Attributes
    {
        public DateTime startDate { get; set; }
        public DateTime finishDate { get; set; }
        public string timeFrame { get; set; }
    }
}
