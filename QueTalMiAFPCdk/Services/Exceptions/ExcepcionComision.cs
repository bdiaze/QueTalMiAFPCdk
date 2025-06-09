using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace QueTalMiAFP.Services.Exceptions {
	public class ExcepcionComision : Exception {
		public ExcepcionComision(Exception ex) : base(null, ex) { }

		public string Afp { get; set; }
		public DateTime MesAnno { get; set; }

		public override string ToString() {
			return string.Format("Error al extraer comisiones de {0} - Mes/Año {1}:\n{2}",
				Afp ?? "todas las AFPs",
				MesAnno.ToString("MM/yyyy"),
				InnerException.Message);
		}

	}
}
