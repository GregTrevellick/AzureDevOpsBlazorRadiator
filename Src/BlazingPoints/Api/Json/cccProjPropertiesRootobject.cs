using Newtonsoft.Json;

namespace BlazingPoints.Api.Json2.ProjProperties.ccc
{
    public class cccProjPropertiesRootobject//gregt rename
    {
        [JsonIgnore]
        public int count { get; set; }

        public Value[] value { get; set; }
    }

    public class Value
    {
        public string typeId { get; set; }
        public string name { get; set; }
        public object referenceName { get; set; }
        public string description { get; set; }
        public string parentProcessTypeId { get; set; }
        public bool isEnabled { get; set; }
        public bool isDefault { get; set; }
        public string customizationType { get; set; }
    }

}
