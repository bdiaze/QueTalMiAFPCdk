using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using QueTalMiAFPCdk.Models.Entities;
using QueTalMiAFPCdk.Models.Others;
using QueTalMiAFPCdk.Models.ViewModels;
using QueTalMiAFPCdk.Repositories;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;

namespace QueTalMiAFPCdk.Controllers {
    public class ResumenController(ICuotaUfComisionDAO cuotaUfComisionDAO) : Controller {
        private const int CANTIDAD_DIAS_RESUMEN = 7; 

        private static readonly Dictionary<string, string> LISTA_AFPS = new() {
            { "CAPITAL", "Capital" },
            { "CUPRUM", "Cuprum" },
            { "HABITAT", "Habitat" },
            { "MODELO", "Modelo" },
            { "PLANVITAL", "PlanVital" },
            { "PROVIDA", "ProVida" },
            { "UNO", "Uno" },
        };

        private static readonly Dictionary<int, string> LISTA_MESES = new() {
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

        public async Task<IActionResult> Index(ResumenViewModel modeloEntrada) {
            string? inputAfp = modeloEntrada.Historial?.Afp;
            int? inputMes = modeloEntrada.Historial?.Mes;
            int? inputAnno = modeloEntrada.Historial?.Anno;

            // Se comienza con consulta de última fecha, mediante Task para poder avanzar en las otras tareas...
            Task<DateTime> taskFechaHasta = cuotaUfComisionDAO.UltimaFechaAlguna();

            DateTime fechaActual = TimeZoneInfo.ConvertTime(DateTime.Now, TimeZoneConverter.TZConvert.GetTimeZoneInfo("Pacific SA Standard Time"));

            // Se obtienen los valores cuota de todas las AFP y fondo seleccionado, para el rango de fechas a mostrar en la tabla, mediante Task...
            Task<List<CuotaUf>> taskValoresCuota = cuotaUfComisionDAO.ObtenerCuotas(
                "CAPITAL,CUPRUM,HABITAT,MODELO,PLANVITAL,PROVIDA,UNO",
                "A,B,C,D,E",
                fechaActual.AddDays(-14), // Se consulta un rango mayor de dias en caso de necesitar rellenar fines de semana
                fechaActual
            );

            // Se añaden valores por defecto para inputs...
            inputAnno ??= fechaActual.Year;
            inputMes ??= fechaActual.Month;
            inputAfp ??= Request.Cookies["FiltroHistorialAfpSeleccionada"];
            inputAfp ??= "CAPITAL";

            // Se prepara rango de fecha a utilizar para historial de valores cuota...
            DateTime? fechaDesdeHistorial = null;
            try {
                fechaDesdeHistorial = new DateTime(inputAnno.GetValueOrDefault(), inputMes.GetValueOrDefault(), 1);
            } catch {
                fechaDesdeHistorial = new DateTime(fechaActual.Year, fechaActual.Month, 1);
            }
            DateTime fechaHastaHistorial = fechaDesdeHistorial.GetValueOrDefault().AddMonths(1).AddDays(-1);

            // Se añade consulta para historial inferior mediante Task, para no detener las consultas asociadas a resumen semanal...
            Task<List<CuotaUf>> taskValoresHistorial = cuotaUfComisionDAO.ObtenerCuotas(
                inputAfp,
                "A,B,C,D,E",
                fechaDesdeHistorial.GetValueOrDefault().AddDays(-7), // Se consulta un rango mayor de dias en caso de necesitar rellenar fines de semana
                fechaHastaHistorial.AddDays(7)
            );

            // Se arma objeto de salida...
            ResumenViewModel salida = new() {
                UltimaSemana = [
                    new UltimaSemanaAfp {
                        Nombre = "Capital",
                        UrlLogo = "/images/logos_afps/LogoAFPCapital.svg"
                    },
                    new UltimaSemanaAfp {
                        Nombre = "Cuprum",
                        UrlLogo = "/images/logos_afps/LogoAFPCuprum.svg"
                    },
                    new UltimaSemanaAfp {
                        Nombre = "Habitat",
                        UrlLogo = "/images/logos_afps/LogoAFPHabitat.svg"
                    },
                    new UltimaSemanaAfp {
                        Nombre = "Modelo",
                        UrlLogo = "/images/logos_afps/LogoAFPModelo.svg"
                    },
                    new UltimaSemanaAfp {
                        Nombre = "PlanVital",
                        UrlLogo = "/images/logos_afps/LogoAFPPlanvital.svg",
                        Alto = 10
                    },
                    new UltimaSemanaAfp {
                        Nombre = "ProVida",
                        UrlLogo = "/images/logos_afps/LogoAFPProvida.png"
                    },
                    new UltimaSemanaAfp {
                        Nombre = "Uno",
                        UrlLogo = "/images/logos_afps/LogoAFPUno.png"
                    },
                ]
            };

            // Se preparan los datos para la grilla de historial de valores cuota...
            List<int> listaAnnos = [.. Enumerable.Range(2002, fechaActual.Year + 1 - 2002)];
            listaAnnos.Reverse();

            salida.Historial = new HistorialAfp {
                Afp = inputAfp,
                ListaAfps = new SelectList(LISTA_AFPS.Select(l => new SelectListItem { Value = l.Key, Text = l.Value }).ToList(), nameof(SelectListItem.Value), nameof(SelectListItem.Text)),
                Mes = fechaDesdeHistorial.GetValueOrDefault().Month,
                ListaMeses = new SelectList(LISTA_MESES.Select(l => new SelectListItem { Value = l.Key.ToString(), Text = l.Value }).ToList(), nameof(SelectListItem.Value), nameof(SelectListItem.Text)),
                Anno = fechaDesdeHistorial.GetValueOrDefault().Year,
                ListaAnnos = new SelectList(listaAnnos.Select(l => new SelectListItem { Value = l.ToString(), Text = l.ToString() }).ToList(), nameof(SelectListItem.Value), nameof(SelectListItem.Text))
            };

            // Se graba en cookie última AFP seleccionada para usar como defecto en consultas posteriores...
            HttpContext.Response.Cookies.Append("FiltroHistorialAfpSeleccionada", inputAfp, new CookieOptions {
                Expires = DateTime.Now.AddDays(365)
            });

            // Se obtiene el filtro por defecto a mostrar en el resumen...
            string? fondoSeleccionado = Request.Cookies["FiltroResumenFondoSeleccionado"];
            fondoSeleccionado ??= "A";
            ViewBag.FiltroResumenFondoSeleccionado = fondoSeleccionado;

            // Se obtiene el filtro por defecto a mostrar en el historial...
            string? fondoHistorialSeleccionado = Request.Cookies["FiltroHistorialFondoSeleccionado"];
            fondoHistorialSeleccionado ??= "A";
            ViewBag.FiltroHistorialFondoSeleccionado = fondoHistorialSeleccionado;

            // Se espera a obtener por última fecha para usar resultado como parámetro de consulta de valores cuota...
            DateTime? fechaHasta = await taskFechaHasta;
            if (fechaHasta == null) {
                fechaHasta = TimeZoneInfo.ConvertTime(DateTime.Now, TimeZoneConverter.TZConvert.GetTimeZoneInfo("Pacific SA Standard Time"));
            }

            // Se crea listado de fechas que será usado en la cabecera de la tabla de valores cuota...
            List<DateTime> fechas = [fechaHasta.GetValueOrDefault()];
            for (int i = -1; i >= -1 * (CANTIDAD_DIAS_RESUMEN - 1); i--) {
                fechas.Add(fechaHasta.GetValueOrDefault().AddDays(i));
            }

            // Se añade listado de fechas a objeto de salida...
            salida.FechasUltimaSemana = fechas;

            List<CuotaUf> valoresCuota = await taskValoresCuota;

            // Se arman las listas de valores cuotas para cada AFP, según el listado de fechas...
            foreach (UltimaSemanaAfp ultimaSemanaAfp in salida.UltimaSemana) {
                foreach (string fondo in "A,B,C,D,E".Split(",")) {
                    foreach (DateTime fecha in salida.FechasUltimaSemana) {
                        CuotaUf? cuotaUf = valoresCuota.FirstOrDefault(c => c.Afp.Equals(ultimaSemanaAfp.Nombre, StringComparison.InvariantCultureIgnoreCase) && c.Fondo == fondo && DateTime.ParseExact(c.Fecha, "yyyy-MM-dd", CultureInfo.InvariantCulture) == fecha);

                        // Si no tenemos valor cuota para la fecha indicada, se marca como null o con la valor cuota anterior.
                        if (cuotaUf == null) {
                            if (ultimaSemanaAfp.ValoresCuota[fondo].Count > 0 && ultimaSemanaAfp.ValoresCuota[fondo].Last() != null) {
                                cuotaUf = valoresCuota.Where(c => c.Afp.Equals(ultimaSemanaAfp.Nombre, StringComparison.InvariantCultureIgnoreCase) && c.Fondo == fondo && DateTime.ParseExact(c.Fecha, "yyyy-MM-dd", CultureInfo.InvariantCulture) < fecha).OrderByDescending(c => c.Fecha).FirstOrDefault();
                                ultimaSemanaAfp.ValoresCuota[fondo].Add(new CuotaRentabilidadDia {
                                    ValorCuota = cuotaUf!.Valor,
                                    Fecha = DateTime.ParseExact(cuotaUf.Fecha, "yyyy-MM-dd", CultureInfo.InvariantCulture),
                                    RentabilidadDia = 0 // Rentabilidad 0 ya que se está usando el valor cuota heredado de un día anterior (sin variación)...
                                });
                            } else {
                                ultimaSemanaAfp.ValoresCuota[fondo].Add(null);
                            }
                        } else {
                            CuotaUf? cuotaUfAnterior = valoresCuota.Where(c => c.Afp.Equals(ultimaSemanaAfp.Nombre, StringComparison.InvariantCultureIgnoreCase) && c.Fondo == fondo && DateTime.ParseExact(c.Fecha, "yyyy-MM-dd", CultureInfo.InvariantCulture) < fecha).OrderByDescending(c => c.Fecha).FirstOrDefault();

                            ultimaSemanaAfp.ValoresCuota[fondo].Add(new CuotaRentabilidadDia {
                                ValorCuota = cuotaUf.Valor,
                                Fecha = DateTime.ParseExact(cuotaUf.Fecha, "yyyy-MM-dd", CultureInfo.InvariantCulture),
                                RentabilidadDia = cuotaUfAnterior != null ? 100 * (cuotaUf.Valor - cuotaUfAnterior!.Valor) / cuotaUfAnterior.Valor : 0
                            });
                        }
                    }
                }
            }

            // Se espera por task en caso de que aún no haya terminado de procesar...
            List<CuotaUf> valoresHistorial = await taskValoresHistorial;

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

            return View(salida);
        }
    }
}
