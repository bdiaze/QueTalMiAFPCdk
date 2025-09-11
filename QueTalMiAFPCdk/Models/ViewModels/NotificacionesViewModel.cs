using QueTalMiAFPCdk.Models.Entities;

namespace QueTalMiAFPCdk.Models.ViewModels {
    public class NotificacionesViewModel {
        public List<TipoNotificacion> TipoNotificaciones { get; set; } = [];
        public List<TipoPeriodicidad> TipoPeriodicidades { get; set; } = [];
        public List<Notificacion> Notificaciones { get; set; } = [];
        public Dictionary<short, short> Seleccion { get; set; } = [];
    }
}
