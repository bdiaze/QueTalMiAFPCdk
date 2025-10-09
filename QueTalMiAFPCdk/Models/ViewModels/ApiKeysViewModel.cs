using QueTalMiAFPCdk.Models.Entities;

namespace QueTalMiAFPCdk.Models.ViewModels {
    public class ApiKeysViewModel {
        public required List<ApiKey> ApiKeys { get; set; }

        public string? Accion { get; set; }

        public long? IdRevocacion { get; set; }

        public string? ApiKeyValue { get; set; }
    }
}
