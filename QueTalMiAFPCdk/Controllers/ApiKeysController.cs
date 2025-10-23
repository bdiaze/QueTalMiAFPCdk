using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using QueTalMiAFPCdk.Models.Entities;
using QueTalMiAFPCdk.Models.ViewModels;
using QueTalMiAFPCdk.Repositories;
using System.Diagnostics;
using System.Security.Claims;

namespace QueTalMiAFPCdk.Controllers {
    [Authorize]
    public class ApiKeysController(ILogger<ApiKeysController> logger, ApiKeyDAO apiKeyDAO) : Controller {

        [HttpGet]
        public async Task<ActionResult> Index() {
            Stopwatch stopwatch = Stopwatch.StartNew();

            string sub = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? throw new Exception("No se logró obtener el user identifier.");

            ApiKeysViewModel model = new() { 
                ApiKeys = await apiKeyDAO.ObtenerPorSub(sub, null)
            };
            long elapsedTimeObtenerPorSub = stopwatch.ElapsedMilliseconds;
            
            model.IdRevocacion = model.ApiKeys.Where(k => k.Vigente == 1).FirstOrDefault()?.Id;

            logger.LogInformation(
                "[{Method}] - [{Controller}] - [{Action}] - [{ElapsedTime} ms] - [{StatusCode}] - [Usuario Autenticado: {IsAuthenticated}] - " +
                "Se retorna exitosamente la página de api keys - " +
                "Elapsed Time Obtener Por Sub: {ElapsedTimeObtenerPorSub}.",
                HttpContext.Request.Method.Replace(Environment.NewLine, " "), ControllerContext.ActionDescriptor.ControllerName, ControllerContext.ActionDescriptor.ActionName,
                stopwatch.ElapsedMilliseconds, StatusCodes.Status200OK, User.Identity?.IsAuthenticated ?? false,
                elapsedTimeObtenerPorSub);

            return View(model);
        }

        [HttpPost]
        public async Task<ActionResult> Index(ApiKeysViewModel modelEntrada) {
            Stopwatch stopwatch = Stopwatch.StartNew();

            string sub = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? throw new Exception("No se logró obtener el user identifier.");

            ApiKeysViewModel model = new() {
                ApiKeys = []
            };

            long? elapsedTimeObtenerPorSub = null;
            long? elapsedTimeEliminar = null;
            long? elapsedTimeCrear = null;
            long? elapsedTimeSegundoPorSub = null;
            // Se trata de revocar la api key, siempre y cuando pertenezca al usuario...
            if (modelEntrada.Accion == "REVOCAR" && modelEntrada.IdRevocacion != null) {
                List<ApiKey> keys = await apiKeyDAO.ObtenerPorSub(sub, null);
                elapsedTimeObtenerPorSub = stopwatch.ElapsedMilliseconds;

                ApiKey? keyARevocar = keys.Where(k => k.Id == modelEntrada.IdRevocacion).FirstOrDefault();
                if (keyARevocar != null && keyARevocar.Vigente == 1) {
                    await apiKeyDAO.Eliminar(keyARevocar.Id);
                    elapsedTimeEliminar = stopwatch.ElapsedMilliseconds;
                }
                model = new() {
                    ApiKeys = await apiKeyDAO.ObtenerPorSub(sub, null),
                };
                elapsedTimeSegundoPorSub = stopwatch.ElapsedMilliseconds;
                
            // Se trata de crear la api key, siempre y cuando no exista una api key vigente...
            } else if (modelEntrada.Accion == "CREAR") {
                ApiKey? keyVigente = (await apiKeyDAO.ObtenerPorSub(sub, 1)).FirstOrDefault();
                elapsedTimeObtenerPorSub = stopwatch.ElapsedMilliseconds;

                string? apiKeyValue = null;
                if (keyVigente == null) {
                    apiKeyValue = await apiKeyDAO.Crear(Guid.NewGuid().ToString(), sub);
                    elapsedTimeCrear = stopwatch.ElapsedMilliseconds;
                }
                model = new() {
                    ApiKeys = await apiKeyDAO.ObtenerPorSub(sub, null),
                };
                elapsedTimeSegundoPorSub = stopwatch.ElapsedMilliseconds;

                model.IdRevocacion = model.ApiKeys.Where(k => k.Vigente == 1).FirstOrDefault()?.Id;
                model.ApiKeyValue = apiKeyValue;
            }

            logger.LogInformation(
                "[{Method}] - [{Controller}] - [{Action}] - [{ElapsedTime} ms] - [{StatusCode}] - [Usuario Autenticado: {IsAuthenticated}] - " +
                "Se retorna exitosamente la página de api keys - " +
                "Accion: {Accion} - IdRevocacion: {IdRevocacion} - " +
                "Elapsed Time Obtener Por Sub: {ElapsedTimeObtenerPorSub} - Elapsed Time Eliminar: {ElapsedTimeEliminar} - " +
                "Elapsed Time Crear: {ElapsedTimeCrear} - Elapsed Time Segundo Por Sub: {ElapsedTimeSegundoPorSub}.",
                HttpContext.Request.Method.Replace(Environment.NewLine, " "), ControllerContext.ActionDescriptor.ControllerName, ControllerContext.ActionDescriptor.ActionName,
                stopwatch.ElapsedMilliseconds, StatusCodes.Status200OK, User.Identity?.IsAuthenticated ?? false,
                modelEntrada.Accion?.Replace(Environment.NewLine, " "), modelEntrada.IdRevocacion,
                elapsedTimeObtenerPorSub, elapsedTimeEliminar, elapsedTimeCrear, elapsedTimeSegundoPorSub);

            return View(model);
        }
    }
}
