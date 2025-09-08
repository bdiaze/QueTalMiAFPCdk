using Amazon.APIGateway.Model;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using QueTalMiAFPCdk.Models.Entities;
using QueTalMiAFPCdk.Models.Others;
using QueTalMiAFPCdk.Services;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using JsonSerializer = System.Text.Json.JsonSerializer;

namespace QueTalMiAFPCdk.Controllers {
	public class ExtractorController(ParameterStoreHelper parameterStore, SecretManagerHelper secretManager, ApiKeyHelper apiKey) : Controller {
		private readonly string _baseUrl = parameterStore.ObtenerParametro("/QueTalMiAFP/Api/Url").Result;
        private readonly string _xApiKey = apiKey.ObtenerApiKey(parameterStore.ObtenerParametro("/QueTalMiAFP/Api/KeyId").Result).Result;

        private readonly JsonSerializerOptions _options = new() { PropertyNameCaseInsensitive = true };

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
			string hashedLlave = (await secretManager.ObtenerSecreto("/QueTalMiAFP")).ExtractorKey;
			byte[] hashedLlaveBytes = Convert.FromBase64String(hashedLlave);
			byte[] salt = new byte[16];
			Array.Copy(hashedLlaveBytes, 0, salt, 0, 16);

			Rfc2898DeriveBytes pbkdf2 = new(llaveExtraccion, salt, 100000, HashAlgorithmName.SHA1);
			byte[] hash = pbkdf2.GetBytes(20);
			byte[] hashBytes = new byte[36];
			Array.Copy(salt, 0, hashBytes, 0, 16);
			Array.Copy(hash, 0, hashBytes, 16, 20);
			string strLlaveExtraccion = Convert.ToBase64String(hashBytes);

			Extractor extractor = new(parameterStore);

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
			List<Task<HashSet<Uf>>> tasksUf = [];
			for (int anno = fechaInicio.Year; anno <= fechaFinal.Year; anno++) {
				DateTime fechaInicioParc = new(
					anno,
					anno > fechaInicio.Year ? 1 : fechaInicio.Month,
					anno > fechaInicio.Year ? 1 : fechaInicio.Day);
				DateTime fechaFinalParc = new(
					anno,
					anno < fechaFinal.Year ? 12 : fechaFinal.Month,
					anno < fechaFinal.Year ? 31 : fechaFinal.Day);

				tasksUf.Add(extractor.ObtenerValoresUF(fechaInicioParc, fechaInicioParc, fechaFinalParc));
			}

			extractor.RegistrarLog("Se inicia proceso de extracción de las comisiones de todas las AFPs.");
			List<Task<List<Comision>>> tasksComisiones = [];
			for (int anno = fechaInicio.Year; anno <= fechaFinal.Year; anno++) {
				DateTime fechaInicioParc = new(
					anno,
					anno > fechaInicio.Year ? 1 : fechaInicio.Month,
					anno > fechaInicio.Year ? 1 : fechaInicio.Day);
				DateTime fechaFinalParc = new(
					anno,
					anno < fechaFinal.Year ? 12 : fechaFinal.Month,
					anno < fechaFinal.Year ? 31 : fechaFinal.Day);

				DateTime fechaMesAnno = new(
					fechaInicioParc.Year,
					fechaInicioParc.Month,
					1);
				while (fechaMesAnno <= fechaFinalParc) {
					tasksComisiones.Add(extractor.ObtenerComisiones(null, fechaMesAnno));

					if (fechaMesAnno >= new DateTime(2008, 10, 1)) {
						tasksComisiones.Add(extractor.ObtenerComisionesCAV(null, fechaMesAnno));
					}

					fechaMesAnno = fechaMesAnno.AddMonths(1);
				}
			}

			extractor.RegistrarLog("Se inicia proceso de extracción de los valores cuotas para todas las AFPs.");
			List<Task<List<Cuota>>> tasksCuotas = [];
			for (int anno = fechaInicio.Year; anno <= fechaFinal.Year; anno++) {
				DateTime fechaInicioParc = new(
					anno,
					anno > fechaInicio.Year ? 1 : fechaInicio.Month,
					anno > fechaInicio.Year ? 1 : fechaInicio.Day);
				DateTime fechaFinalParc = new(
					anno,
					anno < fechaFinal.Year ? 12 : fechaFinal.Month,
					anno < fechaFinal.Year ? 31 : fechaFinal.Day);

                List<string> fondos = [ "A", "B", "C", "D", "E" ];
                foreach (string fondo in fondos) {
					tasksCuotas.Add(extractor.ObtenerCuotasSPensiones(fechaInicioParc, fechaFinalParc, fondo));
                }

				tasksCuotas.Add(extractor.ObtenerCuotasHabitat(fechaInicioParc, fechaFinalParc, null));
				tasksCuotas.Add(extractor.ObtenerCuotasPlanvital(fechaInicioParc, fechaFinalParc, null));

				if (anno >= 2010) {
					DateTime fechaInicioParcModelo = fechaInicioParc >= new DateTime(2010, 9, 1) ? fechaInicioParc : new DateTime(2010, 9, 1);
					tasksCuotas.Add(extractor.ObtenerCuotasModeloV3(fechaInicioParcModelo, fechaFinalParc, null));
				}

				if (anno >= 2019) {
					DateTime fechaInicioParcUno = fechaInicioParc >= new DateTime(2019, 10, 1) ? fechaInicioParc : new DateTime(2019, 10, 1);
					tasksCuotas.Add(extractor.ObtenerCuotasUno(fechaInicioParcUno, fechaFinalParc, null));
				}

				DateTime fechaInicioParcProvida = new(
					fechaInicioParc.Year,
					fechaInicioParc.Month,
					1);
				while (fechaInicioParcProvida <= fechaFinalParc) {
					tasksCuotas.Add(extractor.ObtenerCuotasProvida(fechaInicioParcProvida, fechaInicioParc, fechaFinalParc, null));
					fechaInicioParcProvida = fechaInicioParcProvida.AddMonths(1);
				}
			}
			tasksCuotas.Add(extractor.ObtenerCuotasCuprum(fechaInicio, fechaFinal, null));

			int cantUfsExtraidas = 0;
			int cantUfsInsertadas = 0;
			int cantUfsActualizadas = 0;
			foreach (Task<HashSet<Uf>> taskUf in tasksUf) {
				try {
					HashSet<Uf> ufParciales = await taskUf;
					if (ufParciales != null) {
						cantUfsExtraidas += ufParciales.Count;

						EntActualizacionMasivaUf entActMasivUf = new() {
							Ufs = [.. ufParciales]
                        };

						using HttpClient client = new(new RetryHandler(new HttpClientHandler(), parameterStore));
						client.DefaultRequestHeaders.Add("x-api-key", _xApiKey);
						var response = await client.PostAsync(_baseUrl + "Uf/ActualizacionMasiva", new StringContent(JsonConvert.SerializeObject(entActMasivUf), Encoding.UTF8, "application/json"));
						using Stream responseStream = await response.Content.ReadAsStreamAsync();
						SalActualizacionMasivaUf? salActMasivUf = await JsonSerializer.DeserializeAsync<SalActualizacionMasivaUf>(responseStream, _options);
						cantUfsInsertadas += salActMasivUf!.CantUfsInsertadas;
						cantUfsActualizadas += salActMasivUf!.CantUfsActualizadas;
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

						EntActualizacionMasivaComision entActMasivComision = new() {
							Comisiones = [.. comisionesParciales]
                        };

						using HttpClient client = new(new RetryHandler(new HttpClientHandler(), parameterStore));
						client.DefaultRequestHeaders.Add("x-api-key", _xApiKey);
						var response = await client.PostAsync(_baseUrl + "Comision/ActualizacionMasiva", new StringContent(JsonConvert.SerializeObject(entActMasivComision), Encoding.UTF8, "application/json"));
						using Stream responseStream = await response.Content.ReadAsStreamAsync();
						SalActualizacionMasivaComision? salActMasivComision = await JsonSerializer.DeserializeAsync<SalActualizacionMasivaComision>(responseStream, _options);
						cantComisionesInsertadas += salActMasivComision!.CantComisionesInsertadas;
						cantComisionesActualizadas += salActMasivComision!.CantComisionesActualizadas;
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

						EntActualizacionMasivaCuota entActMasivCuota = new() {
							Cuotas = [.. cuotasParciales]
                        };

						using HttpClient client = new(new RetryHandler(new HttpClientHandler(), parameterStore));
						client.DefaultRequestHeaders.Add("x-api-key", _xApiKey);
						var response = await client.PostAsync(_baseUrl + "Cuota/ActualizacionMasiva", new StringContent(JsonConvert.SerializeObject(entActMasivCuota), Encoding.UTF8, "application/json"));
						using Stream responseStream = await response.Content.ReadAsStreamAsync();
						SalActualizacionMasivaCuota? salActMasivCuota = await JsonSerializer.DeserializeAsync<SalActualizacionMasivaCuota>(responseStream, _options);
						cantCuotasInsertadas += salActMasivCuota!.CantCuotasInsertadas;
						cantCuotasActualizadas += salActMasivCuota!.CantCuotasActualizadas;
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
