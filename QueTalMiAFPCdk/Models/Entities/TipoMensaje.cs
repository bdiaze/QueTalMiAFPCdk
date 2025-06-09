using System.Collections.Generic;

namespace QueTalMiAFP.Models.Entities {
	public class TipoMensaje {
		public short IdTipoMensaje { get; set; }

		public required string DescripcionCorta { get; set; }

		public required string DescripcionLarga { get; set; }

		public byte Vigencia { get; set; }

		public ICollection<MensajeUsuario>? MensajeUsuarios { get; set; }
	}
}
