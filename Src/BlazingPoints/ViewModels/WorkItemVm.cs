using System;

namespace BlazingPoints.ViewModels
{
    public class WorkItemVm
    {
        public DateTime AsOf { get; set; }
        public float? Effort { get; set; }
        public int? Id { get; set; }
        public string State { get; set; }
    }
}
