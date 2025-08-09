$(document).ready(function () {
    let today = new Date();
    let dd = String(today.getDate()).padStart(2, '0');
    let mm = String(today.getMonth() + 1).padStart(2, '0'); //January is 0!
    let yyyy = today.getFullYear();

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

    am4core.ready(function () {
        am4core.options.queue = true;

        $("#collapseGPFiltrosAvanzados").on("show.bs.collapse", function () {
            $("#headingGPFiltrosAvanzados").find("i.fa-chevron-down").hide();
            $("#headingGPFiltrosAvanzados").find("i.fa-chevron-up").show();
        });

        $("#collapseGPFiltrosAvanzados").on("hide.bs.collapse", function () {
            $("#headingGPFiltrosAvanzados").find("i.fa-chevron-down").show();
            $("#headingGPFiltrosAvanzados").find("i.fa-chevron-up").hide();
        });

        ["A", "B", "C", "D", "E"].forEach(fondo => {
            $("#collapseGPFondo" + fondo).on("show.bs.collapse", function () {
                $("#headingGPFondo" + fondo).find("i.fa-chevron-down").hide();
                $("#headingGPFondo" + fondo).find("i.fa-chevron-up").show();
                marcarGraficoAbierto("GPFondo" + fondo);
                obtenerGananPesosSoloTipo(fondo);
            });

            $("#collapseGPFondo" + fondo).on("hide.bs.collapse", function () {
                $("#headingGPFondo" + fondo).find("i.fa-chevron-down").show();
                $("#headingGPFondo" + fondo).find("i.fa-chevron-up").hide();
                marcarGraficoCerrado("GPFondo" + fondo);
            });
        });

        ["A", "B", "C", "D", "E"].forEach(fondo => {
            $("#collapseCAVFondo" + fondo).on("show.bs.collapse", function () {
                $("#headingCAVFondo" + fondo).find("i.fa-chevron-down").hide();
                $("#headingCAVFondo" + fondo).find("i.fa-chevron-up").show();
                marcarGraficoAbierto("CAVFondo" + fondo);
                obtenerCAVSoloTipo(fondo);
            });

            $("#collapseCAVFondo" + fondo).on("hide.bs.collapse", function () {
                $("#headingCAVFondo" + fondo).find("i.fa-chevron-down").show();
                $("#headingCAVFondo" + fondo).find("i.fa-chevron-up").hide();
                marcarGraficoCerrado("CAVFondo" + fondo);
            });
        });

        // Se abren los gráficos que ya estaban abiertos, si no hay ninguno se abre el primero por defecto...
        let graficosAbiertos = $.cookie("GraficosAbiertosSimulador");
        if (graficosAbiertos == undefined) {
            graficosAbiertos = ["GPFondoA"];
        } else {
            graficosAbiertos = graficosAbiertos.split(",");
        }
        graficosAbiertos.forEach(grafico => {
            $("#collapse" + grafico).collapse("show");
        });
    });
});

$("#sueldoMensual").focus(function () {
    let montoSinFormato = $("#sueldoMensual").val().replace(/(\.|\$)/g, "");
    $("#sueldoMensual").val(montoSinFormato);
});

$("#sueldoMensual").blur(function () {
    let montoIngresado = $("#sueldoMensual").val();
    let montoSinFormato = "";
    for (let i = 0; i < montoIngresado.length; i++) {
        let caracter = montoIngresado[i];
        if ("0123456789".includes(caracter)) {
            montoSinFormato += caracter
        } else if (caracter == ",") {
            break;
        }
    }
    let formateador = new Intl.NumberFormat("es-ES");
    $("#sueldoMensual").val("$" + formateador.format(montoSinFormato));
});

$("#ahorroInicialMaximo").focus(function () {
    let montoSinFormato = $("#ahorroInicialMaximo").val().replace(/(\.|\$)/g, "");
    $("#ahorroInicialMaximo").val(montoSinFormato);
});

$("#ahorroInicialMaximo").blur(function () {
    let montoIngresado = $("#ahorroInicialMaximo").val();
    let montoSinFormato = "";
    for (let i = 0; i < montoIngresado.length; i++) {
        let caracter = montoIngresado[i];
        if ("0123456789".includes(caracter)) {
            montoSinFormato += caracter
        } else if (caracter == ",") {
            break;
        }
    }
    let formateador = new Intl.NumberFormat("es-ES");
    $("#ahorroInicialMaximo").val("$" + formateador.format(montoSinFormato));
});

$("#efectuarSimulacionCada").focus(function () {
    let montoSinFormato = $("#efectuarSimulacionCada").val().replace(/(\.|\$)/g, "");
    $("#efectuarSimulacionCada").val(montoSinFormato);
});

$("#efectuarSimulacionCada").blur(function () {
    let montoIngresado = $("#efectuarSimulacionCada").val();
    let montoSinFormato = "";
    for (let i = 0; i < montoIngresado.length; i++) {
        let caracter = montoIngresado[i];
        if ("0123456789".includes(caracter)) {
            montoSinFormato += caracter
        } else if (caracter == ",") {
            break;
        }
    }
    let formateador = new Intl.NumberFormat("es-ES");
    $("#efectuarSimulacionCada").val("$" + formateador.format(montoSinFormato));
});

function marcarGraficoAbierto(grafico) {
    let graficosAbiertos = $.cookie("GraficosAbiertosSimulador");
    if (graficosAbiertos == undefined) {
        graficosAbiertos = [];
    } else {
        graficosAbiertos = graficosAbiertos.split(",");
    }

    if (!graficosAbiertos.includes(grafico)) {
        graficosAbiertos.push(grafico);
    }
    $.cookie("GraficosAbiertosSimulador", graficosAbiertos.join(","), { expires: 365, path: '/Simulador' });
}

function marcarGraficoCerrado(grafico) {
    let graficosAbiertos = $.cookie("GraficosAbiertosSimulador");
    if (graficosAbiertos == undefined) {
        graficosAbiertos = [];
    } else {
        graficosAbiertos = graficosAbiertos.split(",");
    }

    let posicion = graficosAbiertos.indexOf(grafico);
    if (posicion >= 0) {
        graficosAbiertos.splice(posicion, 1);
    }

    if (graficosAbiertos.length > 0) {
        $.cookie("GraficosAbiertosSimulador", graficosAbiertos.join(","), { expires: 365, path: '/Simulador' });
    } else {
        $.removeCookie('GraficosAbiertosSimulador', { path: '/Simulador' });
    }
}

function btnFiltrarGanancias() {
    // Registramos en las cookies el sueldo imponible y dia de cotizacion para facilitar experiencia del usuario...
    let date = new Date();
    date.setTime(date.getTime() + (10 * 365 * 24 * 60 * 60 * 1000));

    // Se efectúa validación sobre sueldoImponible
    let sueldoImponible = $("#sueldoMensual").val();
    if (Number(sueldoImponible.replace(/(\.|\$)/g, "")) <= 0) {
        sueldoImponible = "$600.000";
        $("#sueldoMensual").val(sueldoImponible);
    }
    document.cookie = "SueldoImponible=" + encodeURIComponent(sueldoImponible) + "; expires=" + date.toGMTString() + "; path=/Simulador";

    let diaCotizacion = $("#diaPagoCotiz").val();
    document.cookie = "DiaCotizacion=" + encodeURIComponent(diaCotizacion) + "; expires=" + date.toGMTString() + "; path=/Simulador";

    let formateador = new Intl.NumberFormat("es-ES");

    // Se efectúa validación sobre ahorroInicialMaximo
    let ahorroInicialMaximo = $("#ahorroInicialMaximo").val();
    let numberAhorroInicialMaximo = Number(ahorroInicialMaximo.replace(/(\.|\$)/g, ""));
    if (numberAhorroInicialMaximo <= 0) {
        numberAhorroInicialMaximo = 50 * 1000 * 1000;
        ahorroInicialMaximo = formateador.format(numberAhorroInicialMaximo);
        $("#ahorroInicialMaximo").val(ahorroInicialMaximo);
    }
    document.cookie = "AhorroInicialMaximo=" + encodeURIComponent(ahorroInicialMaximo) + "; expires=" + date.toGMTString() + "; path=/Simulador";

    // Se efectúa validación sobre efectuarSimulacionCada
    let efectuarSimulacionCada = $("#efectuarSimulacionCada").val();
    let numberEfectuarSimulacionCada = Number(efectuarSimulacionCada.replace(/(\.|\$)/g, ""));
    if (numberEfectuarSimulacionCada <= 0) {
        numberEfectuarSimulacionCada = 500000;
        efectuarSimulacionCada = formateador.format(numberEfectuarSimulacionCada);
        $("#efectuarSimulacionCada").val(efectuarSimulacionCada);
    }
    if (numberEfectuarSimulacionCada > numberAhorroInicialMaximo) {
        numberEfectuarSimulacionCada = numberAhorroInicialMaximo;
        efectuarSimulacionCada = formateador.format(numberEfectuarSimulacionCada);
        $("#efectuarSimulacionCada").val(efectuarSimulacionCada);
    }
    if (numberAhorroInicialMaximo / numberEfectuarSimulacionCada > 1000) {
        numberEfectuarSimulacionCada = Math.max(Math.round(numberAhorroInicialMaximo / 1000), 1);
        efectuarSimulacionCada = formateador.format(numberEfectuarSimulacionCada);
        $("#efectuarSimulacionCada").val(efectuarSimulacionCada);
    }
    document.cookie = "EfectuarSimulacionCada=" + encodeURIComponent(efectuarSimulacionCada) + "; expires=" + date.toGMTString() + "; path=/Simulador";

    ["A", "B", "C", "D", "E"].forEach(fondo => {
        if ($("#headingGPFondo" + fondo).children("button").attr("aria-expanded") == "true") {
            obtenerGananPesosSoloTipo(fondo);
        }
    });
}

function obtenerGananPesosSoloTipo(tipoFondo) {
    let sueldoImponible = Number($("#sueldoMensual").val().replace(/(\.|\$)/g, ""));
    let diaCotizacion = Number($("#diaPagoCotiz").val());
    let fechaInicio = $("#fechaInicio").val();
    let fechaFinal = $("#fechaFinal").val();
    let ahorroInicialMaximo = Number($("#ahorroInicialMaximo").val().replace(/(\.|\$)/g, ""));
    let efectuarSimulacionCada = Number($("#efectuarSimulacionCada").val().replace(/(\.|\$)/g, ""));

    if (sueldoImponible != $("#chartGananPesos" + tipoFondo).attr("sueldoImponible") ||
        diaCotizacion != $("#chartGananPesos" + tipoFondo).attr("diaCotizacion") ||
        fechaInicio != $("#chartGananPesos" + tipoFondo).attr("fechaInicio") ||
        fechaFinal != $("#chartGananPesos" + tipoFondo).attr("fechaFinal") ||
        ahorroInicialMaximo != $("#chartGananPesos" + tipoFondo).attr("ahorroInicialMaximo") ||
        efectuarSimulacionCada != $("#chartGananPesos" + tipoFondo).attr("efectuarSimulacionCada")) {

        obtenerGananPesos(tipoFondo, sueldoImponible, diaCotizacion, fechaInicio, fechaFinal, ahorroInicialMaximo, efectuarSimulacionCada);

    }
}

function obtenerGananPesos(tipoFondo, sueldoImponible, diaCotizacion, fechaInicio, fechaFinal, ahorroInicialMaximo, efectuarSimulacionCada) {
    let diaMesAnnoInicio = fechaInicio.split("/");
    let diaMesAnnoFinal = fechaFinal.split("/");
    let dtFechaInicio = new Date(diaMesAnnoInicio[2], parseInt(diaMesAnnoInicio[1]) - 1, diaMesAnnoInicio[0]);
    let dtFechaFinal = new Date(diaMesAnnoFinal[2], parseInt(diaMesAnnoFinal[1]) - 1, diaMesAnnoFinal[0]);

    let esFechaInicialPagoCotizacion = false;
    let esFechaFinalPagoCotizacion = false;

    let montoPesosCotizMensual = sueldoImponible * 0.1;

    let lstFechas = dtFechaInicio.getDate() + "/" + (dtFechaInicio.getMonth() + 1) + "/" + dtFechaInicio.getFullYear() + ",";
    let dtFechaAux = new Date(dtFechaInicio.getFullYear(), dtFechaInicio.getMonth(), 1);
    let dtFechaAuxFin = new Date(dtFechaFinal.getFullYear(), dtFechaFinal.getMonth(), 1);
    while (dtFechaAux <= dtFechaAuxFin) {
        let dtFecha = new Date(dtFechaAux.getFullYear(), dtFechaAux.getMonth(), dtFechaAux.getDate());
        dtFecha.setDate(dtFecha.getDate() + diaCotizacion - 1);
        if (dtFechaInicio.getTime() < dtFecha.getTime() && dtFecha.getTime() < dtFechaFinal.getTime()) {
            lstFechas += dtFecha.getDate() + "/" + (dtFecha.getMonth() + 1) + "/" + dtFecha.getFullYear() + ",";
        }

        if (dtFechaInicio.getTime() == dtFecha.getTime()) {
            esFechaInicialPagoCotizacion = true;
        }

        if (dtFechaFinal.getTime() == dtFecha.getTime()) {
            esFechaFinalPagoCotizacion = true;
        }

        dtFechaAux.setMonth(dtFechaAux.getMonth() + 1);
    }

    if (dtFechaInicio.getTime() != dtFechaFinal.getTime()) {
        lstFechas += dtFechaFinal.getDate() + "/" + (dtFechaFinal.getMonth() + 1) + "/" + dtFechaFinal.getFullYear();
    }

    if (lstFechas.charAt(lstFechas.length - 1) == ",") {
        lstFechas = lstFechas.substr(0, lstFechas.length - 1);
    }

    // Nos aseguramos que la frecuencia con la que se hacen las simulaciones sea mayor a 0
    efectuarSimulacionCada = typeof efectuarSimulacionCada == "number" && efectuarSimulacionCada > 0 ? efectuarSimulacionCada : 500 * 1000;

    let formateador = new Intl.NumberFormat("es-ES");

    let zoomInicio = "$0";
    let zoomFin = "$" + formateador.format(Math.max(Math.floor(ahorroInicialMaximo * 0.2 / efectuarSimulacionCada) * efectuarSimulacionCada, efectuarSimulacionCada));

    crearGrafica(
        "chartGananPesos" + tipoFondo,
        null,
        fechaInicio,
        zoomInicio,
        zoomFin
    );

    consultarUltimasCuotas(
        "CAPITAL,CUPRUM,HABITAT,MODELO,PLANVITAL,PROVIDA,UNO",
        tipoFondo,
        lstFechas,
        1,
        esFechaInicialPagoCotizacion,
        esFechaFinalPagoCotizacion,
        function (jqXHR, settings) {
            $("#chartGananPesos" + tipoFondo).removeAttr("sueldoImponible");
            $("#chartGananPesos" + tipoFondo).removeAttr("diaCotizacion");
            $("#chartGananPesos" + tipoFondo).removeAttr("fechaInicio");
            $("#chartGananPesos" + tipoFondo).removeAttr("fechaFinal");
            $("#chartGananPesos" + tipoFondo).removeAttr("ahorroInicialMaximo");
            $("#chartGananPesos" + tipoFondo).removeAttr("efectuarSimulacionCada");
        },
        function (parametrosAFPs, textStatus, jqXHR) {
            let listaGanancias = [];

            pagosCotizaciones = {};
            for (let afp in parametrosAFPs) {
                let cantPesosDepositados = 0;
                let cantCuotasCompradas = 0;
                let cantPesosPagadosComision = 0;
                parametrosAFPs[afp]["valoresCuotasPagoCotiz"].forEach(valorCuota => {
                    cantPesosDepositados += montoPesosCotizMensual;
                    cantCuotasCompradas += montoPesosCotizMensual / valorCuota["valor"];
                    cantPesosPagadosComision += sueldoImponible * valorCuota["comision"] / 100;
                });

                pagosCotizaciones[afp] = {};
                pagosCotizaciones[afp]["cantPesosDepositados"] = cantPesosDepositados;
                pagosCotizaciones[afp]["cantCuotasCompradas"] = cantCuotasCompradas;
                pagosCotizaciones[afp]["cantPesosPagadosComision"] = cantPesosPagadosComision;
            }

            for (let cantPesosIniciales = 0; cantPesosIniciales <= ahorroInicialMaximo; cantPesosIniciales += efectuarSimulacionCada) {
                for (let afp in parametrosAFPs) {
                    let montoInicial = "$" + formateador.format(cantPesosIniciales);
                    // let montoInicial = new Date(cantPesosIniciales); 
                    
                    let cantCuotasFinales = (cantPesosIniciales / parametrosAFPs[afp]["valorT0"]["valor"]) + pagosCotizaciones[afp]["cantCuotasCompradas"];
                    let cantPesosFinales = cantCuotasFinales * parametrosAFPs[afp]["valorTf"]["valor"];
                    let gananciasPesos = cantPesosFinales - pagosCotizaciones[afp]["cantPesosDepositados"] - cantPesosIniciales - pagosCotizaciones[afp]["cantPesosPagadosComision"];

                    let objInfoExtra = new Object();
                    objInfoExtra["cantPesosIniciales"] = cantPesosIniciales;
                    objInfoExtra["cantPesosDepositados"] = pagosCotizaciones[afp]["cantPesosDepositados"];
                    objInfoExtra["cantPesosPagadosComision"] = pagosCotizaciones[afp]["cantPesosPagadosComision"];
                    objInfoExtra["cantPesosFinales"] = cantPesosFinales;

                    if (listaGanancias.length == 0) {
                        let nodoGanancias = new Object();
                        nodoGanancias["montoInicial"] = montoInicial;
                        nodoGanancias["ganancia" + afp.charAt(0) + afp.substr(1).toLowerCase()] = gananciasPesos;
                        nodoGanancias["datosExtra" + afp.charAt(0) + afp.substr(1).toLowerCase()] = objInfoExtra;
                        listaGanancias.push(nodoGanancias);
                    } else {
                        let ultimoNodo = listaGanancias[listaGanancias.length - 1];
                        if (ultimoNodo["montoInicial"] == montoInicial) {
                            ultimoNodo["ganancia" + afp.charAt(0) + afp.substr(1).toLowerCase()] = gananciasPesos;
                            ultimoNodo["datosExtra" + afp.charAt(0) + afp.substr(1).toLowerCase()] = objInfoExtra;
                        } else {
                            let nodoGanancias = new Object();
                            nodoGanancias["montoInicial"] = montoInicial;
                            nodoGanancias["ganancia" + afp.charAt(0) + afp.substr(1).toLowerCase()] = gananciasPesos;
                            nodoGanancias["datosExtra" + afp.charAt(0) + afp.substr(1).toLowerCase()] = objInfoExtra;
                            listaGanancias.push(nodoGanancias);
                        }
                    }
                }
            }

            $("#chartGananPesos" + tipoFondo).attr("sueldoImponible", sueldoImponible);
            $("#chartGananPesos" + tipoFondo).attr("diaCotizacion", diaCotizacion);
            $("#chartGananPesos" + tipoFondo).attr("fechaInicio", fechaInicio);
            $("#chartGananPesos" + tipoFondo).attr("fechaFinal", fechaFinal);
            $("#chartGananPesos" + tipoFondo).attr("ahorroInicialMaximo", ahorroInicialMaximo);
            $("#chartGananPesos" + tipoFondo).attr("efectuarSimulacionCada", efectuarSimulacionCada);

            // console.log(listaGanancias);

            actualizarDataGrafica(
                "chartGananPesos" + tipoFondo,
                listaGanancias
            );
        }
    );
}

/*
function obtenerCAVSoloTipo(tipoFondo) {
    let sueldoImponible = $("#sueldoMensual").val().replace(/(\.|\$)/g, "");
    let diaCotizacion = $("#diaPagoCotiz").val();
    let fechaInicio = $("#fechaInicio").val();
    let fechaFinal = $("#fechaFinal").val();
    if (sueldoImponible != $("#chartCAV" + tipoFondo).attr("sueldoImponible") ||
        diaCotizacion != $("#chartCAV" + tipoFondo).attr("diaCotizacion") ||
        fechaInicio != $("#chartCAV" + tipoFondo).attr("fechaInicio") ||
        fechaFinal != $("#chartCAV" + tipoFondo).attr("fechaFinal")) {
        obtenerCAV(tipoFondo, sueldoImponible, diaCotizacion, fechaInicio, fechaFinal);
    }
}
*/

/*
function obtenerCAV(tipoFondo, sueldoImponible, diaCotizacion, fechaInicio, fechaFinal) {
    let diaMesAnnoInicio = fechaInicio.split("/");
    let diaMesAnnoFinal = fechaFinal.split("/");
    let dtFechaInicio = new Date(diaMesAnnoInicio[2], parseInt(diaMesAnnoInicio[1]) - 1, diaMesAnnoInicio[0]);
    let dtFechaFinal = new Date(diaMesAnnoFinal[2], parseInt(diaMesAnnoFinal[1]) - 1, diaMesAnnoFinal[0]);

    let esFechaInicialPagoComision = false;
    let esFechaFinalPagoComision = false;

    let lstFechas = dtFechaInicio.getDate() + "/" + (dtFechaInicio.getMonth() + 1) + "/" + dtFechaInicio.getFullYear() + ",";
    let dtFechaAux = new Date(dtFechaInicio.getFullYear(), dtFechaInicio.getMonth(), 1);
    let dtFechaAuxFin = new Date(dtFechaFinal.getFullYear(), dtFechaFinal.getMonth(), 1);
    while (dtFechaAux <= dtFechaAuxFin) {
        let dtFecha = new Date(dtFechaAux.getFullYear(), dtFechaAux.getMonth(), dtFechaAux.getDate());
        dtFecha.setMonth(dtFecha.getMonth() + 1);
        dtFecha.setDate(dtFecha.getDate() - 1);
        if (dtFechaInicio.getTime() < dtFecha.getTime() && dtFecha.getTime() < dtFechaFinal.getTime()) {
            lstFechas += dtFecha.getDate() + "/" + (dtFecha.getMonth() + 1) + "/" + dtFecha.getFullYear() + ",";
        }

        if (dtFechaInicio.getTime() == dtFecha.getTime()) {
            esFechaInicialPagoComision = true;
        }

        if (dtFechaFinal.getTime() == dtFecha.getTime()) {
            esFechaFinalPagoComision = true;
        }

        dtFechaAux.setMonth(dtFechaAux.getMonth() + 1);
    }

    if (dtFechaInicio.getTime() != dtFechaFinal.getTime()) {
        lstFechas += dtFechaFinal.getDate() + "/" + (dtFechaFinal.getMonth() + 1) + "/" + dtFechaFinal.getFullYear();
    }

    if (lstFechas.charAt(lstFechas.length - 1) == ",") {
        lstFechas = lstFechas.substr(0, lstFechas.length - 1);
    }

    crearGrafica(
        "chartCAV" + tipoFondo,
        null,
        fechaInicio,
        2
    );

    consultarUltimasCuotas(
        "CAPITAL,CUPRUM,HABITAT,MODELO,PLANVITAL,PROVIDA,UNO",
        tipoFondo,
        lstFechas,
        2,
        esFechaInicialPagoComision,
        esFechaFinalPagoComision,
        function (jqXHR, settings) {
            $("#chartCAV" + tipoFondo).removeAttr("sueldoImponible");
            $("#chartCAV" + tipoFondo).removeAttr("diaCotizacion");
            $("#chartCAV" + tipoFondo).removeAttr("fechaInicio");
            $("#chartCAV" + tipoFondo).removeAttr("fechaFinal");
        },
        function (parametrosAFPs, textStatus, jqXHR) {
            let listaGanancias = [];

            let formateador = new Intl.NumberFormat("es-ES");

            for (let i = 0; i <= 50 * 1000 * 1000; i += 500 * 1000) {
                for (let afp in parametrosAFPs) {
                    let montoInicial = "$" + formateador.format(i);

                    let cantPesosIniciales = i;
                    let cantCuotasIniciales = i / parametrosAFPs[afp]["valorT0"]["valor"];
                    let cantPesosDepositados = 0;
                    let cantCuotasCompradas = 0;
                    let cantPesosPagadosComision = 0;
                    let cantCuotasPagadosComision = 0;
                    let cantPagosComision = 0;
                    parametrosAFPs[afp]["valoresCuotasPagoCotiz"].forEach(valorCuota => {
                        // cantPesosPagados += montoPesosCotizMensual;
                        // cantCuotasCompradas += montoPesosCotizMensual / valorCuota["valor"];
                        let comisionCuotas = (cantCuotasIniciales + cantCuotasCompradas - cantCuotasPagadosComision) * valorCuota["comision"] / 1200;
                        cantPesosPagadosComision += comisionCuotas * valorCuota["valor"];
                        cantCuotasPagadosComision += comisionCuotas;
                        cantPagosComision++;
                    });
                    let cantCuotasFinales = cantCuotasIniciales + cantCuotasCompradas - cantCuotasPagadosComision;
                    let cantPesosFinales = cantCuotasFinales * parametrosAFPs[afp]["valorTf"]["valor"];
                    let gananciasPesos = cantPesosFinales - cantPesosDepositados - cantPesosIniciales;

                    let objInfoExtra = new Object();
                    objInfoExtra["cantCuotasIniciales"] = cantCuotasIniciales;
                    objInfoExtra["cantCuotasCompradas"] = cantCuotasCompradas;
                    objInfoExtra["cantCuotasPagadosComision"] = cantCuotasPagadosComision;
                    objInfoExtra["cantCuotasFinales"] = cantCuotasFinales;
                    objInfoExtra["cantPagosCotizaciones"] = cantPagosComision;
                    objInfoExtra["cantPesosIniciales"] = cantPesosIniciales;
                    objInfoExtra["cantPesosDepositados"] = cantPesosDepositados;
                    objInfoExtra["cantPesosPagadosComision"] = cantPesosPagadosComision;
                    objInfoExtra["cantPesosFinales"] = cantPesosFinales;

                    if (listaGanancias.length == 0) {
                        let nodoGanancias = new Object();
                        nodoGanancias["montoInicial"] = montoInicial;
                        nodoGanancias["ganancia" + afp.charAt(0) + afp.substr(1).toLowerCase()] = gananciasPesos;
                        nodoGanancias["datosExtra" + afp.charAt(0) + afp.substr(1).toLowerCase()] = objInfoExtra;
                        listaGanancias.push(nodoGanancias);
                    } else {
                        let ultimoNodo = listaGanancias[listaGanancias.length - 1];
                        if (ultimoNodo["montoInicial"] == montoInicial) {
                            ultimoNodo["ganancia" + afp.charAt(0) + afp.substr(1).toLowerCase()] = gananciasPesos;
                            ultimoNodo["datosExtra" + afp.charAt(0) + afp.substr(1).toLowerCase()] = objInfoExtra;
                        } else {
                            let nodoGanancias = new Object();
                            nodoGanancias["montoInicial"] = montoInicial;
                            nodoGanancias["ganancia" + afp.charAt(0) + afp.substr(1).toLowerCase()] = gananciasPesos;
                            nodoGanancias["datosExtra" + afp.charAt(0) + afp.substr(1).toLowerCase()] = objInfoExtra;
                            listaGanancias.push(nodoGanancias);
                        }
                    }
                }
            }

            $("#chartCAV" + tipoFondo).attr("sueldoImponible", sueldoImponible);
            $("#chartCAV" + tipoFondo).attr("diaCotizacion", diaCotizacion);
            $("#chartCAV" + tipoFondo).attr("fechaInicio", fechaInicio);
            $("#chartCAV" + tipoFondo).attr("fechaFinal", fechaFinal);

            actualizarDataGrafica(
                "chartCAV" + tipoFondo,
                listaGanancias
            );
        }
    );
}
*/

var graficos = {};

function actualizarDataGrafica(idDiv, data) {
    graficos[idDiv].data = data;
}

function crearGrafica(idDiv, data, fechaInicio, zoomInicio, zoomFin, tipo = 1) {
    if (graficos[idDiv] == undefined) {
        // Create chart instance
        let chart = am4core.create(idDiv, am4charts.XYChart);
        chart.language.locale = am4lang_es_ES;
        // chart.dateFormatter.dateFormat = "x";
        // chart.dateFormatter.utc = true;

        // Add data
        // chart.data = data;

        // Create axes
        let xAxis = chart.xAxes.push(new am4charts.CategoryAxis());
        // let xAxis = chart.xAxes.push(new am4charts.DateAxis());
        xAxis.dataFields.category = "montoInicial";
        xAxis.title.text = "Ahorros Iniciales al " + fechaInicio;
        // xAxis.groupData = true;
        // xAxis.groupCount = 50;
        /* 
        xAxis.tooltipDateFormat = "x";
        xAxis.adapter.add("getTooltipText", (text) => {
            return "$" + text.replace(/\B(?=(\d{3})+(?!\d))/g, ".");
        });
        */
        // xAxis.tooltipDateFormat = "dd/MM/yyyy HH:mm:ss SSS";
        // xAxis.tooltipText = "'$'{dateX.formatDate('x').formatNumber('###,###,##0.00')}";

        /*
        xAxis.dateFormats.setKey("millisecond", "$x");
        xAxis.dateFormats.setKey("second", "$x");
        xAxis.dateFormats.setKey("minute", "$x");
        xAxis.dateFormats.setKey("hour", "$x");
        xAxis.dateFormats.setKey("day", "$x");
        xAxis.dateFormats.setKey("week", "$x");
        xAxis.dateFormats.setKey("month", "$x");
        xAxis.dateFormats.setKey("year", "$x");
        xAxis.periodChangeDateFormats.setKey("millisecond", "$x");
        xAxis.periodChangeDateFormats.setKey("second", "$x");
        xAxis.periodChangeDateFormats.setKey("minute", "$x");
        xAxis.periodChangeDateFormats.setKey("hour", "$x");
        xAxis.periodChangeDateFormats.setKey("day", "$x");
        xAxis.periodChangeDateFormats.setKey("week", "$x");
        xAxis.periodChangeDateFormats.setKey("month", "$x");
        xAxis.periodChangeDateFormats.setKey("year", "$x");
        */

        // xAxis.baseInterval = { "timeUnit": "second", "count": 10 };

        /*
        xAxis.groupIntervals.setAll([
            { timeUnit: "second", count: 10 },
            { timeUnit: "second", count: 30 },
            { timeUnit: "minute", count: 1 },
            { timeUnit: "minute", count: 10 },
            { timeUnit: "minute", count: 30 },
            { timeUnit: "hour", count: 1 }
        ]);
        */

        xAxis.tooltip.background.fill = am4core.color("#6794dc");
        xAxis.tooltip.background.cornerRadius = 4;
        xAxis.tooltip.background.strokeWidth = 0;
        // xAxis.skipEmptyPeriods = true;

        let yAxis = chart.yAxes.push(new am4charts.ValueAxis());
        yAxis.title.text = "Ganancias/Perdidas ($)";
        yAxis.title.valign = "top";
        yAxis.title.dy = 55;
        yAxis.cursorTooltipEnabled = false;

        let range = yAxis.axisRanges.create();
        range.value = 0;
        range.endValue = 1000 * 1000 * 1000;
        range.axisFill.fill = am4core.color("#28a745");
        range.axisFill.fillOpacity = 0.1;

        let range2 = yAxis.axisRanges.create();
        range2.value = -1000 * 1000 * 1000;
        range2.endValue = 0;
        range2.axisFill.fill = am4core.color("#dc3545");
        range2.axisFill.fillOpacity = 0.1;

        // Create series
        let colores = {
            "Capital": "#E3E829",
            "Cuprum": "#f2af32",
            "Habitat": "#2E8ECD",
            "Modelo": "#72b500",
            "PlanVital": "#c23b33",
            "ProVida": "#0061a0",
            "Uno": "#e3007d"
        };
        let series = [];
        ["Capital", "Cuprum", "Habitat", "Modelo", "PlanVital", "ProVida", "Uno"].forEach(afp => {
            let serie = chart.series.push(new am4charts.LineSeries());
            serie.dataFields.valueY = "ganancia" + afp.charAt(0) + afp.substr(1).toLowerCase();
            // serie.dataFields.dateX = "montoInicial";
            serie.dataFields.categoryX = "montoInicial";
            serie.dataFields.dummyData = "datosExtra" + afp.charAt(0) + afp.substr(1).toLowerCase();

            // serie.groupFields.valueY = "high";
            // serie.groupFields.valueX = "high";

            serie.name = afp;
            serie.tooltipText = "{name}: [bold]{valueY.formatNumber(\"'$'###,###,##0.00\")}[/]";
            if (tipo == 1) {
                serie.legendSettings.valueText = `[font-size: 14px]Ganancias: [/][bold font-size: 14px]{valueY.formatNumber("'$'###,###,##0.")}[/]
                                          [font-size: 14px]Aho. Inicial: {dummyData.cantPesosIniciales.formatNumber("'$'###,###,##0.")}[/]
                                          [font-size: 14px]Depósitos:    {dummyData.cantPesosDepositados.formatNumber("'$'###,###,##0.")}[/]
                                          [font-size: 14px]Comisiones:   {dummyData.cantPesosPagadosComision.formatNumber("'$'###,###,##0.")}[/]
                                          [font-size: 14px]Aho. Final:   {dummyData.cantPesosFinales.formatNumber("'$'###,###,##0.")}[/]
                                          [font-size: 14px]_____________________________[/]`;
            } else {
                serie.legendSettings.valueText = `[font-size: 14px]Ganancias: [/][bold font-size: 14px]{valueY.formatNumber("'$'###,###,##0.")}[/]
                                          [font-size: 14px]Aho. Inicial: {dummyData.cantPesosIniciales.formatNumber("'$'###,###,##0.")} ({dummyData.cantCuotasIniciales.formatNumber("###,##0.00' cts.'")})[/]
                                          [font-size: 14px]Depósitos:    {dummyData.cantPesosDepositados.formatNumber("'$'###,###,##0.")} ({dummyData.cantCuotasCompradas.formatNumber("###,##0.00' cts.'")})[/]
                                          [font-size: 14px]Comisiones:   {dummyData.cantPesosPagadosComision.formatNumber("'$'###,###,##0.")} ({dummyData.cantCuotasPagadosComision.formatNumber("###,##0.00' cts.'")})[/]
                                          [font-size: 14px]Aho. Final:   {dummyData.cantPesosFinales.formatNumber("'$'###,###,##0.")} ({dummyData.cantCuotasFinales.formatNumber("###,##0.00' cts.'")})[/]
                                          [font-size: 14px]_____________________________[/]`;
            }
            serie.strokeWidth = 2;
            serie.stroke = am4core.color(colores[afp]);
            serie.fill = am4core.color(colores[afp]);
            serie.simplifiedProcessing = true;

            /*
            serie.adapter.add("groupDataItem", function (val) {
                console.log(val);
                return val;
            });
            */

            series.push(serie);
        });

        // Add scrollbar
        chart.scrollbarX = new am4charts.XYChartScrollbar();
        chart.scrollbarX.startGrip.background.fill = am4core.color("#6794dc");
        chart.scrollbarX.startGrip.background.strokeWidth = 0;
        chart.scrollbarX.endGrip.background.fill = am4core.color("#6794dc");
        chart.scrollbarX.endGrip.background.strokeWidth = 0;
        chart.scrollbarX.thumb.background.fill = am4core.color("#6794dc");
        chart.scrollbarX.thumb.background.fillOpacity = 0.2;

        series.forEach(serie => {
            chart.scrollbarX.series.push(serie);
        });

        // Add cursor
        chart.cursor = new am4charts.XYCursor();
        chart.cursor.behavior = "panX";
        chart.cursor.lineY.disabled = true;

        // Add legend
        chart.legend = new am4charts.Legend();
        chart.legend.maxHeight = 260;
        chart.legend.scrollable = true;
        chart.legend.valueLabels.template.align = "right";
        chart.legend.valueLabels.template.textAlign = "end";

        // Create custom preloader
        let indicator = chart.tooltipContainer.createChild(am4core.Container);
        indicator.background.fill = am4core.color("#fff");
        indicator.background.fillOpacity = 0.95;
        indicator.width = am4core.percent(100);
        indicator.height = am4core.percent(100);
        indicator.dy = 1;
        let indicatorLabel = indicator.createChild(am4core.Label);
        indicatorLabel.text = "Consultando...";
        indicatorLabel.align = "center";
        indicatorLabel.valign = "middle";
        indicatorLabel.fontSize = 17;
        let hourglass = indicator.createChild(am4core.Image);
        hourglass.href = "images/loading.gif";
        hourglass.align = "center";
        hourglass.valign = "middle";
        hourglass.horizontalCenter = "middle";
        hourglass.verticalCenter = "middle";
        hourglass.dx = -80;
        hourglass.dy = -1;
        hourglass.scale = 0.7;

        chart["customPreloader"] = indicator;

        graficos[idDiv] = chart;
    }

    // Config initial zoom
    graficos[idDiv].events.on("datavalidated", function () {
        if (graficos[idDiv].data != null && graficos[idDiv].data.length > 0) {
            graficos[idDiv].xAxes.getIndex(0).zoomToCategories(
                zoomInicio,
                zoomFin,
                false,
                true
            );

            setTimeout(
                function () {
                    graficos[idDiv].customPreloader.hide();
                },
                500
            );
        }
    });

    graficos[idDiv].customPreloader.show();
}