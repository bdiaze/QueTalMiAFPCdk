﻿using System;

namespace QueTalMiAFPCdk.Models.Entities {
	public class CuotaUfComision {
		public required string Afp { get; set; }

		public DateTime Fecha { get; set; }

		public required string Fondo { get; set; }

		public decimal Valor { get; set; }

		public decimal? ValorUf { get; set; }

		public decimal? ComisDeposCotizOblig { get; set; }

		public decimal? ComisAdminCtaAhoVol { get; set; }
	}
}
