using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace QueTalMiAFPCdk.Controllers {
	public class ErrorController(ILogger<ErrorController> logger) : Controller {
        public IActionResult Index() {
            Stopwatch stopwatch = Stopwatch.StartNew();

			IExceptionHandlerFeature? exception = HttpContext.Features.Get<IExceptionHandlerFeature>();

            if (exception != null) {
                ViewData["ClaseError"] = exception.Error.GetType().FullName;
                if (ViewData["ClaseError"]?.ToString() == "QueTalMiAFP.Models.Others.ExceptionQueTalMiAFP") {
                    ViewData["Message"] = exception.Error.Message;
                }
            }

            logger.LogInformation(
                "[{Method}] - [{Controller}] - [{Action}] - [{ElapsedTime} ms] - [{StatusCode}] - " +
                "Ocurrió un error no controlado en la aplicación. Autenticado: {IsAutheticated} - " +
                "Path: {Path} - Error: {Error}.",
                HttpContext.Request.Method.Replace(Environment.NewLine, " "), ControllerContext.ActionDescriptor.ControllerName, ControllerContext.ActionDescriptor.ActionName,
                stopwatch.ElapsedMilliseconds, StatusCodes.Status200OK, User.Identity?.IsAuthenticated ?? false,
                exception?.Path.Replace(Environment.NewLine, " "), exception?.Error.ToString().Replace(Environment.NewLine, " "));

            return View();
		}
	}
}
