using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QueTalMiAFPCdk.Models.Entities;
using QueTalMiAFPCdk.Models.Others;
using QueTalMiAFPCdk.Models.ViewModels;
using QueTalMiAFPCdk.Repositories;
using System.Diagnostics;
using System.Security.Claims;

namespace QueTalMiAFPCdk.Controllers {
    [Authorize]
    public class NotificacionesController(ILogger<NotificacionesController> logger, NotificacionDAO notificacionDAO) : Controller {
        [HttpGet]
        public ActionResult Index() {
            Stopwatch stopwatch = Stopwatch.StartNew();

            string? sub = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? throw new Exception("No se logró obtener el user identifier.");

            Task<List<TipoNotificacion>> taskTipoNotificaciones = notificacionDAO.ObtenerTipoNotificaciones();
            Task<List<TipoPeriodicidad>> taskTipoPeriodicidades = notificacionDAO.ObtenerTipoPeriodicidades();
            Task<List<Notificacion>> taskNotificaciones = notificacionDAO.ObtenerNotificaciones(sub);

            Task.WaitAll(taskTipoNotificaciones, taskTipoPeriodicidades, taskNotificaciones);
            long elapsedTimeNotificaciones = stopwatch.ElapsedMilliseconds;

            NotificacionesViewModel model = new() {
                TipoNotificaciones = taskTipoNotificaciones.Result,
                TipoPeriodicidades = taskTipoPeriodicidades.Result,
                Notificaciones = taskNotificaciones.Result,
            };

            foreach (TipoNotificacion tipoNotificacion in model.TipoNotificaciones) {
                model.Seleccion.Add(tipoNotificacion.Id, model.Notificaciones.FirstOrDefault(n => n.IdTipoNotificacion == tipoNotificacion.Id)?.Habilitado ?? 0);
            }

            logger.LogInformation(
                "[{Method}] - [{Controller}] - [{Action}] - [{ElapsedTime} ms] - [{StatusCode}] - [Usuario Autenticado: {IsAuthenticated}] - " +
                "Se retorna exitosamente la página de notificaciones - " +
                "Elapsed Time Notificaciones: {ElapsedTimeNotificaciones}.",
                HttpContext.Request.Method.Replace(Environment.NewLine, " "), ControllerContext.ActionDescriptor.ControllerName, ControllerContext.ActionDescriptor.ActionName,
                stopwatch.ElapsedMilliseconds, StatusCodes.Status200OK, User.Identity?.IsAuthenticated ?? false,
                elapsedTimeNotificaciones);

            return View(model);
        }

        [HttpPost]
        public async Task<ActionResult> Index(NotificacionesViewModel model) {
            Stopwatch stopwatch = Stopwatch.StartNew();

            string? sub = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? throw new Exception("No se logró obtener el user identifier.");
            string? email = User.FindFirstValue(ClaimTypes.Email) ?? throw new Exception("No se logró obtener el user email.");

            List<Notificacion> notificaciones = await notificacionDAO.ObtenerNotificaciones(sub);
            long elapsedTimeNotificaciones = stopwatch.ElapsedMilliseconds;

            foreach (short idTipoNotificacion in model.Seleccion.Keys) {
                Notificacion? notificion = notificaciones.FirstOrDefault(n => n.IdTipoNotificacion == idTipoNotificacion);

                if (notificion == null) {
                    if (model.Seleccion[idTipoNotificacion] == 1) {
                        notificaciones.Add(await notificacionDAO.IngresarNotificacion(new EntNotificacionIngresar() {
                            Sub = sub,
                            CorreoNotificacion = email,
                            IdTipoNotificacion = idTipoNotificacion,
                        }));
                    }
                } else if (notificion.Habilitado != model.Seleccion[idTipoNotificacion]) {
                    notificion.Habilitado = model.Seleccion[idTipoNotificacion];
                    notificion = await notificacionDAO.ModificarNotificacion(notificion);
                }
            }
            long elapsedTimeIngresosModificaciones = stopwatch.ElapsedMilliseconds;

            Task<List<TipoNotificacion>> taskTipoNotificaciones = notificacionDAO.ObtenerTipoNotificaciones();
            Task<List<TipoPeriodicidad>> taskTipoPeriodicidades = notificacionDAO.ObtenerTipoPeriodicidades();
            Task.WaitAll(taskTipoNotificaciones, taskTipoPeriodicidades);
            long elapsedTimeTipos = stopwatch.ElapsedMilliseconds;

            NotificacionesViewModel modelSalida = new() {
                TipoNotificaciones = taskTipoNotificaciones.Result,
                TipoPeriodicidades = taskTipoPeriodicidades.Result,
                Notificaciones = notificaciones,
                GrabadoRecien = true,
            };

            foreach (TipoNotificacion tipoNotificacion in modelSalida.TipoNotificaciones) {
                modelSalida.Seleccion.Add(tipoNotificacion.Id, modelSalida.Notificaciones.FirstOrDefault(n => n.IdTipoNotificacion == tipoNotificacion.Id)?.Habilitado ?? 0);
            }

            logger.LogInformation(
                "[{Method}] - [{Controller}] - [{Action}] - [{ElapsedTime} ms] - [{StatusCode}] - [Usuario Autenticado: {IsAuthenticated}] - " +
                "Se retorna exitosamente la página de notificaciones - " +
                "Selección: {Seleccion} - " +
                "Elapsed Time Notificaciones: {ElapsedTimeNotificaciones} - Elapsed Time Ingresos/Modificaciones: {ElapsedTimeIngresosModificaciones} - Elapsed Time Tipos: {ElapsedTimeTipos}.",
                HttpContext.Request.Method.Replace(Environment.NewLine, " "), ControllerContext.ActionDescriptor.ControllerName, ControllerContext.ActionDescriptor.ActionName,
                stopwatch.ElapsedMilliseconds, StatusCodes.Status200OK, User.Identity?.IsAuthenticated ?? false,
                "[" + string.Join(", ", model.Seleccion.Select(s => $"{{{s.Key}, {s.Value}}}")) + "]",
                elapsedTimeNotificaciones, elapsedTimeIngresosModificaciones, elapsedTimeTipos);

            return View(modelSalida);
        }
    }
}
