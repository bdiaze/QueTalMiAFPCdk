using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using QueTalMiAFP.Models;
using QueTalMiAFP.Models.Entities;
using QueTalMiAFP.Models.ViewModels;
using QueTalMiAFP.Repositories;
using QueTalMiAFP.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace QueTalMiAFP.Controllers {
	public class SimuladorController : Controller {
        private readonly IConfiguration _configuration;
        private readonly ICuotaUfComisionDAO _cuotaUfComisionDAO;

        private readonly string _baseUrl;
        private readonly string _xApiKey;

        public SimuladorController(IConfiguration configuration, ICuotaUfComisionDAO cuotaUfComisionDAO) {
            _configuration = configuration;
            _cuotaUfComisionDAO = cuotaUfComisionDAO;

            _baseUrl = _configuration.GetValue<string>("AWSGatewayAPIKey:api-url");
            _xApiKey = _configuration.GetValue<string>("AWSGatewayAPIKey:x-api-key");
        }

		public async Task<IActionResult> Index() {
            ViewBag.UltimaFechaTodosValoresCuota = await _cuotaUfComisionDAO.UltimaFechaTodas();
            string sueldoImponible = Request.Cookies["SueldoImponible"];
            sueldoImponible ??= "$600.000";
            ViewBag.SueldoImponible = sueldoImponible;

            string diaCotizacion = Request.Cookies["DiaCotizacion"];
            diaCotizacion ??= "5";
            ViewBag.DiaCotizacion = Convert.ToInt32(diaCotizacion);

            string ahorroInicialMaximo = Request.Cookies["AhorroInicialMaximo"];
            ahorroInicialMaximo ??= "$50.000.000";
            ViewBag.AhorroInicialMaximo = ahorroInicialMaximo;

            string efectuarSimulacionCada = Request.Cookies["EfectuarSimulacionCada"];
            efectuarSimulacionCada ??= "$500.000";
            ViewBag.EfectuarSimulacionCada = efectuarSimulacionCada;

            return View();
		}
	}
}
