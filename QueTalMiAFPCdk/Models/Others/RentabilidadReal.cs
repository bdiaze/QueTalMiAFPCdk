using System;

namespace QueTalMiAFPCdk.Models.Others {
	public class RentabilidadReal {
		public required string Afp { get; set; }
		public required string Fondo { get; set; }
		public decimal ValorCuotaInicial { get; set; }
		public decimal ValorUfInicial { get; set; }
		public decimal ValorCuotaFinal { get; set; }
		public decimal ValorUfFinal { get; set; }
		public decimal Rentabilidad { get; set; }
	}
}
