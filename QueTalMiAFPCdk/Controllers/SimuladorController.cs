using Microsoft.AspNetCore.Mvc;
using QueTalMiAFPCdk.Repositories;

namespace QueTalMiAFPCdk.Controllers {
	public class SimuladorController(ICuotaUfComisionDAO cuotaUfComisionDAO) : Controller {

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

            return View();
		}
	}
}
