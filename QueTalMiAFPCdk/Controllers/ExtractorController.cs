using Amazon.APIGateway.Model;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using QueTalMiAFPCdk.Models.Entities;
using QueTalMiAFPCdk.Models.Others;
using QueTalMiAFPCdk.Repositories;
using QueTalMiAFPCdk.Services;
using System.Diagnostics;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using JsonSerializer = System.Text.Json.JsonSerializer;

namespace QueTalMiAFPCdk.Controllers {
	public class ExtractorController(ILogger<ExtractorController> logger, ParameterStoreHelper parameterStore, SecretManagerHelper secretManager, CuotaUfComisionDAO cuotaUfComisionDAO) : Controller {
		public IActionResult Index() {
            Stopwatch stopwatch = Stopwatch.StartNew();

            logger.LogInformation(
                "[{Method}] - [{Controller}] - [{Action}] - [{ElapsedTime} ms] - [{StatusCode}] - [Usuario Autenticado: {IsAuthenticated}] - " +
                "Se retorna exitosamente la página de extracción.",
                HttpContext.Request.Method, ControllerContext.ActionDescriptor.ControllerName, ControllerContext.ActionDescriptor.ActionName,
                stopwatch.ElapsedMilliseconds, StatusCodes.Status200OK, User.Identity?.IsAuthenticated ?? false);

            return View();
		}

		[HttpPost]
		public async Task<IActionResult> Index(int tipoExtraccion, string llaveExtraccion) {
            Stopwatch stopwatch = Stopwatch.StartNew();

            Extractor extractor = new(parameterStore, secretManager, cuotaUfComisionDAO);
            List<Log> logs = await extractor.ExtraerValores(tipoExtraccion, llaveExtraccion);
			logs.Reverse();

            logger.LogInformation(
                "[{Method}] - [{Controller}] - [{Action}] - [{ElapsedTime} ms] - [{StatusCode}] - [Usuario Autenticado: {IsAuthenticated}] - " +
                "Se retorna exitosamente la página de extracción - " +
                "Cantidad de Logs {CantLogs}.",
                HttpContext.Request.Method, ControllerContext.ActionDescriptor.ControllerName, ControllerContext.ActionDescriptor.ActionName,
                stopwatch.ElapsedMilliseconds, StatusCodes.Status200OK, User.Identity?.IsAuthenticated ?? false,
                logs.Count);

            return View(logs);
		}

		[HttpPost]
		[Route("api/[controller]/[action]")]
		public async Task<List<Log>> ExtraerValores(int tipoExtraccion, string llaveExtraccion) {
            Stopwatch stopwatch = Stopwatch.StartNew();

            Extractor extractor = new(parameterStore, secretManager, cuotaUfComisionDAO);
            List<Log> logs = await extractor.ExtraerValores(tipoExtraccion, llaveExtraccion);

            logger.LogInformation(
                "[{Method}] - [{Controller}] - [{Action}] - [{ElapsedTime} ms] - [{StatusCode}] - [Usuario Autenticado: {IsAuthenticated}] - " +
                "Se retornan exitosamente los logs de la extracción - " +
                "Cantidad de Logs {CantLogs}.",
                HttpContext.Request.Method, ControllerContext.ActionDescriptor.ControllerName, ControllerContext.ActionDescriptor.ActionName,
                stopwatch.ElapsedMilliseconds, StatusCodes.Status200OK, User.Identity?.IsAuthenticated ?? false,
                logs.Count);

            return extractor.Logs;
		}
	}
}
