﻿$(document).ready(function () {
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

    // am4core.useTheme(am4themes_animated);

    am4core.ready(function () {
        am4core.options.queue = true;

        ["A", "B", "C", "D", "E"].forEach(fondo => {
            $("#collapseRRFondo" + fondo).on("shown.bs.collapse", function () {
                $("#headingRRFondo" + fondo).find("i.fa-chevron-down").hide();
                $("#headingRRFondo" + fondo).find("i.fa-chevron-up").show();

                obtenerRentRealSoloTipo(fondo);
            });

            $("#collapseRRFondo" + fondo).on("hide.bs.collapse", function () {
                $("#headingRRFondo" + fondo).find("i.fa-chevron-down").show();
                $("#headingRRFondo" + fondo).find("i.fa-chevron-up").hide();
            });
        });

        ["A", "B", "C", "D", "E"].forEach(fondo => {
            $("#collapseRTFondo" + fondo).on("shown.bs.collapse", function () {
                $("#headingRTFondo" + fondo).find("i.fa-chevron-down").hide();
                $("#headingRTFondo" + fondo).find("i.fa-chevron-up").show();

                obtenerRentSoloTipo(fondo);
            });

            $("#collapseRTFondo" + fondo).on("hide.bs.collapse", function () {
                $("#headingRTFondo" + fondo).find("i.fa-chevron-down").show();
                $("#headingRTFondo" + fondo).find("i.fa-chevron-up").hide();
            });
        });

        ["A", "B", "C", "D", "E"].forEach(fondo => {
            $("#collapseVCFondo" + fondo).on("show.bs.collapse", function () {
                $("#headingVCFondo" + fondo).find("i.fa-chevron-down").hide();
                $("#headingVCFondo" + fondo).find("i.fa-chevron-up").show();

                obtenerCuotaSoloTipo(fondo);
            });

            $("#collapseVCFondo" + fondo).on("hide.bs.collapse", function () {
                $("#headingVCFondo" + fondo).find("i.fa-chevron-down").show();
                $("#headingVCFondo" + fondo).find("i.fa-chevron-up").hide();
            });
        });

        $("#collapseRRFondoA").collapse("show");
    });
});

function btnFiltrarRentabilidad() {
    ["A", "B", "C", "D", "E"].forEach(fondo => {
        if ($("#headingRRFondo" + fondo).children("button").attr("aria-expanded") == "true") {
            obtenerRentRealSoloTipo(fondo);
        }

        if ($("#headingRTFondo" + fondo).children("button").attr("aria-expanded") == "true") {
            obtenerRentSoloTipo(fondo);
        }

        if ($("#headingVCFondo" + fondo).children("button").attr("aria-expanded") == "true") {
            obtenerCuotaSoloTipo(fondo);
        }
    });
}

function obtenerRentRealSoloTipo(tipoFondo) {
    let fechaInicio = $("#fechaInicial").val();
    let fechaFinal = $("#fechaFinal").val();

    if (fechaInicio.trim().length == 0 || fechaFinal.trim().length == 0) {
        return;
    }

    if (fechaInicio != $("#chartRentReal" + tipoFondo).attr("fechaInicial") ||
        fechaFinal != $("#chartRentReal" + tipoFondo).attr("fechaFinal")) {
        obtenerRentReal(tipoFondo, fechaInicio, fechaFinal);
    }
}

function obtenerRentReal(tipoFondo, fechaInicial, fechaFinal) {
    crearGrafica(
        "chartRentReal" + tipoFondo,
        null,
        "Rentabilidad Real (%)",
        null, "%"
    );

    consultarCuotas(
        "CAPITAL,CUPRUM,HABITAT,MODELO,PLANVITAL,PROVIDA,UNO",
        tipoFondo,
        fechaInicial,
        fechaFinal,
        function (jqXHR, settings) {
            $("#chartRentReal" + tipoFondo).removeAttr("fechaInicial");
            $("#chartRentReal" + tipoFondo).removeAttr("fechaFinal");
        },
        function (listaCuotas, textStatus, jqXHR) {
            let valorCuotaT0 = {};
            for (let i = 0; i < listaCuotas.length; i++) {
                ["Capital", "Cuprum", "Habitat", "Modelo", "Planvital", "Provida", "Uno"].forEach(afp => {
                    if (valorCuotaT0[afp] == undefined || valorCuotaT0[afp]["valor"] == undefined) valorCuotaT0[afp] = {
                        "valor": listaCuotas[i]["valor" + afp],
                        "valorUf": listaCuotas[i]["valorUf"]
                    };

                    if (listaCuotas[i]["valor" + afp] != undefined) listaCuotas[i]["valor" + afp] = listaCuotas[i]["valor" + afp] * 100 / (valorCuotaT0[afp]["valor"] * listaCuotas[i]["valorUf"] / valorCuotaT0[afp]["valorUf"]) - 100;
                });
            }

            $("#chartRentReal" + tipoFondo).attr("fechaInicial", fechaInicial);
            $("#chartRentReal" + tipoFondo).attr("fechaFinal", fechaFinal);

            actualizarDataGrafica(
                "chartRentReal" + tipoFondo,
                listaCuotas
            );
        }
    );
}

function obtenerRentSoloTipo(tipoFondo) {
    let fechaInicio = $("#fechaInicial").val();
    let fechaFinal = $("#fechaFinal").val();

    if (fechaInicio.trim().length == 0 || fechaFinal.trim().length == 0) {
        return;
    }

    if (fechaInicio != $("#chartRentabilidad" + tipoFondo).attr("fechaInicial") ||
        fechaFinal != $("#chartRentabilidad" + tipoFondo).attr("fechaFinal")) {
        obtenerRentabilidades(tipoFondo, fechaInicio, fechaFinal);
    }
}

function obtenerRentabilidades(tipoFondo, fechaInicial, fechaFinal) {
    crearGrafica(
        "chartRentabilidad" + tipoFondo,
        null,
        "Rentabilidad (%)",
        null, "%"
    );

    consultarCuotas(
        "CAPITAL,CUPRUM,HABITAT,MODELO,PLANVITAL,PROVIDA,UNO",
        tipoFondo,
        fechaInicial,
        fechaFinal,
        function (jqXHR, settings) {
            $("#chartRentabilidad" + tipoFondo).removeAttr("fechaInicial");
            $("#chartRentabilidad" + tipoFondo).removeAttr("fechaFinal");
        },
        function (listaCuotas, textStatus, jqXHR) {
            let valorCuotaT0 = {};
            for (let i = 0; i < listaCuotas.length; i++) {
                ["Capital", "Cuprum", "Habitat", "Modelo", "Planvital", "Provida", "Uno"].forEach(afp => {
                    if (valorCuotaT0[afp] == undefined)
                        valorCuotaT0[afp] = listaCuotas[i]["valor" + afp];

                    if (listaCuotas[i]["valor" + afp] != undefined) listaCuotas[i]["valor" + afp] = listaCuotas[i]["valor" + afp] * 100 / valorCuotaT0[afp] - 100;
                });
            }

            $("#chartRentabilidad" + tipoFondo).attr("fechaInicial", fechaInicial);
            $("#chartRentabilidad" + tipoFondo).attr("fechaFinal", fechaFinal);

            actualizarDataGrafica(
                "chartRentabilidad" + tipoFondo,
                listaCuotas
            );
        }
    );
}

function obtenerCuotaSoloTipo(tipoFondo) {
    let fechaInicio = $("#fechaInicial").val();
    let fechaFinal = $("#fechaFinal").val();

    if (fechaInicio.trim().length == 0 || fechaFinal.trim().length == 0) {
        return;
    }

    if (fechaInicio != $("#chartFondo" + tipoFondo).attr("fechaInicial") ||
        fechaFinal != $("#chartFondo" + tipoFondo).attr("fechaFinal")) {
        consultarFondo(tipoFondo, fechaInicio, fechaFinal);
    }
}

function consultarFondo(tipoFondo, fechaInicial, fechaFinal) {
    crearGrafica(
        "chartFondo" + tipoFondo,
        null,
        "Valor Cuota ($)",
        "$"
    );

    consultarCuotas(
        "CAPITAL,CUPRUM,HABITAT,MODELO,PLANVITAL,PROVIDA,UNO",
        tipoFondo,
        fechaInicial,
        fechaFinal,
        function (jqXHR, settings) {
            $("#chartFondo" + tipoFondo).removeAttr("fechaInicial");
            $("#chartFondo" + tipoFondo).removeAttr("fechaFinal");
        },
        function (listaCuotas, textStatus, jqXHR) {
            $("#chartFondo" + tipoFondo).attr("fechaInicial", fechaInicial);
            $("#chartFondo" + tipoFondo).attr("fechaFinal", fechaFinal);

            actualizarDataGrafica(
                "chartFondo" + tipoFondo,
                listaCuotas
            );
        }
    );
}

var graficos = {};

function actualizarDataGrafica(idDiv, data) {
    graficos[idDiv].data = data;
}

function crearGrafica(idDiv, data, tituloEjeY, charPrepend, charAppend) {
    if (graficos[idDiv] == undefined) {
        charPrepend = charPrepend != null && charPrepend.trim().length > 0 ? charPrepend.trim() : "";
        charAppend = charAppend != null && charAppend.trim().length > 0 ? charAppend.trim() : "";
        let charPrependQuotes = charPrepend.length > 0 ? "'" + charPrepend + "'" : "";
        let charAppendQuotes = charAppend.length > 0 ? "'" + charAppend + "'" : "";

        // Create chart instance
        let chart = am4core.create(idDiv, am4charts.XYChart);
        chart.language.locale = am4lang_es_ES;
        chart.preloader.disabled = true;

        // Add data
        // chart.data = data;

        // Create axes
        let dateAxis = chart.xAxes.push(new am4charts.DateAxis());
        dateAxis.dateFormats.setKey("month", "MMM yyyy");
        dateAxis.groupData = true;
        dateAxis.tooltipDateFormat = "EEE, dd MMM yyyy";
        dateAxis.tooltip.background.fill = am4core.color("#6794dc");
        dateAxis.tooltip.background.cornerRadius = 4;
        dateAxis.tooltip.background.strokeWidth = 0;
        dateAxis.skipEmptyPeriods = true;
        dateAxis.showOnInit = false;

        let valueAxis = chart.yAxes.push(new am4charts.ValueAxis());
        valueAxis.title.text = tituloEjeY;
        valueAxis.cursorTooltipEnabled = false;

        // Add ranges for percentage
        if (charAppend == "%") {
            let range = valueAxis.axisRanges.create();
            range.value = 0;
            range.endValue = 1000;
            range.axisFill.fill = am4core.color("#28a745");
            range.axisFill.fillOpacity = 0.1;

            let range2 = valueAxis.axisRanges.create();
            range2.value = -1000;
            range2.endValue = 0;
            range2.axisFill.fill = am4core.color("#dc3545");
            range2.axisFill.fillOpacity = 0.1;
        }

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
            serie.dataFields.valueY = "valor" + afp.charAt(0) + afp.substr(1).toLowerCase();
            serie.dataFields.dateX = "fecha";
            serie.name = afp;
            serie.tooltipText = "{name}: [bold]" + charPrepend + "{valueY}" + charAppend + "[/]";
            serie.legendSettings.valueText = `[bold]{valueY.formatNumber("` + charPrependQuotes + `###,###,##0.00` + charAppendQuotes + `")}[/]`;
            serie.strokeWidth = 2;
            serie.stroke = am4core.color(colores[afp]);
            serie.fill = am4core.color(colores[afp]);
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
        chart.legend.maxHeight = 78;
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
        hourglass.href = "/images/loading.gif";
        hourglass.align = "center";
        hourglass.valign = "middle";
        hourglass.horizontalCenter = "middle";
        hourglass.verticalCenter = "middle";
        hourglass.dx = -80;
        hourglass.dy = -1;
        hourglass.scale = 0.7;

        chart["customPreloader"] = indicator;

        // Config initial zoom
        chart.events.on("datavalidated", function () {
            if (chart.data != null && chart.data.length > 0) {
                let last_data = chart.data[chart.data.length - 1];
                let fechaInicio = new Date(last_data["fecha"].getTime());
                let fechaFinal = new Date(last_data["fecha"].getTime());;
                fechaInicio.setMonth(fechaInicio.getMonth() - 2);
                fechaFinal.setDate(fechaFinal.getDate() + 1);

                dateAxis.zoomToDates(
                    fechaInicio,
                    fechaFinal,
                    false,
                    true
                );

                setTimeout(
                    function () {
                        chart.customPreloader.hide();
                    },
                    500
                );
            }
        });

        graficos[idDiv] = chart;
    }

    graficos[idDiv].customPreloader.show();
}