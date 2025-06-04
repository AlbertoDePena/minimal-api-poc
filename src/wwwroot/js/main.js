document.addEventListener('DOMContentLoaded', function () {
    // override config to handle htmx quirks
    htmx.config.disableInheritance = true;
    htmx.config.historyEnabled = false;
    htmx.config.historyCacheSize = 0;

    htmx.on("htmx:afterRequest", function (event) {
        if (event.detail.failed) {
            Swal.fire({
                scrollbarPadding: false,
                title: "An unhandled error occurred",
                text: event.detail.xhr.response                
            });
        }
    });
});