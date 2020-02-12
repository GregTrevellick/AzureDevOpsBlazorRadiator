//WASM: ex.message1 sprintDateYMDTHMSMSZ is not defined
//blazor.webassembly.js: 1 WASM: ReferenceError: sprintDateYMDTHMSMSZ is not defined
//blazor.webassembly.js: 1 WASM: at handleGetWorkItemAttributesBatchData(https://gregtrevellick.gallerycdn.vsassets.io/extensions/gregtrevellick/blazorradiator5/1.0.16/1581492388058/wwwroot/Scripts/bundle.min.js:1:3692)


var authHeader;
var vsoContextAccountName;
var vsoContextHostUri;
var vsoContextProjectName;
var vsoContextTeamName;

/* ITERATION DATA */
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

                                //https://docs.microsoft.com/en-us/rest/api/azure/devops/processes/processes/get?view=azure-devops-rest-5.1#examples

                                resolveA(jsonResponseSprintData);
                            });
                    });
            });
    });
}
function fetchTheIterationDataFallback() {
    return "fallback_blah1";//todo should return a promise!
}
async function handleGetIterationData() {
    let iAdoData;
    try {
        iAdoData = await fetchTheIterationData(); 
    }
    catch (e) {
        iAdoData = await fetchTheIterationDataFallback();
    }
    //console.log("VSIX: iAdoData=" + iAdoData);
    return iAdoData;
}

/* WORK ITEM PROCESS DATA */
function fetchTheWorkItemProcessForProjectData() {
    return new Promise((resolveAA, rejectAA) => {
        var jsonResponseWorkItemProcessData = "";
        var requestUrlGetProjId = "https://dev.azure.com/" + vsoContextAccountName + "/_apis/projects/" + vsoContextProjectName + "?api-version=5.1";
        ///////////////////////////////////https://dev.azure.com/gregtrevellick/_apis/projects/FooBarScrum?api-version=5.1
        $.ajax({
            dataType: "json",
            headers: { "Authorization": authHeader },
            type: "GET",
            url: requestUrlGetProjId
        })
            .then((responseWorkItemProcessData) => {
                jsonResponseWorkItemProcessData = JSON.stringify(responseWorkItemProcessData, null, 4);
                resolveAA(jsonResponseWorkItemProcessData);
            });
    });
}
function fetchTheWorkItemProcessForProjectDataFallback() {
    return "fallback_blah1b";//todo should return a promise!
}
async function handleGetWorkItemProcessForProjectData() {
    let wipAdoData;
    try {
        wipAdoData = await fetchTheWorkItemProcessForProjectData();
    }
    catch (e) {
        wipAdoData = await fetchTheWorkItemProcessForProjectDataFallback();
    }
    console.log("VSIX: wipAdoData=" + wipAdoData);
    return wipAdoData;
}

/* WORK ITEM PROCESS DATA 2 */
//gregt rename
function fetchTheWorkItemProcessForProjectData2(projId2) {
    return new Promise((resolveAA2, rejectAA2) => {
        var jsonResponseWorkItemProcessData2 = "";
        var requestUrlGetProjId2 = "https://dev.azure.com/" + vsoContextAccountName + "/_apis/projects/" + projId2 + "/properties?api-version=5.1-preview.1";
        $.ajax({
            dataType: "json",
            headers: { "Authorization": authHeader },
            type: "GET",
            url: requestUrlGetProjId2
        })
            .then((responseWorkItemProcessData2) => {
                jsonResponseWorkItemProcessData2 = JSON.stringify(responseWorkItemProcessData2, null, 4);
                resolveAA2(jsonResponseWorkItemProcessData2);
            });
    });
}
function fetchTheWorkItemProcessForProjectDataFallback2(projId2) {
    return "fallback2_blah1b" + projId2;//todo should return a promise!
}
async function handleGetWorkItemProcessForProjectData2(projId2) {
    let wipAdoData2;
    try {
        wipAdoData2 = await fetchTheWorkItemProcessForProjectData2(projId2);
    }
    catch (e) {
        wipAdoData2 = await fetchTheWorkItemProcessForProjectDataFallback2(projId2);
    }
    console.log("VSIX: wipAdoData2=" + wipAdoData2);
    return wipAdoData2;
}

/* WORK ITEM PROCESS DATA 3 */
//gregt rename
function fetchTheWorkItemProcessForProjectData3() {
    return new Promise((resolveAA2, rejectAA2) => {
        var jsonResponseWorkItemProcessData3 = "";
        var requestUrlGetProjId3 = "https://dev.azure.com/" + vsoContextAccountName + "/_apis/work/processes?api-version=5.1-preview.2";
        $.ajax({
            dataType: "json",
            headers: { "Authorization": authHeader },
            type: "GET",
            url: requestUrlGetProjId3
        })
            .then((responseWorkItemProcessData3) => {
                jsonResponseWorkItemProcessData3 = JSON.stringify(responseWorkItemProcessData3, null, 4);
                resolveAA2(jsonResponseWorkItemProcessData3);
            });
    });
}
function fetchTheWorkItemProcessForProjectDataFallback3() {
    return "fallback3_blah1b";//todo should return a promise!
}
async function handleGetWorkItemProcessForProjectData3() {
    let wipAdoData3;
    try {
        wipAdoData3 = await fetchTheWorkItemProcessForProjectData3();
    }
    catch (e) {
        wipAdoData3 = await fetchTheWorkItemProcessForProjectDataFallback3();
    }
    console.log("VSIX: wipAdoData3=" + wipAdoData3);
    return wipAdoData3;
}

/* WORK ITEM DATA */
function fetchTheWorkItemData(sprintDate) {
    return new Promise((resolveB, rejectB) => {
        var jsonResponseWorkItemData = "";
        var requestUrl2 = vsoContextHostUri + vsoContextProjectName + "/_apis/wit/wiql?api-version=5.1";
        var querySelect = "Select [System.Id] From WorkItems Where [System.TeamProject] = '" + vsoContextProjectName + "' And [System.IterationPath] = @CurrentIteration ASOF '" + sprintDate + "'";
        $.ajax({
            contentType: "application/json",
            dataType: "json",
            data: JSON.stringify({ "query": querySelect }),
            headers: { "Authorization": authHeader },
            type: "POST",
            url: requestUrl2
        })
            .then((responseWorkItemData) => {
                jsonResponseWorkItemData = JSON.stringify(responseWorkItemData, null, 4);
                resolveB(jsonResponseWorkItemData);
            });
    });
}
function fetchTheWorkItemDataFallback(sprintDate) {
    return "fallback_blah2" + sprintDate;//todo should return a promise!
}
async function handleGetWorkItemData(sprintDate) {
    let wiAdoData;
    try {
        wiAdoData = await fetchTheWorkItemData(sprintDate);
    }
    catch (e) {
        wiAdoData = await fetchTheWorkItemDataFallback(sprintDate);
    }
    //console.log("VSIX: wiAdoData=" + wiAdoData);
    return wiAdoData;
}

/* WORK ITEM ATTRIBUTES DATA */
function fetchTheWorkItemAttributesBatchData(workItemIds, sprintDateYMDTHMSMSZ) {
    return new Promise((resolveC, rejectC) => {
        var jsonResponseWorkItemAttributesBatchData = "";
        var requestUrl3 = "https://dev.azure.com/" + vsoContextAccountName + "/" + vsoContextProjectName + "/_apis/wit/workitemsbatch?api-version=5.1";
        var requestBody = '{ "ids": ' + workItemIds + ' , "asOf": "' + sprintDateYMDTHMSMSZ + '", "fields": ["System.Id", "System.Title", "System.WorkItemType", "Microsoft.VSTS.Scheduling.StoryPoints", "System.State", "Microsoft.VSTS.Scheduling.Effort", "Microsoft.VSTS.Scheduling.OriginalEstimate", "Microsoft.VSTS.Scheduling.Size" ] }';
        $.ajax({
            contentType: "application/json",
            dataType: "json",
            data: requestBody,
            headers: { "Authorization": authHeader },
            type: "POST",
            url: requestUrl3
        })
            .then((responseWorkItemAttributesBatchData) => {
                jsonResponseWorkItemAttributesBatchData = JSON.stringify(responseWorkItemAttributesBatchData, null, 4);
                resolveC(jsonResponseWorkItemAttributesBatchData);
            });
    });
}
function fetchTheWorkItemAttributesBatchDataFallback(workItemIds, sprintDateYMDTHMSMSZ) {
    return "fallback_blah3" + workItemIds + " " + sprintDateYMDTHMSMSZ;//todo should return a promise!
}
async function handleGetWorkItemAttributesBatchData(workItemIds, sprintDateYMDTHMSMSZ) {
    let wiabAdoData;
    try {
        wiabAdoData = await fetchTheWorkItemAttributesBatchData(workItemIds, sprintDateYMDTHMSMSZ);
    }
    catch (e) {
        wiabAdoData = await fetchTheWorkItemAttributesBatchDataFallback(workItemIds, sprintDateYMDTHMSMSZ);
    }
    //console.log("VSIX: wiabAdoData=" + wiabAdoData);
    return wiabAdoData;
}
