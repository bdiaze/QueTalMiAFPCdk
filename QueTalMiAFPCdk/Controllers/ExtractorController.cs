using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using QueTalMiAFP.Models;
using QueTalMiAFP.Models.Entities;
using QueTalMiAFP.Models.Others;
using QueTalMiAFP.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using JsonSerializer = System.Text.Json.JsonSerializer;

namespace QueTalMiAFP.Controllers {
	public class ExtractorController : Controller {
		private readonly ILogger<EstadisticasController> _logger;
		private readonly IConfiguration _configuration;

		private readonly string _baseUrl;
		private readonly string _xApiKey;

		public ExtractorController(ILogger<EstadisticasController> logger, IConfiguration configuration) {
			_logger = logger;
			_configuration = configuration;

			_baseUrl = _configuration.GetValue<string>("AWSGatewayAPIKey:api-url");
			_xApiKey = _configuration.GetValue<string>("AWSGatewayAPIKey:x-api-key");
		}

		public IActionResult Index() {
			return View();
		}

		[HttpPost]
		public async Task<IActionResult> Index(int tipoExtraccion, string llaveExtraccion) {
			List<Log> logs = await ExtraerValores(tipoExtraccion, llaveExtraccion);
			logs.Reverse();
			return View(logs);
		}

		[HttpPost]
		[Route("api/[controller]/[action]")]
		public async Task<List<Log>> ExtraerValores(int tipoExtraccion, string llaveExtraccion) {
			string hashedLlave = _configuration.GetValue<string>("AccesoExtraccion:Llave");
			byte[] hashedLlaveBytes = Convert.FromBase64String(hashedLlave);
			byte[] salt = new byte[16];
			Array.Copy(hashedLlaveBytes, 0, salt, 0, 16);

			Rfc2898DeriveBytes pbkdf2 = new Rfc2898DeriveBytes(llaveExtraccion, salt, 100000);
			byte[] hash = pbkdf2.GetBytes(20);
			byte[] hashBytes = new byte[36];
			Array.Copy(salt, 0, hashBytes, 0, 16);
			Array.Copy(hash, 0, hashBytes, 16, 20);
			string strLlaveExtraccion = Convert.ToBase64String(hashBytes);

			Extractor extractor = new Extractor();

			if (strLlaveExtraccion != hashedLlave) {
				extractor.RegistrarLog("Error, no se efectúa la extracción debido a que la llave ingresada no es correcta.", 1);
				return extractor.Logs;
			}

			DateTime fechaInicio;
			switch (tipoExtraccion) {
				case 1: // Extracción completa:
					fechaInicio = new DateTime(2002, 8, 1);
					break;
				case 2: // Extracción último año:
					fechaInicio = DateTime.Now.ToUniversalTime().Date.AddYears(-1);
					break;
				case 3: // Extracción último mes:
					fechaInicio = DateTime.Now.ToUniversalTime().Date.AddMonths(-1);
					break;
				case 4: // Extracción última semana:
					fechaInicio = DateTime.Now.ToUniversalTime().Date.AddDays(-7);
					break;
				default:
					extractor.RegistrarLog("Error, no se efectúa la extracción debido a que el tipo de extracción indicado no es válido.", 1);
					return extractor.Logs;
			}
			DateTime fechaFinal = DateTime.Now.ToUniversalTime().Date;

			extractor.RegistrarLog("Se inicia proceso de extracción de los valores UF.");
			List<Task<HashSet<Uf>>> tasksUf = new List<Task<HashSet<Uf>>>();
			for (int anno = fechaInicio.Year; anno <= fechaFinal.Year; anno++) {
				DateTime fechaInicioParc = new DateTime(
					anno,
					anno > fechaInicio.Year ? 1 : fechaInicio.Month,
					anno > fechaInicio.Year ? 1 : fechaInicio.Day);
				DateTime fechaFinalParc = new DateTime(
					anno,
					anno < fechaFinal.Year ? 12 : fechaFinal.Month,
					anno < fechaFinal.Year ? 31 : fechaFinal.Day);

				tasksUf.Add(extractor.obtenerValoresUF(fechaInicioParc, fechaInicioParc, fechaFinalParc));
			}

			extractor.RegistrarLog("Se inicia proceso de extracción de las comisiones de todas las AFPs.");
			List<Task<List<Comision>>> tasksComisiones = new List<Task<List<Comision>>>();
			for (int anno = fechaInicio.Year; anno <= fechaFinal.Year; anno++) {
				DateTime fechaInicioParc = new DateTime(
					anno,
					anno > fechaInicio.Year ? 1 : fechaInicio.Month,
					anno > fechaInicio.Year ? 1 : fechaInicio.Day);
				DateTime fechaFinalParc = new DateTime(
					anno,
					anno < fechaFinal.Year ? 12 : fechaFinal.Month,
					anno < fechaFinal.Year ? 31 : fechaFinal.Day);

				DateTime fechaMesAnno = new DateTime(
					fechaInicioParc.Year,
					fechaInicioParc.Month,
					1);
				while (fechaMesAnno <= fechaFinalParc) {
					tasksComisiones.Add(extractor.obtenerComisiones(null, fechaMesAnno));

					if (fechaMesAnno >= new DateTime(2008, 10, 1)) {
						tasksComisiones.Add(extractor.obtenerComisionesCAV(null, fechaMesAnno));
					}

					fechaMesAnno = fechaMesAnno.AddMonths(1);
				}
			}

			extractor.RegistrarLog("Se inicia proceso de extracción de los valores cuotas para todas las AFPs.");
			List<Task<List<Cuota>>> tasksCuotas = new List<Task<List<Cuota>>>();
			for (int anno = fechaInicio.Year; anno <= fechaFinal.Year; anno++) {
				DateTime fechaInicioParc = new DateTime(
					anno,
					anno > fechaInicio.Year ? 1 : fechaInicio.Month,
					anno > fechaInicio.Year ? 1 : fechaInicio.Day);
				DateTime fechaFinalParc = new DateTime(
					anno,
					anno < fechaFinal.Year ? 12 : fechaFinal.Month,
					anno < fechaFinal.Year ? 31 : fechaFinal.Day);
				tasksCuotas.Add(extractor.obtenerCuotasCapital(fechaInicioParc, fechaFinalParc, null));
				tasksCuotas.Add(extractor.obtenerCuotasHabitat(fechaInicioParc, fechaFinalParc, null));
				tasksCuotas.Add(extractor.obtenerCuotasPlanvital(fechaInicioParc, fechaFinalParc, null));

				if (anno >= 2010) {
					DateTime fechaInicioParcModelo = fechaInicioParc >= new DateTime(2010, 9, 1) ? fechaInicioParc : new DateTime(2010, 9, 1);
					tasksCuotas.Add(extractor.obtenerCuotasModeloV3(fechaInicioParcModelo, fechaFinalParc, null));
				}

				if (anno >= 2019) {
					DateTime fechaInicioParcUno = fechaInicioParc >= new DateTime(2019, 10, 1) ? fechaInicioParc : new DateTime(2019, 10, 1);
					tasksCuotas.Add(extractor.obtenerCuotasUno(fechaInicioParcUno, fechaFinalParc, null));
				}

				DateTime fechaInicioParcProvida = new DateTime(
					fechaInicioParc.Year,
					fechaInicioParc.Month,
					1);
				while (fechaInicioParcProvida <= fechaFinalParc) {
					tasksCuotas.Add(extractor.obtenerCuotasProvida(fechaInicioParcProvida, fechaInicioParc, fechaFinalParc, null));
					fechaInicioParcProvida = fechaInicioParcProvida.AddMonths(1);
				}
			}
			tasksCuotas.Add(extractor.obtenerCuotasCuprum(fechaInicio, fechaFinal, null));

			int cantUfsExtraidas = 0;
			int cantUfsInsertadas = 0;
			int cantUfsActualizadas = 0;
			foreach (Task<HashSet<Uf>> taskUf in tasksUf) {
				try {
					HashSet<Uf> ufParciales = await taskUf;
					if (ufParciales != null) {
						cantUfsExtraidas += ufParciales.Count;

						EntActualizacionMasivaUf entActMasivUf = new EntActualizacionMasivaUf() {
							Ufs = new List<Uf>(ufParciales)
						};

						using HttpClient client = new HttpClient(new RetryHandler(new HttpClientHandler(), _configuration));
						client.DefaultRequestHeaders.Add("x-api-key", _xApiKey);
						var response = await client.PostAsync(_baseUrl + "Uf/ActualizacionMasiva", new StringContent(JsonConvert.SerializeObject(entActMasivUf), Encoding.UTF8, "application/json"));
						using Stream responseStream = await response.Content.ReadAsStreamAsync();
						JsonSerializerOptions options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
						SalActualizacionMasivaUf salActMasivUf = await JsonSerializer.DeserializeAsync<SalActualizacionMasivaUf>(responseStream, options);
						cantUfsInsertadas += salActMasivUf.CantUfsInsertadas;
						cantUfsActualizadas += salActMasivUf.CantUfsActualizadas;
					}
				} catch (Exception ex) {
					extractor.RegistrarLog($"{ ex }", 1);
				}
			}
			
			int cantComisionesExtraidas = 0;
			int cantComisionesInsertadas = 0;
			int cantComisionesActualizadas = 0;
			foreach (Task<List<Comision>> taskComisiones in tasksComisiones) {
				try {
					List<Comision> comisionesParciales = await taskComisiones;
					if (comisionesParciales != null) {
						cantComisionesExtraidas += comisionesParciales.Count;

						EntActualizacionMasivaComision entActMasivComision = new EntActualizacionMasivaComision() {
							Comisiones = new List<Comision>(comisionesParciales)
						};

						using HttpClient client = new HttpClient(new RetryHandler(new HttpClientHandler(), _configuration));
						client.DefaultRequestHeaders.Add("x-api-key", _xApiKey);
						var response = await client.PostAsync(_baseUrl + "Comision/ActualizacionMasiva", new StringContent(JsonConvert.SerializeObject(entActMasivComision), Encoding.UTF8, "application/json"));
						using Stream responseStream = await response.Content.ReadAsStreamAsync();
						JsonSerializerOptions options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
						SalActualizacionMasivaComision salActMasivComision = await JsonSerializer.DeserializeAsync<SalActualizacionMasivaComision>(responseStream, options);
						cantComisionesInsertadas += salActMasivComision.CantComisionesInsertadas;
						cantComisionesActualizadas += salActMasivComision.CantComisionesActualizadas;
					}
				} catch (Exception ex) {
					extractor.RegistrarLog($"{ ex }", 1);
				}
			}
			
			int cantCuotasExtraidas = 0;
			int cantCuotasInsertadas = 0;
			int cantCuotasActualizadas = 0;
			foreach (Task<List<Cuota>> taskCuotas in tasksCuotas) {
				try {
					List<Cuota> cuotasParciales = await taskCuotas;
					if (cuotasParciales != null) {
						cantCuotasExtraidas += cuotasParciales.Count;

						EntActualizacionMasivaCuota entActMasivCuota = new EntActualizacionMasivaCuota() {
							Cuotas = new List<Cuota>(cuotasParciales)
						};

						using HttpClient client = new HttpClient(new RetryHandler(new HttpClientHandler(), _configuration));
						client.DefaultRequestHeaders.Add("x-api-key", _xApiKey);
						var response = await client.PostAsync(_baseUrl + "Cuota/ActualizacionMasiva", new StringContent(JsonConvert.SerializeObject(entActMasivCuota), Encoding.UTF8, "application/json"));
						using Stream responseStream = await response.Content.ReadAsStreamAsync();
						JsonSerializerOptions options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
						SalActualizacionMasivaCuota salActMasivCuota = await JsonSerializer.DeserializeAsync<SalActualizacionMasivaCuota>(responseStream, options);
						cantCuotasInsertadas += salActMasivCuota.CantCuotasInsertadas;
						cantCuotasActualizadas += salActMasivCuota.CantCuotasActualizadas;
					}
				} catch (Exception ex) {
					extractor.RegistrarLog($"{ ex }", 1);
				}
			}

			extractor.RegistrarLog($"Se termina proceso de extracción de los valores UF con una cantidad de {cantUfsExtraidas} registros.");
			extractor.RegistrarLog($"Se termina proceso de extracción de las comisiones con una cantidad de {cantComisionesExtraidas} registros.");
			extractor.RegistrarLog($"Se termina proceso de extracción de los valores cuotas con una cantidad de {cantCuotasExtraidas} registros.");

			extractor.RegistrarLog($"Cantidad de valores UF insertados en proceso de extracción: {cantUfsInsertadas}");
			extractor.RegistrarLog($"Cantidad de valores UF actualizados en proceso de extracción: {cantUfsActualizadas}");
			extractor.RegistrarLog($"Cantidad de comisiones insertadas en proceso de extracción: {cantComisionesInsertadas}");
			extractor.RegistrarLog($"Cantidad de comisiones actualizadas en proceso de extracción: {cantComisionesActualizadas}");
			extractor.RegistrarLog($"Cantidad de cuotas insertadas en proceso de extracción: {cantCuotasInsertadas}");
			extractor.RegistrarLog($"Cantidad de cuotas actualizadas en proceso de extracción: {cantCuotasActualizadas}");

			int cantLogsError = 0;
			foreach (Log log in extractor.Logs) {
				if (log.Tipo == 1) cantLogsError++;
			}

			if (cantLogsError > 0) {
				string mensaje;
				if (cantLogsError == 1) {
					mensaje = $"Se observó un error en el proceso de extracción de valores.";
				} else {
					mensaje = $"Se observaron {cantLogsError} errores en el proceso de extracción de valores.";
				}
				extractor.RegistrarLog(mensaje, 1);
			}

			return extractor.Logs;
		}
	}
}
