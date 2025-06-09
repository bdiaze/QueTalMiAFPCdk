using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace QueTalMiAFP.Controllers {
    public class AccederCuotasController : Controller { 
		private readonly ILogger<AccederCuotasController> _logger;
		private readonly IWebHostEnvironment _environment;

		public AccederCuotasController(ILogger<AccederCuotasController> logger, IWebHostEnvironment environment) {
			_logger = logger;
			_environment = environment;
		}

        public IActionResult Index() {
            return View(_environment.IsDevelopment());
        }
    }
}