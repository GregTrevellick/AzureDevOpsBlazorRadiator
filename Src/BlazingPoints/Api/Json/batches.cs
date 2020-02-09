using Newtonsoft.Json;

namespace BlazingPoints.Api.Json2
{
    public class batchesRootobject
    {
        [JsonIgnore]
        public int count { get; set; }

        public Value[] value { get; set; }
    }

    public class Value
    {
        public int id { get; set; }

        [JsonIgnore]
        public int rev { get; set; }
        
        public Fields fields { get; set; }
        
        [JsonIgnore]
        public string url { get; set; }
    }

    public class Fields
    {
        [JsonProperty("System.Id")]
        public int SystemId { get; set; }

        [JsonProperty("System.WorkItemType")]
        public string SystemWorkItemType { get; set; }

        [JsonProperty("System.State")]
        public string SystemState { get; set; }

        [JsonProperty("System.Title")]
        public string SystemTitle { get; set; }

        [JsonProperty("Microsoft.VSTS.Scheduling.StoryPoints")]
        public float? MicrosoftVSTSSchedulingStoryPoints { get; set; }

        [JsonProperty("Microsoft.VSTS.Scheduling.Effort")]
        public float? MicrosoftVSTSSchedulingEffort { get; set; }

        [JsonProperty("Microsoft.VSTS.Scheduling.OriginalEstimate")]
        public float? MicrosoftVSTSSchedulingOriginalEstimate { get; set; }

        [JsonProperty("Microsoft.VSTS.Scheduling.Size")]
        public float? MicrosoftVSTSSchedulingSize { get; set; }
    }
}
