using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using QueTalMiAFPCdk.Models.Others;
using QueTalMiAFPCdk.Models.ViewModels;
using QueTalMiAFPCdk.Repositories;

namespace QueTalMiAFPCdk.Controllers {
	public class EstadisticasController(ILogger<EstadisticasController> logger, CuotaUfComisionDAO cuotaUfComisionDAO) : Controller {

        public async Task<IActionResult> Index() {
            Stopwatch stopwatch = Stopwatch.StartNew();

            DateTime? ultimaFechaAlgunValorCuota = await cuotaUfComisionDAO.UltimaFechaAlguna();
            long elapsedTimeFechaAlguna = stopwatch.ElapsedMilliseconds;

            if (ultimaFechaAlgunValorCuota == null) {
				ultimaFechaAlgunValorCuota = TimeZoneInfo.ConvertTime(DateTime.Now, TimeZoneConverter.TZConvert.GetTimeZoneInfo("Pacific SA Standard Time"));
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

            DateTime? ultimaFechaAlgunValorCuota = await cuotaUfComisionDAO.UltimaFechaAlguna();
            long elapsedTimeFechaAlguna = stopwatch.ElapsedMilliseconds;

            if (ultimaFechaAlgunValorCuota == null) {
				ultimaFechaAlgunValorCuota = TimeZoneInfo.ConvertTime(DateTime.Now, TimeZoneConverter.TZConvert.GetTimeZoneInfo("Pacific SA Standard Time"));
			}
			ViewBag.UltimaFechaAlgunValorCuota = ultimaFechaAlgunValorCuota.Value;

            //Se limpian gráficas de cookies que ya no existen...
            string? graficosAbiertos = Request.Cookies["GraficosAbiertosPorAFP"];
            if (graficosAbiertos != null) {
                graficosAbiertos = String.Join(",", graficosAbiertos.Split(",").Where(g => g.StartsWith("Tab")));

                HttpContext.Response.Cookies.Append("GraficosAbiertosPorAFP", graficosAbiertos, new CookieOptions {
                    Expires = DateTime.Now.AddDays(365),
                    Path = "/Estadisticas/ComparandoFondos"
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
	}
}
