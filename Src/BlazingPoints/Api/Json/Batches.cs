using Newtonsoft.Json;

namespace BlazingPoints.Api.Json
{
    public class Batches
    {
        [JsonIgnore]
        public int count { get; set; }

        [JsonProperty("value")]
        public ValueB[] valueB { get; set; }
    }

    public class ValueB
    {
        public int id { get; set; }

        [JsonIgnore]
        public int rev { get; set; }

        [JsonProperty("fields")]
        public FieldsB fieldsB { get; set; }
        
        [JsonIgnore]
        public string url { get; set; }
    }

    public class FieldsB
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
