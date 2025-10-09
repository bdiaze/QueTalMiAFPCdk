using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using QueTalMiAFPCdk.Models.Entities;
using QueTalMiAFPCdk.Models.ViewModels;
using QueTalMiAFPCdk.Repositories;
using System.Security.Claims;

namespace QueTalMiAFPCdk.Controllers {
    [Authorize]
    public class ApiKeysController(ApiKeyDAO apiKeyDAO) : Controller {

        [HttpGet]
        public async Task<ActionResult> Index() {
            string sub = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? throw new Exception("No se logró obtener el user identifier.");

            ApiKeysViewModel model = new() { 
                ApiKeys = await apiKeyDAO.ObtenerPorSub(sub, null)
            };
            model.IdRevocacion = model.ApiKeys.Where(k => k.Vigente == 1).FirstOrDefault()?.Id;

            return View(model);
        }

        [HttpPost]
        public async Task<ActionResult> Index(ApiKeysViewModel modelEntrada) {
            string sub = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? throw new Exception("No se logró obtener el user identifier.");

            ApiKeysViewModel model = new() {
                ApiKeys = []
            };

            // Se trata de revocar la api key, siempre y cuando pertenezca al usuario...
            if (modelEntrada.Accion == "REVOCAR" && modelEntrada.IdRevocacion != null) {
                List<ApiKey> keys = await apiKeyDAO.ObtenerPorSub(sub, null);
                ApiKey? keyARevocar = keys.Where(k => k.Id == modelEntrada.IdRevocacion).FirstOrDefault();
                if (keyARevocar != null && keyARevocar.Vigente == 1) {
                    await apiKeyDAO.Eliminar(keyARevocar.Id);
                }
                model = new() {
                    ApiKeys = await apiKeyDAO.ObtenerPorSub(sub, null),
                };
            // Se trata de crear la api key, siempre y cuando no exista una api key vigente...
            } else if (modelEntrada.Accion == "CREAR") {
                ApiKey? keyVigente = (await apiKeyDAO.ObtenerPorSub(sub, 1)).FirstOrDefault();
                string? apiKeyValue = null;
                if (keyVigente == null) {
                    apiKeyValue = await apiKeyDAO.Crear(Guid.NewGuid().ToString(), sub);
                }
                model = new() {
                    ApiKeys = await apiKeyDAO.ObtenerPorSub(sub, null),
                };
                model.IdRevocacion = model.ApiKeys.Where(k => k.Vigente == 1).FirstOrDefault()?.Id;
                model.ApiKeyValue = apiKeyValue;
            }

            return View(model);
        }
    }
}
