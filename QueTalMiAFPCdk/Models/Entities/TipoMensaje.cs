using System.Collections.Generic;

namespace QueTalMiAFP.Models.Entities {
	public class TipoMensaje {
		public short IdTipoMensaje { get; set; }

		public string DescripcionCorta { get; set; }

		public string DescripcionLarga { get; set; }

		public byte Vigencia { get; set; }

		public ICollection<MensajeUsuario> MensajeUsuarios { get; set; }
	}
}
