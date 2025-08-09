using Microsoft.AspNetCore.Mvc.Rendering;
using QueTalMiAFPCdk.Models.Entities;
using QueTalMiAFPCdk.Models.Others;
using System.Security.Policy;

namespace QueTalMiAFPCdk.Models.ViewModels {
    public class ResumenViewModel {
        public required List<ResumenAfp> Resumenes { get; set; }

        public List<DateTime>? Fechas { get; set; }

        public HistorialAfp? Historial { get; set; }
    }

    public class ResumenAfp {
        public required string Nombre { get; set; }
        public required string UrlLogo { get; set; }
        public int Ancho { get; set; } = 20;
        public int? Alto { get; set; } = null;

        public Dictionary<string, List<decimal?>> ValoresCuota { get; set; } = new Dictionary<string, List<decimal?>> {
            { "A", [] },
            { "B", [] },
            { "C", [] },
            { "D", [] },
            { "E", [] },
        };
    }

    public class HistorialAfp {
        public string? Afp { get; set; }
        public SelectList? ListaAfps { get; set; }
        public int? Mes { get; set; }
        public SelectList? ListaMeses { get; set; }
        public int? Anno { get; set; }
        public SelectList? ListaAnnos { get; set; }
        public Dictionary<string, List<decimal?>> ValoresCuota { get; set; } = new Dictionary<string, List<decimal?>> {
            { "A", [] },
            { "B", [] },
            { "C", [] },
            { "D", [] },
            { "E", [] },
        };
    }
}
