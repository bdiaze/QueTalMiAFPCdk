using System;

namespace QueTalMiAFPCdk.Models.Others {
	public class SalObtenerUltimaCuota {
		public required string Afp { get; set; }
		public DateOnly Fecha { get; set; }
		public required string Fondo { get; set; }
		public decimal Valor { get; set; }
		public DateOnly? FechaUf { get; set; }
		public decimal? ValorUf { get; set; }
		public decimal? Comision { get; set; }
	}
}
