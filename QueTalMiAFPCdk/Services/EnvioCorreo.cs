using Google.Apis.Auth.OAuth2;
using Google.Apis.Gmail.v1;
using Google.Apis.Gmail.v1.Data;
using Google.Apis.Services;
using MimeKit;
using QueTalMiAFPCdk.Models.Others;
using System.Net.Mail;
using System.Reflection;
using System.Text;

namespace QueTalMiAFPCdk.Services {
	public class EnvioCorreo(ParameterStoreHelper parameterStore, SecretManagerHelper secretManager) {
		private readonly string _direccion = parameterStore.ObtenerParametro("/QueTalMiAFP/Gmail/Direccion").Result;
		private readonly string _direccionAlias = parameterStore.ObtenerParametro("/QueTalMiAFP/Gmail/DireccionAlias").Result;
		private readonly string _nombre = parameterStore.ObtenerParametro("/QueTalMiAFP/Gmail/Nombre").Result;
		private readonly string _direccionNotificacion = parameterStore.ObtenerParametro("/QueTalMiAFP/Gmail/DireccionNotificacion").Result;
		private readonly string _nombreNotificacion = parameterStore.ObtenerParametro("/QueTalMiAFP/Gmail/NombreNotificacion").Result;
		private readonly string _servAccountClientEmail = parameterStore.ObtenerParametro("/QueTalMiAFP/Gmail/ClientEmail").Result;
		private readonly string _servAccountPrivateKey = secretManager.ObtenerSecreto("/QueTalMiAFP").Result.GmailPrivateKey;

        public static string ArmarCuerpo(Dictionary<string, string> datos, string plantilla) {
			string pathTemplate = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)!, "TemplatesCorreos", plantilla);
			string[] lineas = File.ReadAllLines(pathTemplate);

			StringBuilder builder = new();
			foreach (string linea in lineas) {
				builder.Append(linea);
			}

			string salida = builder.ToString();
			foreach(string key in datos.Keys) {
				salida = salida.Replace(key, datos.GetValueOrDefault(key));
			}
			return salida;
		}

		public async Task Notificar(string subject, string body, string? responderADireccion = null, string? responderANombre = null) {
			ServiceAccountCredential credential = new(
				new ServiceAccountCredential.Initializer(_servAccountClientEmail) {
					User = _direccion,
					Scopes = [GmailService.Scope.GmailSend]
				}.FromPrivateKey(_servAccountPrivateKey)
			);

			bool gotAccessToken = await credential.RequestAccessTokenAsync(CancellationToken.None);
			if (!gotAccessToken) {
				throw new ExceptionQueTalMiAFP("No se logró obtener token de acceso para autenticación de credenciales de Google.");
			}

			GmailService service = new(
				new BaseClientService.Initializer() {
					HttpClientInitializer = credential
				}
			);

			MailAddress fromAddress = new(_direccionAlias, _nombre, Encoding.UTF8);
			MailAddress toAddress = new(_direccionNotificacion, _nombreNotificacion, Encoding.UTF8);
			MailMessage message = new(fromAddress, toAddress) { 
				Subject = subject,
				Body = body,
				SubjectEncoding = Encoding.UTF8,
				HeadersEncoding = Encoding.UTF8,
				BodyEncoding = Encoding.UTF8,
				IsBodyHtml = true
			};

			if (responderADireccion != null) {
				MailAddress replyToAddress = new(
					responderADireccion,
                    responderANombre ?? responderADireccion,
					Encoding.UTF8
				);

				message.ReplyToList.Add(replyToAddress);
			}

			MimeMessage mimeMessage = MimeMessage.CreateFromMailMessage(message);
			using MemoryStream stream = new();
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
