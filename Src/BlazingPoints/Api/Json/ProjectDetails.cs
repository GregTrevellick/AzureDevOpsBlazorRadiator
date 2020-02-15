using Newtonsoft.Json;
using System;

namespace BlazingPoints.Api.Json
{
    public class ProjectDetails
    {
        [JsonProperty("id")]
        public string ProjectId { get; set; }

        public string name { get; set; }

        [JsonIgnore]
        public string url { get; set; }

        [JsonIgnore]
        public string state { get; set; }

        [JsonIgnore]
        public int revision { get; set; }

        [JsonIgnore]
        [JsonProperty("_links")]
        public _LinksPD _linksPD { get; set; }

        [JsonIgnore]
        public string visibility { get; set; }

        [JsonIgnore]
        public Defaultteam defaultTeam { get; set; }

        [JsonIgnore]
        public DateTime lastUpdateTime { get; set; }
    }

    public class _LinksPD
    {
        [JsonIgnore]
        [JsonProperty("self")]
        public SelfPD selfPD { get; set; }
        
        [JsonIgnore]
        public Collection collection { get; set; }
        
        [JsonIgnore]
        public Web web { get; set; }
    }

    public class SelfPD
    {
        [JsonIgnore]
        public string href { get; set; }
    }

    public class Collection
    {
        [JsonIgnore]
        public string href { get; set; }
    }

    public class Web
    {
        [JsonIgnore]
        public string href { get; set; }
    }

    public class Defaultteam
    {
        [JsonIgnore]
        public string id { get; set; }

        [JsonIgnore]
        public string name { get; set; }
        
        [JsonIgnore]
        public string url { get; set; }
    }

}
