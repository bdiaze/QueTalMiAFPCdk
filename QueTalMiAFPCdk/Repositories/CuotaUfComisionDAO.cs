using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Configuration;
using QueTalMiAFP.Models.Others;
using QueTalMiAFP.Services;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace QueTalMiAFP.Repositories {
	public interface ICuotaUfComisionDAO {
		Task<DateTime> UltimaFechaTodas();
		Task<DateTime> UltimaFechaAlguna();
		Task<DateTime?> UltimaFechaAlgunaConTimeout();
		Task<List<RentabilidadReal>> ObtenerRentabilidadReal(string listaAFPs, string listaFondos, DateTime fechaInicial, DateTime fechaFinal);
	}

	public class CuotaUfComisionDAO : ICuotaUfComisionDAO {
        private readonly IConfiguration _configuration;
		private readonly string _baseUrl;
		private readonly string _xApiKey;
		private readonly int _milisegForzarTimeout;

		public CuotaUfComisionDAO(IConfiguration configuration) {
			_configuration = configuration;

			_baseUrl = _configuration.GetValue<string>("AWSGatewayAPIKey:api-url");
			_xApiKey = _configuration.GetValue<string>("AWSGatewayAPIKey:x-api-key");
			_milisegForzarTimeout = _configuration.GetValue<int>("AWSGatewayAPIKey:milisegundosForzarTimeout");

		}

		public async Task<DateTime> UltimaFechaTodas() {
			using HttpClient client = new HttpClient(new RetryHandler(new HttpClientHandler(), _configuration));
			client.DefaultRequestHeaders.Add("x-api-key", _xApiKey);

			HttpResponseMessage response = await client.GetAsync(_baseUrl + "CuotaUfComision/UltimaFechaTodas");
			string responseString = await response.Content.ReadAsStringAsync();
			responseString = responseString.Replace("\"", "");

			return DateTime.ParseExact(responseString, "s", CultureInfo.InvariantCulture);
		}

		public async Task<DateTime> UltimaFechaAlguna() {
			using HttpClient client = new HttpClient(new RetryHandler(new HttpClientHandler(), _configuration));
			client.DefaultRequestHeaders.Add("x-api-key", _xApiKey);

			HttpResponseMessage response = await client.GetAsync(_baseUrl + "CuotaUfComision/UltimaFechaAlguna");
			string responseString = await response.Content.ReadAsStringAsync();
			responseString = responseString.Replace("\"", "");

			return DateTime.ParseExact(responseString, "s", CultureInfo.InvariantCulture);
		}

		public async Task<DateTime?> UltimaFechaAlgunaConTimeout() {
			using HttpClient client = new HttpClient();
			client.DefaultRequestHeaders.Add("x-api-key", _xApiKey);

			CancellationTokenSource cts = new CancellationTokenSource();
			cts.CancelAfter(_milisegForzarTimeout);
			try {
				HttpResponseMessage response = await client.GetAsync(_baseUrl + "CuotaUfComision/UltimaFechaAlguna", cts.Token);
				string responseString = await response.Content.ReadAsStringAsync();
				responseString = responseString.Replace("\"", "");
				return DateTime.ParseExact(responseString, "s", CultureInfo.InvariantCulture);
			} catch (TaskCanceledException ex) {
				if (!cts.Token.IsCancellationRequested) {
					throw ex;
				}
			}

			return null;
		}

		public async Task<List<RentabilidadReal>> ObtenerRentabilidadReal(string listaAFPs, string listaFondos, DateTime fechaInicial, DateTime fechaFinal) {
			using HttpClient client = new HttpClient(new RetryHandler(new HttpClientHandler(), _configuration));
			client.DefaultRequestHeaders.Add("x-api-key", _xApiKey);

			Dictionary<string, string> parameters = new Dictionary<string, string>() {
				{ "listaAFPs", listaAFPs },
				{ "listaFondos", listaFondos },
				{ "fechaInicial", fechaInicial.ToString("s", CultureInfo.InvariantCulture) },
				{ "fechaFinal", fechaFinal.ToString("s", CultureInfo.InvariantCulture) }
			};

			string requestUri = QueryHelpers.AddQueryString(_baseUrl + "CuotaUfComision/ObtenerRentabilidadReal", parameters);
			HttpResponseMessage response = await client.GetAsync(requestUri);
			using Stream responseStream = await response.Content.ReadAsStreamAsync();
			JsonSerializerOptions options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
			return await JsonSerializer.DeserializeAsync<List<RentabilidadReal>>(responseStream, options);
		}
	}
}
