using Microsoft.AspNetCore.Mvc;
using QueTalMiAFPCdk.Repositories;
using System.Diagnostics;

namespace QueTalMiAFPCdk.Controllers {
	public class SimuladorController(ILogger<SimuladorController> logger, CuotaUfComisionDAO cuotaUfComisionDAO) : Controller {

		public async Task<IActionResult> Index() {
            Stopwatch stopwatch = Stopwatch.StartNew();

            ViewBag.UltimaFechaTodosValoresCuota = await cuotaUfComisionDAO.UltimaFechaTodas();
            long elapsedTimeFechaTodas = stopwatch.ElapsedMilliseconds;

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
                    Path = "/Simulador",
                    Secure = true
                });
            }

            logger.LogInformation(
                "[{Method}] - [{Controller}] - [{Action}] - [{ElapsedTime} ms] - [{StatusCode}] - [Usuario Autenticado: {IsAuthenticated}] - " +
                "Se retorna exitosamente la página de simulación - " +
                "Elapsed Time Fecha Todas: {FechaTodas}.",
                HttpContext.Request.Method.Replace(Environment.NewLine, " "), ControllerContext.ActionDescriptor.ControllerName, ControllerContext.ActionDescriptor.ActionName,
                stopwatch.ElapsedMilliseconds, StatusCodes.Status200OK, User.Identity?.IsAuthenticated ?? false,
                elapsedTimeFechaTodas);

            return View();
		}
	}
}
