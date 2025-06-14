using Microsoft.AspNetCore.Mvc;

namespace QueTalMiAFPCdk.Controllers {
    public class AccederCuotasController(IWebHostEnvironment environment) : Controller { 
		private readonly IWebHostEnvironment _environment = environment;

        public IActionResult Index() {
            return View(_environment.IsDevelopment());
        }
    }
}