using QueTalMiAFP.Models.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace QueTalMiAFP.Models.Others {
	public class EntActualizacionMasivaCuota {
		public required List<Cuota> Cuotas { get; set; }
	}
}
