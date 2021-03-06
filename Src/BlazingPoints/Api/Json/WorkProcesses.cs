﻿using Newtonsoft.Json;

namespace BlazingPoints.Api.Json
{
    public class WorkProcesses
    {
        [JsonIgnore]
        public int count { get; set; }

        [JsonProperty("value")]
        public ValueWP[] valueWP { get; set; }
    }

    public class ValueWP
    {
        public string typeId { get; set; }
        public string name { get; set; }
        public object referenceName { get; set; }
        public string description { get; set; }

        [JsonIgnore]
        public string parentProcessTypeId { get; set; }

        [JsonIgnore]
        public bool isEnabled { get; set; }

        [JsonIgnore]
        public bool isDefault { get; set; }

        [JsonIgnore]
        public string customizationType { get; set; }
    }
}
