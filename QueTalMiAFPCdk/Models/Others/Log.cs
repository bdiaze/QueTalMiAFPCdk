using System;

namespace QueTalMiAFPCdk.Models.Others {
	public class Log {
		public DateTime Fecha { get; set; }
		public required string Mensaje { get; set; }
		public int Tipo { get; set; }
	}
}
