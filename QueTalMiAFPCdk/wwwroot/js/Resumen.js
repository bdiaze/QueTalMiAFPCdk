$(document).ready(function () {
    $("#filtroFondos").on("change", "input[name='resumenFondo']", function () {
        let fondo = $(this).val();

        // Se graba en cookie el último fondo seleccionado...
        $.cookie("FiltroResumenFondoSeleccionado", fondo, { expires: 365, path: '/' });

        ["A", "B", "C", "D", "E"].forEach(function (value, index, array) {
            $("#resumen" + value).hide();
        });

        $("#resumen" + fondo).show();
    });

    $("#selectPremioRentabilidad").on("change", function () {
        this.form.submit();
    });
});