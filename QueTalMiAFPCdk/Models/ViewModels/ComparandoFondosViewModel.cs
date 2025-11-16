using System.Security.Policy;

namespace QueTalMiAFPCdk.Models.ViewModels {
	public class ComparandoFondosViewModel {
		public required DateOnly UltimaFechaAlgunValorCuota { get; set; }
		public required string[] GraficosAbiertos { get; set; }
		public List<RentabilidadPorRango> Rentabilidades { get; set; } = [];
	}

	public class RentabilidadPorRango {
		public required string TipoRango { get; set; }
		public required DateOnly FechaDesde { get; set; }
		public required DateOnly FechaHasta { get; set; }

		// Dictionary<AFP, Dictionary<FONDO, RENTABILIDAD>>
		public Dictionary<string, Dictionary<string, decimal?>> Rentabilidades { get; set; } = new Dictionary<string, Dictionary<string, decimal?>> {
			{ "CAPITAL", new Dictionary<string, decimal?> {
				{ "A", null },
				{ "B", null },
				{ "C", null },
				{ "D", null },
				{ "E", null },
			} },
			{ "CUPRUM", new Dictionary<string, decimal?> {
				{ "A", null },
				{ "B", null },
				{ "C", null },
				{ "D", null },
				{ "E", null },
			} },
			{ "HABITAT", new Dictionary<string, decimal?> {
				{ "A", null },
				{ "B", null },
				{ "C", null },
				{ "D", null },
				{ "E", null },
			} },
			{ "MODELO", new Dictionary<string, decimal?> {
				{ "A", null },
				{ "B", null },
				{ "C", null },
				{ "D", null },
				{ "E", null },
			} },
			{ "PLANVITAL", new Dictionary<string, decimal?> {
				{ "A", null },
				{ "B", null },
				{ "C", null },
				{ "D", null },
				{ "E", null },
			} },
			{ "PROVIDA", new Dictionary<string, decimal?> {
				{ "A", null },
				{ "B", null },
				{ "C", null },
				{ "D", null },
				{ "E", null },
			} },
			{ "UNO", new Dictionary<string, decimal?> {
				{ "A", null },
				{ "B", null },
				{ "C", null },
				{ "D", null },
				{ "E", null },
			} },
		};

		// Dictionary<AFP, Dictionary<FONDO, RENTABILIDAD>>
		public Dictionary<string, Dictionary<string, decimal?>> RentabilidadesReales { get; set; } = new Dictionary<string, Dictionary<string, decimal?>> {
			{ "CAPITAL", new Dictionary<string, decimal?> {
				{ "A", null },
				{ "B", null },
				{ "C", null },
				{ "D", null },
				{ "E", null },
			} },
			{ "CUPRUM", new Dictionary<string, decimal?> {
				{ "A", null },
				{ "B", null },
				{ "C", null },
				{ "D", null },
				{ "E", null },
			} },
			{ "HABITAT", new Dictionary<string, decimal?> {
				{ "A", null },
				{ "B", null },
				{ "C", null },
				{ "D", null },
				{ "E", null },
			} },
			{ "MODELO", new Dictionary<string, decimal?> {
				{ "A", null },
				{ "B", null },
				{ "C", null },
				{ "D", null },
				{ "E", null },
			} },
			{ "PLANVITAL", new Dictionary<string, decimal?> {
				{ "A", null },
				{ "B", null },
				{ "C", null },
				{ "D", null },
				{ "E", null },
			} },
			{ "PROVIDA", new Dictionary<string, decimal?> {
				{ "A", null },
				{ "B", null },
				{ "C", null },
				{ "D", null },
				{ "E", null },
			} },
			{ "UNO", new Dictionary<string, decimal?> {
				{ "A", null },
				{ "B", null },
				{ "C", null },
				{ "D", null },
				{ "E", null },
			} },
		};
	}
}
