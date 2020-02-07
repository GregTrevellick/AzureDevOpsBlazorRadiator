using System;
using System.Collections.Generic;

namespace BlazingPoints.Api.DTOs
{
    public class SprintProgressDto
    {
        public string IterationNumber { get; set; }
        public DateTime SprintEnd { get; set; }
        public DateTime SprintStart { get; set; }
        public IList<WorkItemDto> WorkItemDtos { get; set; }
    }
}
