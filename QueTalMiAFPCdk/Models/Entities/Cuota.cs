using System;

namespace QueTalMiAFPCdk.Models.Entities {
	public class Cuota {
		public long Id { get; set; }
		
		public string? Afp { get; set; }

		public DateOnly Fecha { get; set; }
		
		public string? Fondo { get; set; }
		
		public decimal Valor { get; set; }

	}
}
