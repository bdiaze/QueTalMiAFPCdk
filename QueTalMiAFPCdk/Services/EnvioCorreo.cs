using Google.Apis.Auth.OAuth2;
using Google.Apis.Gmail.v1;
using Google.Apis.Gmail.v1.Data;
using Google.Apis.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using MimeKit;
using QueTalMiAFP.Models.Others;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Mail;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace QueTalMiAFP.Services {
	public class EnvioCorreo {
		private readonly string _direccion;
		private readonly string _direccionAlias;
		private readonly string _nombre;
		private readonly string _direccionNotificacion;
		private readonly string _nombreNotificacion;
		private readonly string _servAccountClientEmail;
		private readonly string _servAccountPrivateKey;

		public EnvioCorreo(IConfiguration configuration) {
			_direccion = configuration.GetValue<string>("Correo:Direccion");
			_direccionAlias = configuration.GetValue<string>("Correo:DireccionAlias");
			_nombre = configuration.GetValue<string>("Correo:Nombre");
			_direccionNotificacion = configuration.GetValue<string>("Correo:DireccionNotificacion");
			_nombreNotificacion = configuration.GetValue<string>("Correo:NombreNotificacion");
			_servAccountClientEmail = configuration.GetValue<string>("Correo:ServiceAccount:client_email");
			_servAccountPrivateKey = configuration.GetValue<string>("Correo:ServiceAccount:private_key");
		}

		public static string ArmarCuerpo(Dictionary<string, string> datos, string plantilla) {
			string pathTemplate = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "TemplatesCorreos", plantilla);
			string[] lineas = File.ReadAllLines(pathTemplate);

			StringBuilder builder = new StringBuilder();
			foreach (string linea in lineas) {
				builder.Append(linea);
			}

			string salida = builder.ToString();
			foreach(string key in datos.Keys) {
				salida = salida.Replace(key, datos.GetValueOrDefault(key));
			}
			return salida;
		}

		public async Task Notificar(string subject, string body, string responderADireccion = null, string responderANombre = null) {
			ServiceAccountCredential credential = new ServiceAccountCredential(
				new ServiceAccountCredential.Initializer(_servAccountClientEmail) {
					User = _direccion,
					Scopes = new[] { GmailService.Scope.GmailSend }
				}.FromPrivateKey(_servAccountPrivateKey)
			);

			bool gotAccessToken = await credential.RequestAccessTokenAsync(CancellationToken.None);
			if (!gotAccessToken) {
				throw new ExceptionQueTalMiAFP("No se logró obtener token de acceso para autenticación de credenciales de Google.");
			}

			GmailService service = new GmailService(
				new BaseClientService.Initializer() {
					HttpClientInitializer = credential
				}
			);

			MailAddress fromAddress = new MailAddress(_direccionAlias, _nombre, Encoding.UTF8);
			MailAddress toAddress = new MailAddress(_direccionNotificacion, _nombreNotificacion, Encoding.UTF8);
			MailMessage message = new MailMessage(fromAddress, toAddress) { 
				Subject = subject,
				Body = body,
				SubjectEncoding = Encoding.UTF8,
				HeadersEncoding = Encoding.UTF8,
				BodyEncoding = Encoding.UTF8,
				IsBodyHtml = true
			};

			if (responderADireccion != null) {
				MailAddress replyToAddress = new MailAddress(
					responderADireccion,
					responderANombre != null ? responderANombre : responderADireccion,
					Encoding.UTF8
				);

				message.ReplyToList.Add(replyToAddress);
			}

			MimeMessage mimeMessage = MimeMessage.CreateFromMailMessage(message);
			MemoryStream stream = new MemoryStream();
			await mimeMessage.WriteToAsync(stream);

			string rawMessage = Base64UrlEncode(stream.ToArray());
			await service.Users.Messages.Send(new Message { 
				Raw = rawMessage
			}, _direccion).ExecuteAsync();
		}
		private static string Base64UrlEncode(byte[] inputBytes) {
			return Convert.ToBase64String(inputBytes)
				.Replace("+", "-")
				.Replace("/", "_")
				.Replace("=", "");
		}
	}
}
