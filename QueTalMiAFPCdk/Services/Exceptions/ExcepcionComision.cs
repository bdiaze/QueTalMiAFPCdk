using System;
using System.Globalization;

namespace QueTalMiAFPCdk.Services.Exceptions {
	public class ExcepcionComision(Exception ex) : Exception(null, ex) {
        public string? Afp { get; set; }
		public DateTime MesAnno { get; set; }

		public override string ToString() {
			return string.Format("Error al extraer comisiones de {0} - Mes/Año {1}:\n{2}",
				Afp ?? "todas las AFPs",
				MesAnno.ToString("MM/yyyy", CultureInfo.InvariantCulture),
				InnerException!.Message);
		}

	}
}
