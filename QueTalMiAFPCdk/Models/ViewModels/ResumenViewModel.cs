using QueTalMiAFPCdk.Models.Entities;
using QueTalMiAFPCdk.Models.Others;

namespace QueTalMiAFPCdk.Models.ViewModels {
    public class ResumenViewModel {
        public required List<ResumenAfp> Resumenes { get; set; }
        public required List<DateTime> Fechas { get; set; }
    }

    public class ResumenAfp {
        public required string Nombre { get; set; }
        public required string UrlLogo { get; set; }
        public int Ancho { get; set; } = 20;
        public int? Alto { get; set; } = null;
        public List<decimal?> ValoresCuota { get; set; } = [];
    }
}
