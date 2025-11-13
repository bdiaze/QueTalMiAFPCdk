using Microsoft.AspNetCore.WebUtilities;
using Newtonsoft.Json;
using QueTalMiAFPCdk.Models.Entities;
using QueTalMiAFPCdk.Models.Others;
using QueTalMiAFPCdk.Services;
using System;
using System.Configuration;
using System.Globalization;
using System.Net;
using System.Text;

namespace QueTalMiAFPCdk.Repositories {
	public class CuotaUfComisionDAO(IHostEnvironment environment, IConfiguration configuration, ParameterStoreHelper parameterStore, ApiKeyHelper apiKey, S3BucketHelper s3BucketHelper) {
		private readonly string _baseUrl = environment.IsProduction() ? parameterStore.ObtenerParametro("/QueTalMiAFP/Api/Url").Result : configuration.GetValue<string>("ApiUrl")!;
		private readonly string _xApiKey = apiKey.ObtenerApiKey(parameterStore.ObtenerParametro("/QueTalMiAFP/Api/KeyId").Result).Result;
		private readonly int _milisegForzarTimeout = int.Parse(parameterStore.ObtenerParametro("/QueTalMiAFP/Api/MilisegForzarTimeout").Result);

		private readonly EntCorreoDireccion _direccionDeDefecto = JsonConvert.DeserializeObject<EntCorreoDireccion>(parameterStore.ObtenerParametro("/QueTalMiAFP/SES/DireccionDeDefecto").Result)!;
		private readonly string _hermesBaseUrl = parameterStore.ObtenerParametro("/Hermes/Api/Url").Result;
		private readonly string _hermesApiKey = apiKey.ObtenerApiKey(parameterStore.ObtenerParametro("/Hermes/Api/KeyId").Result).Result;

		public async Task<DateOnly> UltimaFechaTodas() {
			using HttpClient client = new(new RetryHandler(new HttpClientHandler(), parameterStore));
			client.DefaultRequestHeaders.Add("x-api-key", _xApiKey);

			HttpResponseMessage response = await client.GetAsync(_baseUrl + "CuotaUfComision/UltimaFechaTodas");
			string responseString = await response.Content.ReadAsStringAsync();
			responseString = responseString.Replace("\"", "");

			return DateOnly.Parse(responseString, CultureInfo.InvariantCulture);
		}

		public async Task<DateOnly> UltimaFechaAlguna() {
			using HttpClient client = new(new RetryHandler(new HttpClientHandler(), parameterStore));
			client.DefaultRequestHeaders.Add("x-api-key", _xApiKey);

			HttpResponseMessage response = await client.GetAsync(_baseUrl + "CuotaUfComision/UltimaFechaAlguna");
			string responseString = await response.Content.ReadAsStringAsync();
			responseString = responseString.Replace("\"", "");

			return DateOnly.Parse(responseString, CultureInfo.InvariantCulture);
		}

		public async Task<DateOnly?> UltimaFechaAlgunaConTimeout() {
			using HttpClient client = new();
			client.DefaultRequestHeaders.Add("x-api-key", _xApiKey);

			CancellationTokenSource cts = new();
			cts.CancelAfter(_milisegForzarTimeout);
			try {
				HttpResponseMessage response = await client.GetAsync(_baseUrl + "CuotaUfComision/UltimaFechaAlguna", cts.Token);
				string responseString = await response.Content.ReadAsStringAsync();
				responseString = responseString.Replace("\"", "");
				return DateOnly.Parse(responseString, CultureInfo.InvariantCulture);
			} catch (TaskCanceledException) {
				if (!cts.Token.IsCancellationRequested) {
					throw;
				}
			}

			return null;
		}


		public async Task<List<CuotaUf>> ObtenerCuotas(string listaAFPs, string listaFondos, DateOnly fechaInicial, DateOnly fechaFinal) {
			Dictionary<string, string?> parameters = new() {
				{ "listaAFPs", listaAFPs },
				{ "listaFondos", listaFondos },
				{ "fechaInicial", fechaInicial.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture) },
				{ "fechaFinal", fechaFinal.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture) }
			};

			using HttpClient client = new(new RetryHandler(new HttpClientHandler(), parameterStore));
			client.DefaultRequestHeaders.Add("x-api-key", _xApiKey);
			string requestUri = QueryHelpers.AddQueryString(_baseUrl + "CuotaUfComision/ObtenerCuotas", parameters);
			HttpResponseMessage response = await client.GetAsync(requestUri);
			string responseString = await response.Content.ReadAsStringAsync();
			SalObtenerCuotas? retornoConsulta = JsonConvert.DeserializeObject<SalObtenerCuotas>(responseString);

			if (retornoConsulta!.S3Url == null) {
				return retornoConsulta!.ListaCuotas!;
			} else {
				return JsonConvert.DeserializeObject<List<CuotaUf>>(await s3BucketHelper.GetFile(retornoConsulta!.S3Url))!;
			}
		}

		public async Task<List<SalObtenerUltimaCuota>> ObtenerUltimaCuota(string listaAFPs, string listaFondos, string listaFechas, int tipoComision) {
			EntObtenerUltimaCuota entradaSanitizada = new() {
				ListaAFPs = WebUtility.HtmlEncode(listaAFPs),
				ListaFondos = WebUtility.HtmlEncode(listaFondos),
				ListaFechas = WebUtility.HtmlEncode(listaFechas),
				TipoComision = tipoComision
			};

			using HttpClient client = new(new RetryHandler(new HttpClientHandler(), parameterStore));
			client.DefaultRequestHeaders.Add("x-api-key", _xApiKey);
			var response = await client.PostAsync(_baseUrl + "CuotaUfComision/ObtenerUltimaCuota", new StringContent(JsonConvert.SerializeObject(entradaSanitizada), Encoding.UTF8, "application/json"));
			string responseString = await response.Content.ReadAsStringAsync();

			return JsonConvert.DeserializeObject<List<SalObtenerUltimaCuota>>(responseString)!;
		}

		public async Task<SalCorreoEnviar> EnviarCorreo(string nombrePara, string correoPara, string? nombreResponderA, string? correoResponderA, string asunto, string cuerpo) {
			EntCorreoEnviar entradaSanitizada = new() {
				De = _direccionDeDefecto,
				Para = [
					new EntCorreoDireccion {
						Nombre = nombrePara,
						Correo = correoPara,
					}
				],
				Asunto = asunto,
				Cuerpo = cuerpo,
			};

			if (correoResponderA != null) {
				entradaSanitizada.ResponderA = [
					new EntCorreoDireccion {
						Nombre = nombreResponderA,
						Correo = correoResponderA,
					}
				];
			}

			using HttpClient client = new(new RetryHandler(new HttpClientHandler(), parameterStore));
			client.DefaultRequestHeaders.Add("x-api-key", _hermesApiKey);
			var response = await client.PostAsync(_hermesBaseUrl + "Correo/Enviar", new StringContent(JsonConvert.SerializeObject(entradaSanitizada), Encoding.UTF8, "application/json"));
			string responseString = await response.Content.ReadAsStringAsync();

			if (response.StatusCode != HttpStatusCode.OK) {
				throw new Exception($"StatusCode: {response.StatusCode}");
			}

			return JsonConvert.DeserializeObject<SalCorreoEnviar>(responseString)!;
		}

		public async Task<SalActualizacionMasivaUf> ActualizacionMasivaUf(HashSet<Uf> ufs) {
			EntActualizacionMasivaUf entActMasivUf = new() {
				Ufs = [.. ufs]
			};

			using HttpClient client = new(new RetryHandler(new HttpClientHandler(), parameterStore));
			client.DefaultRequestHeaders.Add("x-api-key", _xApiKey);
			HttpResponseMessage response = await client.PostAsync(_baseUrl + "Uf/ActualizacionMasiva", new StringContent(JsonConvert.SerializeObject(entActMasivUf), Encoding.UTF8, "application/json"));
			if (response.StatusCode != HttpStatusCode.OK) {
				throw new Exception($"Ocurrió un error al actualizar los valores UF. StatusCode: {response.StatusCode} - Content: {await response.Content.ReadAsStringAsync()}");
			}

			return JsonConvert.DeserializeObject<SalActualizacionMasivaUf>(await response.Content.ReadAsStringAsync())!;
		}

		public async Task<SalActualizacionMasivaComision> ActualizacionMasivaComision(List<Comision> comisiones) {
            EntActualizacionMasivaComision entActMasivComision = new() {
                Comisiones = [.. comisiones]
            };

            using HttpClient client = new(new RetryHandler(new HttpClientHandler(), parameterStore));
            client.DefaultRequestHeaders.Add("x-api-key", _xApiKey);
            HttpResponseMessage response = await client.PostAsync(_baseUrl + "Comision/ActualizacionMasiva", new StringContent(JsonConvert.SerializeObject(entActMasivComision), Encoding.UTF8, "application/json"));
            if (response.StatusCode != HttpStatusCode.OK) {
                throw new Exception($"Ocurrió un error al actualizar las comisiones. StatusCode: {response.StatusCode} - Content: {await response.Content.ReadAsStringAsync()}");
            }

			return JsonConvert.DeserializeObject<SalActualizacionMasivaComision>(await response.Content.ReadAsStringAsync())!;
        }

		public async Task<SalActualizacionMasivaCuota> ActualizacionMasivaCuota(List<Cuota> cuotas) {
            EntActualizacionMasivaCuota entActMasivCuota = new() {
                Cuotas = [.. cuotas]
            };

            using HttpClient client = new(new RetryHandler(new HttpClientHandler(), parameterStore));
            client.DefaultRequestHeaders.Add("x-api-key", _xApiKey);
            HttpResponseMessage response = await client.PostAsync(_baseUrl + "Cuota/ActualizacionMasiva", new StringContent(JsonConvert.SerializeObject(entActMasivCuota), Encoding.UTF8, "application/json"));
            if (response.StatusCode != HttpStatusCode.OK) {
                throw new Exception($"Ocurrió un error al actualizar los valores cuota. StatusCode: {response.StatusCode} - Content: {await response.Content.ReadAsStringAsync()}");
            }

			return JsonConvert.DeserializeObject<SalActualizacionMasivaCuota>(await response.Content.ReadAsStringAsync())!;
        }
    }
}
