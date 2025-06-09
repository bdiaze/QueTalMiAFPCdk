using System;

namespace QueTalMiAFP.Models.Entities {
	public class MensajeUsuario {
		public long IdMensaje { get; set; }

		public short IdTipoMensaje { get; set; }

		public DateTime FechaIngreso { get; set; }

		public required string Nombre { get; set; }

		public required string Correo { get; set; }

		public required string Mensaje { get; set; }

		public TipoMensaje? TipoMensaje { get; set; }
	}
}
