using BlazingPoints.Api;
using BlazingPoints.Api.DTOs;
using BlazingPoints.Api.Json.Batches;
using BlazingPoints.Api.Json.ProjectDetail2;
using BlazingPoints.Api.Json.ProjectDetails;
using BlazingPoints.Api.Json2.ProjProperties.ccc;
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

        //private JsInterop _jsInterop;
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

        public async Task<SprintProgressVm> SetSprintProgressVm(IJSRuntime jsRuntime, string uri)
        {
            //_jsInterop = new JsInterop(_jsRuntime);
            _jsRuntime = jsRuntime;
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
                var teamSettingsIterationsJson = await _jsRuntime.InvokeAsync<string>("handleGetIterationData");

                //get the project id
                var projectDetailsDataJson = await _jsRuntime.InvokeAsync<string>("handleGetWorkItemProcessForProjectData");

                var projectDetails = JsonConvert.DeserializeObject<ProjectDetails>(projectDetailsDataJson);

                //get template id for project id
                var projectDetails2DataJson = await _jsRuntime.InvokeAsync<string>("handleGetWorkItemProcessForProjectData2", projectDetails.ProjectId);

                var projectDetail2 = JsonConvert.DeserializeObject<ProjectDetail2>(projectDetails2DataJson);
                var kvp = projectDetail2.value.FirstOrDefault(x => x.name == "System.ProcessTemplateType");

                //get list of all project types
                var workProcessDataJson = await _jsRuntime.InvokeAsync<string>("handleGetWorkProcessesData");

                var workProcesses = JsonConvert.DeserializeObject<WorkProcesses>(workProcessDataJson);
                var workProcess = workProcesses.value.FirstOrDefault(x => x.typeId == (string)kvp.value);

                //get the field name containing the effort figure
                var workItemProcess = GetWorkItemProcess(workProcess.name);
                var effortType = GetEffortType(workItemProcess);

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
                        var workItemJson = await GetWorkItemData(sprintDateWithTime);

                        //set up date format
                        var sprintDateYMDTHMSMSZ = GetFormattedDate(sprintDateWithTime);
                        Console.WriteLine($"VSIX: {sprintDateYMDTHMSMSZ}");

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

                            //deserialise
                            var batchesFull = _workItemProcessor.GetWorkItemAttributesBatchesByJson(workItemsAttributesJsons);

                            var batchesValue = GetLivingWorkItems(batchesFull);

                            InitialiseWorkItemDtos(sprintProgressDto);

                            foreach (var batchValue in batchesValue)
                            {
                                var effort = GetEffort(effortType, batchValue);

                                var workItemDto = new WorkItemDto
                                {
                                    AsOf = sprintDateWithTime,
                                    Effort = effort,
                                    Id = batchValue.id,
                                    State = batchValue.fields.SystemState
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

        private static WorkItemProcess GetWorkItemProcess(string systemProcessTemplateType3name)
        {
            WorkItemProcess workItemProcess;

            switch (systemProcessTemplateType3name.ToLower())
            {
                case "agile":
                    workItemProcess = WorkItemProcess.Agile;
                    break;
                case "basic":
                    workItemProcess = WorkItemProcess.Basic;
                    break;
                case "cmmi":
                    workItemProcess = WorkItemProcess.Cmmi;
                    break;
                case "scrum":
                    workItemProcess = WorkItemProcess.Scrum;
                    break;
                default:
                    workItemProcess = WorkItemProcess.Unknown;
                    break;
            }
            
            return workItemProcess;
        }

        private static EffortType GetEffortType(WorkItemProcess workItemProcess)
        {
            EffortType effortType;

            switch (workItemProcess)
            {
                case WorkItemProcess.Agile:
                    effortType = EffortType.StoryPoints;
                    break;
                case WorkItemProcess.Basic:
                    effortType = EffortType.Effort;
                    break;
                case WorkItemProcess.Cmmi:
                    effortType = EffortType.Size;
                    break;
                case WorkItemProcess.Scrum:
                    effortType = EffortType.Effort;
                    break;
                default:
                    effortType = EffortType.Unknown;
                    break;
            }
            
            return effortType;
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

        private static IEnumerable<Api.Json.Batches.Value> GetLivingWorkItems(Batches batchesFull)
        {
            return batchesFull.value.Where(x =>
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

        private float? GetEffort(EffortType effortType, Api.Json.Batches.Value batchValue)
        {
            float? effort;

            switch (effortType)
            {
                case EffortType.Effort:
                    effort = batchValue.fields.MicrosoftVSTSSchedulingEffort;
                    break;
                case EffortType.StoryPoints:
                    effort = batchValue.fields.MicrosoftVSTSSchedulingStoryPoints;
                    break;
                case EffortType.Size:
                    effort = batchValue.fields.MicrosoftVSTSSchedulingSize;
                    break;
                default:
                    effort = 0;
                    break;
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
            var json = await _jsRuntime.InvokeAsync<string>("handleGetWorkItemAttributesBatchData", workItemIdsJavascriptStringArray, sprintDateYMDTHMSMSZ);

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

        private async Task<string> GetWorkItemData(DateTime sprintDate)
        {
            if (sprintDate >= DateTime.Now)
            {
                return null;
            }
            else
            {
                return await _jsRuntime.InvokeAsync<string>("handleGetWorkItemData", sprintDate);
            }
        }
    }
}
