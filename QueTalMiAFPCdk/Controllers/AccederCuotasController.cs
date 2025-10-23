using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
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

        public async Task<IActionResult> Index(AccederCuotasViewModel modeloEntrada) {
            Stopwatch stopwatch = Stopwatch.StartNew();

            string? inputAfp = modeloEntrada.Historial?.Afp;
            int? inputMes = modeloEntrada.Historial?.Mes;
            int? inputAnno = modeloEntrada.Historial?.Anno;

            DateTime fechaActual = await cuotaUfComisionDAO.UltimaFechaTodas();
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
                        HttpContext.Request.Method, ControllerContext.ActionDescriptor.ControllerName, ControllerContext.ActionDescriptor.ActionName,
                        stopwatch.ElapsedMilliseconds, StatusCodes.Status401Unauthorized, User.Identity?.IsAuthenticated ?? false,
                        modeloEntrada.Historial?.Afp, modeloEntrada.Historial?.Mes, modeloEntrada.Historial?.Anno,
                        elapsedTimeFechaTodas);

                    return Challenge();
                }
            }

            // Se prepara rango de fecha a utilizar para historial de valores cuota...
            DateTime? fechaDesdeHistorial = null;
            try {
                fechaDesdeHistorial = new DateTime(inputAnno.GetValueOrDefault(), inputMes.GetValueOrDefault(), 1);
            } catch {
                fechaDesdeHistorial = new DateTime(fechaActual.Year, fechaActual.Month, 1);
            }
            DateTime fechaHastaHistorial = fechaDesdeHistorial.GetValueOrDefault().AddMonths(1).AddDays(-1);

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
                Expires = DateTime.Now.AddDays(365)
            });

            // Se obtiene el filtro por defecto a mostrar en el historial...
            string? fondoHistorialSeleccionado = Request.Cookies["FiltroHistorialFondoSeleccionado"];
            fondoHistorialSeleccionado ??= "A";
            salida.Historial.FiltroHistorialFondoSeleccionado = fondoHistorialSeleccionado;

            // Se espera por task en caso de que aún no haya terminado de procesar...
            List<CuotaUf> valoresHistorial = await taskValoresHistorial;
            long elapsedTimeValoresCuota = stopwatch.ElapsedMilliseconds;

            foreach (string fondo in "A,B,C,D,E".Split(",")) {
                DateTime fecha = fechaHastaHistorial;
                while (fecha >= fechaDesdeHistorial) {
                    CuotaUf? cuotaUf = valoresHistorial.Where(c => c.Afp == inputAfp && c.Fondo == fondo && c.Fecha == fecha.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture)).FirstOrDefault();

                    // Si no tenemos valor cuota para la fecha indicada, se marca como null o con la valor cuota anterior.
                    if (cuotaUf == null) {
                        if (valoresHistorial.Any(c => c.Afp == inputAfp && c.Fondo == fondo && DateTime.ParseExact(c.Fecha, "yyyy-MM-dd", CultureInfo.InvariantCulture) > fecha)) {
                            cuotaUf = valoresHistorial.Where(c => c.Afp == inputAfp && c.Fondo == fondo && DateTime.ParseExact(c.Fecha, "yyyy-MM-dd", CultureInfo.InvariantCulture) <= fecha).OrderByDescending(c => c.Fecha).FirstOrDefault();
                            salida.Historial.ValoresCuota[fondo].Add(cuotaUf?.Valor);
                        } else {
                            salida.Historial.ValoresCuota[fondo].Add(null);
                        }
                    } else {
                        salida.Historial.ValoresCuota[fondo].Add(cuotaUf.Valor);
                    }

                    fecha = fecha.AddDays(-1);
                }

                salida.Historial.ValoresCuota[fondo].Reverse();
            }

            salida.Historial.NombreAfp = LISTA_AFPS[salida.Historial.Afp];

            logger.LogInformation(
                "[{Method}] - [{Controller}] - [{Action}] - [{ElapsedTime} ms] - [{StatusCode}] - [Usuario Autenticado: {IsAuthenticated}] - " +
                "Se retorna exitosamente la página de acceder cuotas - " +
                "HistorialAfp: {HistorialAfp} - HistorialMes: {HistorialMes} - HistorialAnno: {HistorialAnno} - " +
                "Elapsed Time Fecha Todas: {ElapsedTimeFechaTodas} - Elapsed Time Valores Cuota: {ElapsedTimeValoresCuota}.",
                HttpContext.Request.Method, ControllerContext.ActionDescriptor.ControllerName, ControllerContext.ActionDescriptor.ActionName,
                stopwatch.ElapsedMilliseconds, StatusCodes.Status200OK, User.Identity?.IsAuthenticated ?? false,
                modeloEntrada.Historial?.Afp, modeloEntrada.Historial?.Mes, modeloEntrada.Historial?.Anno,
                elapsedTimeFechaTodas, elapsedTimeValoresCuota);

            return View(salida);
        }
    }
}