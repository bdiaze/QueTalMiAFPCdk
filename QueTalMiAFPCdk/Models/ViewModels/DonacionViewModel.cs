using System.ComponentModel.DataAnnotations;

namespace QueTalMiAFPCdk.Models.ViewModels {
    public class DonacionViewModel {
        [Required(ErrorMessage = "Debes indicar el monto del donativo.")]
        [RegularExpression(@"^\$[1-9]\d{0,2}(.\d{3})*$", ErrorMessage = "Debes indicar el monto del donativo.")]
        public required string Monto { get; set; }
    }
}
