using Google.Apis.Auth.OAuth2;
using Google.Apis.Gmail.v1;
using Google.Apis.Gmail.v1.Data;
using Google.Apis.Services;
using MimeKit;
using QueTalMiAFPCdk.Models.Others;
using QueTalMiAFPCdk.Repositories;
using System.Net.Mail;
using System.Reflection;
using System.Text;

namespace QueTalMiAFPCdk.Services {
	public class EnvioCorreo(ParameterStoreHelper parameterStore, ICuotaUfComisionDAO cuotaUfComisionDAO) {
		private readonly string _direccionNotificacion = parameterStore.ObtenerParametro("/QueTalMiAFP/Email/DireccionNotificacion").Result;
		private readonly string _nombreNotificacion = parameterStore.ObtenerParametro("/QueTalMiAFP/Email/NombreNotificacion").Result;

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
			SalCorreoEnviar salida = await cuotaUfComisionDAO.EnviarCorreo(
				_nombreNotificacion, 
				_direccionNotificacion,
				responderANombre,
                responderADireccion,
                subject,
                body
            );

			if (salida.CodigoEstado != 200) {
				throw new Exception("Ocurrió un error al enviar correo informando el mensaje de un usuario mediante el formulario de contacto.");
			}
        }
	}
}
