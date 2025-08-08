using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using QueTalMiAFPCdk.Models.Entities;
using QueTalMiAFPCdk.Models.Others;
using QueTalMiAFPCdk.Models.ViewModels;
using QueTalMiAFPCdk.Repositories;
using System.Collections.Generic;
using System.Globalization;

namespace QueTalMiAFPCdk.Controllers {
    public class ResumenController(ICuotaUfComisionDAO cuotaUfComisionDAO) : Controller {
        private const int CANTIDAD_DIAS_RESUMEN = 7;

        public async Task<IActionResult> Index(ResumenViewModel modeloEntrada) {
            string? inputAfp = modeloEntrada.Historial?.Afp;
            int? inputMes = modeloEntrada.Historial?.Mes;
            int? inputAnno = modeloEntrada.Historial?.Anno;

            // Se define la fecha hasta como la última fecha en la que se tiene al menos un valor cuota...
            DateTime? fechaHasta = await cuotaUfComisionDAO.UltimaFechaAlguna();
            if (fechaHasta == null) {
                fechaHasta = TimeZoneInfo.ConvertTime(DateTime.Now, TimeZoneConverter.TZConvert.GetTimeZoneInfo("Pacific SA Standard Time"));
            }

            // Se crea listado de fechas que será usado en la cabecera de la tabla de valores cuota...
            List<DateTime> fechas = [fechaHasta.GetValueOrDefault()];
            for (int i = -1; i >= -1 * (CANTIDAD_DIAS_RESUMEN - 1); i--) {
                fechas.Add(fechaHasta.GetValueOrDefault().AddDays(i));
            }

            // Se obtienen los valores cuota de todas las AFP y fondo seleccionado, para el rango de fechas a mostrar en la tabla...
            List<CuotaUf> valoresCuota = await cuotaUfComisionDAO.ObtenerCuotas(
                "CAPITAL,CUPRUM,HABITAT,MODELO,PLANVITAL,PROVIDA,UNO",
                "A,B,C,D,E",
                fechas.Last().AddDays(-7), // Se consulta un rango mayor de dias en caso de necesitar rellenar fines de semana
                fechas.First()
            );

            ResumenViewModel salida = new() {
                Fechas = fechas,
                Resumenes = [
                    new ResumenAfp {
                        Nombre = "Capital",
                        UrlLogo = "/images/logos_afps/LogoAFPCapital.svg"
                    },
                    new ResumenAfp {
                        Nombre = "Cuprum",
                        UrlLogo = "/images/logos_afps/LogoAFPCuprum.svg"
                    },
                    new ResumenAfp {
                        Nombre = "Habitat",
                        UrlLogo = "/images/logos_afps/LogoAFPHabitat.svg"
                    },
                    new ResumenAfp {
                        Nombre = "Modelo",
                        UrlLogo = "/images/logos_afps/LogoAFPModelo.svg"
                    },
                    new ResumenAfp {
                        Nombre = "PlanVital",
                        UrlLogo = "/images/logos_afps/LogoAFPPlanvital.svg",
                        Alto = 10
                    },
                    new ResumenAfp {
                        Nombre = "ProVida",
                        UrlLogo = "/images/logos_afps/LogoAFPProvida.png"
                    },
                    new ResumenAfp {
                        Nombre = "Uno",
                        UrlLogo = "/images/logos_afps/LogoAFPUno.png"
                    },
                ]
            };

            // Se arman las listas de valores cuotas para cada AFP, según el listado de fechas...
            foreach (ResumenAfp resumenAfp in salida.Resumenes) {
                foreach (string fondo in "A,B,C,D,E".Split(",")) {
                    foreach (DateTime fecha in salida.Fechas) {
                        CuotaUf? cuotaUf = valoresCuota.FirstOrDefault(c => c.Afp.Equals(resumenAfp.Nombre, StringComparison.InvariantCultureIgnoreCase) && c.Fondo == fondo && c.Fecha == fecha.ToString("yyyy-MM-dd"));

                        // Si no tenemos valor cuota para la fecha indicada, se marca como null o con la valor cuota anterior.
                        if (cuotaUf == null) {
                            if (resumenAfp.ValoresCuota[fondo].Count > 0 && resumenAfp.ValoresCuota[fondo].Last() != null) {
                                cuotaUf = valoresCuota.Where(c => c.Afp.Equals(resumenAfp.Nombre, StringComparison.InvariantCultureIgnoreCase) && c.Fondo == fondo && DateTime.ParseExact(c.Fecha, "yyyy-MM-dd", CultureInfo.InvariantCulture) <= fecha).OrderByDescending(c => c.Fecha).FirstOrDefault();
                                resumenAfp.ValoresCuota[fondo].Add(cuotaUf?.Valor);
                            } else {
                                resumenAfp.ValoresCuota[fondo].Add(null);
                            }
                        } else {
                            resumenAfp.ValoresCuota[fondo].Add(cuotaUf.Valor);
                        }
                    }
                }
            }

            // Se obtiene el filtro por defecto a mostrar en el resumen...
            string? fondoSeleccionado = Request.Cookies["FiltroResumenFondoSeleccionado"];
            fondoSeleccionado ??= "A";
            ViewBag.FiltroResumenFondoSeleccionado = fondoSeleccionado;

            // Se añade consulta para calendario inferior...
            DateTime fechaActual = TimeZoneInfo.ConvertTime(DateTime.Now, TimeZoneConverter.TZConvert.GetTimeZoneInfo("Pacific SA Standard Time"));
            inputAnno ??= fechaActual.Year;
            inputMes ??= fechaActual.Month;
            inputAfp ??= Request.Cookies["FiltroHistorialAfpSeleccionada"];
            inputAfp ??= "CAPITAL";

            HttpContext.Response.Cookies.Append("FiltroHistorialAfpSeleccionada", inputAfp, new CookieOptions {
                Expires = DateTime.Now.AddDays(365)
            });

            DateTime? fechaDesdeHistorial = null;
            try {
                fechaDesdeHistorial = new DateTime(inputAnno.GetValueOrDefault(), inputMes.GetValueOrDefault(), 1);
            } catch {
                fechaDesdeHistorial = new DateTime(fechaActual.Year, fechaActual.Month, 1);
            }
            DateTime fechaHastaHistorial = fechaDesdeHistorial.GetValueOrDefault().AddMonths(1).AddDays(-1);

            List<CuotaUf> valoresHistorial = await cuotaUfComisionDAO.ObtenerCuotas(
                inputAfp,
                "A,B,C,D,E",
                fechaDesdeHistorial.GetValueOrDefault().AddDays(-7), // Se consulta un rango mayor de dias en caso de necesitar rellenar fines de semana
                fechaHastaHistorial.AddDays(7)
            );

            Dictionary<string, string> listaAfps = new() {
                { "CAPITAL", "Capital" },
                { "CUPRUM", "Cuprum" },
                { "HABITAT", "Habitat" },
                { "MODELO", "Modelo" },
                { "PLANVITAL", "PlanVital" },
                { "PROVIDA", "ProVida" },
                { "UNO", "Uno" },
            };

            Dictionary<int, string> listaMeses = new() {
                { 1, "Enero" },
                { 2, "Febrero" },
                { 3, "Marzo" },
                { 4, "Abril" },
                { 5, "Mayo" },
                { 6, "Junio" },
                { 7, "Julio" },
                { 8, "Agosto" },
                { 9, "Septiembre" },
                { 10, "Octubre" },
                { 11, "Noviembre" },
                { 12, "Diciembre" },
            };

            List<int> listaAnnos = [.. Enumerable.Range(2002, fechaActual.Year + 1 - 2002)];
            listaAnnos.Reverse();

            salida.Historial = new HistorialAfp {
                Afp = inputAfp,
                ListaAfps = new SelectList(listaAfps.Select(l => new SelectListItem { Value = l.Key, Text = l.Value }).ToList(), nameof(SelectListItem.Value), nameof(SelectListItem.Text)),
                Mes = fechaDesdeHistorial.GetValueOrDefault().Month,
                ListaMeses = new SelectList(listaMeses.Select(l => new SelectListItem { Value = l.Key.ToString(), Text = l.Value }).ToList(), nameof(SelectListItem.Value), nameof(SelectListItem.Text)),
                Anno = fechaDesdeHistorial.GetValueOrDefault().Year,
                ListaAnnos = new SelectList(listaAnnos.Select(l => new SelectListItem { Value = l.ToString(), Text = l.ToString()}).ToList(), nameof(SelectListItem.Value), nameof(SelectListItem.Text))
            };
            
            foreach (string fondo in "A,B,C,D,E".Split(",")) {
                DateTime fecha = fechaHastaHistorial;
                while (fecha >= fechaDesdeHistorial) {
                    CuotaUf? cuotaUf = valoresHistorial.Where(c => c.Afp == inputAfp && c.Fondo == fondo && c.Fecha == fecha.ToString("yyyy-MM-dd")).FirstOrDefault();

                    // Si no tenemos valor cuota para la fecha indicada, se marca como null o con la valor cuota anterior.
                    if (cuotaUf == null) {
                        if (valoresHistorial.Any(c => c.Afp == inputAfp && c.Fondo == fondo && DateTime.ParseExact(c.Fecha, "yyyy-MM-dd", CultureInfo.InvariantCulture) > fecha)) {
                            cuotaUf = valoresHistorial.Where(c => c.Afp == inputAfp && c.Fondo == fondo && DateTime.ParseExact(c.Fecha, "yyyy-MM-dd", CultureInfo.InvariantCulture) <= fecha).OrderByDescending(c => c.Fecha).FirstOrDefault();
                            salida.Historial.ValoresCuota[fondo].Add(cuotaUf?.Valor);
                        } else {
                            salida.Historial.ValoresCuota[fondo].Add(null);
                        }
                    } else {
                        salida.Historial.ValoresCuota[fondo].Add(cuotaUf.Valor);
                    }

                    fecha = fecha.AddDays(-1);
                }

                salida.Historial.ValoresCuota[fondo].Reverse();
            }

            // Se obtiene el filtro por defecto a mostrar en el resumen...
            string? fondoHistorialSeleccionado = Request.Cookies["FiltroHistorialFondoSeleccionado"];
            fondoHistorialSeleccionado ??= "A";
            ViewBag.FiltroHistorialFondoSeleccionado = fondoHistorialSeleccionado;

            return View(salida);
        }
    }
}
