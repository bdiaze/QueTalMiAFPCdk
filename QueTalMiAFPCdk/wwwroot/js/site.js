$(function () {
    $('[data-toggle="tooltip"]').tooltip();

    // Se graba en cookie la timezone del cliente...
    $.cookie("Timezone", Intl.DateTimeFormat().resolvedOptions().timeZone, { expires: 365, path: '/' });
})

var cuotasConsultadas = {};
var cuotasErrorCallbacks = {};
var cuotasAfterSuccessCallbacks = {};
var cuotasCompleteCallbacks = {};

function consultarCuotas(afps, fondo, fechaInicial, fechaFinal, beforeSendCallback, afterSuccessCallback, errorCallback, completeCallback) {
    if (cuotasConsultadas[fondo] != undefined &&
        cuotasConsultadas[fondo]["afps"] == afps &&
        cuotasConsultadas[fondo]["fechaInicial"] == fechaInicial &&
        cuotasConsultadas[fondo]["fechaFinal"] == fechaFinal &&
        cuotasConsultadas[fondo]["beforeSend"] != undefined) {

        if (beforeSendCallback != undefined) {
            beforeSendCallback(
                _.cloneDeep(cuotasConsultadas[fondo]["beforeSend"]["jqXHR"]),
                _.cloneDeep(cuotasConsultadas[fondo]["beforeSend"]["settings"])
            );
        }

        if (cuotasConsultadas[fondo]["complete"] != undefined) {
            if (errorCallback != undefined && cuotasConsultadas[fondo]["error"] != undefined) {
                errorCallback(
                    _.cloneDeep(cuotasConsultadas[fondo]["error"]["jqXHR"]),
                    _.cloneDeep(cuotasConsultadas[fondo]["error"]["textStatus"]),
                    _.cloneDeep(cuotasConsultadas[fondo]["error"]["errorThrown"])
                );
            }

            if (afterSuccessCallback != undefined && cuotasConsultadas[fondo]["success"] != undefined) {
                afterSuccessCallback(
                    _.cloneDeep(cuotasConsultadas[fondo]["success"]["listaCuotas"]),
                    _.cloneDeep(cuotasConsultadas[fondo]["success"]["textStatus"]),
                    _.cloneDeep(cuotasConsultadas[fondo]["success"]["jqXHR"])
                );
            }

            if (completeCallback != undefined) {
                completeCallback(
                    _.cloneDeep(cuotasConsultadas[fondo]["complete"]["jqXHR"]),
                    _.cloneDeep(cuotasConsultadas[fondo]["complete"]["textStatus"])
                );
            }
        } else {
            if (cuotasErrorCallbacks[fondo] == undefined) cuotasErrorCallbacks[fondo] = [];
            if (cuotasAfterSuccessCallbacks[fondo] == undefined) cuotasAfterSuccessCallbacks[fondo] = [];
            if (cuotasCompleteCallbacks[fondo] == undefined) cuotasCompleteCallbacks[fondo] = [];

            cuotasErrorCallbacks[fondo].push(errorCallback);
            cuotasAfterSuccessCallbacks[fondo].push(afterSuccessCallback);
            cuotasCompleteCallbacks[fondo].push(completeCallback);
        }

        return;
    }

    let diaMesAnnoInicio = fechaInicial.split("/");
    let dtFechaInicial = new Date(diaMesAnnoInicio[2], parseInt(diaMesAnnoInicio[1]) - 1, diaMesAnnoInicio[0]);
    let dtFechaIniPrev = new Date(dtFechaInicial.getTime());
    dtFechaIniPrev.setDate(dtFechaIniPrev.getDate() - 7);
    let fechaInicialPre = dtFechaIniPrev.getDate() + "/" + (dtFechaIniPrev.getMonth() + 1) + "/" + dtFechaIniPrev.getFullYear();

    let diaMesAnnoFinal = fechaFinal.split("/");
    let dtFechaFinal = new Date(diaMesAnnoFinal[2], parseInt(diaMesAnnoFinal[1]) - 1, diaMesAnnoFinal[0]);
    let dtFechaFinPost = new Date(dtFechaFinal.getTime());
    dtFechaFinPost.setDate(dtFechaFinPost.getDate() + 7);
    let fechaFinalPost = dtFechaFinPost.getDate() + "/" + (dtFechaFinPost.getMonth() + 1) + "/" + dtFechaFinPost.getFullYear();

    $.ajax({
        url: "/api/Cuota/ObtenerCuotas" +
            "?listaAFPs=" + encodeURIComponent(afps) +
            "&listaFondos=" + encodeURIComponent(fondo) +
            "&fechaInicial=" + encodeURIComponent(fechaInicialPre) +
            "&fechaFinal=" + encodeURIComponent(fechaFinalPost),
        beforeSend: function (jqXHR, settings) {
            cuotasConsultadas[fondo] = {
                "afps": _.cloneDeep(afps),
                "fechaInicial": _.cloneDeep(fechaInicial),
                "fechaFinal": _.cloneDeep(fechaFinal)
            };

            cuotasConsultadas[fondo]["beforeSend"] = {
                "jqXHR": _.cloneDeep(jqXHR),
                "settings": _.cloneDeep(settings)
            };

            if (beforeSendCallback != undefined) {
                beforeSendCallback(
                    _.cloneDeep(jqXHR),
                    _.cloneDeep(settings)
                );
            }
        },
        error: function (jqXHR, textStatus, errorThrown) {
            cuotasConsultadas[fondo]["error"] = {
                "jqXHR": _.cloneDeep(jqXHR),
                "textStatus": _.cloneDeep(textStatus),
                "errorThrown": _.cloneDeep(errorThrown)
            };

            if (errorCallback != undefined) {
                errorCallback(
                    _.cloneDeep(jqXHR),
                    _.cloneDeep(textStatus),
                    _.cloneDeep(errorThrown)
                );
            }

            if (cuotasErrorCallbacks[fondo] != undefined) {
                cuotasErrorCallbacks[fondo].forEach(callback => {
                    if (callback != undefined) {
                        callback(
                            _.cloneDeep(jqXHR),
                            _.cloneDeep(textStatus),
                            _.cloneDeep(errorThrown)
                        );
                    }
                });
            }
            cuotasErrorCallbacks[fondo] = undefined;
        },
        success: function (data, textStatus, jqXHR) {
            let listaCuotas = [];
            for (let i = 0; i < data.length; i++) {
                let valorCuota = data[i];
                let annoMesDia = valorCuota["fecha"].split("T")[0].split("-");
                valorCuota["fecha"] = new Date(annoMesDia[0], parseInt(annoMesDia[1]) - 1, annoMesDia[2]);

                let ultFondoAgr = listaCuotas.length >= 1 ? listaCuotas[listaCuotas.length - 1] : null;
                if (ultFondoAgr != null && valorCuota["fecha"].getTime() == ultFondoAgr["fecha"].getTime()) {
                    ultFondoAgr["valor" + valorCuota["afp"].substr(0, 1) + valorCuota["afp"].substr(1).toLowerCase()] = valorCuota["valor"];
                } else {
                    let nuevoFondo = new Object();
                    nuevoFondo["fecha"] = valorCuota["fecha"];
                    nuevoFondo["valor" + valorCuota["afp"].substr(0, 1) + valorCuota["afp"].substr(1).toLowerCase()] = valorCuota["valor"];
                    nuevoFondo["valorUf"] = valorCuota["valorUf"];
                    listaCuotas.push(nuevoFondo);
                }
            }

            let valoresEncontrados = new Object();
            ["Capital", "Cuprum", "Habitat", "Modelo", "Planvital", "Provida", "Uno"].forEach(afp => {
                valoresEncontrados["valor" + afp] = false;
            });
            for (let i = listaCuotas.length - 1; i >= 0; i--) {
                ["Capital", "Cuprum", "Habitat", "Modelo", "Planvital", "Provida", "Uno"].forEach(afp => {
                    if (listaCuotas[i]["valor" + afp] != undefined) {
                        valoresEncontrados["valor" + afp] = true;
                    } else {
                        if (!valoresEncontrados["valor" + afp]) {
                            listaCuotas[i]["valor" + afp] = "Sin Informar";
                        }
                    }
                });

                let encontroTodos = true;
                ["Capital", "Cuprum", "Habitat", "Modelo", "Planvital", "Provida", "Uno"].forEach(afp => {
                    if (!valoresEncontrados["valor" + afp]) {
                        encontroTodos = false;
                        return;
                    }
                });
                if (encontroTodos) {
                    break;
                }
            }

            let ultimosValores = {};
            let indicesEliminar = [];
            for (let i = 0; i < listaCuotas.length; i++) {
                let soloRepetidos = true;
                ["Capital", "Cuprum", "Habitat", "Modelo", "Planvital", "Provida", "Uno"].forEach(afp => {
                    if (listaCuotas[i]["valor" + afp] == undefined) {
                        listaCuotas[i]["valor" + afp] = ultimosValores["valor" + afp];
                    } else if (listaCuotas[i]["valor" + afp] == "Sin Informar") {
                        listaCuotas[i]["valor" + afp] = undefined;
                    }

                    if (listaCuotas[i]["valor" + afp] != undefined &&
                        listaCuotas[i]["valor" + afp] != ultimosValores["valor" + afp]) {
                        soloRepetidos = false;
                    }

                    ultimosValores["valor" + afp] = listaCuotas[i]["valor" + afp];
                });

                if (soloRepetidos) indicesEliminar.unshift(i);
            }
            indicesEliminar.forEach(indice => {
                listaCuotas.splice(indice, 1);
            });
            
            let indexInicio = 0;
            for (let i = 0; i < listaCuotas.length; i++) {
                if (listaCuotas[i]["fecha"] >= dtFechaInicial) {
                    indexInicio = i;
                    break;
                }
            }

            let indexFinal = listaCuotas.length;
            for (let i = listaCuotas.length - 1; i >= 0; i--) {
                if (listaCuotas[i]["fecha"] <= dtFechaFinal) {
                    indexFinal = i + 1;
                    break;
                }
            }

            listaCuotas = listaCuotas.slice(indexInicio, indexFinal);

            cuotasConsultadas[fondo]["success"] = {
                "listaCuotas": _.cloneDeep(listaCuotas),
                "textStatus": _.cloneDeep(textStatus),
                "jqXHR": _.cloneDeep(jqXHR)
            };

            if (afterSuccessCallback != undefined) {
                afterSuccessCallback(
                    _.cloneDeep(listaCuotas),
                    _.cloneDeep(textStatus),
                    _.cloneDeep(jqXHR)
                );
            }

            if (cuotasAfterSuccessCallbacks[fondo] != undefined) {
                cuotasAfterSuccessCallbacks[fondo].forEach(callback => {
                    if (callback != undefined) {
                        callback(
                            _.cloneDeep(listaCuotas),
                            _.cloneDeep(textStatus),
                            _.cloneDeep(jqXHR)
                        );
                    }
                });
            }
            cuotasAfterSuccessCallbacks[fondo] = undefined;
        },
        complete: function (jqXHR, textStatus) {
            cuotasConsultadas[fondo]["complete"] = {
                "jqXHR": _.cloneDeep(jqXHR),
                "textStatus": _.cloneDeep(textStatus)
            };

            if (completeCallback != undefined) {
                completeCallback(
                    _.cloneDeep(jqXHR),
                    _.cloneDeep(textStatus)
                );
            }

            if (cuotasCompleteCallbacks[fondo] != undefined) {
                cuotasCompleteCallbacks[fondo].forEach(callback => {
                    if (callback != undefined) {
                        callback(
                            _.cloneDeep(jqXHR),
                            _.cloneDeep(textStatus)
                        );
                    }
                });
            }
            cuotasCompleteCallbacks[fondo] = undefined;
        }
    });
}

var cuotasPorAFPConsultadas = {};
var cuotasPorAFPErrorCallbacks = {};
var cuotasPorAFPAfterSuccessCallbacks = {};
var cuotasPorAFPCompleteCallbacks = {};

function consultarCuotasPorAFP(afp, fondos, fechaInicial, fechaFinal, beforeSendCallback, afterSuccessCallback, errorCallback, completeCallback) {
    if (cuotasPorAFPConsultadas[afp] != undefined &&
        cuotasPorAFPConsultadas[afp]["fondos"] == fondos &&
        cuotasPorAFPConsultadas[afp]["fechaInicial"] == fechaInicial &&
        cuotasPorAFPConsultadas[afp]["fechaFinal"] == fechaFinal &&
        cuotasPorAFPConsultadas[afp]["beforeSend"] != undefined) {

        if (beforeSendCallback != undefined) {
            beforeSendCallback(
                _.cloneDeep(cuotasPorAFPConsultadas[afp]["beforeSend"]["jqXHR"]),
                _.cloneDeep(cuotasPorAFPConsultadas[afp]["beforeSend"]["settings"])
            );
        }

        if (cuotasPorAFPConsultadas[afp]["complete"] != undefined) {
            if (errorCallback != undefined && cuotasPorAFPConsultadas[afp]["error"] != undefined) {
                errorCallback(
                    _.cloneDeep(cuotasPorAFPConsultadas[afp]["error"]["jqXHR"]),
                    _.cloneDeep(cuotasPorAFPConsultadas[afp]["error"]["textStatus"]),
                    _.cloneDeep(cuotasPorAFPConsultadas[afp]["error"]["errorThrown"])
                );
            }

            if (afterSuccessCallback != undefined && cuotasPorAFPConsultadas[afp]["success"] != undefined) {
                afterSuccessCallback(
                    _.cloneDeep(cuotasPorAFPConsultadas[afp]["success"]["listaCuotas"]),
                    _.cloneDeep(cuotasPorAFPConsultadas[afp]["success"]["textStatus"]),
                    _.cloneDeep(cuotasPorAFPConsultadas[afp]["success"]["jqXHR"])
                );
            }

            if (completeCallback != undefined) {
                completeCallback(
                    _.cloneDeep(cuotasPorAFPConsultadas[afp]["complete"]["jqXHR"]),
                    _.cloneDeep(cuotasPorAFPConsultadas[afp]["complete"]["textStatus"])
                );
            }
        } else {
            if (cuotasPorAFPErrorCallbacks[afp] == undefined) cuotasPorAFPErrorCallbacks[afp] = [];
            if (cuotasPorAFPAfterSuccessCallbacks[afp] == undefined) cuotasPorAFPAfterSuccessCallbacks[afp] = [];
            if (cuotasPorAFPCompleteCallbacks[afp] == undefined) cuotasPorAFPCompleteCallbacks[afp] = [];

            cuotasPorAFPErrorCallbacks[afp].push(errorCallback);
            cuotasPorAFPAfterSuccessCallbacks[afp].push(afterSuccessCallback);
            cuotasPorAFPCompleteCallbacks[afp].push(completeCallback);
        }

        return;
    }

    let diaMesAnnoInicio = fechaInicial.split("/");
    let dtFechaInicial = new Date(diaMesAnnoInicio[2], parseInt(diaMesAnnoInicio[1]) - 1, diaMesAnnoInicio[0]);
    let dtFechaIniPrev = new Date(dtFechaInicial.getTime());
    dtFechaIniPrev.setDate(dtFechaIniPrev.getDate() - 7);
    let fechaInicialPre = dtFechaIniPrev.getDate() + "/" + (dtFechaIniPrev.getMonth() + 1) + "/" + dtFechaIniPrev.getFullYear();

    let diaMesAnnoFinal = fechaFinal.split("/");
    let dtFechaFinal = new Date(diaMesAnnoFinal[2], parseInt(diaMesAnnoFinal[1]) - 1, diaMesAnnoFinal[0]);
    let dtFechaFinPost = new Date(dtFechaFinal.getTime());
    dtFechaFinPost.setDate(dtFechaFinPost.getDate() + 7);
    let fechaFinalPost = dtFechaFinPost.getDate() + "/" + (dtFechaFinPost.getMonth() + 1) + "/" + dtFechaFinPost.getFullYear();

    $.ajax({
        url: "/api/Cuota/ObtenerCuotas" +
            "?listaAFPs=" + encodeURIComponent(afp) +
            "&listaFondos=" + encodeURIComponent(fondos) +
            "&fechaInicial=" + encodeURIComponent(fechaInicialPre) +
            "&fechaFinal=" + encodeURIComponent(fechaFinalPost),
        beforeSend: function (jqXHR, settings) {
            cuotasPorAFPConsultadas[afp] = {
                "fondos": _.cloneDeep(fondos),
                "fechaInicial": _.cloneDeep(fechaInicial),
                "fechaFinal": _.cloneDeep(fechaFinal)
            };

            cuotasPorAFPConsultadas[afp]["beforeSend"] = {
                "jqXHR": _.cloneDeep(jqXHR),
                "settings": _.cloneDeep(settings)
            };

            if (beforeSendCallback != undefined) {
                beforeSendCallback(
                    _.cloneDeep(jqXHR),
                    _.cloneDeep(settings)
                );
            }
        },
        error: function (jqXHR, textStatus, errorThrown) {
            cuotasPorAFPConsultadas[afp]["error"] = {
                "jqXHR": _.cloneDeep(jqXHR),
                "textStatus": _.cloneDeep(textStatus),
                "errorThrown": _.cloneDeep(errorThrown)
            };

            if (errorCallback != undefined) {
                errorCallback(
                    _.cloneDeep(jqXHR),
                    _.cloneDeep(textStatus),
                    _.cloneDeep(errorThrown)
                );
            }

            if (cuotasPorAFPErrorCallbacks[afp] != undefined) {
                cuotasPorAFPErrorCallbacks[afp].forEach(callback => {
                    if (callback != undefined) {
                        callback(
                            _.cloneDeep(jqXHR),
                            _.cloneDeep(textStatus),
                            _.cloneDeep(errorThrown)
                        );
                    }
                });
            }
            cuotasPorAFPErrorCallbacks[afp] = undefined;
        },
        success: function (data, textStatus, jqXHR) {
            let listaCuotas = [];
            for (let i = 0; i < data.length; i++) {
                let valorCuota = data[i];
                let annoMesDia = valorCuota["fecha"].split("T")[0].split("-");
                valorCuota["fecha"] = new Date(annoMesDia[0], parseInt(annoMesDia[1]) - 1, annoMesDia[2]);

                let ultAFPAgr = listaCuotas.length >= 1 ? listaCuotas[listaCuotas.length - 1] : null;
                if (ultAFPAgr != null && valorCuota["fecha"].getTime() == ultAFPAgr["fecha"].getTime()) {
                    ultAFPAgr["valor" + valorCuota["fondo"]] = valorCuota["valor"];
                } else {
                    let nuevoFondo = new Object();
                    nuevoFondo["fecha"] = valorCuota["fecha"];
                    nuevoFondo["valor" + valorCuota["fondo"]] = valorCuota["valor"];
                    nuevoFondo["valorUf"] = valorCuota["valorUf"];
                    listaCuotas.push(nuevoFondo);
                }
            }

            let valoresEncontrados = new Object();
            ["A", "B", "C", "D", "E"].forEach(fondo => {
                valoresEncontrados["valor" + fondo] = false;
            });
            for (let i = listaCuotas.length - 1; i >= 0; i--) {
                ["A", "B", "C", "D", "E"].forEach(fondo => {
                    if (listaCuotas[i]["valor" + fondo] != undefined) {
                        valoresEncontrados["valor" + fondo] = true;
                    } else {
                        if (!valoresEncontrados["valor" + fondo]) {
                            listaCuotas[i]["valor" + fondo] = "Sin Informar";
                        }
                    }
                });

                let encontroTodos = true;
                ["A", "B", "C", "D", "E"].forEach(fondo => {
                    if (!valoresEncontrados["valor" + fondo]) {
                        encontroTodos = false;
                        return;
                    }
                });
                if (encontroTodos) {
                    break;
                }
            }

            let ultimosValores = {};
            let indicesEliminar = [];
            for (let i = 0; i < listaCuotas.length; i++) {
                let soloRepetidos = true;
                ["A", "B", "C", "D", "E"].forEach(fondo => {
                    if (listaCuotas[i]["valor" + fondo] == undefined) {
                        listaCuotas[i]["valor" + fondo] = ultimosValores["valor" + fondo];
                    } else if (listaCuotas[i]["valor" + fondo] == "Sin Informar") {
                        listaCuotas[i]["valor" + fondo] = undefined;
                    }

                    if (listaCuotas[i]["valor" + fondo] != undefined &&
                        listaCuotas[i]["valor" + fondo] != ultimosValores["valor" + fondo]) {
                        soloRepetidos = false;
                    }

                    ultimosValores["valor" + fondo] = listaCuotas[i]["valor" + fondo];
                });

                if (soloRepetidos) indicesEliminar.unshift(i);
            }
            indicesEliminar.forEach(indice => {
                listaCuotas.splice(indice, 1);
            });

            let indexInicio = 0;
            for (let i = 0; i < listaCuotas.length; i++) {
                if (listaCuotas[i]["fecha"] >= dtFechaInicial) {
                    indexInicio = i;
                    break;
                }
            }

            let indexFinal = listaCuotas.length;
            for (let i = listaCuotas.length - 1; i >= 0; i--) {
                if (listaCuotas[i]["fecha"] <= dtFechaFinal) {
                    indexFinal = i + 1;
                    break;
                }
            }

            listaCuotas = listaCuotas.slice(indexInicio, indexFinal);

            cuotasPorAFPConsultadas[afp]["success"] = {
                "listaCuotas": _.cloneDeep(listaCuotas),
                "textStatus": _.cloneDeep(textStatus),
                "jqXHR": _.cloneDeep(jqXHR)
            };

            if (afterSuccessCallback != undefined) {
                afterSuccessCallback(
                    _.cloneDeep(listaCuotas),
                    _.cloneDeep(textStatus),
                    _.cloneDeep(jqXHR)
                );
            }

            if (cuotasPorAFPAfterSuccessCallbacks[afp] != undefined) {
                cuotasPorAFPAfterSuccessCallbacks[afp].forEach(callback => {
                    if (callback != undefined) {
                        callback(
                            _.cloneDeep(listaCuotas),
                            _.cloneDeep(textStatus),
                            _.cloneDeep(jqXHR)
                        );
                    }
                });
            }
            cuotasPorAFPAfterSuccessCallbacks[afp] = undefined;
        },
        complete: function (jqXHR, textStatus) {
            cuotasPorAFPConsultadas[afp]["complete"] = {
                "jqXHR": _.cloneDeep(jqXHR),
                "textStatus": _.cloneDeep(textStatus)
            };

            if (completeCallback != undefined) {
                completeCallback(
                    _.cloneDeep(jqXHR),
                    _.cloneDeep(textStatus)
                );
            }

            if (cuotasPorAFPCompleteCallbacks[afp] != undefined) {
                cuotasPorAFPCompleteCallbacks[afp].forEach(callback => {
                    if (callback != undefined) {
                        callback(
                            _.cloneDeep(jqXHR),
                            _.cloneDeep(textStatus)
                        );
                    }
                });
            }
            cuotasPorAFPCompleteCallbacks[afp] = undefined;
        }
    });
}

var ultimasCuotasConsultadas = {};
var ultimasCuotasErrorCallbacks = {};
var ultimasCuotasAfterSuccessCallbacks = {};
var ultimasCuotasCompleteCallbacks = {};

function consultarUltimasCuotas(afps, fondo, listaFechas, tipoComision, esFechaInicialPagoCotizacion, esFechaFinalPagoCotizacion,
                                beforeSendCallback, afterSuccessCallback, errorCallback, completeCallback) {

    if (ultimasCuotasConsultadas[fondo + "-" + tipoComision] != undefined &&
        ultimasCuotasConsultadas[fondo + "-" + tipoComision]["afps"] == afps &&
        ultimasCuotasConsultadas[fondo + "-" + tipoComision]["listaFechas"] == listaFechas &&
        ultimasCuotasConsultadas[fondo + "-" + tipoComision]["esFechaInicialPagoCotizacion"] == esFechaInicialPagoCotizacion &&
        ultimasCuotasConsultadas[fondo + "-" + tipoComision]["esFechaFinalPagoCotizacion"] == esFechaFinalPagoCotizacion &&
        ultimasCuotasConsultadas[fondo + "-" + tipoComision]["beforeSend"] != undefined) {

        if (beforeSendCallback != undefined) {
            beforeSendCallback(
                _.cloneDeep(ultimasCuotasConsultadas[fondo + "-" + tipoComision]["beforeSend"]["jqXHR"]),
                _.cloneDeep(ultimasCuotasConsultadas[fondo + "-" + tipoComision]["beforeSend"]["settings"])
            );
        }

        if (ultimasCuotasConsultadas[fondo + "-" + tipoComision]["complete"] != undefined) {
            if (errorCallback != undefined && ultimasCuotasConsultadas[fondo + "-" + tipoComision]["error"] != undefined) {
                errorCallback(
                    _.cloneDeep(ultimasCuotasConsultadas[fondo + "-" + tipoComision]["error"]["jqXHR"]),
                    _.cloneDeep(ultimasCuotasConsultadas[fondo + "-" + tipoComision]["error"]["textStatus"]),
                    _.cloneDeep(ultimasCuotasConsultadas[fondo + "-" + tipoComision]["error"]["errorThrown"])
                );
            }

            if (afterSuccessCallback != undefined && ultimasCuotasConsultadas[fondo + "-" + tipoComision]["success"] != undefined) {
                afterSuccessCallback(
                    _.cloneDeep(ultimasCuotasConsultadas[fondo + "-" + tipoComision]["success"]["parametrosAFPs"]),
                    _.cloneDeep(ultimasCuotasConsultadas[fondo + "-" + tipoComision]["success"]["textStatus"]),
                    _.cloneDeep(ultimasCuotasConsultadas[fondo + "-" + tipoComision]["success"]["jqXHR"])
                );
            }

            if (completeCallback != undefined) {
                completeCallback(
                    _.cloneDeep(ultimasCuotasConsultadas[fondo + "-" + tipoComision]["complete"]["jqXHR"]),
                    _.cloneDeep(ultimasCuotasConsultadas[fondo + "-" + tipoComision]["complete"]["textStatus"])
                );
            }
        } else {
            if (ultimasCuotasErrorCallbacks[fondo + "-" + tipoComision] == undefined) ultimasCuotasErrorCallbacks[fondo + "-" + tipoComision] = [];
            if (ultimasCuotasAfterSuccessCallbacks[fondo + "-" + tipoComision] == undefined) ultimasCuotasAfterSuccessCallbacks[fondo + "-" + tipoComision] = [];
            if (ultimasCuotasCompleteCallbacks[fondo + "-" + tipoComision] == undefined) ultimasCuotasCompleteCallbacks[fondo + "-" + tipoComision] = [];

            ultimasCuotasErrorCallbacks[fondo + "-" + tipoComision].push(errorCallback);
            ultimasCuotasAfterSuccessCallbacks[fondo + "-" + tipoComision].push(afterSuccessCallback);
            ultimasCuotasCompleteCallbacks[fondo + "-" + tipoComision].push(completeCallback);
        }

        return;
    }

    let data = {
        listaAFPs: afps,
        listaFondos: fondo,
        listaFechas: listaFechas,
        tipoComision: tipoComision
    };

    $.post({
        url: "/api/Cuota/ObtenerUltimaCuota",
        data: JSON.stringify(data),
        contentType: "application/json",
        beforeSend: function (jqXHR, settings) {
            ultimasCuotasConsultadas[fondo + "-" + tipoComision] = {
                "afps": _.cloneDeep(afps),
                "listaFechas": _.cloneDeep(listaFechas),
                "esFechaInicialPagoCotizacion": _.cloneDeep(esFechaInicialPagoCotizacion),
                "esFechaFinalPagoCotizacion": _.cloneDeep(esFechaFinalPagoCotizacion)
            };

            ultimasCuotasConsultadas[fondo + "-" + tipoComision]["beforeSend"] = {
                "jqXHR": _.cloneDeep(jqXHR),
                "settings": _.cloneDeep(settings)
            };

            if (beforeSendCallback != undefined) {
                beforeSendCallback(
                    _.cloneDeep(jqXHR),
                    _.cloneDeep(settings)
                );
            }
        },
        error: function (jqXHR, textStatus, errorThrown) {
            ultimasCuotasConsultadas[fondo + "-" + tipoComision]["error"] = {
                "jqXHR": _.cloneDeep(jqXHR),
                "textStatus": _.cloneDeep(textStatus),
                "errorThrown": _.cloneDeep(errorThrown)
            };

            if (errorCallback != undefined) {
                errorCallback(
                    _.cloneDeep(jqXHR),
                    _.cloneDeep(textStatus),
                    _.cloneDeep(errorThrown)
                );
            }

            if (ultimasCuotasErrorCallbacks[fondo + "-" + tipoComision] != undefined) {
                ultimasCuotasErrorCallbacks[fondo + "-" + tipoComision].forEach(callback => {
                    if (callback != undefined) {
                        callback(
                            _.cloneDeep(jqXHR),
                            _.cloneDeep(textStatus),
                            _.cloneDeep(errorThrown)
                        );
                    }
                });
            }
            ultimasCuotasErrorCallbacks[fondo + "-" + tipoComision] = undefined;
        },
        success: function (data, textStatus, jqXHR) {
            let parametrosAFPs = {};
            for (let i = 0; i < data.length; i++) {
                let annoMesDia = data[i]["fecha"].split("T")[0].split("-");

                let afp = data[i]["afp"];
                let valor = data[i]["valor"];
                let fecha = new Date(annoMesDia[0], parseInt(annoMesDia[1]) - 1, annoMesDia[2]);
                let comision = data[i]["comision"];

                if (parametrosAFPs[afp] == undefined) {
                    parametrosAFPs[afp] = new Object();
                    parametrosAFPs[afp]["valorT0"] = {
                        "valor": valor,
                        "fecha": fecha,
                        "comision": comision
                    };
                    parametrosAFPs[afp]["valoresCuotasPagoCotiz"] = [];
                    if (esFechaInicialPagoCotizacion) {
                        parametrosAFPs[afp]["valoresCuotasPagoCotiz"].push({
                            "valor": valor,
                            "fecha": fecha,
                            "comision": comision
                        });
                    }
                    parametrosAFPs[afp]["valorTf"] = {
                        "valor": valor,
                        "fecha": fecha,
                        "comision": comision
                    };
                } else {
                    parametrosAFPs[afp]["valoresCuotasPagoCotiz"].push({
                        "valor": valor,
                        "fecha": fecha,
                        "comision": comision
                    });
                    parametrosAFPs[afp]["valorTf"] = {
                        "valor": valor,
                        "fecha": fecha,
                        "comision": comision
                    };
                }
            }

            if (!esFechaFinalPagoCotizacion) {
                for (let afp in parametrosAFPs) {
                    parametrosAFPs[afp]["valoresCuotasPagoCotiz"] = parametrosAFPs[afp]["valoresCuotasPagoCotiz"].slice(0, -1);
                }
            }

            ultimasCuotasConsultadas[fondo + "-" + tipoComision]["success"] = {
                "parametrosAFPs": _.cloneDeep(parametrosAFPs),
                "textStatus": _.cloneDeep(textStatus),
                "jqXHR": _.cloneDeep(jqXHR)
            };

            if (afterSuccessCallback != undefined) {
                afterSuccessCallback(
                    _.cloneDeep(parametrosAFPs),
                    _.cloneDeep(textStatus),
                    _.cloneDeep(jqXHR)
                );
            }

            if (ultimasCuotasAfterSuccessCallbacks[fondo + "-" + tipoComision] != undefined) {
                ultimasCuotasAfterSuccessCallbacks[fondo + "-" + tipoComision].forEach(callback => {
                    if (callback != undefined) {
                        callback(
                            _.cloneDeep(parametrosAFPs),
                            _.cloneDeep(textStatus),
                            _.cloneDeep(jqXHR)
                        );
                    }
                });
            }
            ultimasCuotasAfterSuccessCallbacks[fondo + "-" + tipoComision] = undefined;
        },
        complete: function (jqXHR, textStatus) {
            ultimasCuotasConsultadas[fondo + "-" + tipoComision]["complete"] = {
                "jqXHR": _.cloneDeep(jqXHR),
                "textStatus": _.cloneDeep(textStatus)
            };

            if (completeCallback != undefined) {
                completeCallback(
                    _.cloneDeep(jqXHR),
                    _.cloneDeep(textStatus)
                );
            }

            if (ultimasCuotasCompleteCallbacks[fondo + "-" + tipoComision] != undefined) {
                ultimasCuotasCompleteCallbacks[fondo + "-" + tipoComision].forEach(callback => {
                    if (callback != undefined) {
                        callback(
                            _.cloneDeep(jqXHR),
                            _.cloneDeep(textStatus)
                        );
                    }
                });
            }
            ultimasCuotasCompleteCallbacks[fondo + "-" + tipoComision] = undefined;
        }
    });
}

function habilitarSubmit() {
    $("input[type='submit']").removeAttr("disabled");
}

function deshabilitarSubmit() {
    $("input[type='submit']").attr("disabled", "disabled");
}