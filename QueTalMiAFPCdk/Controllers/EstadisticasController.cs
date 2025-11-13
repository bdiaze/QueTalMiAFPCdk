using Microsoft.AspNetCore.Mvc;
using QueTalMiAFPCdk.Models.Others;
using QueTalMiAFPCdk.Models.ViewModels;
using QueTalMiAFPCdk.Repositories;
using System.Diagnostics;
using System.Globalization;
using System.Linq;

namespace QueTalMiAFPCdk.Controllers {
	public class EstadisticasController(ILogger<EstadisticasController> logger, CuotaUfComisionDAO cuotaUfComisionDAO) : Controller {

        public async Task<IActionResult> Index() {
            Stopwatch stopwatch = Stopwatch.StartNew();

			DateOnly? ultimaFechaAlgunValorCuota = await cuotaUfComisionDAO.UltimaFechaAlguna();
            long elapsedTimeFechaAlguna = stopwatch.ElapsedMilliseconds;

            if (ultimaFechaAlgunValorCuota == null) {
				ultimaFechaAlgunValorCuota = DateOnly.FromDateTime(TimeZoneInfo.ConvertTime(DateTime.Now, TimeZoneConverter.TZConvert.GetTimeZoneInfo("Pacific SA Standard Time")));
			}
			ViewBag.UltimaFechaAlgunValorCuota = ultimaFechaAlgunValorCuota.Value;

            //Se limpian gráficas de cookies que ya no existen...
            string? graficosAbiertos = Request.Cookies["GraficosAbiertos"];
			if (graficosAbiertos != null) {
				graficosAbiertos = String.Join(",", graficosAbiertos.Split(",").Where(g => g.StartsWith("Tab")));

                HttpContext.Response.Cookies.Append("GraficosAbiertos", graficosAbiertos, new CookieOptions {
                    Expires = DateTime.Now.AddDays(365),
					Path = "/Estadisticas"
                });
            }

            logger.LogInformation(
                "[{Method}] - [{Controller}] - [{Action}] - [{ElapsedTime} ms] - [{StatusCode}] - [Usuario Autenticado: {IsAuthenticated}] - " +
                "Se retorna exitosamente la página de estadística - " +
                "Elapsed Time Fecha Alguna: {ElapsedTimeFechaAlguna}.",
                HttpContext.Request.Method.Replace(Environment.NewLine, " "), ControllerContext.ActionDescriptor.ControllerName, ControllerContext.ActionDescriptor.ActionName,
                stopwatch.ElapsedMilliseconds, StatusCodes.Status200OK, User.Identity?.IsAuthenticated ?? false,
                elapsedTimeFechaAlguna);

            return View();
		}

		public async Task<IActionResult> ComparandoFondos() {
            Stopwatch stopwatch = Stopwatch.StartNew();

			DateOnly? ultimaFechaAlgunValorCuota = await cuotaUfComisionDAO.UltimaFechaAlguna();
            long elapsedTimeFechaAlguna = stopwatch.ElapsedMilliseconds;

            if (ultimaFechaAlgunValorCuota == null) {
				ultimaFechaAlgunValorCuota = DateOnly.FromDateTime(TimeZoneInfo.ConvertTime(DateTime.Now, TimeZoneConverter.TZConvert.GetTimeZoneInfo("Pacific SA Standard Time")));
			}

			ComparandoFondosViewModel model = new() {
                UltimaFechaAlgunValorCuota = ultimaFechaAlgunValorCuota.Value
            };

			//Se limpian gráficas de cookies que ya no existen...
			string graficosAbiertos = Request.Cookies["GraficosAbiertosPorAFP"] ?? "";

            if (!graficosAbiertos.Split(",").Where(g => g.StartsWith("TabRR")).Any()) {
                if (graficosAbiertos.Length > 0) graficosAbiertos += ",";
                graficosAbiertos += "TabRRCapital";
            }

            if (!graficosAbiertos.Split(",").Where(g => g.StartsWith("TabRT")).Any()) {
                if (graficosAbiertos.Length > 0) graficosAbiertos += ",";
                graficosAbiertos += "TabRTCapital";
            }

            if (!graficosAbiertos.Split(",").Where(g => g.StartsWith("TabVC")).Any()) {
                if (graficosAbiertos.Length > 0) graficosAbiertos += ",";
                graficosAbiertos += "TabVCCapital";
            }

            if (!graficosAbiertos.Split(",").Where(g => g.StartsWith("TabRentRango")).Any()) {
                if (graficosAbiertos.Length > 0) graficosAbiertos += ",";
                graficosAbiertos += "TabRentRangoCapital";
            }

            graficosAbiertos = String.Join(",", graficosAbiertos.Split(",").Where(g => g.StartsWith("Tab")));
            HttpContext.Response.Cookies.Append("GraficosAbiertosPorAFP", graficosAbiertos, new CookieOptions {
                Expires = DateTime.Now.AddDays(365),
                Path = "/Estadisticas/ComparandoFondos"
            });

            // Se calcularán las rentabilidades para:
            //      - mes actual
            //      - mes anterior
            //      - 3 meses anteriores
            //      - 6 meses anteriores
            //      - un año / 12 meses anteriores
            //      - 3 años / 36 meses anteriores
            //      - 5 años / 60 meses anteriores
            //      - 10 años / 120 meses anteriores
            List<DateOnly> listaFechasRentabilidades = [ ultimaFechaAlgunValorCuota.Value ];
            DateOnly fechaAuxiliar = new DateOnly(listaFechasRentabilidades[0].Year, listaFechasRentabilidades[0].Month, 1).AddDays(-1);
            foreach (int delta in new List<int> { 0, -1, -3, -6, -12, -36, -60, -120 }) {
                model.Rentabilidades.Add(new RentabilidadPorRango { 
                    TipoRango =  delta == 0 ? "Mes Actual" : 
                        Math.Abs(delta) == 1 ? "Mes Anterior" : 
                        Math.Abs(delta) <= 12 ? $"Últimos {Math.Abs(delta)} meses" : 
                        $"Últimos {Math.Abs(delta) / 12} años",
                    FechaDesde = fechaAuxiliar.AddMonths(delta),
                    FechaHasta = delta != 0 ? fechaAuxiliar : listaFechasRentabilidades.Last(),                
                });

				listaFechasRentabilidades.Add(fechaAuxiliar.AddMonths(delta));
			}

            List<SalObtenerUltimaCuota> cuotas = await cuotaUfComisionDAO.ObtenerUltimaCuota(
				"CAPITAL,CUPRUM,HABITAT,MODELO,PLANVITAL,PROVIDA,UNO",
				"A,B,C,D,E",
				String.Join(",", listaFechasRentabilidades.Select(f => f.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture))),
				1
			);
            foreach (string afp in "CAPITAL,CUPRUM,HABITAT,MODELO,PLANVITAL,PROVIDA,UNO".Split(',')) {
                List<SalObtenerUltimaCuota> cuotasAfp = [.. cuotas.Where(c => c.Afp == afp)];
                foreach (string fondo in "A,B,C,D,E".Split(',')) {
                    List<SalObtenerUltimaCuota> cuotasAfpFondo = [.. cuotasAfp.Where(c => c.Fondo == fondo)];
                    foreach (RentabilidadPorRango rentabilidad in model.Rentabilidades) {
                        SalObtenerUltimaCuota? cuotaDesde = cuotasAfpFondo.Where(c => c.Fecha <= rentabilidad.FechaDesde).OrderByDescending(c => c.Fecha).FirstOrDefault();
                        SalObtenerUltimaCuota? cuotaHasta = cuotasAfpFondo.Where(c => c.Fecha <= rentabilidad.FechaHasta).OrderByDescending(c => c.Fecha).FirstOrDefault();
                        if (cuotaDesde != null & cuotaHasta != null) {
                            rentabilidad.Rentabilidades[afp][fondo] = (cuotaHasta!.Valor - cuotaDesde!.Valor) * 100 / cuotaDesde!.Valor;
                        }
                    }
                }
            }

			logger.LogInformation(
                "[{Method}] - [{Controller}] - [{Action}] - [{ElapsedTime} ms] - [{StatusCode}] - [Usuario Autenticado: {IsAuthenticated}] - " +
                "Se retorna exitosamente la página de estadística - " +
                "Elapsed Time Fecha Alguna: {ElapsedTimeFechaAlguna}.",
                HttpContext.Request.Method.Replace(Environment.NewLine, " "), ControllerContext.ActionDescriptor.ControllerName, ControllerContext.ActionDescriptor.ActionName,
                stopwatch.ElapsedMilliseconds, StatusCodes.Status200OK, User.Identity?.IsAuthenticated ?? false,
                elapsedTimeFechaAlguna);

            return View(model);
		}
	}
}
