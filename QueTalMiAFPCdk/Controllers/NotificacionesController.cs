using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QueTalMiAFPCdk.Models.Entities;
using QueTalMiAFPCdk.Models.Others;
using QueTalMiAFPCdk.Models.ViewModels;
using QueTalMiAFPCdk.Repositories;
using System.Security.Claims;

namespace QueTalMiAFPCdk.Controllers {
    [Authorize]
    public class NotificacionesController(NotificacionDAO notificacionDAO) : Controller {
        [HttpGet]
        public async Task<ActionResult> Index() {
            string? sub = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? throw new Exception("No se logró obtener el user identifier.");

            NotificacionesViewModel model = new() {
                TipoNotificaciones = await notificacionDAO.ObtenerTipoNotificaciones(),
                TipoPeriodicidades = await notificacionDAO.ObtenerTipoPeriodicidades(),
                Notificaciones = await notificacionDAO.ObtenerNotificaciones(sub),
            };

            foreach (TipoNotificacion tipoNotificacion in model.TipoNotificaciones) {
                model.Seleccion.Add(tipoNotificacion.Id, model.Notificaciones.FirstOrDefault(n => n.IdTipoNotificacion == tipoNotificacion.Id)?.Habilitado ?? 0);
            }

            return View(model);
        }

        [HttpPost]
        public async Task<ActionResult> Index(NotificacionesViewModel model) {
            string? sub = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? throw new Exception("No se logró obtener el user identifier.");
            string? email = User.FindFirstValue(ClaimTypes.Email) ?? throw new Exception("No se logró obtener el user email.");

            List<Notificacion> notificaciones = await notificacionDAO.ObtenerNotificaciones(sub);
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

            model = new() {
                TipoNotificaciones = await notificacionDAO.ObtenerTipoNotificaciones(),
                TipoPeriodicidades = await notificacionDAO.ObtenerTipoPeriodicidades(),
                Notificaciones = notificaciones,
                GrabadoRecien = true,
            };

            foreach (TipoNotificacion tipoNotificacion in model.TipoNotificaciones) {
                model.Seleccion.Add(tipoNotificacion.Id, model.Notificaciones.FirstOrDefault(n => n.IdTipoNotificacion == tipoNotificacion.Id)?.Habilitado ?? 0);
            }

            return View(model);
        }
    }
}
