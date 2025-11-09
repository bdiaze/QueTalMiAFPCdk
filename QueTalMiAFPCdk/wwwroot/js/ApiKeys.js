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

    actualizarUrlEjemplo();
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
        listaAFPs = listaAFPs.substring(0, listaAFPs.length - 1);
    }

    let listaFondos = $("#fondoAEjemplo").is(":checked") ? $("#fondoAEjemplo").val() + "," : "";
    listaFondos += $("#fondoBEjemplo").is(":checked") ? $("#fondoBEjemplo").val() + "," : "";
    listaFondos += $("#fondoCEjemplo").is(":checked") ? $("#fondoCEjemplo").val() + "," : "";
    listaFondos += $("#fondoDEjemplo").is(":checked") ? $("#fondoDEjemplo").val() + "," : "";
    listaFondos += $("#fondoEEjemplo").is(":checked") ? $("#fondoEEjemplo").val() + "," : "";
    if (listaFondos.charAt(listaFondos.length - 1) == ",") {
        listaFondos = listaFondos.substring(0, listaFondos.length - 1);
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
    let apiKey = $('#apiKeyEjemplo').val();
    if (apiKey.length == 0) {
        apiKey = "x";
    }
    $.ajax({
        url: $("#urlResultanteEjemplo").val(),
        headers: {
            "X-API-Key": apiKey
        },
        beforeSend: function (jqXHR, settings) {
            $("#btnConsultaAPIEjemplo").find("span").css("display", "");
            $("#btnConsultaAPIEjemplo").prop("disabled", true);
            $("#resultadoEjemplo").parent("div").css("display", "none");
        },
        success: function (data, textStatus, jqXHR) {
            var strJSON = JSON.stringify(data, null, 4);
            $("#resultadoEjemplo").val(strJSON.substring(0, 5000) + (strJSON.length > 5000 ? "...\n\nSolo se presentan los primeros 5.000 caracteres de la salida." : ""));
            $("#resultadoEjemplo").attr("rows", 20);
        },
        error: function (jqXHR, textStatus, errorThrown) {
            if (jqXHR["status"] == 401) {
                $("#resultadoEjemplo").val("401 - Unauthorized");
                $("#resultadoEjemplo").attr("rows", 1);
            } else {
                $("#resultadoEjemplo").val(JSON.stringify(jqXHR["responseJSON"], null, 4));
                $("#resultadoEjemplo").attr("rows", 20);
            }
        },
        complete: function (jqXHR, textStatus) {
            $("#btnConsultaAPIEjemplo").find("span").css("display", "none");
            $("#btnConsultaAPIEjemplo").prop("disabled", false);
            $("#resultadoEjemplo").parent("div").css("display", "");
        }
    });
}