(function () {
    var baseIframeUrl = location.href.replace("index.min.html", "");
    var baseElement = document.createElement('base');
    baseElement.href = baseIframeUrl;
    document.head.appendChild(baseElement);
})();
