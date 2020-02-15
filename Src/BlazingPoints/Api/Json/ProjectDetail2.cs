using Newtonsoft.Json;

namespace BlazingPoints.Api.Json
{
    public class ProjectDetail2
    {
        [JsonIgnore]
        public int count { get; set; }

        [JsonProperty("value")]
        public ValuePD2[] valuePD2 { get; set; }
    }

    public class ValuePD2
    {
        public string name { get; set; }
        public object value { get; set; }
    }
}
