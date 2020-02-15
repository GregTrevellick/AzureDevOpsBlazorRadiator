﻿var authHeader;
var vsoContextAccountName;
var vsoContextHostUri;
var vsoContextProjectName;
var vsoContextTeamName;

function commonFallback1(arg1, arg2) {
    return "commonFallback|" + arg1 + "|" + arg2;//todo should return a promise!
}
function commonFetch1(httpVerb, requestUrl, requestBody) {
    return new Promise((resolve1, reject1) => {
        var jsonResponseData = "";

        if (httpVerb === "GET") {
            $.ajax({
                dataType: "json",
                headers: { "Authorization": authHeader },
                type: httpVerb,
                url: requestUrl
            })
                .then((responseData) => {
                    jsonResponseData = JSON.stringify(responseData, null, 4);
                    resolve1(jsonResponseData);
                });
        }
        else {
            $.ajax({
                contentType: "application/json",
                data: requestBody,
                dataType: "json",
                headers: { "Authorization": authHeader },
                type: httpVerb,
                url: requestUrl
            })
                .then((responseData) => {
                    jsonResponseData = JSON.stringify(responseData, null, 4);
                    resolve1(jsonResponseData);
                });
        }
    });
}

/* 1 ITERATION DATA */
function fetchTheIterationData() {
    return new Promise((resolveA, rejectA) => {
        var jsonResponseSprintData = "";
        VSS.require(
            ["VSS/Authentication/Services"],
            function (VSS_Auth_Service) {//this is callback function after the modules are loaded
                VSS.getAccessToken()
                    .then((responseToken) => {
                        authHeader = VSS_Auth_Service.authTokenManager.getAuthorizationHeader(responseToken);
                        var vsoContext = VSS.getWebContext();
                        vsoContextAccountName = vsoContext.account.name;
                        vsoContextHostUri = vsoContext.host.uri;
                        vsoContextProjectName = vsoContext.project.name;
                        vsoContextTeamName = vsoContext.team.name;
                        var requestUrl1 = vsoContextHostUri + vsoContextProjectName + "/" + vsoContextTeamName + "/_apis/work/teamsettings/iterations?$timeframe=Current&api-version=5.1";
                        $.ajax({
                            dataType: "json",
                            headers: { "Authorization": authHeader },
                            type: "GET",
                            url: requestUrl1
                        })
                            .then((responseSprintData) => {
                                jsonResponseSprintData = JSON.stringify(responseSprintData, null, 4);
                                resolveA(jsonResponseSprintData);
                            });
                    });
            });
    });
}
async function handleGetIterationData() {
    let iAdoData;
    try {
        iAdoData = await fetchTheIterationData(); 
    }
    catch (e) {
        iAdoData = await commonFallback1();
    }
    return iAdoData;
}

/* 2 WORK ITEM PROCESS DATA */
async function handleGetWorkItemProcessForProjectData() {
    let wipAdoData;
    try {
        var requestUrl2 = "https://dev.azure.com/" + vsoContextAccountName + "/_apis/projects/" + vsoContextProjectName + "?api-version=5.1";
        wipAdoData = await commonFetch1("GET", requestUrl2);
    }
    catch (e) {
        wipAdoData = await commonFallback1();
    }
    return wipAdoData;
}

/* 3 WORK ITEM PROCESS DATA 2. */
//function fetchTheWorkItemProcessForProjectData2(projectId2) {
//    return new Promise((resolveAA2, rejectAA2) => {
//        var jsonResponseWorkItemProcessData2 = "";
//        var requestUrlGetProjectId2 = "https://dev.azure.com/" + vsoContextAccountName + "/_apis/projects/" + projectId2 + "/properties?api-version=5.1-preview.1";
//        $.ajax({
//            dataType: "json",
//            headers: { "Authorization": authHeader },
//            type: "GET",
//            url: requestUrlGetProjectId2
//        })
//            .then((responseWorkItemProcessData2) => {
//                jsonResponseWorkItemProcessData2 = JSON.stringify(responseWorkItemProcessData2, null, 4);
//                resolveAA2(jsonResponseWorkItemProcessData2);
//            });
//    });
//}
async function handleGetWorkItemProcessForProjectData2(projectId2) {
    let wipAdoData2;
    try {
        //wipAdoData2 = await fetchTheWorkItemProcessForProjectData2(projectId2);
        var requestUrl = "https://dev.azure.com/" + vsoContextAccountName + "/_apis/projects/" + projectId2 + "/properties?api-version=5.1-preview.1";
        wipAdoData2 = await commonFetch1("GET", requestUrl);
    }
    catch (e) {
        wipAdoData2 = await commonFallback1(projectId2);
    }
    return wipAdoData2;
}

/* 4 WORK PROCESSES DATA */
//function fetchTheWorkProcessesData() {
//    return new Promise((resolveAA2, rejectAA2) => {
//        var jsonResponseWorkItemProcessData3 = "";
//        var requestUrlGetProjId3 = "https://dev.azure.com/" + vsoContextAccountName + "/_apis/work/processes?api-version=5.1-preview.2";
//        $.ajax({
//            dataType: "json",
//            headers: { "Authorization": authHeader },
//            type: "GET",
//            url: requestUrlGetProjId3
//        })
//            .then((responseWorkItemProcessData3) => {
//                jsonResponseWorkItemProcessData3 = JSON.stringify(responseWorkItemProcessData3, null, 4);
//                resolveAA2(jsonResponseWorkItemProcessData3);
//            });
//    });
//}
async function handleGetWorkProcessesData() {
    let wipAdoData3;
    try {
        //wipAdoData3 = await fetchTheWorkProcessesData();
        var requestUrl = "https://dev.azure.com/" + vsoContextAccountName + "/_apis/work/processes?api-version=5.1-preview.2";
        wipAdoData3 = await commonFetch1("GET", requestUrl);
    }
    catch (e) {
        wipAdoData3 = await commonFallback1();
    }
    return wipAdoData3;
}

/* 5 WORK ITEM DATA */
//function fetchTheWorkItemData(sprintDate) {
//    return new Promise((resolveB, rejectB) => {
//        var jsonResponseWorkItemData = "";
//        var requestUrl2 = vsoContextHostUri + vsoContextProjectName + "/_apis/wit/wiql?api-version=5.1";
//        var querySelect = "Select [System.Id] From WorkItems Where [System.TeamProject] = '" + vsoContextProjectName + "' And [System.IterationPath] = @CurrentIteration ASOF '" + sprintDate + "'";
//        $.ajax({
//            contentType: "application/json",
//            dataType: "json",
//            data: JSON.stringify({ "query": querySelect }),
//            headers: { "Authorization": authHeader },
//            type: "POST",
//            url: requestUrl2
//        })
//            .then((responseWorkItemData) => {
//                jsonResponseWorkItemData = JSON.stringify(responseWorkItemData, null, 4);
//                resolveB(jsonResponseWorkItemData);
//            });
//    });
//}
async function handleGetWorkItemData(sprintDate) {
    let wiAdoData;
    try {
        //wiAdoData = await fetchTheWorkItemData(sprintDate);
        //////////////////var requestUrl5 = "https://dev.azure.com/" + vsoContextAccountName + "/_apis/projects/" + vsoContextProjectName + "?api-version=5.1";
        var requestUrl5 = vsoContextHostUri + vsoContextProjectName + "/_apis/wit/wiql?api-version=5.1";
        var querySelect = "Select [System.Id] From WorkItems Where [System.TeamProject] = '" + vsoContextProjectName + "' And [System.IterationPath] = @CurrentIteration ASOF '" + sprintDate + "'";
        console.log("3333333");
        console.log(querySelect);
        var requestBody222 = JSON.stringify({ "query": querySelect });
        console.log(requestBody222);
        var requestBody = "{ \"query\": \"Select [System.Id] From WorkItems Where [System.TeamProject] = '" + vsoContextProjectName + "' And [System.IterationPath] = @CurrentIteration ASOF '" + sprintDate + "'\" }";
        console.log("3333333");
        console.log(requestBody);
        wiAdoData = await commonFetch1("POST", requestUrl5, requestBody);
    }
    catch (e) {
        wiAdoData = await commonFallback1(sprintDate);
    }
    return wiAdoData;
}

/* 6 WORK ITEM ATTRIBUTES DATA */
//function fetchTheWorkItemAttributesBatchData(workItemIds, sprintDateYMDTHMSMSZ) {
//    return new Promise((resolveC, rejectC) => {
//        var jsonResponseWorkItemAttributesBatchData = "";
//        var requestUrl3 = "https://dev.azure.com/" + vsoContextAccountName + "/" + vsoContextProjectName + "/_apis/wit/workitemsbatch?api-version=5.1";
//        var requestBody = '{ "ids": ' + workItemIds + ' , "asOf": "' + sprintDateYMDTHMSMSZ + '", "fields": ["System.Id", "System.Title", "System.WorkItemType", "Microsoft.VSTS.Scheduling.StoryPoints", "System.State", "Microsoft.VSTS.Scheduling.Effort", "Microsoft.VSTS.Scheduling.OriginalEstimate", "Microsoft.VSTS.Scheduling.Size" ] }';
//        $.ajax({
//            contentType: "application/json",
//            dataType: "json",
//            data: requestBody,
//            headers: { "Authorization": authHeader },
//            type: "POST",
//            url: requestUrl3
//        })
//            .then((responseWorkItemAttributesBatchData) => {
//                jsonResponseWorkItemAttributesBatchData = JSON.stringify(responseWorkItemAttributesBatchData, null, 4);
//                resolveC(jsonResponseWorkItemAttributesBatchData);
//            });
//    });
//}
async function handleGetWorkItemAttributesBatchData(workItemIds, sprintDateYMDTHMSMSZ) {
    let wiabAdoData;
    try {
        //wiabAdoData = await fetchTheWorkItemAttributesBatchData(workItemIds, sprintDateYMDTHMSMSZ);
        var requestUrl = "https://dev.azure.com/" + vsoContextAccountName + "/" + vsoContextProjectName + "/_apis/wit/workitemsbatch?api-version=5.1";
        var requestBody = '{ "ids": ' + workItemIds + ' , "asOf": "' + sprintDateYMDTHMSMSZ + '", "fields": ["System.Id", "System.Title", "System.WorkItemType", "Microsoft.VSTS.Scheduling.StoryPoints", "System.State", "Microsoft.VSTS.Scheduling.Effort", "Microsoft.VSTS.Scheduling.OriginalEstimate", "Microsoft.VSTS.Scheduling.Size" ] }';
        wiabAdoData = await commonFetch1("POST", requestUrl, requestBody);
    }
    catch (e) {
        wiabAdoData = await commonFallback1(workItemIds, sprintDateYMDTHMSMSZ);
    }
    return wiabAdoData;
}
