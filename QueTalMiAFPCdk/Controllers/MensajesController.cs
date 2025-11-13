using Amazon.APIGateway.Model;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using QueTalMiAFPCdk.Models.Entities;
using QueTalMiAFPCdk.Models.Others;
using QueTalMiAFPCdk.Models.ViewModels;
using QueTalMiAFPCdk.Repositories;
using QueTalMiAFPCdk.Services;
using System;
using System.Diagnostics;
using System.Globalization;
using System.Net;
using System.Text;

namespace QueTalMiAFPCdk.Controllers {
	public class MensajesController(ILogger<MensajesController> logger, ParameterStoreHelper parameterStore, MensajeDAO mensajeDAO, EnvioCorreo envioCorreo) : Controller {
        private readonly string _reCaptchaClientKey = parameterStore.ObtenerParametro("/QueTalMiAFP/GoogleRecaptcha/ClientKey").Result;

        public async Task<IActionResult> Index() {
            Stopwatch stopwatch = Stopwatch.StartNew();

            ViewBag.TiposMensaje = await mensajeDAO.ObtenerTiposVigentes();
            long elapsedTimeTiposMensajes = stopwatch.ElapsedMilliseconds;

			ViewBag.reCaptchaClientKey = _reCaptchaClientKey;

            logger.LogInformation(
                "[{Method}] - [{Controller}] - [{Action}] - [{ElapsedTime} ms] - [{StatusCode}] - [Usuario Autenticado: {IsAuthenticated}] - " +
                "Se retorna exitosamente la página de mensajes - " +
                "Elapsed Time Tipos Mensajes: {ElapsedTimeTiposMensajes}.",
                HttpContext.Request.Method.Replace(Environment.NewLine, " "), ControllerContext.ActionDescriptor.ControllerName, ControllerContext.ActionDescriptor.ActionName,
                stopwatch.ElapsedMilliseconds, StatusCodes.Status200OK, User.Identity?.IsAuthenticated ?? false,
                elapsedTimeTiposMensajes);

            return View();
		}

		[HttpPost]
		public async Task<IActionResult> IngresarMensaje(IngresarMensajeViewModel model) {
            Stopwatch stopwatch = Stopwatch.StartNew();

            if (!ModelState.IsValid) {
				throw new ExceptionQueTalMiAFP(ModelState.Values
					.SelectMany(v => v.Errors)
					.Select(e => e.ErrorMessage)
					.First());
			}

            MensajeUsuario? mensajeResultado = await mensajeDAO.IngresarMensaje(model.IdTipoMensaje, model.Nombre, model.Correo, model.Mensaje);

			long elapsedTimeIngresoMensaje = stopwatch.ElapsedMilliseconds;


			// Se formatea fecha de ingreso según timezone de America/Santiago para el envío de correo...
			TimeZoneInfo timeZoneInfoCorreo = TimeZoneInfo.FindSystemTimeZoneById("America/Santiago");
			DateTime localTimeCorreo = TimeZoneInfo.ConvertTimeFromUtc(mensajeResultado.FechaIngreso.UtcDateTime, timeZoneInfoCorreo);

			Dictionary<string, string> datos = new() {
                { "[IdMensaje]", WebUtility.HtmlEncode(mensajeResultado!.IdMensaje.ToString()) },
                { "[FechaIngreso]", WebUtility.HtmlEncode(localTimeCorreo.ToString("dd-MM-yyyy HH:mm:ss", CultureInfo.InvariantCulture)) },
                { "[Nombre]", WebUtility.HtmlEncode(mensajeResultado!.Nombre) },
                { "[Correo]", WebUtility.HtmlEncode(mensajeResultado!.Correo) },
                { "[Mensaje]", WebUtility.HtmlEncode(mensajeResultado!.Mensaje) },
                { "[TipoMensaje]", WebUtility.HtmlEncode(mensajeResultado!.TipoMensaje!.DescripcionLarga) }
            };
			string body = EnvioCorreo.ArmarCuerpo(datos, "MensajeRecibido.html");
			
			await envioCorreo.Notificar($"¡Ha llegado un mensaje de {mensajeResultado.Nombre}!", body, mensajeResultado.Correo, mensajeResultado.Nombre);
            long elapsedTimeEnvioCorreo = stopwatch.ElapsedMilliseconds;

			// Se formatea fecha de ingreso según timezone de cliente para ser mostrado en pantalla...
			string? timezoneCliente = Request.Cookies["Timezone"];
            if (timezoneCliente != null) {
                TimeZoneInfo timeZoneInfoCliente;
				try {
					timeZoneInfoCliente = TimeZoneInfo.FindSystemTimeZoneById(timezoneCliente);
				} catch (TimeZoneNotFoundException) {
					timeZoneInfoCliente = TimeZoneInfo.Utc;
				} catch (InvalidTimeZoneException) {
					timeZoneInfoCliente = TimeZoneInfo.Utc;
				}
				mensajeResultado.FechaIngreso = new DateTimeOffset(TimeZoneInfo.ConvertTimeFromUtc(mensajeResultado.FechaIngreso.UtcDateTime, timeZoneInfoCliente));
			}


			logger.LogInformation(
                "[{Method}] - [{Controller}] - [{Action}] - [{ElapsedTime} ms] - [{StatusCode}] - [Usuario Autenticado: {IsAuthenticated}] - " +
                "Se ingresa exitosamente el mensaje - " +
                "Elapsed Time Ingreso Mensaje: {ElapsedTimeIngresoMensaje} - Elapsed Time Envío Correo: {ElapsedTimeEnvioCorreo}.",
                HttpContext.Request.Method.Replace(Environment.NewLine, " "), ControllerContext.ActionDescriptor.ControllerName, ControllerContext.ActionDescriptor.ActionName,
                stopwatch.ElapsedMilliseconds, StatusCodes.Status200OK, User.Identity?.IsAuthenticated ?? false,
                elapsedTimeIngresoMensaje, elapsedTimeEnvioCorreo);

            return View(mensajeResultado);
		}
	}
}
