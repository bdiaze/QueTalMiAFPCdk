$(document).ready(function () {
    var today = new Date();
    var dd = String(today.getDate()).padStart(2, '0');
    var mm = String(today.getMonth() + 1).padStart(2, '0'); //January is 0!
    var yyyy = today.getFullYear();

    $('.input-daterange').datepicker({
        language: "es",
        autoclose: true,
        startDate: "01/08/2002",
        endDate: dd + '/' + mm + '/' + yyyy,
        maxViewMode: 2,
        templates: {
            leftArrow: '<i class="fa-solid fa-chevron-left"></i>',
            rightArrow: '<i class="fa-solid fa-chevron-right"></i>'
        }
    });

    $("#filtroFondosHistorial").on("change", "input[name='resumenFondoHistorial']", function () {
        let fondo = $(this).val();

        // Se graba en cookie el último fondo seleccionado...
        $.cookie("FiltroHistorialFondoSeleccionado", fondo, { expires: 365, path: '/' });

        ["A", "B", "C", "D", "E"].forEach(function (value, index, array) {
            $("#historial" + value).hide();
        });

        $("#historial" + fondo).show();
    });
});


$("#btnDescargarCSV").click(function () {
    $("#alertaDescargarCSV").css("display", "none");

    let listaAFPs = $("#afpCapital").is(":checked") ? $("#afpCapital").val() + "," : "";
    listaAFPs += $("#afpCuprum").is(":checked") ? $("#afpCuprum").val() + "," : "";
    listaAFPs += $("#afpHabitat").is(":checked") ? $("#afpHabitat").val() + "," : "";
    listaAFPs += $("#afpModelo").is(":checked") ? $("#afpModelo").val() + "," : "";
    listaAFPs += $("#afpPlanvital").is(":checked") ? $("#afpPlanvital").val() + "," : "";
    listaAFPs += $("#afpProvida").is(":checked") ? $("#afpProvida").val() + "," : "";
    listaAFPs += $("#afpUno").is(":checked") ? $("#afpUno").val() + "," : "";
    if (listaAFPs.charAt(listaAFPs.length - 1) == ",") {
        listaAFPs = listaAFPs.substr(0, listaAFPs.length - 1);
    }

    if (listaAFPs.trim().length == 0) {
        $("#alertaDescargarCSV").children("p").html("Debe seleccionar al menos una AFP.");
        $("#alertaDescargarCSV").fadeIn("fast");
        return;
    }

    let listaFondos = $("#fondoA").is(":checked") ? $("#fondoA").val() + "," : "";
    listaFondos += $("#fondoB").is(":checked") ? $("#fondoB").val() + "," : "";
    listaFondos += $("#fondoC").is(":checked") ? $("#fondoC").val() + "," : "";
    listaFondos += $("#fondoD").is(":checked") ? $("#fondoD").val() + "," : "";
    listaFondos += $("#fondoE").is(":checked") ? $("#fondoE").val() + "," : "";
    if (listaFondos.charAt(listaFondos.length - 1) == ",") {
        listaFondos = listaFondos.substr(0, listaFondos.length - 1);
    }

    if (listaFondos.trim().length == 0) {
        $("#alertaDescargarCSV").children("p").html("Debe seleccionar al menos un fondo.");
        $("#alertaDescargarCSV").fadeIn("fast");
        return;
    }

    let fechaInicio = $("#fechaInicio").val();
    let fechaFinal = $("#fechaFinal").val();

    let url = "/api/Cuota/DescargarCuotasCSV?";
    url += "listaAFPs=" + encodeURIComponent(listaAFPs);
    url += "&listaFondos=" + encodeURIComponent(listaFondos);
    url += "&fechaInicial=" + encodeURIComponent(fechaInicio);
    url += "&fechaFinal=" + encodeURIComponent(fechaFinal);
        
    location.href = url;
});