using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace QueTalMiAFP.Models.Others {
	public class EntObtenerUltimaCuota {
		public required string ListaAFPs { get; set; }
		public required string ListaFondos { get; set; }
		public required string ListaFechas { get; set; }
		public int TipoComision { get; set; }
	}
}
