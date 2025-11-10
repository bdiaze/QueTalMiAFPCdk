using Microsoft.AspNetCore.Mvc.Rendering;

namespace QueTalMiAFPCdk.Models.ViewModels {
    public class AccederCuotasViewModel {
        public bool AmbienteDesarrollo { get; set; }

        public HistorialAfp? Historial { get; set; }
    }

    public class HistorialAfp {
        public string? Afp { get; set; }
        public SelectList? ListaAfps { get; set; }
        public int? Mes { get; set; }
        public SelectList? ListaMeses { get; set; }
        public int? Anno { get; set; }
        public SelectList? ListaAnnos { get; set; }
        public string? NombreAfp { get; set; }
        public string? FiltroHistorialFondoSeleccionado { get; set; }
        public Dictionary<string, decimal?> RentabilidadMensual { get; set; } = new Dictionary<string, decimal?> {
            { "A", null },
            { "B", null },
            { "C", null },
            { "D", null },
            { "E", null },
        };
        public Dictionary<string, List<CuotaRentabilidad>> ValoresCuota { get; set; } = new Dictionary<string, List<CuotaRentabilidad>> {
            { "A", [] },
            { "B", [] },
            { "C", [] },
            { "D", [] },
            { "E", [] },
        };
    }

    public class CuotaRentabilidad {
        public decimal? ValorCuota { get; set; }
        public decimal? Rentabilidad { get; set; }
    }
}
