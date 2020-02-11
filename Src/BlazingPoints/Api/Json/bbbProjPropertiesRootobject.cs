using Newtonsoft.Json;

namespace BlazingPoints.Api.Json2.ProjProperties
{
    public class bbbProjPropertiesRootobject
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
