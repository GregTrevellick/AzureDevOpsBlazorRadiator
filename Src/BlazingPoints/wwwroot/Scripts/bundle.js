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
        var requestBody = '{ "ids": ' + workItemIds + ' , "asOf": "' + sprintDateYMDTHMSMSZ + '", "fields": ["System.Id", "System.Title", "System.WorkItemType", "Microsoft.VSTS.Scheduling.StoryPoints", "System.State", "Microsoft.VSTS.Scheduling.Effort" ] }';
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

(function () {
    var baseIframeUrl = location.href.replace("index.min.html", "");
    console.log("VSIX: biu=" + baseIframeUrl);
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
            "%chttps://bit.ly/BlazorRadiator",
            "color:red;font-family:system-ui;font-size:4rem;-webkit-text-stroke: 1px black;font-weight:bold"
        );
    } catch (e) {
        //Do nothing - doesn't matter if it failed
        console.log(e);//
    }
}
