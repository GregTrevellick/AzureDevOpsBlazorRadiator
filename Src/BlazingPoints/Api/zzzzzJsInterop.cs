//using Microsoft.JSInterop;
//using System;
//using System.Threading.Tasks;

//namespace BlazingPoints.Api
//{
//    public class JsInterop
//    {
//        private readonly IJSRuntime _jsRuntime;

//        public JsInterop(IJSRuntime jsRuntime)
//        {
//            _jsRuntime = jsRuntime;
//        }

//        //public async Task<string> GetIterationData()
//        //{
//        //    return await _jsRuntime.InvokeAsync<string>("handleGetIterationData");//gregt call Xxxxx() direct
//        //}

//        //public async Task<string> GetProjectDetailsData()
//        //{
//        //    return await _jsRuntime.InvokeAsync<string>("handleGetWorkItemProcessForProjectData");//gregt call Xxxxx() direct
//        //}

//        //public async Task<string> GetProjectDetails2Data(string projectId)
//        //{
//        //    return await _jsRuntime.InvokeAsync<string>("handleGetWorkItemProcessForProjectData2", projectId);//gregt call Xxxxx() direct
//        //}

//        //public async Task<string> GetWorkProcessesData()
//        //{
//        //    return await _jsRuntime.InvokeAsync<string>("handleGetWorkProcessesData");//gregt call Xxxxx() direct
//        //}

//        //public async Task<string> GetWorkItemData(DateTime sprintDate)
//        //{
//        //    if (sprintDate >= DateTime.Now)
//        //    {
//        //        return null;
//        //    }
//        //    else
//        //    { 
//        //        return await _jsRuntime.InvokeAsync<string>("handleGetWorkItemData", sprintDate);
//        //    }
//        //}

//        //public async Task<string> GetWorkItemAttributesBatchData(string workItemIds, string sprintDateYMDTHMSMSZ)
//        //{
//        //    return await _jsRuntime.InvokeAsync<string>("handleGetWorkItemAttributesBatchData", workItemIds, sprintDateYMDTHMSMSZ);//gregt call Xxxxx() direct
//        //}
//    }
//}