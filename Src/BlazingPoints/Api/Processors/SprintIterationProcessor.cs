using BlazingPoints.Api.DTOs;
using BlazingPoints.Api.Json;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;

namespace BlazingPoints.Api.Processors
{
    public class SprintIterationProcessor
    {
        public SprintProgressDto GetSprintProgressDto(string teamSettingsIterationsJson)
        {
            var sprintIteration = JsonConvertGetSprintIteration(teamSettingsIterationsJson);

            var sprintProgressDto = new SprintProgressDto
            {
                IterationNumber = sprintIteration.IterationNumber,
                SprintEnd = sprintIteration.SprintEnd.HasValue ? sprintIteration.SprintEnd.Value : DateTime.MinValue,
                SprintStart = sprintIteration.SprintStart.HasValue ? sprintIteration.SprintStart.Value : DateTime.MinValue,
            };

            return sprintProgressDto;
        }

        private SprintIteration JsonConvertGetSprintIteration(string teamSettingsIterationsJson)
        {
            var iterationRootobject = JsonConvert.DeserializeObject<iterationRootobject>(teamSettingsIterationsJson);
            var sprintIteration = GetSprintIteration(iterationRootobject.value.ToList());
            return sprintIteration;
        }

        private SprintIteration GetSprintIteration(List<Json.Value> teamSettingsIterations)
        {
            var sprintIteration = new SprintIteration();

            foreach (var teamSettingsIteration in teamSettingsIterations)
            {
                sprintIteration.IterationNumber = teamSettingsIteration.name;
                sprintIteration.SprintEnd = teamSettingsIteration.attributes.finishDate;
                sprintIteration.SprintStart = teamSettingsIteration.attributes.startDate;
                break;
            }

            return sprintIteration;
        }
    }
}
