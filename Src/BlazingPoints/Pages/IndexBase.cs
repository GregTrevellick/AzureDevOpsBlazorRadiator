using BlazingPoints.Api;
using BlazingPoints.Api.DTOs;
using BlazingPoints.Api.Json2;
using BlazingPoints.Api.Processors;
using BlazingPoints.ViewModels;
using LINQtoCSV;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;

namespace BlazingPoints
{
    public class IndexBase : LayoutComponentBase
    {
        public IJSRuntime _jsRuntime;

        private JsInterop _jsInterop;
        private SprintIterationProcessor _sprintIterationProcessor;
        private WorkItemProcessor _workItemProcessor;

        public string GetUiDataJson(SprintProgressVm sprintProgressVm)//TODO convert to property on SprintProgressVm.cs
        {
            var jsonString = JsonConvert.SerializeObject(sprintProgressVm.UiDataObject, Newtonsoft.Json.Formatting.Indented);
            return jsonString;
        }

        public string GetUiDataXml(SprintProgressVm sprintProgressVm)//TODO convert to property on SprintProgressVm.cs
        {
            var xmlSerializer = new XmlSerializer(typeof(UiDataObject));
            
            var xml = "";
            
            using (var stringWriter = new StringWriter())
            {
                using (var xmlTextWriter = XmlTextWriter.Create(stringWriter))
                {
                    xmlSerializer.Serialize(xmlTextWriter, sprintProgressVm.UiDataObject);
                    xml = stringWriter.ToString();
                }
            }

            return xml;
        }

        public string GetUiDataCsv(SprintProgressVm sprintProgressVm)//TODO convert to property on SprintProgressVm.cs
        {           
            var separatorChar = ',';

            var csv = $"{sprintProgressVm.UiDataObject.IterationDetails.Iteration}{separatorChar}{sprintProgressVm.UiDataObject.IterationDetails.Start}{separatorChar}{sprintProgressVm.UiDataObject.IterationDetails.End}{Environment.NewLine}";
            
            var csvFileDescription = new CsvFileDescription
            {
                SeparatorChar = separatorChar,// '\t', // tab delimited
                //FirstLineHasColumnNames = false, // no column names in first record
                //FileCultureName = "nl-NL" // use formats used in The Netherlands
                UseOutputFormatForParsingCsvValue = true
            };

            using (var memoryStream = new MemoryStream())
            {
                using (TextWriter textWriter = new StreamWriter(memoryStream))
                {
                    var csvContext = new CsvContext();
                    csvContext.Write(sprintProgressVm.UiDataObject.WorkItemData.ToList(), textWriter, csvFileDescription);
                    textWriter.Flush();
                    memoryStream.Position = 0;
                    csv += Encoding.ASCII.GetString(memoryStream.ToArray());
                }
            }

            return csv;
        }

        public async Task<SprintProgressVm> SetSprintProgressVm(IJSRuntime _jsRuntime, string uri)
        {
            _jsInterop = new JsInterop(_jsRuntime);
            _sprintIterationProcessor = new SprintIterationProcessor();
            _workItemProcessor = new WorkItemProcessor();
            SprintProgressDto sprintProgressDto;

            if (uri.Contains("localhost"))
            {
                sprintProgressDto = GetSprintProgressDtoTest();
            }
            else
            {
                sprintProgressDto = await GetSprintProgressDtoLive();
            }

            var sprintProgressVm = GetSprintProgressVm(sprintProgressDto);

            //do not delete the following DEBUG line!
            //sprintProgressVm.DebugString = JsonConvert.SerializeObject(sprintProgressDto, Formatting.Indented); 

            return sprintProgressVm;
        }

        private async Task<SprintProgressDto> GetSprintProgressDtoLive()
        {
            SprintProgressDto sprintProgressDto;
                
            try
            {
                //get the sprint start end dates json response
                var teamSettingsIterationsJson = await GetTeamSettingsIterationsJson();

                //deserialize to a poco
                sprintProgressDto = _sprintIterationProcessor.GetSprintProgressDto(teamSettingsIterationsJson);

                //loop thru each of the 10 sprint days
                for (var sprintDateWithoutTime = sprintProgressDto.SprintStart;
                     sprintDateWithoutTime <= sprintProgressDto.SprintEnd;
                     sprintDateWithoutTime = sprintDateWithoutTime.AddDays(1))
                {
                    if (sprintDateWithoutTime.DayOfWeek != DayOfWeek.Saturday &&
                        sprintDateWithoutTime.DayOfWeek != DayOfWeek.Sunday)
                    {
                        var sprintDateWithTime = GetSprintDateWithTime(sprintProgressDto, sprintDateWithoutTime);

                        //get work item ids (json response) in the sprint on this specific date
                        var workItemJson = await GetWorkItemJson(sprintDateWithTime);

                        //set up date format
                        var sprintDateYMDTHMSMSZ = GetFormattedDate(sprintDateWithTime);
                        //Console.WriteLine($"VSIX: {sprintDateYMDTHMSMSZ}");

                        //deserialize to a list of ids/urls for that date
                        var workItemsInSprintOnSprintDate = _workItemProcessor.GetWorkItemsByJson(workItemJson).ToList();

                        if (workItemsInSprintOnSprintDate == null || workItemsInSprintOnSprintDate.Count == 0)
                        {
                            //handle zero work items on day zero/one of the sprint (or on dates in the future) as the first (aka "zeroth") day of sprint seems to return zero work items e.g. if sprint starts on 10th Jan
                            var workItemDto = new WorkItemDto
                            {
                                AsOf = sprintDateWithTime
                            };

                            InitialiseWorkItemDtos(sprintProgressDto);

                            sprintProgressDto.WorkItemDtos.Add(workItemDto);
                        }
                        else
                        {
                            //from the list of ids/urls for that date create a list of just the ids (for later use in getting effort, state, etc for a batch of PBIs)
                            var workItemIds = new List<int>();

                            foreach (var workItemInSprintOnSprintDate in workItemsInSprintOnSprintDate)
                            {
                                //exclude certain ids to prevent this response fromn the api {"$id":"1","innerException":null,"message":"TF401232: Work item 27142 does not exist, or you do not have permissions to read it.","typeName":"Microsoft.TeamFoundation.WorkItemTracking.Server.WorkItemUnauthorizedAccessException, Microsoft.TeamFoundation.WorkItemTracking.Server","typeKey":"WorkItemUnauthorizedAccessException","errorCode":0,"eventId":3200}
                                if (workItemInSprintOnSprintDate.id != 27142)
                                {
                                    workItemIds.Add(workItemInSprintOnSprintDate.id);
                                }
                            }

                            //get effort, state, etc (json response) for the batch of PBIs in the sprint on this specific date
                            var workItemsAttributesJsons = await GetWorkItemAttributesJsonByBatch(workItemIds, sprintDateYMDTHMSMSZ);

                            //deserialise to batchesRootobject
                            var batchesRootobjectFull = _workItemProcessor.GetWorkItemAttributesBatchesByJson(workItemsAttributesJsons);

                            var totalMicrosoftVSTSSchedulingEffort = batchesRootobjectFull.value.Any(x => x.fields.MicrosoftVSTSSchedulingEffort.HasValue);
                            var totalMicrosoftVSTSSchedulingStoryPoints = batchesRootobjectFull.value.Any(x => x.fields.MicrosoftVSTSSchedulingStoryPoints.HasValue);
                            var useEffort = totalMicrosoftVSTSSchedulingEffort && !totalMicrosoftVSTSSchedulingStoryPoints;

                            var batchesRootobjectValue = GetLivingWorkItems(batchesRootobjectFull);

                            InitialiseWorkItemDtos(sprintProgressDto);

                            foreach (var batchvalue in batchesRootobjectValue)
                            {
                                var effort = GetEffort(useEffort, batchvalue);

                                var workItemDto = new WorkItemDto
                                {
                                    AsOf = sprintDateWithTime,
                                    Effort = effort,
                                    Id = batchvalue.id,
                                    State = batchvalue.fields.SystemState
                                };

                                sprintProgressDto.WorkItemDtos.Add(workItemDto);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                var exceptionMessage = $"ex.message1 {ex.Message}";
                Console.WriteLine(exceptionMessage);
                sprintProgressDto = new SprintProgressDto { IterationNumber = exceptionMessage };
            }

            return sprintProgressDto;
        }

        private static SprintProgressDto GetSprintProgressDtoTest()
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

        private static IEnumerable<Value> GetLivingWorkItems(batchesRootobject batchesRootobjectFull)
        {
            return batchesRootobjectFull.value.Where(x =>
                x.fields.SystemState.ToLower() != "failed" &&
                x.fields.SystemState.ToLower() != "removed" &&
                x.fields.SystemState.ToLower() != "to do");
        }

        private static void InitialiseWorkItemDtos(SprintProgressDto sprintProgressDto)
        {
            if (sprintProgressDto.WorkItemDtos == null)
            {
                sprintProgressDto.WorkItemDtos = new List<WorkItemDto>();
            }
        }

        private float? GetEffort(bool useEffort, Value batchvalue)
        {
            float? effort;

            if (useEffort)
            {
                effort = batchvalue.fields.MicrosoftVSTSSchedulingEffort;
            }
            else
            {
                effort = batchvalue.fields.MicrosoftVSTSSchedulingStoryPoints;
            }

            return effort;
        }

        private DateTime GetSprintDateWithTime(SprintProgressDto sprintProgressDto, DateTime sprintDateWithoutTime)
        {
            DateTime sprintDateWithTime;

            if (sprintDateWithoutTime.Date == DateTime.Now.Date)
            {
                sprintDateWithTime = new DateTime(sprintDateWithoutTime.Year, sprintDateWithoutTime.Month, sprintDateWithoutTime.Day, DateTime.Now.Hour, DateTime.Now.Minute, 0);
            }
            else
            {
                sprintDateWithTime = new DateTime(sprintDateWithoutTime.Year, sprintDateWithoutTime.Month, sprintDateWithoutTime.Day, 23, 59, 59);
            }

            return sprintDateWithTime;
        }

        private string GetFormattedDate(DateTime sprintDate)
        {
            var dd = sprintDate.Day < 10 ? "0" + sprintDate.Day.ToString() : sprintDate.Day.ToString();
            var mm = sprintDate.Month < 10 ? "0" + sprintDate.Month.ToString() : sprintDate.Month.ToString();
            var hh = sprintDate.Hour < 10 ? "0" + sprintDate.Hour.ToString() : sprintDate.Hour.ToString();
            var mi = sprintDate.Minute < 10 ? "0" + sprintDate.Minute.ToString() : sprintDate.Minute.ToString();
            var ss = sprintDate.Second < 10 ? "0" + sprintDate.Second.ToString() : sprintDate.Second.ToString();
            
            var sprintDateYMDTHMSMSZ = $"{sprintDate.Year}-{mm}-{dd}T{hh}:{mi}:{ss}.000Z";
            
            return sprintDateYMDTHMSMSZ;
        }

        private async Task<string> GetWorkItemAttributesJsonByBatch(List<int> workItemIds, string sprintDateYMDTHMSMSZ)
        {
            var workItemIdsJavascriptStringArray = " [ ";
            
            foreach (var workItemId in workItemIds)
            {
                workItemIdsJavascriptStringArray += " " + workItemId.ToString() + ",";
            }
            
            workItemIdsJavascriptStringArray = workItemIdsJavascriptStringArray.TrimEnd(',');
            workItemIdsJavascriptStringArray += " ] ";

            //TODO? minimise a repeat lookup of same pbi's day after day
            var json = await _jsInterop.GetWorkItemAttributesBatchData(workItemIdsJavascriptStringArray, sprintDateYMDTHMSMSZ);
            return json;
        }

        private async Task<string> GetTeamSettingsIterationsJson()
        {
            var json = await _jsInterop.GetIterationData();
            return json;
        }

        private async Task<string> GetWorkItemJson(DateTime sprintDate)
        {
            var json = await _jsInterop.GetWorkItemData(sprintDate);
            return json;
        }

        private SprintProgressVm GetSprintProgressVm(SprintProgressDto sprintProgressDto)
        {
            var sprintProgressVm = new SprintProgressVm();

            sprintProgressVm.IterationNumber = sprintProgressDto.IterationNumber;
            sprintProgressVm.SprintEnd = sprintProgressDto.SprintEnd;
            sprintProgressVm.SprintStart = sprintProgressDto.SprintStart;
            sprintProgressVm.WorkItemVms = new List<WorkItemVm>();

            if (sprintProgressDto.WorkItemDtos != null && sprintProgressDto.WorkItemDtos.Any())
            {
                foreach (var workItemDto in sprintProgressDto.WorkItemDtos)
                {
                    var workItemVm = GetWorkItemVm(workItemDto);
                    sprintProgressVm.WorkItemVms.Add(workItemVm);
                }
            }

            return sprintProgressVm;
        }

        private WorkItemVm GetWorkItemVm(WorkItemDto workItemDto)
        {
            return new WorkItemVm
            {
                AsOf = workItemDto.AsOf,
                Effort = workItemDto.Effort,
                Id = workItemDto.Id,
                State = workItemDto.State,
            };
        }
    }
}
