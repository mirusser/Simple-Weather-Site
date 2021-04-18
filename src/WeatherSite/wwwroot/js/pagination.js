

function pagination(elementId, url, pageNumber, numberOfEntitiesOnPage) {

    showBaseLoader(elementId);

    //$("#wyszukaj-pozycje-nr-strony-hidden-input").val(numerStrony);
    //$("#wyszukaj-pozycje-ilosc-na-stronie-hidden-input").val(iloscNaStronie);

    $.post(url,
        {
            'pageNumber': pageNumber,
            'numberOfEntitiesOnPage': numberOfEntitiesOnPage
        }, function (data) {
            console.log('loading pagination: success');
            $(elementId).html(data);
        })
        .fail(function (data) {

            console.log('loading pagination: fails');
            console.log(data);
            console.log(data.responseText);

            //$(elementId).html('<p>' + data.responseText + '</p> <p>loading pagination: fails</p>');

            $.get('/Home/ErrorPartial/', { 'code': data.responseJSON.code, 'message': data.responseJSON.message }, function (response) {

                $(elementId).html(response);
            });
        });
}

function getNumberOfEntitiesOnPageSelectedValue() {
    return $('#number-of-entities-on-page-select').val();
}

function getPageNumberUserInputValue() {
    return $("#page-number-user-input").val();
}