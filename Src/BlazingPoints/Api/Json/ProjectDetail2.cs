using Newtonsoft.Json;

namespace BlazingPoints.Api.Json.ProjectDetail2
{
    public class ProjectDetail2
    {
        [JsonIgnore]
        public int count { get; set; }

        public Value[] value { get; set; }
    }

    public class Value
    {
        public string name { get; set; }
        public object value { get; set; }
    }
}
