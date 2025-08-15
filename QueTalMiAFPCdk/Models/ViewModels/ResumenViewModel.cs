using Microsoft.AspNetCore.Mvc.Rendering;
using QueTalMiAFPCdk.Models.Entities;
using QueTalMiAFPCdk.Models.Others;
using System.Security.Policy;

namespace QueTalMiAFPCdk.Models.ViewModels {
    public class ResumenViewModel {
        public ResumenSemanal Resumen { get; set; } = new ResumenSemanal();

        public PremiosRentabilidad Premios { get; set; } = new PremiosRentabilidad();
    }

    public class ResumenSemanal {
        public string? FiltroResumenFondoSeleccionado { get; set; }

        public List<DateTime>? FechasUltimaSemana { get; set; }

        public List<UltimaSemanaAfp> UltimaSemana { get; set; } = [];
    }

    public class UltimaSemanaAfp {
        public required string Nombre { get; set; }
        public required string UrlLogo { get; set; }
        public int Ancho { get; set; } = 20;
        public int? Alto { get; set; } = null;

        public Dictionary<string, List<CuotaRentabilidadDia?>> ValoresCuota { get; set; } = new Dictionary<string, List<CuotaRentabilidadDia?>> {
            { "A", [] },
            { "B", [] },
            { "C", [] },
            { "D", [] },
            { "E", [] },
        };
    }

    public class CuotaRentabilidadDia {
        public required decimal ValorCuota { get; set; }
        public required decimal RentabilidadDia { get; set; }
        public required DateTime Fecha { get; set; }
    }

    public class PremiosRentabilidad {
        public DateTime FechasTodas { get; set; }
        public DateTime PrimerDiaMesPremio { get; set; }
        public DateTime UltimoDiaMesPremio { get; set; }
        public decimal PorcMesPremio { get; set; }
        public int? Anno { get; set; }
        public SelectList? ListaAnnos { get; set; }
        public Dictionary<int, GanadorMes> Ganadores { set; get; } = [];
    }

    public class GanadorMes {
        public string? Afp { get; set; }
        public string? Fondo { get; set; }
        public decimal? Rentabilidad { get; set; }
        public DateTime FechaDesde { get; set; }
        public DateTime FechaHasta { get; set; }
    }
}
