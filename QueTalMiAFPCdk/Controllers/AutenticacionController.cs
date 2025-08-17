using Amazon.Runtime.Internal.Transform;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QueTalMiAFPCdk.Models.Others;
using QueTalMiAFPCdk.Services;
using System.Security.Claims;

namespace QueTalMiAFPCdk.Controllers {
    public class AutenticacionController(ParameterStoreHelper parameterStoreHelper, IWebHostEnvironment environment) : Controller {
        [Authorize]
        [Route("login")]
        public IActionResult Login() {
            string? nameIdentifier = User.FindFirstValue(ClaimTypes.NameIdentifier);
            string? email = User.FindFirstValue(ClaimTypes.Email);
            string? givenName = User.FindFirstValue(ClaimTypes.GivenName);
            string? surname = User.FindFirstValue(ClaimTypes.Surname);

            return RedirectToAction("Index", "Resumen");
        }

        [Route("logout")]
        public async Task<IActionResult> Logout() {
            if (User.Identity != null && User.Identity.IsAuthenticated) {
                await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
                await HttpContext.SignOutAsync(OpenIdConnectDefaults.AuthenticationScheme);

                string cognitoBaseUrl = await parameterStoreHelper.ObtenerParametro("/QueTalMiAFP/Cognito/BaseUrl");
                string userPoolClientId = await parameterStoreHelper.ObtenerParametro("/QueTalMiAFP/Cognito/UserPoolClientId");
                string[] cognitoLogouts = (await parameterStoreHelper.ObtenerParametro("/QueTalMiAFP/Cognito/Logouts")).Split(",");

                string logoutUrl = cognitoLogouts.First(l => !l.Contains("localhost"));
                if (environment.IsDevelopment()) {
                    logoutUrl = cognitoLogouts.First(l => l.Contains("localhost"));
                }

                return Redirect($"{cognitoBaseUrl}/logout?client_id={userPoolClientId}&logout_uri={logoutUrl}");
            }

            return RedirectToAction("Index", "Resumen");
        }

        [Route("PoliticaPrivacidad")]
        public IActionResult PoliticaPrivacidad() {
            return View();
        }
    }
}
