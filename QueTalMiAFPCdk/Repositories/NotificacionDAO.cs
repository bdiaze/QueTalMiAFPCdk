using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.WebUtilities;
using Newtonsoft.Json;
using QueTalMiAFPCdk.Models.Entities;
using QueTalMiAFPCdk.Models.Others;
using QueTalMiAFPCdk.Services;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace QueTalMiAFPCdk.Repositories {
    public class NotificacionDAO(ParameterStoreHelper parameterStore, ApiKeyHelper apiKey) {
        private readonly string _baseUrl = parameterStore.ObtenerParametro("/QueTalMiAFP/Api/Url").Result;
        private readonly string _xApiKey = apiKey.ObtenerApiKey(parameterStore.ObtenerParametro("/QueTalMiAFP/Api/KeyId").Result).Result;

        public async Task<List<TipoNotificacion>> ObtenerTipoNotificaciones(short habilitado = 1) {
            using HttpClient client = new(new RetryHandler(new HttpClientHandler(), parameterStore));
            client.DefaultRequestHeaders.Add("x-api-key", _xApiKey);

            HttpResponseMessage response = await client.GetAsync(_baseUrl + "TipoNotificacion/ObtenerTodas");
            if (!response.IsSuccessStatusCode) {
                throw new Exception($"Ocurrió un error al obtener todos los tipos notificaciones. StatusCode: {response.StatusCode} - Content: {await response.Content.ReadAsStringAsync()}");
            }

            List<TipoNotificacion> retorno = JsonConvert.DeserializeObject<List<TipoNotificacion>>(await response.Content.ReadAsStringAsync())!;
            return [.. retorno.Where(tn => tn.Habilitado == habilitado)];
        }

        public async Task<List<TipoPeriodicidad>> ObtenerTipoPeriodicidades() {
            using HttpClient client = new(new RetryHandler(new HttpClientHandler(), parameterStore));
            client.DefaultRequestHeaders.Add("x-api-key", _xApiKey);

            HttpResponseMessage response = await client.GetAsync(_baseUrl + "TipoPeriodicidad/ObtenerTodas");
            if (!response.IsSuccessStatusCode) {
                throw new Exception($"Ocurrió un error al obtener todos los tipos de periodicidades. StatusCode: {response.StatusCode} - Content: {await response.Content.ReadAsStringAsync()}");
            }

            return JsonConvert.DeserializeObject<List<TipoPeriodicidad>>(await response.Content.ReadAsStringAsync())!;
        }

        public async Task<List<Notificacion>> ObtenerNotificaciones(string sub) {
            using HttpClient client = new(new RetryHandler(new HttpClientHandler(), parameterStore));
            client.DefaultRequestHeaders.Add("x-api-key", _xApiKey);

            Dictionary<string, string?> parameters = new() {
                { "sub", sub },
                { "vigente", "1" },
            };
            HttpResponseMessage response = await client.GetAsync(QueryHelpers.AddQueryString(_baseUrl + "Notificacion/ObtenerPorSub", parameters));
            if (!response.IsSuccessStatusCode) {
                throw new Exception($"Ocurrió un error al obtener las notificaciones del usuario {sub}. StatusCode: {response.StatusCode} - Content: {await response.Content.ReadAsStringAsync()}");
            }

            return JsonConvert.DeserializeObject<List<Notificacion>>(await response.Content.ReadAsStringAsync())!;
        }

        public async Task<Notificacion> IngresarNotificacion(EntNotificacionIngresar entrada) {
            using HttpClient client = new(new RetryHandler(new HttpClientHandler(), parameterStore));
            client.DefaultRequestHeaders.Add("x-api-key", _xApiKey);


            HttpResponseMessage response = await client.PostAsync(_baseUrl + "Notificacion/Ingresar", new StringContent(JsonConvert.SerializeObject(entrada), Encoding.UTF8, "application/json"));
            if (!response.IsSuccessStatusCode) {
                throw new Exception($"Ocurrió un error al ingresar la notificación. StatusCode: {response.StatusCode} - Content: {await response.Content.ReadAsStringAsync()}");
            }

            return JsonConvert.DeserializeObject<Notificacion>(await response.Content.ReadAsStringAsync())!;
        }

        public async Task<Notificacion> ModificarNotificacion(Notificacion entrada) {
            using HttpClient client = new(new RetryHandler(new HttpClientHandler(), parameterStore));
            client.DefaultRequestHeaders.Add("x-api-key", _xApiKey);

            HttpResponseMessage response = await client.PutAsync(_baseUrl + "Notificacion/Modificar", new StringContent(JsonConvert.SerializeObject(entrada), Encoding.UTF8, "application/json"));
            if (!response.IsSuccessStatusCode) {
                throw new Exception($"Ocurrió un error al modificar la notificación. StatusCode: {response.StatusCode} - Content: {await response.Content.ReadAsStringAsync()}");
            }

            return JsonConvert.DeserializeObject<Notificacion>(await response.Content.ReadAsStringAsync())!;
        }
    }
}
