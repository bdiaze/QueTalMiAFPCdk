using System.ComponentModel.DataAnnotations;

namespace QueTalMiAFP.Models.ViewModels {
	public class IngresarMensajeViewModel : GoogleReCaptchaModelBase {
		[Display(Name = "Me gustaría:")]
		[Required(ErrorMessage = "Debes seleccionar un motivo.")]
		public short IdTipoMensaje { get; set; }

		[Display(Name = "Mi nombre es:")]
		[Required(ErrorMessage = "Debes ingresar tu nombre.")]
		public required string Nombre { get; set; }

		[Display(Name = "Mi correo es:")]
		[Required(ErrorMessage = "Debes ingresar tu correo electrónico.")]
		[EmailAddress(ErrorMessage = "El correo electrónico que ingresaste no tiene un formato válido.")]
		public required string Correo { get; set; }

		[Display(Name = "Y quiero decirles que:")]
		[Required(ErrorMessage = "Debes ingresar tu mensaje.")]
		public required string Mensaje { get; set; }
	}
}
