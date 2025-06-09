using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace QueTalMiAFP.Models.Others {
	public class SalObtenerUltimaCuota {
		public string Afp { get; set; }
		public DateTime Fecha { get; set; }
		public string Fondo { get; set; }
		public decimal Valor { get; set; }
		public decimal? Comision { get; set; }
	}
}
