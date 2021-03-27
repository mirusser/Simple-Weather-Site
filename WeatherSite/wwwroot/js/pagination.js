
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
        .fail(function () {
            console.log('loading pagination: fails');
            $(elementId).html('<p>loading pagination: fails</p>');
        });
}

function getNumberOfEntitiesOnPageSelectedValue() {
    return $('#number-of-entities-on-page-select').val();
}

function getPageNumberUserInputValue() {
    return $("#page-number-user-input").val();
}