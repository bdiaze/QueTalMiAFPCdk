using MercadoPago.Client.Preference;
using MercadoPago.Resource.Preference;
using Microsoft.AspNetCore.Mvc;
using QueTalMiAFPCdk.Models.ViewModels;
using QueTalMiAFPCdk.Services;
using System.Diagnostics;

namespace QueTalMiAFPCdk.Controllers {
    public class DonacionController(ILogger<DonacionController> logger, MercadoPagoHelper mercadoPagoHelper) : Controller {
        public IActionResult Index() {
            Stopwatch stopwatch = Stopwatch.StartNew();

            DonacionViewModel model = new() { 
                Monto = "$1.000"
            };
            ViewBag.MontoCargado = false;
            ViewBag.OcultarCompleto = true;

            logger.LogInformation(
                "[{Method}] - [{Controller}] - [{Action}] - [{ElapsedTime} ms] - [{StatusCode}] - [Usuario Autenticado: {IsAuthenticated}] - " +
                "Se retorna exitosamente la página de donación.",
                HttpContext.Request.Method.Replace(Environment.NewLine, " "), ControllerContext.ActionDescriptor.ControllerName, ControllerContext.ActionDescriptor.ActionName,
                stopwatch.ElapsedMilliseconds, StatusCodes.Status200OK, User.Identity?.IsAuthenticated ?? false);

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> EmitiendoPago(DonacionViewModel model) {
            Stopwatch stopwatch = Stopwatch.StartNew();

            // Se le quita el formato al monto ingresado...
            string montoSinFormato = "";
            for (int i = 0; i < model.Monto.Length; i++) {
                if ("0123456789".Contains(model.Monto[i])) {
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

            logger.LogInformation(
                "[{Method}] - [{Controller}] - [{Action}] - [{ElapsedTime} ms] - [{StatusCode}] - [Usuario Autenticado: {IsAuthenticated}] - " +
                "Se retorna exitosamente la página de emisión de pago.",
                HttpContext.Request.Method.Replace(Environment.NewLine, " "), ControllerContext.ActionDescriptor.ControllerName, ControllerContext.ActionDescriptor.ActionName,
                stopwatch.ElapsedMilliseconds, StatusCodes.Status200OK, User.Identity?.IsAuthenticated ?? false);

            return View("Index", model);    
        }

        [Route("[controller]/[action]/{resultado}")]
        public IActionResult RetornoMercadoPago(string resultado) {
            Stopwatch stopwatch = Stopwatch.StartNew();

            ViewBag.Resultado = resultado.ToLower();
            ViewBag.OcultarCompleto = true;

            logger.LogInformation(
                "[{Method}] - [{Controller}] - [{Action}] - [{ElapsedTime} ms] - [{StatusCode}] - [Usuario Autenticado: {IsAuthenticated}] - " +
                "Se retorna exitosamente la página de retorno de mercado pago.",
                HttpContext.Request.Method.Replace(Environment.NewLine, " "), ControllerContext.ActionDescriptor.ControllerName, ControllerContext.ActionDescriptor.ActionName,
                stopwatch.ElapsedMilliseconds, StatusCodes.Status200OK, User.Identity?.IsAuthenticated ?? false);

            return View();
        }
    }
}
