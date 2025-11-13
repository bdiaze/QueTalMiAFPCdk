using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace QueTalMiAFPCdk.Services.Exceptions {
	class ExcepcionUf(Exception ex) : Exception(null, ex) {
        public DateOnly FechaInicio { get; set; }
		public DateOnly FechaFinal { get; set; }

		public override string ToString() {
			return string.Format("Error al extraer valores UF - Fecha Inicio {0} - Fecha Final {1}:\n{2}",
				FechaInicio.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture),
				FechaFinal.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture),
				InnerException!.Message);
		}
	}
}
