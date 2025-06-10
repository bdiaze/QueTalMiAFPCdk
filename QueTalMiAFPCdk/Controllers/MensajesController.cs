using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using QueTalMiAFP.Models.Entities;
using QueTalMiAFP.Models.Others;
using QueTalMiAFP.Models.ViewModels;
using QueTalMiAFP.Services;
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
	public class MensajesController : Controller {
		private readonly ILogger<MensajesController> _logger;
		private readonly IConfiguration _configuration;

		private readonly string _baseUrl;
		private readonly string _xApiKey;
		private readonly string _reCaptchaClientKey;

		public MensajesController(ILogger<MensajesController> logger, IConfiguration configuration) {
			_logger = logger;
			_configuration = configuration;

			_baseUrl = _configuration.GetValue<string>("AWSGatewayAPIKey:api-url")!;
			_xApiKey = _configuration.GetValue<string>("AWSGatewayAPIKey:x-api-key")!;
			_reCaptchaClientKey = _configuration.GetValue<string>("GoogleReCaptcha:ClientKey")!;
		}

		public async Task<IActionResult> Index() {
			using HttpClient client = new HttpClient(new RetryHandler(new HttpClientHandler(), _configuration));
			client.DefaultRequestHeaders.Add("x-api-key", _xApiKey);
			HttpResponseMessage response = await client.GetAsync(_baseUrl + "TipoMensaje/ObtenerVigentes");
			using Stream responseStream = await response.Content.ReadAsStreamAsync();
			JsonSerializerOptions options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };

			ViewBag.TiposMensaje = await JsonSerializer.DeserializeAsync<List<TipoMensaje>>(responseStream, options);
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

			using HttpClient client = new HttpClient(new RetryHandler(new HttpClientHandler(), _configuration));
			client.DefaultRequestHeaders.Add("x-api-key", _xApiKey);

			model.Nombre = WebUtility.HtmlEncode(model.Nombre);
			model.Correo = WebUtility.HtmlEncode(model.Correo);
			model.Mensaje = WebUtility.HtmlEncode(model.Mensaje);
			string content = JsonConvert.SerializeObject(model);

            HttpResponseMessage response = await client.PostAsync(_baseUrl + "MensajeUsuario/IngresarMensaje", new StringContent(content, Encoding.UTF8, "application/json"));
			string responseString = await response.Content.ReadAsStringAsync();
            MensajeUsuario? mensajeResultado = JsonConvert.DeserializeObject<MensajeUsuario>(responseString);

            Dictionary<string, string> datos = new Dictionary<string, string>();
			datos.Add("[IdMensaje]", mensajeResultado!.IdMensaje.ToString());
			datos.Add("[FechaIngreso]", mensajeResultado!.FechaIngreso.ToString("dd-MM-yyyy HH:mm:ss"));
			datos.Add("[Nombre]", mensajeResultado!.Nombre);
			datos.Add("[Correo]", mensajeResultado!.Correo);
			datos.Add("[Mensaje]", mensajeResultado!.Mensaje);
			datos.Add("[TipoMensaje]", mensajeResultado!.TipoMensaje!.DescripcionLarga);
			string body = EnvioCorreo.ArmarCuerpo(datos, "MensajeRecibido.html");
			
			EnvioCorreo envioCorreo = new EnvioCorreo(_configuration);
			await envioCorreo.Notificar($"¡Ha llegado un mensaje de {mensajeResultado.Nombre}!", body, mensajeResultado.Correo, mensajeResultado.Nombre);

			return View(mensajeResultado);
		}
	}
}
