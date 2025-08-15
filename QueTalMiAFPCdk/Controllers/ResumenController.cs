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
        private const int CANTIDAD_MESES_PREMIO = 6;

        private static readonly Dictionary<string, string> LISTA_AFPS = new() {
            { "CAPITAL", "Capital" },
            { "CUPRUM", "Cuprum" },
            { "HABITAT", "Habitat" },
            { "MODELO", "Modelo" },
            { "PLANVITAL", "PlanVital" },
            { "PROVIDA", "ProVida" },
            { "UNO", "Uno" },
        };

        private static readonly Dictionary<string, string> IMAGENES_AFP = new() {
            { "CAPITAL", "/images/logos_afps/LogoAFPCapital.svg" },
            { "CUPRUM", "/images/logos_afps/LogoAFPCuprum.svg" },
            { "HABITAT", "/images/logos_afps/LogoAFPHabitat.svg" },
            { "MODELO", "/images/logos_afps/LogoAFPModelo.svg" },
            { "PLANVITAL", "/images/logos_afps/LogoAFPPlanvital.svg" },
            { "PROVIDA", "/images/logos_afps/LogoAFPProvida.png" },
            { "UNO", "/images/logos_afps/LogoAFPUno.png" },
        };

        public async Task<IActionResult> Index(ResumenViewModel modeloEntrada) {
            // Se comienza con consulta de última fecha, mediante Task para poder avanzar en las otras tareas...
            Task<DateTime> taskFechaHasta = cuotaUfComisionDAO.UltimaFechaAlguna();
            Task<DateTime> taskFechaTodas = cuotaUfComisionDAO.UltimaFechaTodas();

            DateTime fechaActual = TimeZoneInfo.ConvertTime(DateTime.Now, TimeZoneConverter.TZConvert.GetTimeZoneInfo("Pacific SA Standard Time"));

            // Se obtienen los valores cuota de todas las AFP y fondo seleccionado, para el rango de fechas a mostrar en la tabla, mediante Task...
            Task <List<CuotaUf>> taskValoresCuota = cuotaUfComisionDAO.ObtenerCuotas(
                "CAPITAL,CUPRUM,HABITAT,MODELO,PLANVITAL,PROVIDA,UNO",
                "A,B,C,D,E",
                fechaActual.AddDays(-14), // Se consulta un rango mayor de dias en caso de necesitar rellenar fines de semana
                fechaActual
            );

            // Se arma objeto de salida...
            ResumenViewModel salida = new() {
                Resumen = {
                    UltimaSemana = IMAGENES_AFP.Keys.ToList().OrderBy(c => c.ToString()).Select(c => new UltimaSemanaAfp { Nombre = LISTA_AFPS[c], UrlLogo = IMAGENES_AFP[c] }).ToList(),
                }
            };

            salida.Resumen.UltimaSemana.First(u => u.Nombre == "PlanVital").Alto = 10;

            // Se obtiene el filtro por defecto a mostrar en el resumen...
            string? fondoSeleccionado = Request.Cookies["FiltroResumenFondoSeleccionado"];
            fondoSeleccionado ??= "A";
            salida.Resumen.FiltroResumenFondoSeleccionado = fondoSeleccionado;

            // Se consultan los valores cuotas de las fechas a utilizar para los premios mensuales de rentabilidad...
            DateTime fechasTodas = await taskFechaTodas;
            modeloEntrada.Premios.Anno ??= fechasTodas.Year;
            modeloEntrada.Premios.Anno = modeloEntrada.Premios.Anno < 2002 ? fechasTodas.Year : modeloEntrada.Premios.Anno;
            modeloEntrada.Premios.Anno = modeloEntrada.Premios.Anno > fechasTodas.Year ? fechasTodas.Year : modeloEntrada.Premios.Anno;
            salida.Premios.Anno = modeloEntrada.Premios.Anno;

            // Se preparan los datos para la grilla de historial de valores cuota...
            List<int> listaAnnos = [.. Enumerable.Range(2002, fechasTodas.Year + 1 - 2002)];
            listaAnnos.Reverse();
            salida.Premios.ListaAnnos = new SelectList(listaAnnos.Select(l => new SelectListItem { Value = l.ToString(), Text = l.ToString() }).ToList(), nameof(SelectListItem.Value), nameof(SelectListItem.Text));

            // Se arma listado de fechas que se necesitan para calcular las rentabilidades mensuales...
            List <DateTime> listaFechasPremios = [];
            DateTime fechaHastaRentMensual = fechasTodas;
            listaFechasPremios.Add(fechaHastaRentMensual);
            DateTime fechaDesdeRentMensual = new DateTime(fechaHastaRentMensual.Year, fechaHastaRentMensual.Month, 1).AddDays(-1);
            listaFechasPremios.Add(fechaDesdeRentMensual);
            salida.Premios.Ganadores.Add(fechaHastaRentMensual.Year * 100 + fechaHastaRentMensual.Month, new GanadorMes {
                FechaDesde = fechaDesdeRentMensual,
                FechaHasta = fechaHastaRentMensual
            });

            // Si estamos cargando los premios del mismo año actual, se cargan desde la fecha actual hasta enero...
            if (salida.Premios.Anno == fechasTodas.Year) {
                fechaHastaRentMensual = fechaDesdeRentMensual;
                // Pero si justo coincide con que la fecha actual es Enero (es decir, la nueva fecha "hasta" quedará en el año anterior), se muestra el año anterior...
                if (salida.Premios.Anno != fechaHastaRentMensual.Year) {
                    salida.Premios.Anno = fechaHastaRentMensual.Year;
                }
            // En su defecto, se carga el año completo desde diciembre hasta enero...
            } else {
                fechaHastaRentMensual = new DateTime(salida.Premios.Anno.GetValueOrDefault(), 12, 31);
                listaFechasPremios.Add(fechaHastaRentMensual);
            }

            while (fechaHastaRentMensual.Year >= salida.Premios.Anno) {
                fechaDesdeRentMensual = new DateTime(fechaHastaRentMensual.Year, fechaHastaRentMensual.Month, 1).AddDays(-1);
                listaFechasPremios.Add(fechaDesdeRentMensual);

                salida.Premios.Ganadores.Add(fechaHastaRentMensual.Year * 100 + fechaHastaRentMensual.Month, new GanadorMes {
                    FechaDesde = fechaDesdeRentMensual,
                    FechaHasta = fechaHastaRentMensual
                });

                fechaHastaRentMensual = fechaDesdeRentMensual;
            }

            Task<List<SalObtenerUltimaCuota>> taskCuotasPremio = cuotaUfComisionDAO.ObtenerUltimaCuota(
                "CAPITAL,CUPRUM,HABITAT,MODELO,PLANVITAL,PROVIDA,UNO",
                "A,B,C,D,E",
                String.Join(",", listaFechasPremios.Select(f => f.ToString("dd/MM/yyyy"))),
                1
            );

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
            salida.Resumen.FechasUltimaSemana = fechas;

            List<CuotaUf> valoresCuota = await taskValoresCuota;

            // Se arman las listas de valores cuotas para cada AFP, según el listado de fechas...
            foreach (UltimaSemanaAfp ultimaSemanaAfp in salida.Resumen.UltimaSemana) {
                foreach (string fondo in "A,B,C,D,E".Split(",")) {
                    foreach (DateTime fecha in salida.Resumen.FechasUltimaSemana) {
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

            salida.Premios.FechasTodas = fechasTodas;

            // Se calcula porcentaje que se lleva del mes para el premio de la rentabilidad del mes...
            DateTime primerDiaMes = new DateTime(fechasTodas.Year, fechasTodas.Month, 1);
            DateTime ultimoDiaMes = primerDiaMes.AddMonths(1).AddDays(-1);
            decimal porcMesPremio = 100 * fechasTodas.Day / ultimoDiaMes.Day;
            salida.Premios.PorcMesPremio = porcMesPremio;
            salida.Premios.PrimerDiaMesPremio = primerDiaMes;
            salida.Premios.UltimoDiaMesPremio = ultimoDiaMes;


            List<SalObtenerUltimaCuota> cuotasPremio = await taskCuotasPremio;
            foreach (int periodo in salida.Premios.Ganadores.Keys) {
                List<GanadorMes> candidatos = [];
                foreach (string afp in "CAPITAL,CUPRUM,HABITAT,MODELO,PLANVITAL,PROVIDA,UNO".Split(",")) {
                    foreach (string fondo in "A,B,C,D,E".Split(",")) {
                        SalObtenerUltimaCuota? cuotaDesde = cuotasPremio.Where(c => c.Afp == afp && c.Fondo == fondo && c.Fecha <= salida.Premios.Ganadores[periodo].FechaDesde).OrderByDescending(c => c.Fecha).FirstOrDefault();
                        SalObtenerUltimaCuota? cuotaHasta = cuotasPremio.Where(c => c.Afp == afp && c.Fondo == fondo && c.Fecha <= salida.Premios.Ganadores[periodo].FechaHasta).OrderByDescending(c => c.Fecha).FirstOrDefault();

                        candidatos.Add(new GanadorMes {
                            Afp = afp,
                            Fondo = fondo,
                            FechaDesde = salida.Premios.Ganadores[periodo].FechaDesde,
                            FechaHasta = salida.Premios.Ganadores[periodo].FechaHasta,
                            Rentabilidad = 100 * (cuotaHasta?.Valor - cuotaDesde?.Valor) / cuotaDesde?.Valor
                        });
                    }
                }
                GanadorMes ganador = candidatos.OrderByDescending(g => g.Rentabilidad).First();
                salida.Premios.Ganadores[periodo].Afp = ganador.Afp;
                salida.Premios.Ganadores[periodo].Fondo = ganador.Fondo;
                salida.Premios.Ganadores[periodo].Rentabilidad = ganador.Rentabilidad;
            }


            return View(salida);
        }
    }
}
