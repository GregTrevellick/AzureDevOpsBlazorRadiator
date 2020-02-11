(function () {
    var baseIframeUrl = location.href.replace("index.min.html", "");
    //console.log("VSIX: biu=" + baseIframeUrl);
    var baseElement = document.createElement('base');
    baseElement.href = baseIframeUrl;
    document.head.appendChild(baseElement);

})();
