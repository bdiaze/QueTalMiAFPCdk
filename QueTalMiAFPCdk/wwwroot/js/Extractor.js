$(document).ready(function () {
    $('form.conCargando').submit(function () {
        $(this).find('button[type=submit]').prop('disabled', true);
        $(this).find('span.spinner-border').css('display', '');
        $(this).find('.logs').hide();
    });
});