using MercadoPago.Client.Preference;
using MercadoPago.Resource.Preference;
using Microsoft.AspNetCore.Mvc;
using QueTalMiAFPCdk.Models.ViewModels;
using QueTalMiAFPCdk.Services;

namespace QueTalMiAFPCdk.Controllers {
    public class DonacionController(MercadoPagoHelper mercadoPagoHelper) : Controller {
        public IActionResult Index() {
            DonacionViewModel model = new() { 
                Monto = "$1.000"
            };
            ViewBag.MontoCargado = false;
            ViewBag.OcultarCompleto = true;
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> EmitiendoPago(DonacionViewModel model) {
            // Se le quita el formato al monto ingresado...
            string montoSinFormato = "";
            for (int i = 0; i < model.Monto.Length; i++) {
                if ("0123456789".IndexOf(model.Monto[i]) >= 0) {
                    montoSinFormato += model.Monto[i];
                } else if (model.Monto[i] == ',') {
                    break;
                }
            }

            PreferenceRequest request = new() {
                Items = [
                    new PreferenceItemRequest {
                        Title = "Donación a ¿Qué tal mi AFP?",
                        Quantity = 1,
                        CurrencyId = "CLP",
                        UnitPrice = decimal.Parse(montoSinFormato)
                    }
                ],
                BackUrls = new PreferenceBackUrlsRequest {
                    Success = mercadoPagoHelper.UrlSuccess,
                    Failure = mercadoPagoHelper.UrlFailure,
                    Pending = mercadoPagoHelper.UrlPending,
                },
                AutoReturn = "approved"
            };

            PreferenceClient client = new();
            Preference preference = await client.CreateAsync(request);

            ViewBag.MercadoPagoPublicKey = mercadoPagoHelper.PublicKey;
            ViewBag.PreferenceId = preference.Id;
            ViewBag.MontoCargado = true;
            ViewBag.OcultarCompleto = true;

            return View("Index", model);    
        }

        [Route("[controller]/[action]/{resultado}")]
        public IActionResult RetornoMercadoPago(string resultado) {
            ViewBag.Resultado = resultado.ToLower();
            ViewBag.OcultarCompleto = true;
            return View();
        }
    }
}
