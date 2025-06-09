using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;

namespace QueTalMiAFP.Services.Exceptions {
	class ExcepcionValorCuota : Exception {

		public ExcepcionValorCuota(Exception ex) : base(null, ex) { }
		public string Afp { get; set; }
		public DateTime FechaInicio { get; set; }
		public DateTime FechaFinal { get; set; }
		public string Fondo { get; set; }

		public override string ToString() {
			return string.Format("Error al extraer valores cuota de {0} - Fondo {1} - Fecha Inicio {2} - Fecha Final {3}:\n{4}", 
				Afp,
				Fondo != null ? Fondo : "*",
				FechaInicio.ToString("dd/MM/yyyy"),
				FechaFinal.ToString("dd/MM/yyyy"),
				InnerException.Message);
		}
	}
}
