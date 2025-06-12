function cargarMonto(monto) {
    $("#inputMonto").val(monto);
}

function quitarFormatoNumero(montoIngresado) {
    let montoSinFormato = "";
    for (let i = 0; i < montoIngresado.length; i++) {
        let caracter = montoIngresado[i];
        if ("0123456789".includes(caracter)) {
            montoSinFormato += caracter
        } else if (caracter == ",") {
            break;
        }
    }
    return montoSinFormato;
}

$("#inputMonto").focus(function () {
    let montoSinFormato = $("#inputMonto").val().replace(/(\.|\$)/g, "");
    $("#inputMonto").val(montoSinFormato);
});

$("#inputMonto").blur(function () {
    let montoIngresado = $("#inputMonto").val();
    let montoSinFormato = quitarFormatoNumero(montoIngresado);
    let formateador = new Intl.NumberFormat("es-ES", { useGrouping: "always" });
    $("#inputMonto").val("$" + formateador.format(montoSinFormato));
});