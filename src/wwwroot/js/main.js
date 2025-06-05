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
                text: event.detail.xhr.response,
                customClass: {
                    confirmButton: 'button is-small is-primary',
                    denyButton: 'button is-small is-danger',
                    cancelButton: 'button is-small'
                } 
            });
        }
    });
});

function cookieForbidden() {
    Swal.fire({
        scrollbarPadding: false,
        title: 'Access not allowed',
        customClass: {
            confirmButton: 'button is-small is-primary',
            denyButton: 'button is-small is-danger',
            cancelButton: 'button is-small'
        }
    });
}

function showToast() {
    Swal.fire({
        scrollbarPadding: false,
        title: "Configuration/s updated",
        toast: true,
        position: 'bottom-end',
        timer: 5000,
        showConfirmButton: false
    });
}