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
        templates: {
            leftArrow: '<i class="fa fa-long-arrow-left"></i>',
            rightArrow: '<i class="fa fa-long-arrow-right"></i>'
        }
    });

    actualizarUrlEjemplo();
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

function actualizarUrlEjemplo() {
    let listaAFPs = $("#afpCapitalEjemplo").is(":checked") ? $("#afpCapitalEjemplo").val() + "," : "";
    listaAFPs += $("#afpCuprumEjemplo").is(":checked") ? $("#afpCuprumEjemplo").val() + "," : "";
    listaAFPs += $("#afpHabitatEjemplo").is(":checked") ? $("#afpHabitatEjemplo").val() + "," : "";
    listaAFPs += $("#afpModeloEjemplo").is(":checked") ? $("#afpModeloEjemplo").val() + "," : "";
    listaAFPs += $("#afpPlanvitalEjemplo").is(":checked") ? $("#afpPlanvitalEjemplo").val() + "," : "";
    listaAFPs += $("#afpProvidaEjemplo").is(":checked") ? $("#afpProvidaEjemplo").val() + "," : "";
    listaAFPs += $("#afpUnoEjemplo").is(":checked") ? $("#afpUnoEjemplo").val() + "," : "";
    if (listaAFPs.charAt(listaAFPs.length - 1) == ",") {
        listaAFPs = listaAFPs.substr(0, listaAFPs.length - 1);
    }

    let listaFondos = $("#fondoAEjemplo").is(":checked") ? $("#fondoAEjemplo").val() + "," : "";
    listaFondos += $("#fondoBEjemplo").is(":checked") ? $("#fondoBEjemplo").val() + "," : "";
    listaFondos += $("#fondoCEjemplo").is(":checked") ? $("#fondoCEjemplo").val() + "," : "";
    listaFondos += $("#fondoDEjemplo").is(":checked") ? $("#fondoDEjemplo").val() + "," : "";
    listaFondos += $("#fondoEEjemplo").is(":checked") ? $("#fondoEEjemplo").val() + "," : "";
    if (listaFondos.charAt(listaFondos.length - 1) == ",") {
        listaFondos = listaFondos.substr(0, listaFondos.length - 1);
    }

    let fechaInicio = $("#fechaInicioEjemplo").val();
    let fechaFinal = $("#fechaFinalEjemplo").val();

    let urlResultante = $("#urlAPI").html() + "?";
    urlResultante += "listaAFPs=" + encodeURIComponent(listaAFPs);
    urlResultante += "&listaFondos=" + encodeURIComponent(listaFondos);
    urlResultante += "&fechaInicial=" + encodeURIComponent(fechaInicio);
    urlResultante += "&fechaFinal=" + encodeURIComponent(fechaFinal);

    $("#urlResultanteEjemplo").val(urlResultante);
}

function consultaAPIEjemplo() {
    $.ajax({
        url: $("#urlResultanteEjemplo").val(),
        beforeSend: function (jqXHR, settings) {
            $("#btnConsultaAPIEjemplo").find("span").css("display", "");
            $("#btnConsultaAPIEjemplo").prop("disabled", true);
            $("#resultadoEjemplo").parent("div").css("display", "none");
        },
        success: function (data, textStatus, jqXHR) {
            var strJSON = JSON.stringify(data, null, 4);
            $("#resultadoEjemplo").val(strJSON.substr(0, 5000) +
                (strJSON.length > 5000 ? "...\n\nSolo se presentan los primeros 5.000 caracteres de la salida." : ""));
        },
        error: function (jqXHR, textStatus, errorThrown) {
            $("#resultadoEjemplo").val(JSON.stringify(jqXHR["responseJSON"], null, 4));
        },
        complete: function (jqXHR, textStatus) {
            $("#btnConsultaAPIEjemplo").find("span").css("display", "none");
            $("#btnConsultaAPIEjemplo").prop("disabled", false);
            $("#resultadoEjemplo").parent("div").css("display", "");
        }
    });
}