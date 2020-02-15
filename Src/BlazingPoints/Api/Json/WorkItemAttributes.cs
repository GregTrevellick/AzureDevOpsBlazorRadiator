using Newtonsoft.Json;
using System;

namespace BlazingPoints.Api.Json
{
    public class WorkItemAttributes
    {
        public int id { get; set; }

        [JsonIgnore]
        public int rev { get; set; }

        [JsonProperty("fields")]
        public FieldsWIA fieldsWIA { get; set; }

        [JsonIgnore]
        public _Links2 _links { get; set; }

        [JsonIgnore]
        public string url { get; set; }
    }

    public class FieldsWIA
    {
        [JsonProperty("System.AreaPath")]
        public string SystemAreaPath { get; set; }

        [JsonProperty("System.TeamProject")]
        public string SystemTeamProject { get; set; }

        [JsonProperty("System.IterationPath")]
        public string SystemIterationPath { get; set; }

        [JsonProperty("System.WorkItemType")]
        public string SystemWorkItemType { get; set; }

        [JsonProperty("System.State")]
        public string SystemState { get; set; }

        [JsonIgnore]
        public string SystemReason { get; set; }

        [JsonIgnore]
        public DateTime SystemCreatedDate { get; set; }

        [JsonIgnore]
        public SystemCreatedby SystemCreatedBy { get; set; }

        [JsonIgnore]
        public DateTime SystemChangedDate { get; set; }

        [JsonIgnore]
        public SystemChangedby SystemChangedBy { get; set; }

        [JsonIgnore]
        public int SystemCommentCount { get; set; }

        [JsonProperty("System.Title")]
        public string SystemTitle { get; set; }
        
        [JsonProperty("Microsoft.VSTS.Scheduling.StoryPoints")]       
        public float MicrosoftVSTSSchedulingStoryPoints { get; set; }
        
        [JsonProperty("Microsoft.VSTS.Scheduling.Effort")]
        public float MicrosoftVSTSSchedulingEffort { get; set; }
        
        [JsonIgnore]      
        public DateTime MicrosoftVSTSCommonStateChangeDate { get; set; }
        
        [JsonIgnore]
        public int MicrosoftVSTSCommonPriority { get; set; }

        [JsonIgnore]
        public string MicrosoftVSTSCommonValueArea { get; set; }

        [JsonIgnore]
        public string SystemDescription { get; set; }
    }

    public class SystemCreatedby
    {
        [JsonIgnore]
        public string displayName { get; set; }

        [JsonIgnore] 
        public string url { get; set; }

        [JsonIgnore] 
        [JsonProperty("_links")]
        public _LinksWIA _linksWIA { get; set; }

        [JsonIgnore] 
        public string id { get; set; }

        [JsonIgnore] 
        public string uniqueName { get; set; }

        [JsonIgnore] 
        public string imageUrl { get; set; }

        [JsonIgnore] 
        public string descriptor { get; set; }
    }

    public class _LinksWIA
    {
        [JsonIgnore]
        public Avatar avatar { get; set; }
    }

    public class Avatar
    {
        [JsonIgnore]
        public string href { get; set; }
    }

    public class SystemChangedby
    {
        [JsonIgnore] 
        public string displayName { get; set; }

        [JsonIgnore] 
        public string url { get; set; }

        [JsonIgnore] 
        public _Links1 _links { get; set; }

        [JsonIgnore] 
        public string id { get; set; }

        [JsonIgnore] 
        public string uniqueName { get; set; }

        [JsonIgnore] 
        public string imageUrl { get; set; }
        
        [JsonIgnore] 
        public string descriptor { get; set; }
    }

    public class _Links1
    {
        [JsonIgnore]
        public Avatar1 avatar { get; set; }
    }

    public class Avatar1
    {
        [JsonIgnore]
        public string href { get; set; }
    }

    public class _Links2
    {
        [JsonIgnore]
        [JsonProperty("self")]
        public SelfWIA selfWIA { get; set; }
        
        [JsonIgnore]
        public Workitemrevisions workItemRevisions { get; set; }
        
        [JsonIgnore] 
        public Parent parent { get; set; }
    }

    public class SelfWIA
    {
        [JsonIgnore]
        public string href { get; set; }
    }

    public class Workitemrevisions
    {
        [JsonIgnore]
        public string href { get; set; }
    }

    public class Parent
    {
        [JsonIgnore]
        public string href { get; set; }
    }

}
