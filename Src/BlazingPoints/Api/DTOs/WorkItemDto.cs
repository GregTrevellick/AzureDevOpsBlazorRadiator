using System;

namespace BlazingPoints.Api.DTOs
{
    public class WorkItemDto
    {
        public DateTime AsOf { get; set; }
        public float? Effort { get; set; }
        public int? Id { get; set; }
        public string State { get; set; }
    }
}
