using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using QueTalMiAFPCdk.Models.Others;
using QueTalMiAFPCdk.Models.ViewModels;
using QueTalMiAFPCdk.Repositories;

namespace QueTalMiAFPCdk.Controllers {
	public class EstadisticasController(ICuotaUfComisionDAO cuotaUfComisionDAO) : Controller {

        public async Task<IActionResult> Index() {
			DateTime? ultimaFechaAlgunValorCuota = await cuotaUfComisionDAO.UltimaFechaAlguna();
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

            return View();
		}

		public async Task<IActionResult> ComparandoFondos() {
			DateTime? ultimaFechaAlgunValorCuota = await cuotaUfComisionDAO.UltimaFechaAlguna();
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
            return View();
		}

		[ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
		public IActionResult Error() {
			return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
		}
	}
}
