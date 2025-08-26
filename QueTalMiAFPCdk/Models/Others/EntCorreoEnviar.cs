namespace QueTalMiAFPCdk.Models.Others {
    public class EntCorreoEnviar {
        public required EntCorreoDireccion De { get; set; }
        public required List<EntCorreoDireccion> Para { get; set; }
        public List<EntCorreoDireccion>? ResponderA { get; set; }
        public required string Asunto { get; set; }
        public required string Cuerpo { get; set; }
    }

    public class EntCorreoDireccion {
        public string? Nombre { get; set; }
        public required string Correo { get; set; }
    }
}
