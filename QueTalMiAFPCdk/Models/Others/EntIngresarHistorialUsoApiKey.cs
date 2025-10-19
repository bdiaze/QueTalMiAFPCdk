namespace QueTalMiAFPCdk.Models.Others {
    public class EntIngresarHistorialUsoApiKey {
        public required string ApiKey { get; set; }
        public required DateTimeOffset FechaUso { get; set; }
        public required string Ruta { get; set; }
        public required string ParametrosEntrada { get; set; }
        public required short CodigoRetorno { get; set; }
        public required int CantRegistrosRetorno { get; set; }
    }
}
