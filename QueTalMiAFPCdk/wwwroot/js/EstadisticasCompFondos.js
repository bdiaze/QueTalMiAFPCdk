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

    // am4core.useTheme(am4themes_animated);

    am4core.ready(function () {
        am4core.options.queue = true;

        ["Capital", "Cuprum", "Habitat", "Modelo", "PlanVital", "ProVida", "Uno"].forEach(afp => {
            $("#collapseRR" + afp).on("shown.bs.collapse", function () {
                $("#headingRR" + afp).find("i.fa-chevron-down").hide();
                $("#headingRR" + afp).find("i.fa-chevron-up").show();

                obtenerRentRealSoloTipo(afp);
            });

            $("#collapseRR" + afp).on("hide.bs.collapse", function () {
                $("#headingRR" + afp).find("i.fa-chevron-down").show();
                $("#headingRR" + afp).find("i.fa-chevron-up").hide();
            });
        });

        ["Capital", "Cuprum", "Habitat", "Modelo", "PlanVital", "ProVida", "Uno"].forEach(afp => {
            $("#collapseRT" + afp).on("shown.bs.collapse", function () {
                $("#headingRT" + afp).find("i.fa-chevron-down").hide();
                $("#headingRT" + afp).find("i.fa-chevron-up").show();

                obtenerRentSoloTipo(afp);
            });

            $("#collapseRT" + afp).on("hide.bs.collapse", function () {
                $("#headingRT" + afp).find("i.fa-chevron-down").show();
                $("#headingRT" + afp).find("i.fa-chevron-up").hide();
            });
        });

        ["Capital", "Cuprum", "Habitat", "Modelo", "PlanVital", "ProVida", "Uno"].forEach(afp => {
            $("#collapseVC" + afp).on("show.bs.collapse", function () {
                $("#headingVC" + afp).find("i.fa-chevron-down").hide();
                $("#headingVC" + afp).find("i.fa-chevron-up").show();

                obtenerCuotaSoloTipo(afp);
            });

            $("#collapseVC" + afp).on("hide.bs.collapse", function () {
                $("#headingVC" + afp).find("i.fa-chevron-down").show();
                $("#headingVC" + afp).find("i.fa-chevron-up").hide();
            });
        });

        $("#collapseRRCapital").collapse("show");
    });
});

function btnFiltrarRentabilidad() {
    ["Capital", "Cuprum", "Habitat", "Modelo", "PlanVital", "ProVida", "Uno"].forEach(afp => {
        if ($("#headingRR" + afp).children("button").attr("aria-expanded") == "true") {
            obtenerRentRealSoloTipo(afp);
        }

        if ($("#headingRT" + afp).children("button").attr("aria-expanded") == "true") {
            obtenerRentSoloTipo(afp);
        }

        if ($("#headingVC" + afp).children("button").attr("aria-expanded") == "true") {
            obtenerCuotaSoloTipo(afp);
        }
    });
}

function obtenerRentRealSoloTipo(afp) {
    let fechaInicio = $("#fechaInicial").val();
    let fechaFinal = $("#fechaFinal").val();

    if (fechaInicio.trim().length == 0 || fechaFinal.trim().length == 0) {
        return;
    }

    if (fechaInicio != $("#chartRentReal" + afp).attr("fechaInicial") ||
        fechaFinal != $("#chartRentReal" + afp).attr("fechaFinal")) {
        obtenerRentReal(afp, fechaInicio, fechaFinal);
    }
}

function obtenerRentReal(afp, fechaInicial, fechaFinal) {
    crearGrafica(
        "chartRentReal" + afp,
        null,
        "Rentabilidad Real (%)",
        null, "%"
    );

    consultarCuotasPorAFP(
        afp.toUpperCase(),
        "A,B,C,D,E",
        fechaInicial,
        fechaFinal,
        function (jqXHR, settings) {
            $("#chartRentReal" + afp).removeAttr("fechaInicial");
            $("#chartRentReal" + afp).removeAttr("fechaFinal");
        },
        function (listaCuotas, textStatus, jqXHR) {
            let valorCuotaT0 = {};
            for (let i = 0; i < listaCuotas.length; i++) {
                ["A", "B", "C", "D", "E"].forEach(fondo => {
                    if (valorCuotaT0[fondo] == undefined || valorCuotaT0[fondo]["valor"] == undefined) valorCuotaT0[fondo] = {
                        "valor": listaCuotas[i]["valor" + fondo],
                        "valorUf": listaCuotas[i]["valorUf"]
                    };

                    if (listaCuotas[i]["valor" + fondo] != undefined) listaCuotas[i]["valor" + fondo] = listaCuotas[i]["valor" + fondo] * 100 / (valorCuotaT0[fondo]["valor"] * listaCuotas[i]["valorUf"] / valorCuotaT0[fondo]["valorUf"]) - 100;
                });
            }

            $("#chartRentReal" + afp).attr("fechaInicial", fechaInicial);
            $("#chartRentReal" + afp).attr("fechaFinal", fechaFinal);

            actualizarDataGrafica(
                "chartRentReal" + afp,
                listaCuotas
            );
        }
    );
}

function obtenerRentSoloTipo(afp) {
    let fechaInicio = $("#fechaInicial").val();
    let fechaFinal = $("#fechaFinal").val();

    if (fechaInicio.trim().length == 0 || fechaFinal.trim().length == 0) {
        return;
    }

    if (fechaInicio != $("#chartRentabilidad" + afp).attr("fechaInicial") ||
        fechaFinal != $("#chartRentabilidad" + afp).attr("fechaFinal")) {
        obtenerRentabilidades(afp, fechaInicio, fechaFinal);
    }
}

function obtenerRentabilidades(afp, fechaInicial, fechaFinal) {
    crearGrafica(
        "chartRentabilidad" + afp,
        null,
        "Rentabilidad (%)",
        null, "%"
    );

    consultarCuotasPorAFP(
        afp.toUpperCase(),
        "A,B,C,D,E",
        fechaInicial,
        fechaFinal,
        function (jqXHR, settings) {
            $("#chartRentabilidad" + afp).removeAttr("fechaInicial");
            $("#chartRentabilidad" + afp).removeAttr("fechaFinal");
        },
        function (listaCuotas, textStatus, jqXHR) {
            let valorCuotaT0 = {};
            for (let i = 0; i < listaCuotas.length; i++) {
                ["A", "B", "C", "D", "E"].forEach(fondo => {
                    if (valorCuotaT0[fondo] == undefined)
                        valorCuotaT0[fondo] = listaCuotas[i]["valor" + fondo];

                    if (listaCuotas[i]["valor" + fondo] != undefined) listaCuotas[i]["valor" + fondo] = listaCuotas[i]["valor" + fondo] * 100 / valorCuotaT0[fondo] - 100;
                });
            }

            $("#chartRentabilidad" + afp).attr("fechaInicial", fechaInicial);
            $("#chartRentabilidad" + afp).attr("fechaFinal", fechaFinal);

            actualizarDataGrafica(
                "chartRentabilidad" + afp,
                listaCuotas
            );
        }
    );
}

function obtenerCuotaSoloTipo(afp) {
    let fechaInicio = $("#fechaInicial").val();
    let fechaFinal = $("#fechaFinal").val();

    if (fechaInicio.trim().length == 0 || fechaFinal.trim().length == 0) {
        return;
    }

    if (fechaInicio != $("#chartValor" + afp).attr("fechaInicial") ||
        fechaFinal != $("#chartValor" + afp).attr("fechaFinal")) {
        consultarFondo(afp, fechaInicio, fechaFinal);
    }
}

function consultarFondo(afp, fechaInicial, fechaFinal) {
    crearGrafica(
        "chartValor" + afp,
        null,
        "Valor Cuota ($)",
        "$"
    );

    consultarCuotasPorAFP(
        afp.toUpperCase(),
        "A,B,C,D,E",
        fechaInicial,
        fechaFinal,
        function (jqXHR, settings) {
            $("#chartValor" + afp).removeAttr("fechaInicial");
            $("#chartValor" + afp).removeAttr("fechaFinal");
        },
        function (listaCuotas, textStatus, jqXHR) {
            $("#chartValor" + afp).attr("fechaInicial", fechaInicial);
            $("#chartValor" + afp).attr("fechaFinal", fechaFinal);

            actualizarDataGrafica(
                "chartValor" + afp,
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
            "A": "#c23b33",
            "B": "#f2af32",
            "C": "#72b500",
            "D": "#2E8ECD",
            "E": "#0061a0"
        };
        let series = [];
        ["A", "B", "C", "D", "E"].forEach(fondo => {
            let serie = chart.series.push(new am4charts.LineSeries());
            serie.dataFields.valueY = "valor" + fondo;
            serie.dataFields.dateX = "fecha";
            serie.name = "Fondo " + fondo;
            serie.tooltipText = "{name}: [bold]" + charPrepend + "{valueY}" + charAppend + "[/]";
            serie.legendSettings.valueText = `[bold]{valueY.formatNumber("` + charPrependQuotes + `###,###,##0.00` + charAppendQuotes + `")}[/]`;
            serie.strokeWidth = 2;
            serie.stroke = am4core.color(colores[fondo]);
            serie.fill = am4core.color(colores[fondo]);
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