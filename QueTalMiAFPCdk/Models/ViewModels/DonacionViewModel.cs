using System.ComponentModel.DataAnnotations;

namespace QueTalMiAFPCdk.Models.ViewModels {
    public class DonacionViewModel {
        [Required(ErrorMessage = "Debes indicar el monto del donativo.")]
        public required string Monto { get; set; }
    }
}
