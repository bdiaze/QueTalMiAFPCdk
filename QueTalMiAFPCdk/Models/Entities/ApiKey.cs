namespace QueTalMiAFPCdk.Models.Entities {
    public class ApiKey {
        public required long Id { get; set; }

        public required string RequestId { get; set; }

        public required string Sub { get; set; }

        public required string ApiKeyPublicId { get; set; }

        public required string ApiKeyHash { get; set; }

        public required DateTimeOffset FechaCreacion { get; set; }

        public DateTimeOffset? FechaEliminacion { get; set; }

        public required short Vigente { get; set; }

        public DateTimeOffset? FechaUltimoUso { get; set; }
    }
}
