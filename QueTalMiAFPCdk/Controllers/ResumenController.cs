using Microsoft.AspNetCore.Mvc;
using QueTalMiAFPCdk.Models.Entities;
using QueTalMiAFPCdk.Models.Others;
using QueTalMiAFPCdk.Models.ViewModels;
using QueTalMiAFPCdk.Repositories;
using System.Collections.Generic;
using System.Globalization;

namespace QueTalMiAFPCdk.Controllers {
    public class ResumenController(ICuotaUfComisionDAO cuotaUfComisionDAO) : Controller {
        private const int CANTIDAD_DIAS_RESUMEN = 7;

        public async Task<IActionResult> Index(string fondo = "A") {
            // Se define la fecha hasta como la última fecha en la que se tiene al menos un valor cuota...
            DateTime? fechaHasta = await cuotaUfComisionDAO.UltimaFechaAlguna();
            if (fechaHasta == null) {
                fechaHasta = TimeZoneInfo.ConvertTime(DateTime.Now, TimeZoneConverter.TZConvert.GetTimeZoneInfo("Pacific SA Standard Time"));
            }

            // Se crea listado de fechas que será usado en la cabecera de la tabla de valores cuota...
            List<DateTime> fechas = [ fechaHasta.GetValueOrDefault() ];
            for (int i = -1; i >= -1 * (CANTIDAD_DIAS_RESUMEN - 1); i--) {
                fechas.Add(fechaHasta.GetValueOrDefault().AddDays(i));
            }

            // Se obtienen los valores cuota de todas las AFP y fondo seleccionado, para el rango de fechas a mostrar en la tabla...
            List<CuotaUf> valoresCuota = await cuotaUfComisionDAO.ObtenerCuotas(
                "CAPITAL,CUPRUM,HABITAT,MODELO,PLANVITAL,PROVIDA,UNO",
                fondo,
                fechas.Last().AddDays(-7), // Se consulta un rango mayor de dias en caso de necesitar rellenar fines de semana
                fechas.First()
            );

            ResumenViewModel resumen = new() {
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
            foreach(ResumenAfp resumenAfp in resumen.Resumenes) {
                foreach (DateTime fecha in resumen.Fechas) {
                    CuotaUf? valorCuota = valoresCuota.Where(c => c.Afp == resumenAfp.Nombre.ToUpper() && c.Fecha == fecha.ToString("yyyy-MM-dd")).FirstOrDefault();

                    // Si no tenemos valor cuota para la fecha indicada, se marca como null o con la valor cuota anterior.
                    if (valorCuota == null) {
                        if (resumenAfp.ValoresCuota.Count > 0 && resumenAfp.ValoresCuota.Last() != null) {
                            valorCuota = valoresCuota.Where(c => c.Afp == resumenAfp.Nombre.ToUpper() && DateTime.ParseExact(c.Fecha, "yyyy-MM-dd", CultureInfo.InvariantCulture) <= fecha).OrderByDescending(c => c.Fecha).FirstOrDefault();
                            resumenAfp.ValoresCuota.Add(valorCuota?.Valor);
                        } else {
                            resumenAfp.ValoresCuota.Add(null);
                        }
                    } else {
                        resumenAfp.ValoresCuota.Add(valorCuota.Valor);
                    }
                }

                // Una vez se cargan los valores cuota, se recorren nuevamente a la inversa para reemplazar los -1 por el valor del día anterior
                for (int i = resumenAfp.ValoresCuota.Count - 2; i >= 0; i--) { 
                    if (resumenAfp.ValoresCuota[i] == -1) {
                        resumenAfp.ValoresCuota[i] = resumenAfp.ValoresCuota[i + 1];
                    }
                }
            }

            return View(resumen);
        }
    }
}
