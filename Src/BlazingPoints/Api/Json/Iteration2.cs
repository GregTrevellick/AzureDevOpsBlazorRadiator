using Newtonsoft.Json;
using System;

namespace BlazingPoints.Api.Json
{
    public class Iteration
    {
        [JsonIgnore]
        public int count { get; set; }
        
        public Value[] value { get; set; }
    }

    public class Value
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
