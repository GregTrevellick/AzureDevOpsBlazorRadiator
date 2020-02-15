using BlazingPoints.Api.DTOs;
using System;
using System.Collections.Generic;

namespace BlazingPoints.Api
{
    public class MockData
    {
        public static SprintProgressDto GetSprintProgressDtoTest()
        {
            SprintProgressDto sprintProgressDto;

            var now = DateTime.Now.Date.AddDays(-1).AddHours(22).AddMinutes(59).AddSeconds(59);
            var i = -4;

            sprintProgressDto = new SprintProgressDto
            {
                IterationNumber = "Sprint 177",
                SprintEnd = now.AddDays(14),
                SprintStart = now.Date.AddDays(i),
                WorkItemDtos = new List<WorkItemDto>
                    {
                        // Day 1
                        new WorkItemDto { AsOf = now.AddDays(i+0), Effort = 100, Id = 1, State = "approved" },
                        new WorkItemDto { AsOf = now.AddDays(i+0), Effort = 0, Id = 1, State = "done" },
                      
                        // Day 2
                        new WorkItemDto { AsOf = now.AddDays(i+1), Effort = 100, Id = 1, State = "approved" },
                        new WorkItemDto { AsOf = now.AddDays(i+1), Effort = 20, Id = 1, State = "done" },
                        
                        // Day 3
                        new WorkItemDto { AsOf = now.AddDays(i+2), Effort = 100, Id = 1, State = "approved" },
                        new WorkItemDto { AsOf = now.AddDays(i+2), Effort = 18, Id = 1, State = "done" },
                        
                        // Day 4
                        new WorkItemDto { AsOf = now.AddDays(i+3), Effort = 100, Id = 1, State = "approved" },
                        new WorkItemDto { AsOf = now.AddDays(i+3), Effort = 41, Id = 1, State = "done" },
                        
                        // Day 5+
                        new WorkItemDto { AsOf = now.AddDays(i+4), Effort = 100, Id = 1, State = "approved" },
                        new WorkItemDto { AsOf = now.AddDays(i+4), Effort = 50, Id = 1, State = "done" },

                        new WorkItemDto { AsOf = now.AddDays(i+5), Effort = 100, Id = 1, State = "approved" },
                        new WorkItemDto { AsOf = now.AddDays(i+5), Effort = 75, Id = 1, State = "done" },

                        new WorkItemDto { AsOf = now.AddDays(i+6), Effort = 100, Id = 1, State = "approved" },
                        new WorkItemDto { AsOf = now.AddDays(i+6), Effort = 80, Id = 1, State = "done" },

                        new WorkItemDto { AsOf = now.AddDays(i+7), Effort = 100, Id = 1, State = "approved" },
                        new WorkItemDto { AsOf = now.AddDays(i+7), Effort = 77, Id = 1, State = "done" },

                        new WorkItemDto { AsOf = now.AddDays(i+8), Effort = 100, Id = 1, State = "approved" },
                        new WorkItemDto { AsOf = now.AddDays(i+8), Effort = 93, Id = 1, State = "done" },

                        new WorkItemDto { AsOf = now.AddDays(i+9), Effort = 100, Id = 1, State = "approved" },
                        new WorkItemDto { AsOf = now.AddDays(i+9), Effort = 95, Id = 1, State = "done" },

                        new WorkItemDto { AsOf = now.AddDays(i+10), Effort = 100, Id = 1, State = "approved" },
                        new WorkItemDto { AsOf = now.AddDays(i+10), Effort = 100, Id = 1, State = "done" },

                        new WorkItemDto { AsOf = now.AddDays(i+11), Effort = 100, Id = 1, State = "approved" },
                        new WorkItemDto { AsOf = now.AddDays(i+11), Effort = 100, Id = 1, State = "done" },

                        new WorkItemDto { AsOf = now.AddDays(i+12), Effort = 100, Id = 1, State = "approved" },
                        new WorkItemDto { AsOf = now.AddDays(i+12), Effort = 100, Id = 1, State = "done" },
                }
            };

            for (int j = 0; j < 22; j++)
            {
                sprintProgressDto.WorkItemDtos.Add(new WorkItemDto { AsOf = now.AddDays(i + 0), Effort = 0, Id = 1, State = "approved" });
                sprintProgressDto.WorkItemDtos.Add(new WorkItemDto { AsOf = now.AddDays(i + 1), Effort = 0, Id = 1, State = "approved" });
                sprintProgressDto.WorkItemDtos.Add(new WorkItemDto { AsOf = now.AddDays(i + 2), Effort = 0, Id = 1, State = "approved" });
                sprintProgressDto.WorkItemDtos.Add(new WorkItemDto { AsOf = now.AddDays(i + 3), Effort = 0, Id = 1, State = "approved" });
                sprintProgressDto.WorkItemDtos.Add(new WorkItemDto { AsOf = now.AddDays(i + 4), Effort = 0, Id = 1, State = "approved" });
                sprintProgressDto.WorkItemDtos.Add(new WorkItemDto { AsOf = now.AddDays(i + 5), Effort = 0, Id = 1, State = "approved" });
                sprintProgressDto.WorkItemDtos.Add(new WorkItemDto { AsOf = now.AddDays(i + 6), Effort = 0, Id = 1, State = "approved" });
                sprintProgressDto.WorkItemDtos.Add(new WorkItemDto { AsOf = now.AddDays(i + 7), Effort = 0, Id = 1, State = "approved" });
                sprintProgressDto.WorkItemDtos.Add(new WorkItemDto { AsOf = now.AddDays(i + 8), Effort = 0, Id = 1, State = "approved" });
                sprintProgressDto.WorkItemDtos.Add(new WorkItemDto { AsOf = now.AddDays(i + 9), Effort = 0, Id = 1, State = "approved" });
                sprintProgressDto.WorkItemDtos.Add(new WorkItemDto { AsOf = now.AddDays(i + 10), Effort = 0, Id = 1, State = "approved" });
                sprintProgressDto.WorkItemDtos.Add(new WorkItemDto { AsOf = now.AddDays(i + 11), Effort = 0, Id = 1, State = "approved" });
                sprintProgressDto.WorkItemDtos.Add(new WorkItemDto { AsOf = now.AddDays(i + 12), Effort = 0, Id = 1, State = "approved" });
            }

            //4 work items added in final days of sprint
            for (int k = 0; k < 4; k++)
            {
                sprintProgressDto.WorkItemDtos.Add(new WorkItemDto { AsOf = now.AddDays(i + 8), Effort = 0, Id = 1, State = "approved" });
                sprintProgressDto.WorkItemDtos.Add(new WorkItemDto { AsOf = now.AddDays(i + 9), Effort = 0, Id = 1, State = "approved" });
                sprintProgressDto.WorkItemDtos.Add(new WorkItemDto { AsOf = now.AddDays(i + 10), Effort = 0, Id = 1, State = "approved" });
                sprintProgressDto.WorkItemDtos.Add(new WorkItemDto { AsOf = now.AddDays(i + 11), Effort = 0, Id = 1, State = "approved" });
                sprintProgressDto.WorkItemDtos.Add(new WorkItemDto { AsOf = now.AddDays(i + 12), Effort = 0, Id = 1, State = "approved" });
            }

            return sprintProgressDto;
        }

    }
}
