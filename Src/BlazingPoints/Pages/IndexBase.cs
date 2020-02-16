using BlazingPoints.Api;
using BlazingPoints.Api.DTOs;
using BlazingPoints.Api.Json;
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
                SeparatorChar = separatorChar,// '\t' for tab delimited
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
            _jsRuntime = jsRuntime;
            _sprintIterationProcessor = new SprintIterationProcessor();
            _workItemProcessor = new WorkItemProcessor();
            SprintProgressDto sprintProgressDto;

            if (uri.Contains("localhost"))
            {
                sprintProgressDto = MockData.GetSprintProgressDtoTest();
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
                var data = await GatherData();
                var effortType = data.Item1;
                sprintProgressDto = data.Item2;

                //loop thru each of the 10 sprint days
                for (var sprintDateWithoutTime = sprintProgressDto.SprintStart;
                     sprintDateWithoutTime <= sprintProgressDto.SprintEnd;
                     sprintDateWithoutTime = sprintDateWithoutTime.AddDays(1))
                {
                    if (sprintDateWithoutTime.DayOfWeek != DayOfWeek.Saturday &&
                        sprintDateWithoutTime.DayOfWeek != DayOfWeek.Sunday)
                    {
                        await PopulateSprintProgressDto(sprintProgressDto, effortType, sprintDateWithoutTime);
                    }
                }
            }
            catch (Exception ex)
            {
                var exceptionMessage = $"ex.message {ex.Message}";
                Console.WriteLine(exceptionMessage);
                sprintProgressDto = new SprintProgressDto { IterationNumber = exceptionMessage };
            }

            return sprintProgressDto;
        }

        private async Task PopulateSprintProgressDto(SprintProgressDto sprintProgressDto, EffortType effortType, DateTime sprintDateWithoutTime)
        {
            var sprintDateWithTime = GetSprintDateWithTime(sprintProgressDto, sprintDateWithoutTime);

            //get work item ids (json response) in the sprint on this specific date
            var workItemJson = await GetWorkItemData(sprintDateWithTime);

            //set up date format
            var sprintDateYMDTHMSMSZ = GetFormattedDate(sprintDateWithTime);
            Console.WriteLine($"VSIX: {sprintDateYMDTHMSMSZ}");

            //deserialize to a list of ids/urls for that date
            var workItemsInSprintOnSprintDate = _workItemProcessor.GetWorkItemsByJson(workItemJson).ToList();

            await PopulateSprintProgressDto(sprintProgressDto, effortType, sprintDateWithTime, sprintDateYMDTHMSMSZ, workItemsInSprintOnSprintDate);
        }

        private async Task PopulateSprintProgressDto(SprintProgressDto sprintProgressDto, EffortType effortType, DateTime sprintDateWithTime, string sprintDateYMDTHMSMSZ, List<Workitem> workItemsInSprintOnSprintDate)
        {
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
                var valueBs = await GetValueBsFromWorkitems(sprintDateYMDTHMSMSZ, workItemsInSprintOnSprintDate);

                InitialiseWorkItemDtos(sprintProgressDto);

                foreach (var valueB in valueBs)
                {
                    var workItemDto = GetWorkItemDto(effortType, sprintDateWithTime, valueB);
                    sprintProgressDto.WorkItemDtos.Add(workItemDto);
                }
            }
        }

        private async Task<IEnumerable<ValueB>> GetValueBsFromWorkitems(string sprintDateYMDTHMSMSZ, List<Workitem> workItemsInSprintOnSprintDate)
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

            var valueBs = await GetValueBs(sprintDateYMDTHMSMSZ, workItemIds);

            return valueBs;
        }

        private async Task<IEnumerable<ValueB>> GetValueBs(string sprintDateYMDTHMSMSZ, List<int> workItemIds)
        {
            //get effort, state, etc (json response) for the batch of PBIs in the sprint on this specific date
            var workItemsAttributesJsons = await GetWorkItemAttributesJsonByBatch(workItemIds, sprintDateYMDTHMSMSZ);

            //deserialise
            var batchesFull = JsonConvert.DeserializeObject<Batches>(workItemsAttributesJsons);

            var valueBs = GetLivingWorkItems(batchesFull);

            return valueBs;
        }

        private WorkItemDto GetWorkItemDto(EffortType effortType, DateTime sprintDateWithTime, ValueB valueB)
        {
            var effort = GetEffort(effortType, valueB);

            var workItemDto = new WorkItemDto
            {
                AsOf = sprintDateWithTime,
                Effort = effort,
                Id = valueB.id,
                State = valueB.fieldsB.SystemState
            };

            return workItemDto;
        }

        private async Task<Tuple<EffortType, SprintProgressDto>> GatherData()
        {
            //get the sprint start end dates json response
            var teamSettingsIterationsJson = await _jsRuntime.InvokeAsync<string>("handleGetIterationData");

            //get the project id
            var projectDetailsDataJson = await _jsRuntime.InvokeAsync<string>("handleGetWorkItemProcessForProjectData");

            var projectDetails = JsonConvert.DeserializeObject<ProjectDetails>(projectDetailsDataJson);

            //get template id for project id
            var projectDetails2DataJson = await _jsRuntime.InvokeAsync<string>("handleGetWorkItemProcessForProjectData2", projectDetails.ProjectId);

            var projectDetail2 = JsonConvert.DeserializeObject<ProjectDetail2>(projectDetails2DataJson);
            var kvp = projectDetail2.valuePD2.FirstOrDefault(x => x.name == "System.ProcessTemplateType");

            //get list of all project types
            var workProcessDataJson = await _jsRuntime.InvokeAsync<string>("handleGetWorkProcessesData");

            var workProcesses = JsonConvert.DeserializeObject<WorkProcesses>(workProcessDataJson);
            var workProcess = workProcesses.valueWP.FirstOrDefault(x => x.typeId == (string)kvp.value);

            //get the field name containing the effort figure
            var workItemProcess = GetWorkItemProcess(workProcess.name);
            var effortType = GetEffortType(workItemProcess);

            //deserialize to a poco
            var sprintProgressDto = _sprintIterationProcessor.GetSprintProgressDto(teamSettingsIterationsJson);

            return new Tuple<EffortType, SprintProgressDto>(effortType, sprintProgressDto);
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


        private static IEnumerable<ValueB> GetLivingWorkItems(Batches batchesFull)
        {
            return batchesFull.valueB.Where(x =>
                x.fieldsB.SystemState.ToLower() != "failed" &&
                x.fieldsB.SystemState.ToLower() != "removed" &&
                x.fieldsB.SystemState.ToLower() != "to do");
        }

        private static void InitialiseWorkItemDtos(SprintProgressDto sprintProgressDto)
        {
            if (sprintProgressDto.WorkItemDtos == null)
            {
                sprintProgressDto.WorkItemDtos = new List<WorkItemDto>();
            }
        }

        private float? GetEffort(EffortType effortType, ValueB batchValue)
        {
            float? effort;

            switch (effortType)
            {
                case EffortType.Effort:
                    effort = batchValue.fieldsB.MicrosoftVSTSSchedulingEffort;
                    break;
                case EffortType.StoryPoints:
                    effort = batchValue.fieldsB.MicrosoftVSTSSchedulingStoryPoints;
                    break;
                case EffortType.Size:
                    effort = batchValue.fieldsB.MicrosoftVSTSSchedulingSize;
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
