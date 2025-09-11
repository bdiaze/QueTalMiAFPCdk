namespace QueTalMiAFPCdk.Models.Entities {
    public class Notificacion {
        public long? Id { get; set; }

        public required string Sub { get; set; }

        public required string CorreoNotificacion { get; set; }

        public required short IdTipoNotificacion { get; set; }

        public required DateTimeOffset FechaCreacion { get; set; }

        public DateTimeOffset? FechaEliminacion { get; set; }

        public required short Vigente { get; set; }

        public required DateTimeOffset FechaHabilitacion { get; set; }

        public DateTimeOffset? FechaDeshabilitacion { get; set; }

        public required short Habilitado { get; set; }
    }
}
