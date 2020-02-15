//There will be errors (e.g. ""No handler found on any channel for message...") when opening the html locally in browser because there is no VSTS host when opening it locally on your computer. This doesn't occur once uploaded to the MarketPlace
VSS.init({//VSS.init performs initial handshake/setup between the iframe hosting the vsix and the host frame window
    explicitNotifyLoaded: true,//the vsix can explicitly notify the host manually when the extension is done loading. This allows us to notify load completion after ensuring dependent modules are loaded. This is used for the loading indicator. VSS.notifyLoadSucceeded() is called later to indicate that the extension is loaded. In general it is good to perform this yourself if you are doing any async / promise work. There is also a notifyLoadFailed function, in case you want to fail the execution of your extension.
    usePlatformScripts: true,//lets you use common libraries included in the host (e.g. jQuery will be made accessible to you)
    usePlatformStyles: true//lets you use the host CSS styles
});

VSS.ready(function () {
    //console.log("VSIX: VSS.ready");//this works when uploaded to the MarketPlace but NOT locally
    VSS.notifyLoadSucceeded();
});

var authHeader;
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
        var requestUrl = "https://dev.azure.com/" + vsoContextAccountName + "/_apis/projects/" + vsoContextProjectName + "?api-version=5.1";
        wipAdoData = await commonFetch1("GET", requestUrl);
    }
    catch (e) {
        wipAdoData = await commonFallback1();
    }
    return wipAdoData;
}

/* 3 WORK ITEM PROCESS DATA 2. */
async function handleGetWorkItemProcessForProjectData2(projectId2) {
    let wipAdoData2;
    try {
        var requestUrl = "https://dev.azure.com/" + vsoContextAccountName + "/_apis/projects/" + projectId2 + "/properties?api-version=5.1-preview.1";
        wipAdoData2 = await commonFetch1("GET", requestUrl);
    }
    catch (e) {
        wipAdoData2 = await commonFallback1(projectId2);
    }
    return wipAdoData2;
}

/* 4 WORK PROCESSES DATA */
async function handleGetWorkProcessesData() {
    let wipAdoData3;
    try {
        var requestUrl = "https://dev.azure.com/" + vsoContextAccountName + "/_apis/work/processes?api-version=5.1-preview.2";
        wipAdoData3 = await commonFetch1("GET", requestUrl);
    }
    catch (e) {
        wipAdoData3 = await commonFallback1();
    }
    return wipAdoData3;
}

/* 5 WORK ITEM DATA */
async function handleGetWorkItemData(sprintDate) {
    let wiAdoData;
    try {
        var requestUrl = vsoContextHostUri + vsoContextProjectName + "/_apis/wit/wiql?api-version=5.1";
        var querySelect = "Select [System.Id] From WorkItems Where [System.TeamProject] = '" + vsoContextProjectName + "' And [System.IterationPath] = @CurrentIteration ASOF '" + sprintDate + "'";
        var requestBody = JSON.stringify({ "query": querySelect });
        wiAdoData = await commonFetch1("POST", requestUrl, requestBody);
    }
    catch (e) {
        wiAdoData = await commonFallback1(sprintDate);
    }
    return wiAdoData;
}

/* 6 WORK ITEM ATTRIBUTES DATA */
async function handleGetWorkItemAttributesBatchData(workItemIds, sprintDateYMDTHMSMSZ) {
    let wiabAdoData;
    try {
        var requestUrl = "https://dev.azure.com/" + vsoContextAccountName + "/" + vsoContextProjectName + "/_apis/wit/workitemsbatch?api-version=5.1";
        var requestBody = '{ "ids": ' + workItemIds + ' , "asOf": "' + sprintDateYMDTHMSMSZ + '", "fields": ["System.Id", "System.Title", "System.WorkItemType", "Microsoft.VSTS.Scheduling.StoryPoints", "System.State", "Microsoft.VSTS.Scheduling.Effort", "Microsoft.VSTS.Scheduling.OriginalEstimate", "Microsoft.VSTS.Scheduling.Size" ] }';
        wiabAdoData = await commonFetch1("POST", requestUrl, requestBody);
    }
    catch (e) {
        wiabAdoData = await commonFallback1(workItemIds, sprintDateYMDTHMSMSZ);
    }
    return wiabAdoData;
}

(function () {
    var baseIframeUrl = location.href.replace("index.min.html", "");
    var baseElement = document.createElement('base');
    baseElement.href = baseIframeUrl;
    document.head.appendChild(baseElement);
})();

async function copyTextToClipboard(clipboardText) {
    var alertPrefix = "Copy to clipboard ";
    if (!navigator.clipboard) {
        var failureText = "failure";
        console.error(alertPrefix + failureText, clipboardText);
        return;
    }
    navigator.clipboard.writeText(clipboardText).then(function () {
    }, function (err) {
        var failedText = "failed";
        console.error(alertPrefix + failedText, err, clipboardText);
    });
}

async function fnImgMeme() {
    try {
        //console.meme("Foo", "Bar", "Good Guy Greg", 200, 150);
        console.meme("Ah", "Grasshopper", "https://i.imgur.com/6vhYZOq.jpg", 250, 250);
        console.log(
            "%chttps://bit.ly/BlazorRad",
            "color:red;font-family:system-ui;font-size:4rem;-webkit-text-stroke: 1px black;font-weight:bold"
        );
    } catch (e) {
        //Do nothing - doesn't matter if it failed
        console.log(e);//
    }
}
