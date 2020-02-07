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
