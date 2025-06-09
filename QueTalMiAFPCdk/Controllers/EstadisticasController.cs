using System;
using System.Linq;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using QueTalMiAFP.Models;
using QueTalMiAFP.Models.Others;
using QueTalMiAFP.Models.ViewModels;
using QueTalMiAFP.Repositories;
using QueTalMiAFPCdk.Models.ViewModels;

namespace QueTalMiAFP.Controllers {
	public class EstadisticasController : Controller {
		private readonly ICuotaUfComisionDAO _cuotaUfComisionDAO;

		public EstadisticasController(ICuotaUfComisionDAO cuotaUfComisionDAO) {
			_cuotaUfComisionDAO = cuotaUfComisionDAO;
		}

		public async Task<IActionResult> Index() {
			DateTime? ultimaFechaAlgunValorCuota = await _cuotaUfComisionDAO.UltimaFechaAlguna();
			if (ultimaFechaAlgunValorCuota == null) {
				ultimaFechaAlgunValorCuota = TimeZoneInfo.ConvertTime(DateTime.Now, TimeZoneConverter.TZConvert.GetTimeZoneInfo("Pacific SA Standard Time"));
			}
			ViewBag.UltimaFechaAlgunValorCuota = ultimaFechaAlgunValorCuota.Value;
			return View();
		}

		public async Task<IActionResult> ComparandoFondos() {
			DateTime? ultimaFechaAlgunValorCuota = await _cuotaUfComisionDAO.UltimaFechaAlguna();
			if (ultimaFechaAlgunValorCuota == null) {
				ultimaFechaAlgunValorCuota = TimeZoneInfo.ConvertTime(DateTime.Now, TimeZoneConverter.TZConvert.GetTimeZoneInfo("Pacific SA Standard Time"));
			}
			ViewBag.UltimaFechaAlgunValorCuota = ultimaFechaAlgunValorCuota.Value;
			return View();
		}

		public async Task<IActionResult> PremioRentabilidad() {
			string afps = "CAPITAL,CUPRUM,HABITAT,MODELO,PLANVITAL,PROVIDA,UNO";
			string fondos = "A,B,C,D,E";
			DateTime ultimaFecha = await _cuotaUfComisionDAO.UltimaFechaTodas();
			DateTime fechaInicial = ultimaFecha.AddYears(-1);
			List<RentabilidadReal> rentabilidades = await _cuotaUfComisionDAO.ObtenerRentabilidadReal(afps, fondos, fechaInicial, ultimaFecha);

			List<RentabilidadReal> rentabilidadesPromedios = new List<RentabilidadReal>();
			foreach (string afp in afps.Split(",")) {
				decimal sumaRentabilidades = rentabilidades.Where(r => r.Afp == afp).Sum(r => r.Rentabilidad);
				int cantRentabilidades = rentabilidades.Where(r => r.Afp == afp).Count();
				decimal rentabilidadPromedio = sumaRentabilidades / cantRentabilidades;
				rentabilidadesPromedios.Add(new RentabilidadReal() { 
					Afp = afp,
					Fondo = "X",
					Rentabilidad = rentabilidadPromedio
				});
			}
			rentabilidadesPromedios = rentabilidadesPromedios.OrderByDescending(r => r.Rentabilidad).ToList();

			if (rentabilidadesPromedios.Count > 0) { 
				string formatoPrimerLugar = ".svg";
				if (rentabilidadesPromedios[0].Afp == "HABITAT" || rentabilidadesPromedios[0].Afp == "PROVIDA" || rentabilidadesPromedios[0].Afp == "UNO") { 
					formatoPrimerLugar = ".png";
				}
				string nombreAfpPrimerLugar = rentabilidadesPromedios[0].Afp.Substring(0, 1) + rentabilidadesPromedios[0].Afp[1..].ToLower();
				ViewBag.UrlImagenPrimerLugar = $"/images/logos_afps/LogoAFP{nombreAfpPrimerLugar + formatoPrimerLugar}";
			}

			if (rentabilidadesPromedios.Count > 1) {
				string formatoSegundoLugar = ".svg";
				if (rentabilidadesPromedios[1].Afp == "HABITAT" || rentabilidadesPromedios[1].Afp == "PROVIDA" || rentabilidadesPromedios[1].Afp == "UNO") {
					formatoSegundoLugar = ".png";
				}
				string nombreAfpSegundoLugar = rentabilidadesPromedios[1].Afp.Substring(0, 1) + rentabilidadesPromedios[1].Afp[1..].ToLower();
				ViewBag.UrlImagenSegundoLugar = $"/images/logos_afps/LogoAFP{nombreAfpSegundoLugar + formatoSegundoLugar}";
			}

			if (rentabilidadesPromedios.Count > 2) {
				string formatoTercerLugar = ".svg";
				if (rentabilidadesPromedios[2].Afp == "HABITAT" || rentabilidadesPromedios[2].Afp == "PROVIDA" || rentabilidadesPromedios[2].Afp == "UNO") {
					formatoTercerLugar = ".png";
				}
				string nombreAfpTercerLugar = rentabilidadesPromedios[2].Afp.Substring(0, 1) + rentabilidadesPromedios[2].Afp[1..].ToLower();
				ViewBag.UrlImagenTercerLugar = $"/images/logos_afps/LogoAFP{nombreAfpTercerLugar + formatoTercerLugar}";
			}

			return PartialView();
		}

		[ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
		public IActionResult Error() {
			return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
		}
	}
}
