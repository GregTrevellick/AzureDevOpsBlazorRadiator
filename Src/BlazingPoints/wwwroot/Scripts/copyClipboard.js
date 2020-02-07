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
