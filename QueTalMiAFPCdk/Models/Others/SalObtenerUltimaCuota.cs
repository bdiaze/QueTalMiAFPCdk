using System;

namespace QueTalMiAFPCdk.Models.Others {
	public class SalObtenerUltimaCuota {
		public required string Afp { get; set; }
		public DateTime Fecha { get; set; }
		public required string Fondo { get; set; }
		public decimal Valor { get; set; }
		public decimal? Comision { get; set; }
	}
}
