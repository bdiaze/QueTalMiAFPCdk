﻿using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using QueTalMiAFPCdk.Models.Entities;
using QueTalMiAFPCdk.Models.Others;
using QueTalMiAFPCdk.Models.ViewModels;
using QueTalMiAFPCdk.Services;
using System.Net;
using System.Text;

namespace QueTalMiAFPCdk.Controllers {
	public class MensajesController(ParameterStoreHelper parameterStore, SecretManagerHelper secretManager) : Controller {
		private readonly string _baseUrl = parameterStore.ObtenerParametro("/QueTalMiAFP/Api/Url").Result;
		private readonly string _xApiKey = secretManager.ObtenerSecreto("/QueTalMiAFP").Result.ApiKey;
		private readonly string _reCaptchaClientKey = parameterStore.ObtenerParametro("/QueTalMiAFP/GoogleRecaptcha/ClientKey").Result;

        public async Task<IActionResult> Index() {
			using HttpClient client = new(new RetryHandler(new HttpClientHandler(), parameterStore));
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

            using HttpClient client = new(new RetryHandler(new HttpClientHandler(), parameterStore));
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
			
			EnvioCorreo envioCorreo = new(parameterStore, secretManager);
			await envioCorreo.Notificar($"¡Ha llegado un mensaje de {mensajeResultado.Nombre}!", body, mensajeResultado.Correo, mensajeResultado.Nombre);

			return View(mensajeResultado);
		}
	}
}
