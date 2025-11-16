using System;

namespace QueTalMiAFPCdk.Models.Others {
	public class EntObtenerUltimaCuota {
		public required string ListaAFPs { get; set; }
		public required string ListaFondos { get; set; }
		public required string ListaFechas { get; set; }
		public int TipoComision { get; set; }
		// Determina si se retorna la UF del mismo día del valor cuota (true),
		// o si se retorna la UF del día solicitado en el request,
		// a pesar de que el valor cuota puede ser de un día anterior (false)
		public bool UsarUfValorCuota { get; set; } = true;
	}
}
