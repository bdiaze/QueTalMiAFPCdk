using Microsoft.AspNetCore.WebUtilities;
using Newtonsoft.Json;
using QueTalMiAFPCdk.Models.Entities;
using QueTalMiAFPCdk.Models.Others;
using QueTalMiAFPCdk.Services;
using System.Globalization;
using System.Net;
using System.Text;

namespace QueTalMiAFPCdk.Repositories {
    public class ApiKeyDAO(IHostEnvironment environment, IConfiguration configuration, ParameterStoreHelper parameterStore, ApiKeyHelper apiKey) {
        private readonly string _baseUrl = environment.IsProduction() ? parameterStore.ObtenerParametro("/QueTalMiAFP/Api/Url").Result : configuration.GetValue<string>("ApiUrl")!;
        private readonly string _xApiKey = apiKey.ObtenerApiKey(parameterStore.ObtenerParametro("/QueTalMiAFP/Api/KeyId").Result).Result;

        public async Task<List<ApiKey>> ObtenerPorSub(string sub, short? vigente = 1) {
            Dictionary<string, string?> parameters = new() {
                { "sub", sub }
            };
            if (vigente != null) {
                parameters.Add("vigente", vigente.Value.ToString(CultureInfo.InvariantCulture));
            }

            using HttpClient client = new(new RetryHandler(new HttpClientHandler(), parameterStore));
            client.DefaultRequestHeaders.Add("x-api-key", _xApiKey);
            string requestUri = QueryHelpers.AddQueryString(_baseUrl + "ApiKey/ObtenerPorSub", parameters);
            HttpResponseMessage response = await client.GetAsync(requestUri);
            if (response.StatusCode != HttpStatusCode.OK) {
                throw new Exception($"Ocurrió un error al obtener por sub las API keys - Sub: {sub} - Vigente: {vigente} - StatusCode: {response.StatusCode} - Content: {await response.Content.ReadAsStringAsync()}");
            }

            return JsonConvert.DeserializeObject<List<ApiKey>>(await response.Content.ReadAsStringAsync())!;
        }

        public async Task<string?> Crear(string requestId, string sub) {
            EntCrearApiKey entradaSanitizada = new() { 
                RequestId = requestId,
                Sub = sub,
            };

            using HttpClient client = new(new RetryHandler(new HttpClientHandler(), parameterStore));
            client.DefaultRequestHeaders.Add("x-api-key", _xApiKey);
            HttpResponseMessage response = await client.PostAsync(_baseUrl + "ApiKey/Crear", new StringContent(JsonConvert.SerializeObject(entradaSanitizada), Encoding.UTF8, "application/json"));
            if (response.StatusCode != HttpStatusCode.OK) {
                throw new Exception($"Ocurrió un error al crear la API key - Request ID: {requestId} - Sub: {sub} - StatusCode: {response.StatusCode} - Content: {await response.Content.ReadAsStringAsync()}");
            }

            SalCrearApiKey salida = JsonConvert.DeserializeObject<SalCrearApiKey>(await response.Content.ReadAsStringAsync())!;
            return salida.ApiKey;
        }

        public async Task<bool> Validar(string apiKey) {
            EntValidarApiKey entradaSanitizada = new() {
                ApiKey = apiKey,
            };

            using HttpClient client = new(new RetryHandler(new HttpClientHandler(), parameterStore));
            client.DefaultRequestHeaders.Add("x-api-key", _xApiKey);

            HttpResponseMessage response = await client.PostAsync(_baseUrl + "ApiKey/Validar", new StringContent(JsonConvert.SerializeObject(entradaSanitizada), Encoding.UTF8, "application/json"));
            if (response.StatusCode != HttpStatusCode.OK && response.StatusCode != HttpStatusCode.Unauthorized) {
                throw new Exception($"Ocurrió un error al validar la API key - StatusCode: {response.StatusCode} - Content: {await response.Content.ReadAsStringAsync()}");
            }

            if (response.StatusCode == HttpStatusCode.OK) {
                return true;
            }
            return false;
        }

        public async Task Eliminar(long id) {
            Dictionary<string, string?> parameters = new() {
                { "id", id.ToString(CultureInfo.InvariantCulture) }
            };

            using HttpClient client = new(new RetryHandler(new HttpClientHandler(), parameterStore));
            client.DefaultRequestHeaders.Add("x-api-key", _xApiKey);
            string requestUri = QueryHelpers.AddQueryString(_baseUrl + "ApiKey/Eliminar", parameters);
            HttpResponseMessage response = await client.DeleteAsync(requestUri);
            if (response.StatusCode != HttpStatusCode.OK) {
                throw new Exception($"Ocurrió un error al eliminar la API key - ID: {id} - StatusCode: {response.StatusCode} - Content: {await response.Content.ReadAsStringAsync()}");
            }
        }

        public async Task IngresarHistorialUso(string apiKey, DateTimeOffset fechaUso, string ruta, string parametrosEntrada, short codigoRetorno, int cantRegistrosRetorno) {
            EntIngresarHistorialUsoApiKey entradaSanitizada = new() {
                ApiKey = apiKey,
                FechaUso = fechaUso,
                Ruta = ruta,
                ParametrosEntrada = parametrosEntrada,
                CodigoRetorno = codigoRetorno,
                CantRegistrosRetorno = cantRegistrosRetorno
            };

            using HttpClient client = new(new RetryHandler(new HttpClientHandler(), parameterStore));
            client.DefaultRequestHeaders.Add("x-api-key", _xApiKey);

            HttpResponseMessage response = await client.PostAsync(_baseUrl + "HistorialUsoApiKey/Ingresar", new StringContent(JsonConvert.SerializeObject(entradaSanitizada), Encoding.UTF8, "application/json"));
            if (response.StatusCode != HttpStatusCode.OK) {
                throw new Exception($"Ocurrió un error al ingresar historial uso de API key - StatusCode: {response.StatusCode} - Content: {await response.Content.ReadAsStringAsync()}");
            }
        }
    }
}
