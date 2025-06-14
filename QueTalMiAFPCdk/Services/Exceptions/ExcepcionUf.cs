using System;
using System.Collections.Generic;
using System.Text;

namespace QueTalMiAFPCdk.Services.Exceptions {
	class ExcepcionUf(Exception ex) : Exception(null, ex) {
        public DateTime FechaInicio { get; set; }
		public DateTime FechaFinal { get; set; }

		public override string ToString() {
			return string.Format("Error al extraer valores UF - Fecha Inicio {0} - Fecha Final {1}:\n{2}",
				FechaInicio.ToString("dd/MM/yyyy"),
				FechaFinal.ToString("dd/MM/yyyy"),
				InnerException!.Message);
		}
	}
}
