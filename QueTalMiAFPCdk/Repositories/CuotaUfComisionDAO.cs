using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Configuration;
using QueTalMiAFPCdk.Models.Others;
using QueTalMiAFPCdk.Services;
using System.Globalization;
using System.Text.Json;

namespace QueTalMiAFPCdk.Repositories {
	public interface ICuotaUfComisionDAO {
		Task<DateTime> UltimaFechaTodas();
		Task<DateTime> UltimaFechaAlguna();
		Task<DateTime?> UltimaFechaAlgunaConTimeout();
		Task<List<RentabilidadReal>> ObtenerRentabilidadReal(string listaAFPs, string listaFondos, DateTime fechaInicial, DateTime fechaFinal);
	}

	public class CuotaUfComisionDAO(ParameterStoreHelper parameterStore, SecretManagerHelper secretManager) : ICuotaUfComisionDAO {
		private readonly string _baseUrl = parameterStore.ObtenerParametro("/QueTalMiAFP/Api/Url").Result;
		private readonly string _xApiKey = secretManager.ObtenerSecreto("/QueTalMiAFP").Result.ApiKey;
		private readonly int _milisegForzarTimeout = int.Parse(parameterStore.ObtenerParametro("/QueTalMiAFP/Api/MilisegForzarTimeout").Result);

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
			return (await JsonSerializer.DeserializeAsync<List<RentabilidadReal>>(responseStream, options))!;
		}
	}
}
