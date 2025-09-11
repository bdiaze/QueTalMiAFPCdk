namespace QueTalMiAFPCdk.Models.Entities {
    public class TipoNotificacion {
        public required short Id { get; set; }

        public required string Nombre { get; set; }

        public required string Descripcion { get; set; }

        public required short IdTipoPeriodicidad { get; set; }

        public required short Habilitado { get; set; }

        public string? IdProceso { get; set; }
    }
}
