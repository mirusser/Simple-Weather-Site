

function showBootstrapLoader(title) {

    if (title == null || title.trim() == '') {

        title = 'Getting data';
    }

    $.post(
        '/Shared/BootstrapLoader',
        {},
        function (response) {

            $(document).Toasts('create', {
                class: 'bg-info loading-toast',
                icon: 'fas fa-download fa-lg',
                title: title,
                close: false,
                body: response,
            });
        }
    );
}

function removeBoostrapLoader() {
    setTimeout(function () {
        $('.loading-toast').remove();
    }, 500);
}