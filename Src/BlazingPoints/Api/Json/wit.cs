using Newtonsoft.Json;
using System;

namespace BlazingPoints.Api.Json
{
    public class Wit
    {
        public string queryType { get; set; }
        public string queryResultType { get; set; }
        public DateTime asOf { get; set; }
        [JsonIgnore] 
        public Column[] columns { get; set; }
        public Workitem[] workItems { get; set; }
    }

    public class Column
    {
        [JsonIgnore]
        public string referenceName { get; set; }
        [JsonIgnore]
        public string name { get; set; }
        [JsonIgnore]
        public string url { get; set; }
    }

    public class Workitem
    {
        public int id { get; set; }
        [JsonIgnore]
        public string url { get; set; }
    }

}
