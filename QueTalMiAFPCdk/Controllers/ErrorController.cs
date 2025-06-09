using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace QueTalMiAFP.Controllers {
	public class ErrorController : Controller {
		private readonly ILogger<ErrorController> _logger;
		private readonly IWebHostEnvironment _environment;

		public ErrorController(ILogger<ErrorController> logger, IWebHostEnvironment environment) {
			_logger = logger;
			_environment = environment;
		}

		public IActionResult Index() {
			IExceptionHandlerFeature? exception = HttpContext.Features.Get<IExceptionHandlerFeature>();
			ViewBag.ClaseError = exception!.Error.GetType().FullName;
			ViewBag.StatusCode = HttpContext.Response.StatusCode;
			ViewBag.Message = exception.Error.Message;
			ViewBag.StackTrace = exception.Error.StackTrace;
			return View();
		}
	}
}
