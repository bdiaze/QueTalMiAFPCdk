using Newtonsoft.Json;
using QueTalMiAFPCdk.Models.Entities;
using QueTalMiAFPCdk.Models.Others;
using QueTalMiAFPCdk.Services;
using System.Net;
using System.Text;

namespace QueTalMiAFPCdk.Repositories {
    public class MensajeDAO(ParameterStoreHelper parameterStore, ApiKeyHelper apiKey) {
        private readonly string _baseUrl = parameterStore.ObtenerParametro("/QueTalMiAFP/Api/Url").Result;
        private readonly string _xApiKey = apiKey.ObtenerApiKey(parameterStore.ObtenerParametro("/QueTalMiAFP/Api/KeyId").Result).Result;

        public async Task<List<TipoMensaje>> ObtenerTiposVigentes() {
            using HttpClient client = new(new RetryHandler(new HttpClientHandler(), parameterStore));
            client.DefaultRequestHeaders.Add("x-api-key", _xApiKey);

            HttpResponseMessage response = await client.GetAsync(_baseUrl + "TipoMensaje/ObtenerVigentes");
            if (response.StatusCode != HttpStatusCode.OK) {
                throw new Exception($"Ocurrió un error al obtener los tipos de mensajes vigentes - StatusCode: {response.StatusCode} - Content: {await response.Content.ReadAsStringAsync()}");
            }

            return JsonConvert.DeserializeObject<List<TipoMensaje>>(await response.Content.ReadAsStringAsync())!;
        }

        public async Task<MensajeUsuario> IngresarMensaje(short idTipoMensaje, string nombre, string correo, string mensaje) {
            EntIngresarMensaje entrada = new() { 
                IdTipoMensaje = idTipoMensaje,
                Nombre = nombre,
                Correo = correo,
                Mensaje = mensaje
            };

            using HttpClient client = new(new RetryHandler(new HttpClientHandler(), parameterStore));
            client.DefaultRequestHeaders.Add("x-api-key", _xApiKey);

            HttpResponseMessage response = await client.PostAsync(_baseUrl + "MensajeUsuario/IngresarMensaje", new StringContent(JsonConvert.SerializeObject(entrada), Encoding.UTF8, "application/json"));
            if (response.StatusCode != HttpStatusCode.OK) {
                throw new Exception($"Ocurrió un error al ingresar el mensaje - StatusCode: {response.StatusCode} - Content: {await response.Content.ReadAsStringAsync()}");
            }

            return JsonConvert.DeserializeObject<MensajeUsuario>(await response.Content.ReadAsStringAsync())!;
        }
    }
}
