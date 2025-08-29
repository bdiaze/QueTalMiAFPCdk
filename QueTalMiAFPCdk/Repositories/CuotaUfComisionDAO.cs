using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using QueTalMiAFPCdk.Models.Others;
using QueTalMiAFPCdk.Services;
using System.Globalization;
using System.Net;
using System.Text;
using System.Text.Json;

namespace QueTalMiAFPCdk.Repositories {
	public interface ICuotaUfComisionDAO {
		Task<DateTime> UltimaFechaTodas();
		Task<DateTime> UltimaFechaAlguna();
		Task<DateTime?> UltimaFechaAlgunaConTimeout();
		Task<List<RentabilidadReal>> ObtenerRentabilidadReal(string listaAFPs, string listaFondos, DateTime fechaInicial, DateTime fechaFinal);
		Task<List<CuotaUf>> ObtenerCuotas(string listaAFPs, string listaFondos, DateTime fechaInicial, DateTime fechaFinal);
		Task<List<SalObtenerUltimaCuota>> ObtenerUltimaCuota(string listaAFPs, string listaFondos, string listaFechas, int tipoComision);
        Task<SalCorreoEnviar> EnviarCorreo(string nombrePara, string correoPara, string? nombreResponderA, string? correoResponderA, string asunto, string cuerpo);
    }

	public class CuotaUfComisionDAO(ParameterStoreHelper parameterStore, ApiKeyHelper apiKey, S3BucketHelper s3BucketHelper) : ICuotaUfComisionDAO {
		private readonly string _baseUrl = parameterStore.ObtenerParametro("/QueTalMiAFPAoT/Api/Url").Result;
		private readonly string _xApiKey = apiKey.ObtenerApiKey(parameterStore.ObtenerParametro("/QueTalMiAFPAoT/Api/KeyId").Result).Result;
        private readonly int _milisegForzarTimeout = int.Parse(parameterStore.ObtenerParametro("/QueTalMiAFP/Api/MilisegForzarTimeout").Result);

		private readonly EntCorreoDireccion _direccionDeDefecto = JsonConvert.DeserializeObject<EntCorreoDireccion>(parameterStore.ObtenerParametro("/QueTalMiAFP/SES/DireccionDeDefecto").Result)!;
		private readonly string _hermesBaseUrl = parameterStore.ObtenerParametro("/Hermes/Api/Url").Result;
		private readonly string _hermesApiKey = apiKey.ObtenerApiKey(parameterStore.ObtenerParametro("/Hermes/Api/KeyId").Result).Result;

        public async Task<DateTime> UltimaFechaTodas() {
			using HttpClient client = new(new RetryHandler(new HttpClientHandler(), parameterStore));
			client.DefaultRequestHeaders.Add("x-api-key", _xApiKey);

			HttpResponseMessage response = await client.GetAsync(_baseUrl + "CuotaUfComision/UltimaFechaTodas");
			string responseString = await response.Content.ReadAsStringAsync();
			responseString = responseString.Replace("\"", "");

			return DateTime.ParseExact(responseString, "s", CultureInfo.InvariantCulture);
		}

		public async Task<DateTime> UltimaFechaAlguna() {
			using HttpClient client = new(new RetryHandler(new HttpClientHandler(), parameterStore));
			client.DefaultRequestHeaders.Add("x-api-key", _xApiKey);

			HttpResponseMessage response = await client.GetAsync(_baseUrl + "CuotaUfComision/UltimaFechaAlguna");
			string responseString = await response.Content.ReadAsStringAsync();
			responseString = responseString.Replace("\"", "");

			return DateTime.ParseExact(responseString, "s", CultureInfo.InvariantCulture);
		}

		public async Task<DateTime?> UltimaFechaAlgunaConTimeout() {
			using HttpClient client = new();
			client.DefaultRequestHeaders.Add("x-api-key", _xApiKey);

			CancellationTokenSource cts = new();
			cts.CancelAfter(_milisegForzarTimeout);
			try {
				HttpResponseMessage response = await client.GetAsync(_baseUrl + "CuotaUfComision/UltimaFechaAlguna", cts.Token);
				string responseString = await response.Content.ReadAsStringAsync();
				responseString = responseString.Replace("\"", "");
				return DateTime.ParseExact(responseString, "s", CultureInfo.InvariantCulture);
			} catch (TaskCanceledException) {
				if (!cts.Token.IsCancellationRequested) {
					throw;
				}
			}

			return null;
		}

		public async Task<List<RentabilidadReal>> ObtenerRentabilidadReal(string listaAFPs, string listaFondos, DateTime fechaInicial, DateTime fechaFinal) {
			using HttpClient client = new(new RetryHandler(new HttpClientHandler(), parameterStore));
			client.DefaultRequestHeaders.Add("x-api-key", _xApiKey);

			Dictionary<string, string?> parameters = new() {
				{ "listaAFPs", listaAFPs },
				{ "listaFondos", listaFondos },
				{ "fechaInicial", fechaInicial.ToString("s", CultureInfo.InvariantCulture) },
				{ "fechaFinal", fechaFinal.ToString("s", CultureInfo.InvariantCulture) }
			};

			string requestUri = QueryHelpers.AddQueryString(_baseUrl + "CuotaUfComision/ObtenerRentabilidadReal", parameters);
			HttpResponseMessage response = await client.GetAsync(requestUri);
			using Stream responseStream = await response.Content.ReadAsStreamAsync();
			JsonSerializerOptions options = new() { PropertyNameCaseInsensitive = true };
			return (await System.Text.Json.JsonSerializer.DeserializeAsync<List<RentabilidadReal>>(responseStream, options))!;
		}

		public async Task<List<CuotaUf>> ObtenerCuotas(string listaAFPs, string listaFondos, DateTime fechaInicial, DateTime fechaFinal) {
            Dictionary<string, string?> parameters = new() {
                { "listaAFPs", listaAFPs },
                { "listaFondos", listaFondos },
                { "fechaInicial", fechaInicial.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture) },
                { "fechaFinal", fechaFinal.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture) }
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
    }
}
