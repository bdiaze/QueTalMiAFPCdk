using Microsoft.AspNetCore.Mvc;
using QueTalMiAFPCdk.Repositories;

namespace QueTalMiAFPCdk.Controllers {
	public class SimuladorController(CuotaUfComisionDAO cuotaUfComisionDAO) : Controller {

		public async Task<IActionResult> Index() {
            ViewBag.UltimaFechaTodosValoresCuota = await cuotaUfComisionDAO.UltimaFechaTodas();
            string? sueldoImponible = Request.Cookies["SueldoImponible"];
            sueldoImponible ??= "$600.000";
            ViewBag.SueldoImponible = sueldoImponible;

            string? diaCotizacion = Request.Cookies["DiaCotizacion"];
            diaCotizacion ??= "5";
            ViewBag.DiaCotizacion = Convert.ToInt32(diaCotizacion);

            string? ahorroInicialMaximo = Request.Cookies["AhorroInicialMaximo"];
            ahorroInicialMaximo ??= "$50.000.000";
            ViewBag.AhorroInicialMaximo = ahorroInicialMaximo;

            string? efectuarSimulacionCada = Request.Cookies["EfectuarSimulacionCada"];
            efectuarSimulacionCada ??= "$500.000";
            ViewBag.EfectuarSimulacionCada = efectuarSimulacionCada;

            //Se limpian gráficas de cookies que ya no existen...
            string? graficosAbiertos = Request.Cookies["GraficosAbiertosSimulador"];
            if (graficosAbiertos != null) {
                graficosAbiertos = String.Join(",", graficosAbiertos.Split(",").Where(g => g.StartsWith("Tab")));

                HttpContext.Response.Cookies.Append("GraficosAbiertosSimulador", graficosAbiertos, new CookieOptions {
                    Expires = DateTime.Now.AddDays(365),
                    Path = "/Simulador"
                });
            }
            return View();
		}
	}
}
