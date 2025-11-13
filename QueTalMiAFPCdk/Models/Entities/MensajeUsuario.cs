using System;

namespace QueTalMiAFPCdk.Models.Entities {
	public class MensajeUsuario {
		public long IdMensaje { get; set; }

		public short IdTipoMensaje { get; set; }

		public DateTimeOffset FechaIngreso { get; set; }

		public required string Nombre { get; set; }

		public required string Correo { get; set; }

		public required string Mensaje { get; set; }

		public TipoMensaje? TipoMensaje { get; set; }
	}
}
