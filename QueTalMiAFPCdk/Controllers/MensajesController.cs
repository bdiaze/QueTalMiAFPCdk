using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using QueTalMiAFP.Models.Entities;
using QueTalMiAFP.Models.Others;
using QueTalMiAFP.Models.ViewModels;
using QueTalMiAFP.Services;
using QueTalMiAFPCdk.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using JsonSerializer = System.Text.Json.JsonSerializer;

namespace QueTalMiAFP.Controllers {
	public class MensajesController(ILogger<MensajesController> logger, ParameterStoreHelper parameterStore, SecretManagerHelper secretManager, IConfiguration configuration) : Controller {
		private readonly ILogger<MensajesController> _logger = logger;
		private readonly IConfiguration _configuration = configuration;

		private readonly string _baseUrl = parameterStore.ObtenerParametro("/QueTalMiAFP/Api/Url").Result;
		private readonly string _xApiKey = secretManager.ObtenerSecreto("/QueTalMiAFP").Result.ApiKey;
		private readonly string _reCaptchaClientKey = parameterStore.ObtenerParametro("/QueTalMiAFP/GoogleRecaptcha/ClientKey").Result;

        public async Task<IActionResult> Index() {
			using HttpClient client = new(new RetryHandler(new HttpClientHandler(), _configuration));
			client.DefaultRequestHeaders.Add("x-api-key", _xApiKey);
			HttpResponseMessage response = await client.GetAsync(_baseUrl + "TipoMensaje/ObtenerVigentes");
			string responseString = await response.Content.ReadAsStringAsync();

			ViewBag.TiposMensaje = JsonConvert.DeserializeObject<List<TipoMensaje>>(responseString);
			ViewBag.reCaptchaClientKey = _reCaptchaClientKey;
			return View();
		}

		[HttpPost]
		public async Task<IActionResult> IngresarMensaje(IngresarMensajeViewModel model) {
			if (!ModelState.IsValid) {
				throw new ExceptionQueTalMiAFP(ModelState.Values
					.SelectMany(v => v.Errors)
					.Select(e => e.ErrorMessage)
					.First());
			}

			IngresarMensajeViewModel modelSanitizado = new() { 
				IdTipoMensaje = model.IdTipoMensaje,
				Nombre = WebUtility.HtmlEncode(model.Nombre),
				Correo = WebUtility.HtmlEncode(model.Correo),
				Mensaje = WebUtility.HtmlEncode(model.Mensaje),
				GoogleReCaptchaResponse = model.GoogleReCaptchaResponse,
			};

            using HttpClient client = new(new RetryHandler(new HttpClientHandler(), _configuration));
			client.DefaultRequestHeaders.Add("x-api-key", _xApiKey);

            HttpResponseMessage response = await client.PostAsync(_baseUrl + "MensajeUsuario/IngresarMensaje", new StringContent(JsonConvert.SerializeObject(modelSanitizado), Encoding.UTF8, "application/json"));
			string responseString = await response.Content.ReadAsStringAsync();
            MensajeUsuario? mensajeResultado = JsonConvert.DeserializeObject<MensajeUsuario>(responseString);

            Dictionary<string, string> datos = new() {
                { "[IdMensaje]", mensajeResultado!.IdMensaje.ToString() },
                { "[FechaIngreso]", mensajeResultado!.FechaIngreso.ToString("dd-MM-yyyy HH:mm:ss") },
                { "[Nombre]", mensajeResultado!.Nombre },
                { "[Correo]", mensajeResultado!.Correo },
                { "[Mensaje]", mensajeResultado!.Mensaje },
                { "[TipoMensaje]", mensajeResultado!.TipoMensaje!.DescripcionLarga }
            };
			string body = EnvioCorreo.ArmarCuerpo(datos, "MensajeRecibido.html");
			
			EnvioCorreo envioCorreo = new(_configuration);
			await envioCorreo.Notificar($"¡Ha llegado un mensaje de {mensajeResultado.Nombre}!", body, mensajeResultado.Correo, mensajeResultado.Nombre);

			return View(mensajeResultado);
		}
	}
}
