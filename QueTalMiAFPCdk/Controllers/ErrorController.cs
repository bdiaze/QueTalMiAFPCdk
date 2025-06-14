using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace QueTalMiAFPCdk.Controllers {
	public class ErrorController : Controller {
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
