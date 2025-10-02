using Amazon.APIGateway.Model;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using Newtonsoft.Json;
using QueTalMiAFPCdk.Models.Others;
using QueTalMiAFPCdk.Repositories;
using QueTalMiAFPCdk.Services;
using System.Globalization;
using System.Net;
using System.Text;

namespace QueTalMiAFPCdk.Controllers {
    [Route("api/[controller]")]
    [ApiController]
    public class CuotaController(ParameterStoreHelper parameterStore, ApiKeyHelper apiKey, ICuotaUfComisionDAO cuotaUfComisionDAO, S3BucketHelper s3BucketHelper) : ControllerBase {
        
        // GET: api/Cuota/ObtenerCuotas?listaAFPs=CAPITAL,UNO&listaFondos=A,B&fechaInicial=01/01/2020&fechaFinal=31/12/2020
        [Route("[action]")]
        [HttpGet]
        public async Task<ActionResult<List<CuotaUf>>> ObtenerCuotas(string listaAFPs, string listaFondos, string fechaInicial, string fechaFinal) {
            if (listaAFPs == null) {
                ModelState.AddModelError(
                    nameof(listaAFPs),
                    "El parámetro listaAFPs debe incluir por lo menos el nombre de una AFP. Ejemplo: CAPITAL.");
                return ValidationProblem();
            }

            if (listaFondos == null) {
                ModelState.AddModelError(
                    nameof(listaFondos),
                    "El parámetro listaFondos debe incluir por lo menos un fondo. Ejemplo: A.");
                return ValidationProblem();
            }

            if (fechaInicial == null || fechaInicial.Trim().Length == 0) {
                fechaInicial = "01/08/2002";
            }
            if (fechaFinal == null || fechaFinal.Trim().Length == 0) {
                fechaFinal = DateTime.Now.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture);
            }
            string[] diaMesAnnoInicio = fechaInicial.Split("/");
            string[] diaMesAnnoFinal = fechaFinal.Split("/");
            if (diaMesAnnoInicio.Length != 3) {
                ModelState.AddModelError(
                    nameof(fechaInicial),
                    "El parámetro fechaInicio debe tener formato dd/mm/yyyy. Ejemplo: 31/12/2020.");
                return ValidationProblem();
            }
            if (diaMesAnnoFinal.Length != 3) {
                ModelState.AddModelError(
                    nameof(fechaFinal),
                    "El parámetro fechaFinal debe tener formato dd/mm/yyyy. Ejemplo: 31/12/2020.");
                return ValidationProblem();
            }
            DateTime dtFechaInicio;
            DateTime dtFechaFinal;
            try {
                dtFechaInicio = new DateTime(
                    int.Parse(diaMesAnnoInicio[2]),
                    int.Parse(diaMesAnnoInicio[1]),
                    int.Parse(diaMesAnnoInicio[0]));
            } catch (ArgumentOutOfRangeException) {
                ModelState.AddModelError(
                    nameof(fechaInicial),
                    "El parámetro fechaInicio no corresponde a una fecha válida.");
                return ValidationProblem();
            }
            try {
                dtFechaFinal = new DateTime(
                    int.Parse(diaMesAnnoFinal[2]),
                    int.Parse(diaMesAnnoFinal[1]),
                    int.Parse(diaMesAnnoFinal[0]));
            } catch (ArgumentOutOfRangeException) {
                ModelState.AddModelError(
                    nameof(fechaFinal),
                    "El parámetro fechaFinal no corresponde a una fecha válida.");
                return ValidationProblem();
            }
            if (dtFechaInicio > dtFechaFinal) {
                ModelState.AddModelError(
                    nameof(fechaInicial),
                    "El parámetro fechaInicio no puede representar una fecha posterior al parámetro fechaFinal.");
                return ValidationProblem();
            }

            // Se valida que si el usuario no ha iniciado sesión solo consulte por el año actual +/- 7 días...
            if (User.Identity == null || !User.Identity.IsAuthenticated) {
                DateTime? ultimaFechaAlgunValorCuota = await cuotaUfComisionDAO.UltimaFechaAlguna();
                if (ultimaFechaAlgunValorCuota == null) {
                    ultimaFechaAlgunValorCuota = TimeZoneInfo.ConvertTime(DateTime.Now, TimeZoneConverter.TZConvert.GetTimeZoneInfo("Pacific SA Standard Time"));
                }

                DateOnly fechaMinima = DateOnly.FromDateTime(ultimaFechaAlgunValorCuota.Value.AddYears(-1).AddDays(-7));
                DateOnly fechaMaxima = DateOnly.FromDateTime(ultimaFechaAlgunValorCuota.Value.AddDays(7));
                if (fechaMinima != DateOnly.FromDateTime(dtFechaInicio) || fechaMaxima != DateOnly.FromDateTime(dtFechaFinal)) {
                    return Unauthorized();
                }
            }

            return await cuotaUfComisionDAO.ObtenerCuotas(listaAFPs, listaFondos, dtFechaInicio, dtFechaFinal); 
        }

        // POST: CuotaUfComision/ObtenerUltimaCuota
        // Parameters: listaAFPs=CAPITAL,UNO&listaFondos=A,B&listaFechas=05/01/2020,05/02/2020&tipoComision=1
        [Route("[action]")]
        [HttpPost]
        public async Task<ActionResult<List<SalObtenerUltimaCuota>>> ObtenerUltimaCuota(EntObtenerUltimaCuota entrada) {
            if (entrada.ListaAFPs == null || entrada.ListaAFPs.Trim().Length == 0) {
                ModelState.AddModelError(
                    nameof(entrada.ListaAFPs),
                    "El parámetro listaAFPs debe incluir por lo menos el nombre de una AFP. Ejemplo: CAPITAL.");
                return ValidationProblem();
            }

            if (entrada.ListaFondos == null || entrada.ListaFondos.Trim().Length == 0) {
                ModelState.AddModelError(
                    nameof(entrada.ListaFondos),
                    "El parámetro listaFondos debe incluir por lo menos un fondo. Ejemplo: A.");
                return ValidationProblem();
            }

            if (entrada.ListaFechas == null || entrada.ListaFechas.Trim().Length == 0) {
                ModelState.AddModelError(
                    nameof(entrada.ListaFechas),
                    "El parámetro listaFechas debe incluir por lo menos una fecha. Ejemplo: 31/12/2020.");
                return ValidationProblem();
            }

            DateOnly? fechaMinima = null;
            DateOnly? fechaMaxima = null;
            int? cantFechasIntermedias = null;
            bool? incluyeMinimo = null;
            bool? incluyeMaximo = null;
            if (User.Identity == null || !User.Identity.IsAuthenticated) {
                DateTime ultimaFechaTodosValoresCuota = await cuotaUfComisionDAO.UltimaFechaTodas();
                fechaMinima = DateOnly.FromDateTime(ultimaFechaTodosValoresCuota.AddYears(-1));
                fechaMaxima = DateOnly.FromDateTime(ultimaFechaTodosValoresCuota);
                cantFechasIntermedias = 0;
                incluyeMinimo = false;
                incluyeMaximo = false;
            }

            string[] fechas = entrada.ListaFechas.Replace(" ", "").Split(",");
            foreach (string fecha in fechas) {
                string[] diaMesAnno = fecha.Split("/");
                if (diaMesAnno.Length != 3) {
                    ModelState.AddModelError(
                        nameof(entrada.ListaFechas),
                        "Las fechas en el parámetro listaFechas deben tener formato dd/mm/yyyy. Ejemplo: 31/12/2020.");
                    return ValidationProblem();
                }

                DateTime dtFecha;
                try {
                    dtFecha = new DateTime(
                        int.Parse(diaMesAnno[2]),
                        int.Parse(diaMesAnno[1]),
                        int.Parse(diaMesAnno[0]));

                    // Se valida que si el usuario no ha iniciado sesión se incluya la consulta dentro del rango de mínimos y máximos...
                    if (fechaMinima != null && fechaMaxima != null && cantFechasIntermedias != null) {
                        DateOnly auxDateOnly = DateOnly.FromDateTime(dtFecha);
                        
                        if (auxDateOnly == fechaMinima) {
                            incluyeMinimo = true;
                        } else if (auxDateOnly == fechaMaxima) {
                            incluyeMaximo = true;
                        } else if (auxDateOnly < fechaMinima || auxDateOnly > fechaMaxima) {
                            return Unauthorized();
                        } else {
                            cantFechasIntermedias++;
                        }
                    }
                } catch (ArgumentOutOfRangeException) {
                    ModelState.AddModelError(
                        nameof(entrada.ListaFechas),
                        "Una de las fechas en el parámetro listaFechas no corresponden a una fecha válida.");
                    return ValidationProblem();
                }
            }

            // Se valida que si el usuario no ha iniciado sesión debe si o si consultar por los valores mínimos y máximos...
            if (incluyeMinimo != null && !incluyeMinimo.Value) {
                return Unauthorized();
            }

            if (incluyeMaximo != null && !incluyeMaximo.Value) {
                return Unauthorized();
            }

            // Se valida que si el usuario no ha iniciado sesión solo consulte por 12 fechas intermedias...
            if (cantFechasIntermedias != null && cantFechasIntermedias > 12) {
                return Unauthorized();
            }

            return await cuotaUfComisionDAO.ObtenerUltimaCuota(entrada.ListaAFPs, entrada.ListaFondos, entrada.ListaFechas, entrada.TipoComision);
        }

        // GET: api/Cuota/ObtenerRentabilidadRealUltimoAnno
        [Route("[action]")]
        [HttpGet]
        [Authorize]
        public async Task<ActionResult<List<RentabilidadReal>>> ObtenerRentabilidadRealUltimoAnno() {
            DateTime fechaFinal = await cuotaUfComisionDAO.UltimaFechaTodas();
            DateTime fechaInicial = fechaFinal.AddYears(-1);

            return await cuotaUfComisionDAO.ObtenerRentabilidadReal(
                "CAPITAL,CUPRUM,HABITAT,MODELO,PLANVITAL,PROVIDA,UNO",
                "A,B,C,D,E",
                fechaInicial,
                fechaFinal);
        }

        // GET: api/Cuota/DescargarCuotasCSV?listaAFPs=CAPITAL,UNO&listaFondos=A,B&fechaInicio=01/01/2020&fechaFinal=31/12/2020
        [Route("[action]")]
        [HttpGet]
        public async Task<ActionResult> DescargarCuotasCSV(string listaAFPs, string listaFondos, string fechaInicial, string fechaFinal) {
            // Si el usuario no está autenticado, se le envía a autenticarse...
            if (User.Identity == null || !User.Identity.IsAuthenticated) {
                return Challenge();
            }

            // Si llegamos a esta URL sin Referer, se asume que llegamos desde pantalla de login, por lo que se redirecciona a pantalla de acceso...
            if (string.IsNullOrEmpty(Request.Headers.Referer.ToString())) {
                return RedirectToAction("Index", "AccederCuotas");
            }
            
            ActionResult<List<CuotaUf>> retorno = await ObtenerCuotas(listaAFPs, listaFondos, fechaInicial, fechaFinal);

            StringBuilder sb = new();
            sb.AppendLine($"AFP;FECHA;FONDO;VALOR_CUOTA;VALOR_UF");
            foreach (CuotaUf cuota in retorno.Value!) {
                sb.AppendLine(
                    $"{cuota.Afp};" +
                    $"{cuota.Fecha};" +
                    $"{cuota.Fondo};" +
                    $"{cuota.Valor.ToString(CultureInfo.InvariantCulture).Replace(".", ",")};" +
                    $"{cuota.ValorUf?.ToString(CultureInfo.InvariantCulture).Replace(".", ",")}");
            }

            byte[] bytes = new UTF8Encoding().GetBytes(sb.ToString());
            return File(bytes, "text/csv", $"QueTalMiAFP_{DateTime.Now:yyyyMMddHHmmss}.csv");
        }
    }
}
