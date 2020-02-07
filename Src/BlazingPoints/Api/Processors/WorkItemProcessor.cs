using BlazingPoints.Api.Json;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace BlazingPoints.Api.Processors
{
    public class WorkItemProcessor
    {
        public IEnumerable<Workitem> GetWorkItemsByJson(string workItemJson)
        {
            if (string.IsNullOrEmpty(workItemJson))
            {
                //workItemJson is null when the sprint date is in future 
                return new List<Workitem>();
            }
            else
            {
                var witRootobject = JsonConvert.DeserializeObject<witRootobject>(workItemJson);
                return witRootobject.workItems;
            }
        }

        public Json2.batchesRootobject GetWorkItemAttributesBatchesByJson(string workItemAttributesJson)
        {
            return JsonConvert.DeserializeObject<Json2.batchesRootobject>(workItemAttributesJson);
        }
    }
}
