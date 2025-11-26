using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using QueTalMiAFPCdk.Models.Entities;
using QueTalMiAFPCdk.Models.Others;
using QueTalMiAFPCdk.Models.ViewModels;
using QueTalMiAFPCdk.Repositories;
using System.Diagnostics;
using System.Globalization;

namespace QueTalMiAFPCdk.Controllers {
    public class AccederCuotasController(ILogger<AccederCuotasController> logger, IWebHostEnvironment environment, CuotaUfComisionDAO cuotaUfComisionDAO) : Controller { 
        private static readonly Dictionary<string, string> LISTA_AFPS = new() {
            { "CAPITAL", "Capital" },
            { "CUPRUM", "Cuprum" },
            { "HABITAT", "Habitat" },
            { "MODELO", "Modelo" },
            { "PLANVITAL", "PlanVital" },
            { "PROVIDA", "ProVida" },
            { "UNO", "Uno" },
        };

        private static readonly Dictionary<int, string> LISTA_MESES = new() {
            { 1, "Enero" },
            { 2, "Febrero" },
            { 3, "Marzo" },
            { 4, "Abril" },
            { 5, "Mayo" },
            { 6, "Junio" },
            { 7, "Julio" },
            { 8, "Agosto" },
            { 9, "Septiembre" },
            { 10, "Octubre" },
            { 11, "Noviembre" },
            { 12, "Diciembre" },
        };

        public static readonly Dictionary<string, string> IMAGENES_AFP = ResumenController.IMAGENES_AFP;

        public static readonly Dictionary<string, string> WIDTH_IMAGENES_AFP = new() {
            { "CAPITAL", "width: 8.0rem;" },
            { "CUPRUM", "width: 8.0rem;" },
            { "HABITAT", "width: 8.2rem;" },
            { "MODELO", "width: 7.5rem;" },
            { "PLANVITAL", "width: 7.5rem;" },
            { "PROVIDA", "width: 8.8rem;" },
            { "UNO", "width: 7.5rem;" },
        };

        public static readonly Dictionary<string, string> TOP_IMAGENES_AFP = new() {
            { "CAPITAL", "top: 1.0rem;" },
            { "CUPRUM", "top: 1.0rem;" },
            { "HABITAT", "top: 1.3rem;" },
            { "MODELO", "top: 0.25rem;" },
            { "PLANVITAL", "top: 0.6rem;" },
            { "PROVIDA", "top: 1.1rem;" },
            { "UNO", "top: 1.1rem;" },
        };

        public static readonly Dictionary<string, string?> HEIGHT_IMAGENES_AFP = new() {
            { "CAPITAL", "" },
            { "CUPRUM", "" },
            { "HABITAT", "" },
            { "MODELO", "" },
            { "PLANVITAL", "height: 3.3rem;" },
            { "PROVIDA", "" },
            { "UNO", "" },
        };

        public async Task<IActionResult> Index(AccederCuotasViewModel modeloEntrada) {
            Stopwatch stopwatch = Stopwatch.StartNew();

            string? inputAfp = modeloEntrada.Historial?.Afp;
            int? inputMes = modeloEntrada.Historial?.Mes;
            int? inputAnno = modeloEntrada.Historial?.Anno;

			DateOnly fechaActual = await cuotaUfComisionDAO.UltimaFechaTodas();
            long elapsedTimeFechaTodas = stopwatch.ElapsedMilliseconds;
            
            // Se añaden valores por defecto para inputs...
            inputAnno ??= fechaActual.Year;
            inputMes ??= fechaActual.Month;
            inputAfp ??= Request.Cookies["FiltroHistorialAfpSeleccionada"];
            inputAfp ??= "CAPITAL";

            if (User.Identity == null || !User.Identity.IsAuthenticated) {
                if (inputAnno != fechaActual.Year) {

                    logger.LogInformation(
                        "[{Method}] - [{Controller}] - [{Action}] - [{ElapsedTime} ms] - [{StatusCode}] - [Usuario Autenticado: {IsAuthenticated}] - " +
                        "Se redirecciona a challenge dado que usuario no está autenticado - " +
                        "HistorialAfp: {HistorialAfp} - HistorialMes: {HistorialMes} - HistorialAnno: {HistorialAnno} - " +
                        "Elapsed Time Fecha Todas: {ElapsedTimeFechaTodas}.",
                        HttpContext.Request.Method.Replace(Environment.NewLine, " "), ControllerContext.ActionDescriptor.ControllerName, ControllerContext.ActionDescriptor.ActionName,
                        stopwatch.ElapsedMilliseconds, StatusCodes.Status401Unauthorized, User.Identity?.IsAuthenticated ?? false,
                        modeloEntrada.Historial?.Afp?.Replace(Environment.NewLine, " "), modeloEntrada.Historial?.Mes, modeloEntrada.Historial?.Anno,
                        elapsedTimeFechaTodas);

                    return Challenge();
                }
            }

			// Se prepara rango de fecha a utilizar para historial de valores cuota...
			DateOnly? fechaDesdeHistorial = null;
            try {
                fechaDesdeHistorial = new DateOnly(inputAnno.GetValueOrDefault(), inputMes.GetValueOrDefault(), 1);
            } catch {
                fechaDesdeHistorial = new DateOnly(fechaActual.Year, fechaActual.Month, 1);
            }
			DateOnly fechaHastaHistorial = fechaDesdeHistorial.GetValueOrDefault().AddMonths(1).AddDays(-1);

            // Se añade consulta para historial inferior mediante Task, para no detener las consultas asociadas a resumen semanal...
            Task<List<CuotaUf>> taskValoresHistorial = cuotaUfComisionDAO.ObtenerCuotas(
                inputAfp,
                "A,B,C,D,E",
                fechaDesdeHistorial.GetValueOrDefault().AddDays(-7), // Se consulta un rango mayor de dias en caso de necesitar rellenar fines de semana
                fechaHastaHistorial.AddDays(7)
            );

            // Se preparan los datos para la grilla de historial de valores cuota...
            List<int> listaAnnos = [.. Enumerable.Range(2002, fechaActual.Year + 1 - 2002)];
            listaAnnos.Reverse();


            AccederCuotasViewModel salida = new() {
                AmbienteDesarrollo = environment.IsDevelopment(),

                Historial = new HistorialAfp {
                    Afp = inputAfp,
                    ListaAfps = new SelectList(LISTA_AFPS.Select(l => new SelectListItem { Value = l.Key, Text = l.Value }).ToList(), nameof(SelectListItem.Value), nameof(SelectListItem.Text)),
                    Mes = fechaDesdeHistorial.GetValueOrDefault().Month,
                    ListaMeses = new SelectList(LISTA_MESES.Select(l => new SelectListItem { Value = l.Key.ToString(), Text = l.Value }).ToList(), nameof(SelectListItem.Value), nameof(SelectListItem.Text)),
                    Anno = fechaDesdeHistorial.GetValueOrDefault().Year,
                    ListaAnnos = new SelectList(listaAnnos.Select(l => new SelectListItem { Value = l.ToString(), Text = l.ToString() }).ToList(), nameof(SelectListItem.Value), nameof(SelectListItem.Text))
                }
            };

            // Se graba en cookie última AFP seleccionada para usar como defecto en consultas posteriores...
            HttpContext.Response.Cookies.Append("FiltroHistorialAfpSeleccionada", inputAfp, new CookieOptions {
                Expires = DateTime.Now.AddDays(365),
				Secure = true
			});

            // Se obtiene el filtro por defecto a mostrar en el historial...
            string? fondoHistorialSeleccionado = Request.Cookies["FiltroHistorialFondoSeleccionado"];
            fondoHistorialSeleccionado ??= "A";
            salida.Historial.FiltroHistorialFondoSeleccionado = fondoHistorialSeleccionado;

            // Se espera por task en caso de que aún no haya terminado de procesar...
            List<CuotaUf> valoresHistorial = await taskValoresHistorial;
            long elapsedTimeValoresCuota = stopwatch.ElapsedMilliseconds;

            foreach (string fondo in "A,B,C,D,E".Split(",")) {
				DateOnly fecha = fechaHastaHistorial;
                while (fecha >= fechaDesdeHistorial) {
                    CuotaUf? cuotaUf = valoresHistorial.Where(c => c.Afp == inputAfp && c.Fondo == fondo && c.Fecha == fecha).FirstOrDefault();

                    // Si no tenemos valor cuota para la fecha indicada, se marca como null o con la valor cuota anterior.
                    if (cuotaUf == null) {
                        if (valoresHistorial.Any(c => c.Afp == inputAfp && c.Fondo == fondo && c.Fecha > fecha)) {
                            cuotaUf = valoresHistorial.Where(c => c.Afp == inputAfp && c.Fondo == fondo && c.Fecha <= fecha).OrderByDescending(c => c.Fecha).FirstOrDefault();
                            salida.Historial.ValoresCuota[fondo].Add(new CuotaRentabilidad { ValorCuota = cuotaUf?.Valor });
                        } else {
                            salida.Historial.ValoresCuota[fondo].Add(new CuotaRentabilidad { ValorCuota = null, Rentabilidad = null });
                        }
                    } else {
                        salida.Historial.ValoresCuota[fondo].Add(new CuotaRentabilidad { ValorCuota = cuotaUf.Valor });
                    }

                    fecha = fecha.AddDays(-1);
                }

                salida.Historial.ValoresCuota[fondo].Reverse();

                // Se calculan rentabilidades diarias...
                CuotaUf? cuotaUfMesAnterior = valoresHistorial.Where(c => c.Afp == inputAfp && c.Fondo == fondo && c.Fecha < fechaDesdeHistorial).OrderByDescending(c => c.Fecha).FirstOrDefault();
                CuotaRentabilidad? cuotaRentabilidadAnterior = null;
                foreach (CuotaRentabilidad cuotaRentabilidad in salida.Historial.ValoresCuota[fondo]) {
                    if (cuotaRentabilidad.ValorCuota != null) {
                        // Se calcula rentabilidad según valor cuota del día anterior...
                        if (cuotaRentabilidadAnterior?.ValorCuota != null) {
                            cuotaRentabilidad.Rentabilidad = (cuotaRentabilidad.ValorCuota - cuotaRentabilidadAnterior?.ValorCuota) * 100 / cuotaRentabilidadAnterior?.ValorCuota;
                        
                        // si no tenemos valor cuota del día anterior (porque estamos en el primer día del mes), se usa el valor cuota del mes anterior...
                        } else {
                            if (cuotaUfMesAnterior != null) {
                                cuotaRentabilidad.Rentabilidad = (cuotaRentabilidad.ValorCuota - cuotaUfMesAnterior.Valor) * 100 / cuotaUfMesAnterior.Valor;
                            }
                        }
                    }

                   cuotaRentabilidadAnterior = cuotaRentabilidad;
                }

                // Se calcula la rentabilidad mensual...
                CuotaRentabilidad? ultimaCuota = salida.Historial.ValoresCuota[fondo].Where(vc => vc.ValorCuota != null).LastOrDefault();
                if (ultimaCuota?.ValorCuota != null && cuotaUfMesAnterior?.Valor != null) {
                    salida.Historial.RentabilidadMensual[fondo] = (ultimaCuota.ValorCuota - cuotaUfMesAnterior.Valor) * 100 / cuotaUfMesAnterior.Valor;
                }
            }

            salida.Historial.NombreAfp = LISTA_AFPS[salida.Historial.Afp];

            logger.LogInformation(
                "[{Method}] - [{Controller}] - [{Action}] - [{ElapsedTime} ms] - [{StatusCode}] - [Usuario Autenticado: {IsAuthenticated}] - " +
                "Se retorna exitosamente la página de acceder cuotas - " +
                "HistorialAfp: {HistorialAfp} - HistorialMes: {HistorialMes} - HistorialAnno: {HistorialAnno} - " +
                "Elapsed Time Fecha Todas: {ElapsedTimeFechaTodas} - Elapsed Time Valores Cuota: {ElapsedTimeValoresCuota}.",
                HttpContext.Request.Method.Replace(Environment.NewLine, " "), ControllerContext.ActionDescriptor.ControllerName, ControllerContext.ActionDescriptor.ActionName,
                stopwatch.ElapsedMilliseconds, StatusCodes.Status200OK, User.Identity?.IsAuthenticated ?? false,
                modeloEntrada.Historial?.Afp?.Replace(Environment.NewLine, " "), modeloEntrada.Historial?.Mes, modeloEntrada.Historial?.Anno,
                elapsedTimeFechaTodas, elapsedTimeValoresCuota);

            return View(salida);
        }
    }
}