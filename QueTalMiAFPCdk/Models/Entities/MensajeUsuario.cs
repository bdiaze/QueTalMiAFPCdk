using System;

namespace QueTalMiAFP.Models.Entities {
	public class MensajeUsuario {
		public long IdMensaje { get; set; }

		public short IdTipoMensaje { get; set; }

		public DateTime FechaIngreso { get; set; }

		public string Nombre { get; set; }

		public string Correo { get; set; }

		public string Mensaje { get; set; }

		public TipoMensaje TipoMensaje { get; set; }
	}
}
