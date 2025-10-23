using Amazon.Runtime.Internal.Transform;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Org.BouncyCastle.Utilities;
using QueTalMiAFPCdk.Models.Others;
using QueTalMiAFPCdk.Services;
using System.Diagnostics;
using System.Security.Claims;

namespace QueTalMiAFPCdk.Controllers {
    public class AutenticacionController(ILogger<AutenticacionController> logger, ParameterStoreHelper parameterStoreHelper, IWebHostEnvironment environment) : Controller {
        [Authorize]
        [Route("login")]
        public IActionResult Login(string? redirect = null) {
            Stopwatch stopwatch = Stopwatch.StartNew();

            string? nameIdentifier = User.FindFirstValue(ClaimTypes.NameIdentifier);
            string? email = User.FindFirstValue(ClaimTypes.Email);
            string? givenName = User.FindFirstValue(ClaimTypes.GivenName);
            string? surname = User.FindFirstValue(ClaimTypes.Surname);

            if (!string.IsNullOrEmpty(redirect)) {

                logger.LogInformation(
                    "[{Method}] - [{Controller}] - [{Action}] - [{ElapsedTime} ms] - [{StatusCode}] - [Usuario Autenticado: {IsAuthenticated}] - " +
                    "Se redirecciona a redirect indicado - " +
                    "Redirect: {Redirect}.",
                    HttpContext.Request.Method, ControllerContext.ActionDescriptor.ControllerName, ControllerContext.ActionDescriptor.ActionName,
                    stopwatch.ElapsedMilliseconds, StatusCodes.Status302Found, User.Identity?.IsAuthenticated ?? false,
                    redirect);

                return Redirect(redirect);
            }

            logger.LogInformation(
                "[{Method}] - [{Controller}] - [{Action}] - [{ElapsedTime} ms] - [{StatusCode}] - [Usuario Autenticado: {IsAuthenticated}] - " +
                "Se redirecciona a página de notificaciones - " +
                "Redirect: {Redirect}.",
                HttpContext.Request.Method, ControllerContext.ActionDescriptor.ControllerName, ControllerContext.ActionDescriptor.ActionName,
                stopwatch.ElapsedMilliseconds, StatusCodes.Status302Found, User.Identity?.IsAuthenticated ?? false,
                redirect);

            return RedirectToAction("Index", "Notificaciones");
        }

        [Route("logout")]
        public async Task<IActionResult> Logout(string? redirect = null) {
            Stopwatch stopwatch = Stopwatch.StartNew();

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

                if (!string.IsNullOrEmpty(redirect)) {
                    TempData["PostLogoutRedirect"] = redirect;
                }

                logger.LogInformation(
                    "[{Method}] - [{Controller}] - [{Action}] - [{ElapsedTime} ms] - [{StatusCode}] - [Usuario Autenticado: {IsAuthenticated}] - " +
                    "Se redirecciona a login de cognito almacenando redirect - " +
                    "Redirect: {Redirect}.",
                    HttpContext.Request.Method, ControllerContext.ActionDescriptor.ControllerName, ControllerContext.ActionDescriptor.ActionName,
                    stopwatch.ElapsedMilliseconds, StatusCodes.Status302Found, User.Identity?.IsAuthenticated ?? false,
                    redirect);

                return Redirect($"{cognitoBaseUrl}/logout?client_id={userPoolClientId}&logout_uri={logoutUrl}");
            }

            if (TempData.TryGetValue("PostLogoutRedirect", out object? value)) {
                string? storedRedirect = value?.ToString();
                if (!string.IsNullOrEmpty(storedRedirect)) {

                    logger.LogInformation(
                        "[{Method}] - [{Controller}] - [{Action}] - [{ElapsedTime} ms] - [{StatusCode}] - [Usuario Autenticado: {IsAuthenticated}] - " +
                        "Se redirecciona a stored redirect - " +
                        "Stored Redirect: {StoredRedirect} - Redirect: {Redirect}.",
                        HttpContext.Request.Method, ControllerContext.ActionDescriptor.ControllerName, ControllerContext.ActionDescriptor.ActionName,
                        stopwatch.ElapsedMilliseconds, StatusCodes.Status302Found, User.Identity?.IsAuthenticated ?? false,
                        storedRedirect, redirect);

                    return Redirect(storedRedirect);
                }
            }

            logger.LogInformation(
                "[{Method}] - [{Controller}] - [{Action}] - [{ElapsedTime} ms] - [{StatusCode}] - [Usuario Autenticado: {IsAuthenticated}] - " +
                "Se redirecciona a página de resumen - " +
                "Redirect: {Redirect}.",
                HttpContext.Request.Method, ControllerContext.ActionDescriptor.ControllerName, ControllerContext.ActionDescriptor.ActionName,
                stopwatch.ElapsedMilliseconds, StatusCodes.Status302Found, User.Identity?.IsAuthenticated ?? false,
                redirect);

            return RedirectToAction("Index", "Resumen");
        }

        [Route("PoliticaPrivacidad")]
        public IActionResult PoliticaPrivacidad() {
            Stopwatch stopwatch = Stopwatch.StartNew();

            logger.LogInformation(
                "[{Method}] - [{Controller}] - [{Action}] - [{ElapsedTime} ms] - [{StatusCode}] - [Usuario Autenticado: {IsAuthenticated}] - " +
                "Se retorna exitosamente la página de política de privacidad.",
                HttpContext.Request.Method, ControllerContext.ActionDescriptor.ControllerName, ControllerContext.ActionDescriptor.ActionName,
                stopwatch.ElapsedMilliseconds, StatusCodes.Status200OK, User.Identity?.IsAuthenticated ?? false);

            return View();
        }
    }
}
