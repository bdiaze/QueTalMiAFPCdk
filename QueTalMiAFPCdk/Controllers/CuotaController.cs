using Amazon.APIGateway.Model;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using Newtonsoft.Json;
using QueTalMiAFPCdk.Models.Others;
using QueTalMiAFPCdk.Repositories;
using QueTalMiAFPCdk.Services;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Net;
using System.Text;

namespace QueTalMiAFPCdk.Controllers {
    [Route("api/[controller]")]
    [ApiController]
    public class CuotaController(ILogger<CuotaController> logger, CuotaUfComisionDAO cuotaUfComisionDAO, ApiKeyDAO apiKeyDAO) : ControllerBase {
        
        // GET: api/Cuota/ObtenerCuotas?listaAFPs=CAPITAL,UNO&listaFondos=A,B&fechaInicial=01/01/2020&fechaFinal=31/12/2020
        [Route("[action]")]
        [HttpGet]
        public async Task<ActionResult<List<CuotaUf>>> ObtenerCuotas(string listaAFPs, string listaFondos, string fechaInicial, string fechaFinal, [FromHeader(Name = "X-API-Key")] string? xApiKey) {
            Stopwatch stopwatch = Stopwatch.StartNew();

            if (string.IsNullOrWhiteSpace(listaAFPs)) {
                ModelState.AddModelError(
                    nameof(listaAFPs),
                    "El parámetro listaAFPs debe incluir por lo menos el nombre de una AFP. Ejemplo: CAPITAL.");

                logger.LogInformation(
                    "[{Method}] - [{Controller}] - [{Action}] - [{ElapsedTime} ms] - [{StatusCode}] - [Usuario Autenticado: {IsAuthenticated}]" +
                    "Error en validación de parámetros de entrada, no se incluye el parámetro \"listaAFPs\" - " +
                    "ListaAFPs: {ListaAFPs} - ListaFondos: {ListaFondos} - FechaInicial: {FechaInicial} - Fecha Final: {FechaFinal}.",
                    HttpContext.Request.Method, ControllerContext.ActionDescriptor.ControllerName, ControllerContext.ActionDescriptor.ActionName,
                    stopwatch.ElapsedMilliseconds, StatusCodes.Status400BadRequest, User.Identity?.IsAuthenticated ?? false,
                    listaAFPs, listaFondos, fechaInicial, fechaFinal);

                return ValidationProblem();
            }

            if (string.IsNullOrEmpty(listaFondos)) {
                ModelState.AddModelError(
                    nameof(listaFondos),
                    "El parámetro listaFondos debe incluir por lo menos un fondo. Ejemplo: A.");

                logger.LogInformation(
                    "[{Method}] - [{Controller}] - [{Action}] - [{ElapsedTime} ms] - [{StatusCode}] - [Usuario Autenticado: {IsAuthenticated}]" +
                    "Error en validación de parámetros de entrada, no se incluye el parámetro \"listaFondos\" - " +
                    "ListaAFPs: {ListaAFPs} - ListaFondos: {ListaFondos} - FechaInicial: {FechaInicial} - Fecha Final: {FechaFinal}.",
                    HttpContext.Request.Method, ControllerContext.ActionDescriptor.ControllerName, ControllerContext.ActionDescriptor.ActionName,
                    stopwatch.ElapsedMilliseconds, StatusCodes.Status400BadRequest, User.Identity?.IsAuthenticated ?? false,
                    listaAFPs, listaFondos, fechaInicial, fechaFinal);

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

                logger.LogInformation(
                    "[{Method}] - [{Controller}] - [{Action}] - [{ElapsedTime} ms] - [{StatusCode}] - [Usuario Autenticado: {IsAuthenticated}]" +
                    "Error en validación de parámetros de entrada, formato incorrecto del parámetro \"fechaInicial\" - " +
                    "ListaAFPs: {ListaAFPs} - ListaFondos: {ListaFondos} - FechaInicial: {FechaInicial} - Fecha Final: {FechaFinal}.",
                    HttpContext.Request.Method, ControllerContext.ActionDescriptor.ControllerName, ControllerContext.ActionDescriptor.ActionName,
                    stopwatch.ElapsedMilliseconds, StatusCodes.Status400BadRequest, User.Identity?.IsAuthenticated ?? false,
                    listaAFPs, listaFondos, fechaInicial, fechaFinal);

                return ValidationProblem();
            }
            if (diaMesAnnoFinal.Length != 3) {
                ModelState.AddModelError(
                    nameof(fechaFinal),
                    "El parámetro fechaFinal debe tener formato dd/mm/yyyy. Ejemplo: 31/12/2020.");

                logger.LogInformation(
                    "[{Method}] - [{Controller}] - [{Action}] - [{ElapsedTime} ms] - [{StatusCode}] - [Usuario Autenticado: {IsAuthenticated}]" +
                    "Error en validación de parámetros de entrada, formato incorrecto del parámetro \"fechaFinal\" - " +
                    "ListaAFPs: {ListaAFPs} - ListaFondos: {ListaFondos} - FechaInicial: {FechaInicial} - Fecha Final: {FechaFinal}.",
                    HttpContext.Request.Method, ControllerContext.ActionDescriptor.ControllerName, ControllerContext.ActionDescriptor.ActionName,
                    stopwatch.ElapsedMilliseconds, StatusCodes.Status400BadRequest, User.Identity?.IsAuthenticated ?? false,
                    listaAFPs, listaFondos, fechaInicial, fechaFinal);

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

                logger.LogInformation(
                    "[{Method}] - [{Controller}] - [{Action}] - [{ElapsedTime} ms] - [{StatusCode}] - [Usuario Autenticado: {IsAuthenticated}]" +
                    "Error en validación de parámetros de entrada, fecha inválida en parámetro \"fechaInicial\" - " +
                    "ListaAFPs: {ListaAFPs} - ListaFondos: {ListaFondos} - FechaInicial: {FechaInicial} - Fecha Final: {FechaFinal}.",
                    HttpContext.Request.Method, ControllerContext.ActionDescriptor.ControllerName, ControllerContext.ActionDescriptor.ActionName,
                    stopwatch.ElapsedMilliseconds, StatusCodes.Status400BadRequest, User.Identity?.IsAuthenticated ?? false,
                    listaAFPs, listaFondos, fechaInicial, fechaFinal);

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

                logger.LogInformation(
                    "[{Method}] - [{Controller}] - [{Action}] - [{ElapsedTime} ms] - [{StatusCode}] - [Usuario Autenticado: {IsAuthenticated}]" +
                    "Error en validación de parámetros de entrada, fecha inválida en parámetro \"fechaFinal\" - " +
                    "ListaAFPs: {ListaAFPs} - ListaFondos: {ListaFondos} - FechaInicial: {FechaInicial} - Fecha Final: {FechaFinal}.",
                    HttpContext.Request.Method, ControllerContext.ActionDescriptor.ControllerName, ControllerContext.ActionDescriptor.ActionName,
                    stopwatch.ElapsedMilliseconds, StatusCodes.Status400BadRequest, User.Identity?.IsAuthenticated ?? false,
                    listaAFPs, listaFondos, fechaInicial, fechaFinal);

                return ValidationProblem();
            }
            if (dtFechaInicio > dtFechaFinal) {
                ModelState.AddModelError(
                    nameof(fechaInicial),
                    "El parámetro fechaInicio no puede representar una fecha posterior al parámetro fechaFinal.");

                logger.LogInformation(
                    "[{Method}] - [{Controller}] - [{Action}] - [{ElapsedTime} ms] - [{StatusCode}] - [Usuario Autenticado: {IsAuthenticated}]" +
                    "Error en validación de parámetros de entrada, \"fechaInicial\" es mayor a \"fechaFinal\" - " +
                    "ListaAFPs: {ListaAFPs} - ListaFondos: {ListaFondos} - FechaInicial: {FechaInicial} - Fecha Final: {FechaFinal}.",
                    HttpContext.Request.Method, ControllerContext.ActionDescriptor.ControllerName, ControllerContext.ActionDescriptor.ActionName,
                    stopwatch.ElapsedMilliseconds, StatusCodes.Status400BadRequest, User.Identity?.IsAuthenticated ?? false,
                    listaAFPs, listaFondos, fechaInicial, fechaFinal);

                return ValidationProblem();
            }

            // Si viene con un API key, se valida...
            bool apiKeyValida = false;
            if (!string.IsNullOrEmpty(xApiKey)) {
                apiKeyValida = await apiKeyDAO.Validar(xApiKey);
                if (!apiKeyValida) {

                    logger.LogInformation(
                        "[{Method}] - [{Controller}] - [{Action}] - [{ElapsedTime} ms] - [{StatusCode}] - [Usuario Autenticado: {IsAuthenticated}]" +
                        "API key no es válida - " +
                        "ListaAFPs: {ListaAFPs} - ListaFondos: {ListaFondos} - FechaInicial: {FechaInicial} - Fecha Final: {FechaFinal}.",
                        HttpContext.Request.Method, ControllerContext.ActionDescriptor.ControllerName, ControllerContext.ActionDescriptor.ActionName,
                        stopwatch.ElapsedMilliseconds, StatusCodes.Status401Unauthorized, User.Identity?.IsAuthenticated ?? false,
                        listaAFPs, listaFondos, fechaInicial, fechaFinal);

                    return Unauthorized();
                }
            }

            // Se valida que si el usuario no ha iniciado sesión (y no incluye una API key válida)...
            if ((User.Identity == null || !User.Identity.IsAuthenticated) && !apiKeyValida) {
                // Solo consulte por el año actual +/ -7 días...
                DateTime? ultimaFechaAlgunValorCuota = await cuotaUfComisionDAO.UltimaFechaAlguna();
                if (ultimaFechaAlgunValorCuota == null) {
                    ultimaFechaAlgunValorCuota = TimeZoneInfo.ConvertTime(DateTime.Now, TimeZoneConverter.TZConvert.GetTimeZoneInfo("Pacific SA Standard Time"));
                }

                DateOnly fechaMinima = DateOnly.FromDateTime(ultimaFechaAlgunValorCuota.Value.AddYears(-1).AddDays(-7));
                DateOnly fechaMaxima = DateOnly.FromDateTime(ultimaFechaAlgunValorCuota.Value.AddDays(7));
                if (fechaMinima != DateOnly.FromDateTime(dtFechaInicio) || fechaMaxima != DateOnly.FromDateTime(dtFechaFinal)) {

                    logger.LogInformation(
                        "[{Method}] - [{Controller}] - [{Action}] - [{ElapsedTime} ms] - [{StatusCode}] - [Usuario Autenticado: {IsAuthenticated}] - [Con API Key: {ApiKeyValida}]" +
                        "Se solicitan fechas distintas a mínimo o máximo para usuario no autenticado - " +
                        "ListaAFPs: {ListaAFPs} - ListaFondos: {ListaFondos} - FechaInicial: {FechaInicial} - Fecha Final: {FechaFinal}.",
                        HttpContext.Request.Method, ControllerContext.ActionDescriptor.ControllerName, ControllerContext.ActionDescriptor.ActionName,
                        stopwatch.ElapsedMilliseconds, StatusCodes.Status401Unauthorized, User.Identity?.IsAuthenticated ?? false, apiKeyValida,
                        listaAFPs, listaFondos, fechaInicial, fechaFinal);

                    return Unauthorized();
                }


                // Solo consulte por una AFP y todos los fondos, o todas las AFP con un solo fondo...
                string[] afps = listaAFPs.Split(",");
                string[] fondos = listaFondos.Split(",");
                if (afps.Length == 1) {
                    if (fondos.Length != 5) {

                        logger.LogInformation(
                            "[{Method}] - [{Controller}] - [{Action}] - [{ElapsedTime} ms] - [{StatusCode}] - [Usuario Autenticado: {IsAuthenticated}] - [Con API Key: {ApiKeyValida}]" +
                            "Se solicita una sola AFP pero no todos los fondos para usuario no autenticado - " +
                            "ListaAFPs: {ListaAFPs} - ListaFondos: {ListaFondos} - FechaInicial: {FechaInicial} - Fecha Final: {FechaFinal}.",
                            HttpContext.Request.Method, ControllerContext.ActionDescriptor.ControllerName, ControllerContext.ActionDescriptor.ActionName,
                            stopwatch.ElapsedMilliseconds, StatusCodes.Status401Unauthorized, User.Identity?.IsAuthenticated ?? false, apiKeyValida,
                            listaAFPs, listaFondos, fechaInicial, fechaFinal);

                        return Unauthorized();
                    }
                } else if (fondos.Length == 1) {
                    if (afps.Length != 7) {

                        logger.LogInformation(
                            "[{Method}] - [{Controller}] - [{Action}] - [{ElapsedTime} ms] - [{StatusCode}] - [Usuario Autenticado: {IsAuthenticated}] - [Con API Key: {ApiKeyValida}]" +
                            "Se solicita un solo fondo pero no todas las AFP para usuario no autenticado - " +
                            "ListaAFPs: {ListaAFPs} - ListaFondos: {ListaFondos} - FechaInicial: {FechaInicial} - Fecha Final: {FechaFinal}.",
                            HttpContext.Request.Method, ControllerContext.ActionDescriptor.ControllerName, ControllerContext.ActionDescriptor.ActionName,
                            stopwatch.ElapsedMilliseconds, StatusCodes.Status401Unauthorized, User.Identity?.IsAuthenticated ?? false, apiKeyValida,
                            listaAFPs, listaFondos, fechaInicial, fechaFinal);

                        return Unauthorized();
                    }
                } else {

                    logger.LogInformation(
                        "[{Method}] - [{Controller}] - [{Action}] - [{ElapsedTime} ms] - [{StatusCode}] - [Usuario Autenticado: {IsAuthenticated}] - [Con API Key: {ApiKeyValida}]" +
                        "Se solicita más de un fondo y más de una AFP para usuario no autenticado - " +
                        "ListaAFPs: {ListaAFPs} - ListaFondos: {ListaFondos} - FechaInicial: {FechaInicial} - Fecha Final: {FechaFinal}.",
                        HttpContext.Request.Method, ControllerContext.ActionDescriptor.ControllerName, ControllerContext.ActionDescriptor.ActionName,
                        stopwatch.ElapsedMilliseconds, StatusCodes.Status401Unauthorized, User.Identity?.IsAuthenticated ?? false, apiKeyValida,
                        listaAFPs, listaFondos, fechaInicial, fechaFinal);

                    return Unauthorized();
                }
            }

            List<CuotaUf> salida = await cuotaUfComisionDAO.ObtenerCuotas(listaAFPs, listaFondos, dtFechaInicio, dtFechaFinal);
            long elapsedTimeObtenerCuotas = stopwatch.ElapsedMilliseconds;

            if (apiKeyValida) {
                await apiKeyDAO.IngresarHistorialUso(
                    xApiKey!,
                    DateTimeOffset.UtcNow,
                    Url.Action("ObtenerCuotas", "Cuota")!,
                    System.Text.Json.JsonSerializer.Serialize(new {
                        listaAFPs,
                        listaFondos,
                        fechaInicial,
                        fechaFinal
                    }),
                    StatusCodes.Status200OK,
                    salida.Count
                );
            }

            logger.LogInformation(
                "[{Method}] - [{Controller}] - [{Action}] - [{ElapsedTime} ms] - [{StatusCode}] - [Usuario Autenticado: {IsAuthenticated}] - [Con API Key: {ApiKeyValida}]" +
                "Se retornan exitosamente los valores cuota - " +
                "ListaAFPs: {ListaAFPs} - ListaFondos: {ListaFondos} - FechaInicial: {FechaInicial} - Fecha Final: {FechaFinal} - " +
                "Cant. Registros: {CantRegistros} - " +
                "Elapsed Time Obtener Cuotas: {ElapsedTimeObtenerCuotas}",
                HttpContext.Request.Method, ControllerContext.ActionDescriptor.ControllerName, ControllerContext.ActionDescriptor.ActionName,
                stopwatch.ElapsedMilliseconds, StatusCodes.Status200OK, User.Identity?.IsAuthenticated ?? false, apiKeyValida,
                listaAFPs, listaFondos, fechaInicial, fechaFinal, salida.Count,
                elapsedTimeObtenerCuotas);

            return salida; 
        }

        // POST: CuotaUfComision/ObtenerUltimaCuota
        // Parameters: listaAFPs=CAPITAL,UNO&listaFondos=A,B&listaFechas=05/01/2020,05/02/2020&tipoComision=1
        [Route("[action]")]
        [HttpPost]
        public async Task<ActionResult<List<SalObtenerUltimaCuota>>> ObtenerUltimaCuota(EntObtenerUltimaCuota entrada) {
            Stopwatch stopwatch = Stopwatch.StartNew();

            if (string.IsNullOrWhiteSpace(entrada.ListaAFPs)) {
                ModelState.AddModelError(
                    nameof(entrada.ListaAFPs),
                    "El parámetro listaAFPs debe incluir por lo menos el nombre de una AFP. Ejemplo: CAPITAL.");

                logger.LogInformation(
                    "[{Method}] - [{Controller}] - [{Action}] - [{ElapsedTime} ms] - [{StatusCode}] - [Usuario Autenticado: {IsAuthenticated}] - " +
                    "Error en validación de parámetros de entrada, no se incluye el parámetro \"ListaAFPs\" - " +
                    "ListaAFPs: {ListaAFPs} - ListaFondos: {ListaFondos} - ListaFechas: {ListaFechas} - TipoComision: {TipoComision}.",
                    HttpContext.Request.Method, ControllerContext.ActionDescriptor.ControllerName, ControllerContext.ActionDescriptor.ActionName,
                    stopwatch.ElapsedMilliseconds, StatusCodes.Status400BadRequest, User.Identity?.IsAuthenticated ?? false,
                    entrada.ListaAFPs, entrada.ListaFondos, entrada.ListaFechas, entrada.TipoComision);

                return ValidationProblem();
            }

            if (string.IsNullOrWhiteSpace(entrada.ListaFondos)) {
                ModelState.AddModelError(
                    nameof(entrada.ListaFondos),
                    "El parámetro listaFondos debe incluir por lo menos un fondo. Ejemplo: A.");

                logger.LogInformation(
                    "[{Method}] - [{Controller}] - [{Action}] - [{ElapsedTime} ms] - [{StatusCode}] - [Usuario Autenticado: {IsAuthenticated}] - " +
                    "Error en validación de parámetros de entrada, no se incluye el parámetro \"ListaFondos\" - " +
                    "ListaAFPs: {ListaAFPs} - ListaFondos: {ListaFondos} - ListaFechas: {ListaFechas} - TipoComision: {TipoComision}.",
                    HttpContext.Request.Method, ControllerContext.ActionDescriptor.ControllerName, ControllerContext.ActionDescriptor.ActionName,
                    stopwatch.ElapsedMilliseconds, StatusCodes.Status400BadRequest, User.Identity?.IsAuthenticated ?? false,
                    entrada.ListaAFPs, entrada.ListaFondos, entrada.ListaFechas, entrada.TipoComision);

                return ValidationProblem();
            }

            if (string.IsNullOrWhiteSpace(entrada.ListaFechas)) {
                ModelState.AddModelError(
                    nameof(entrada.ListaFechas),
                    "El parámetro listaFechas debe incluir por lo menos una fecha. Ejemplo: 31/12/2020.");

                logger.LogInformation(
                    "[{Method}] - [{Controller}] - [{Action}] - [{ElapsedTime} ms] - [{StatusCode}] - [Usuario Autenticado: {IsAuthenticated}] - " +
                    "Error en validación de parámetros de entrada, no se incluye el parámetro \"ListaFechas\" - " +
                    "ListaAFPs: {ListaAFPs} - ListaFondos: {ListaFondos} - ListaFechas: {ListaFechas} - TipoComision: {TipoComision}.",
                    HttpContext.Request.Method, ControllerContext.ActionDescriptor.ControllerName, ControllerContext.ActionDescriptor.ActionName,
                    stopwatch.ElapsedMilliseconds, StatusCodes.Status400BadRequest, User.Identity?.IsAuthenticated ?? false,
                    entrada.ListaAFPs, entrada.ListaFondos, entrada.ListaFechas, entrada.TipoComision);

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

                // Se valida que solo consulte por un fondo y por todas las AFPs...
                string[] fondos = entrada.ListaFondos.Split(",");
                string[] afps = entrada.ListaAFPs.Split(",");
                if (fondos.Length != 1 || afps.Length != 7) {

                    logger.LogInformation(
                        "[{Method}] - [{Controller}] - [{Action}] - [{ElapsedTime} ms] - [{StatusCode}] - [Usuario Autenticado: {IsAuthenticated}] - " +
                        "Se solicita más de un fondo o no todas las AFP - " +
                        "ListaAFPs: {ListaAFPs} - ListaFondos: {ListaFondos} - ListaFechas: {ListaFechas} - TipoComision: {TipoComision}.",
                        HttpContext.Request.Method, ControllerContext.ActionDescriptor.ControllerName, ControllerContext.ActionDescriptor.ActionName,
                        stopwatch.ElapsedMilliseconds, StatusCodes.Status401Unauthorized, User.Identity?.IsAuthenticated ?? false,
                        entrada.ListaAFPs, entrada.ListaFondos, entrada.ListaFechas, entrada.TipoComision);

                    return Unauthorized();
                }
            }

            string[] fechas = entrada.ListaFechas.Replace(" ", "").Split(",");
            foreach (string fecha in fechas) {
                string[] diaMesAnno = fecha.Split("/");
                if (diaMesAnno.Length != 3) {
                    ModelState.AddModelError(
                        nameof(entrada.ListaFechas),
                        "Las fechas en el parámetro listaFechas deben tener formato dd/mm/yyyy. Ejemplo: 31/12/2020.");

                    logger.LogInformation(
                        "[{Method}] - [{Controller}] - [{Action}] - [{ElapsedTime} ms] - [{StatusCode}] - [Usuario Autenticado: {IsAuthenticated}] - " +
                        "Error en validación de parámetros de entrada, formato de fecha inválida en parámetro \"ListaFechas\" - " +
                        "ListaAFPs: {ListaAFPs} - ListaFondos: {ListaFondos} - ListaFechas: {ListaFechas} - TipoComision: {TipoComision}.",
                        HttpContext.Request.Method, ControllerContext.ActionDescriptor.ControllerName, ControllerContext.ActionDescriptor.ActionName,
                        stopwatch.ElapsedMilliseconds, StatusCodes.Status400BadRequest, User.Identity?.IsAuthenticated ?? false,
                        entrada.ListaAFPs, entrada.ListaFondos, entrada.ListaFechas, entrada.TipoComision);

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

                            logger.LogInformation(
                                "[{Method}] - [{Controller}] - [{Action}] - [{ElapsedTime} ms] - [{StatusCode}] - [Usuario Autenticado: {IsAuthenticated}] - " +
                                "Se solicita fecha intermedia menor o mayor a mínima/máximo - " +
                                "ListaAFPs: {ListaAFPs} - ListaFondos: {ListaFondos} - ListaFechas: {ListaFechas} - TipoComision: {TipoComision}.",
                                HttpContext.Request.Method, ControllerContext.ActionDescriptor.ControllerName, ControllerContext.ActionDescriptor.ActionName,
                                stopwatch.ElapsedMilliseconds, StatusCodes.Status401Unauthorized, User.Identity?.IsAuthenticated ?? false,
                                entrada.ListaAFPs, entrada.ListaFondos, entrada.ListaFechas, entrada.TipoComision);

                            return Unauthorized();
                        } else {
                            cantFechasIntermedias++;
                        }
                    }
                } catch (ArgumentOutOfRangeException) {
                    ModelState.AddModelError(
                        nameof(entrada.ListaFechas),
                        "Una de las fechas en el parámetro listaFechas no corresponden a una fecha válida.");

                    logger.LogInformation(
                        "[{Method}] - [{Controller}] - [{Action}] - [{ElapsedTime} ms] - [{StatusCode}] - [Usuario Autenticado: {IsAuthenticated}] - " +
                        "Error en validación de parámetros de entrada, fecha inválida en parámetro \"ListaFechas\" - " +
                        "ListaAFPs: {ListaAFPs} - ListaFondos: {ListaFondos} - ListaFechas: {ListaFechas} - TipoComision: {TipoComision}.",
                        HttpContext.Request.Method, ControllerContext.ActionDescriptor.ControllerName, ControllerContext.ActionDescriptor.ActionName,
                        stopwatch.ElapsedMilliseconds, StatusCodes.Status400BadRequest, User.Identity?.IsAuthenticated ?? false,
                        entrada.ListaAFPs, entrada.ListaFondos, entrada.ListaFechas, entrada.TipoComision);

                    return ValidationProblem();
                }
            }

            // Se valida que si el usuario no ha iniciado sesión debe si o si consultar por los valores mínimos y máximos...
            if (incluyeMinimo != null && !incluyeMinimo.Value) {

                logger.LogInformation(
                    "[{Method}] - [{Controller}] - [{Action}] - [{ElapsedTime} ms] - [{StatusCode}] - [Usuario Autenticado: {IsAuthenticated}] - " +
                    "No se solicita fecha mínima para usuario no autenticado - " +
                    "ListaAFPs: {ListaAFPs} - ListaFondos: {ListaFondos} - ListaFechas: {ListaFechas} - TipoComision: {TipoComision}.",
                    HttpContext.Request.Method, ControllerContext.ActionDescriptor.ControllerName, ControllerContext.ActionDescriptor.ActionName,
                    stopwatch.ElapsedMilliseconds, StatusCodes.Status401Unauthorized, User.Identity?.IsAuthenticated ?? false,
                    entrada.ListaAFPs, entrada.ListaFondos, entrada.ListaFechas, entrada.TipoComision);

                return Unauthorized();
            }

            if (incluyeMaximo != null && !incluyeMaximo.Value) {

                logger.LogInformation(
                    "[{Method}] - [{Controller}] - [{Action}] - [{ElapsedTime} ms] - [{StatusCode}] - [Usuario Autenticado: {IsAuthenticated}] - " +
                    "No se solicita fecha máxima para usuario no autenticado - " +
                    "ListaAFPs: {ListaAFPs} - ListaFondos: {ListaFondos} - ListaFechas: {ListaFechas} - TipoComision: {TipoComision}.",
                    HttpContext.Request.Method, ControllerContext.ActionDescriptor.ControllerName, ControllerContext.ActionDescriptor.ActionName,
                    stopwatch.ElapsedMilliseconds, StatusCodes.Status401Unauthorized, User.Identity?.IsAuthenticated ?? false,
                    entrada.ListaAFPs, entrada.ListaFondos, entrada.ListaFechas, entrada.TipoComision);

                return Unauthorized();
            }

            // Se valida que si el usuario no ha iniciado sesión solo consulte por 12 fechas intermedias...
            if (cantFechasIntermedias != null && cantFechasIntermedias > 12) {

                logger.LogInformation(
                    "[{Method}] - [{Controller}] - [{Action}] - [{ElapsedTime} ms] - [{StatusCode}] - [Usuario Autenticado: {IsAuthenticated}] - " +
                    "Se solicitan más de doce fechas intermedias para usuario no autenticado - " +
                    "ListaAFPs: {ListaAFPs} - ListaFondos: {ListaFondos} - ListaFechas: {ListaFechas} - TipoComision: {TipoComision}.",
                    HttpContext.Request.Method, ControllerContext.ActionDescriptor.ControllerName, ControllerContext.ActionDescriptor.ActionName,
                    stopwatch.ElapsedMilliseconds, StatusCodes.Status401Unauthorized, User.Identity?.IsAuthenticated ?? false,
                    entrada.ListaAFPs, entrada.ListaFondos, entrada.ListaFechas, entrada.TipoComision);

                return Unauthorized();
            }

            List<SalObtenerUltimaCuota> salida = await cuotaUfComisionDAO.ObtenerUltimaCuota(entrada.ListaAFPs, entrada.ListaFondos, entrada.ListaFechas, entrada.TipoComision);

            logger.LogInformation(
                "[{Method}] - [{Controller}] - [{Action}] - [{ElapsedTime} ms] - [{StatusCode}] - [Usuario Autenticado: {IsAuthenticated}] - " +
                "Se retornan exitosamente las últimas cuotas - " +
                "ListaAFPs: {ListaAFPs} - ListaFondos: {ListaFondos} - ListaFechas: {ListaFechas} - TipoComision: {TipoComision} - " +
                "Cant. Registros: {CantRegistros}.",
                HttpContext.Request.Method, ControllerContext.ActionDescriptor.ControllerName, ControllerContext.ActionDescriptor.ActionName,
                stopwatch.ElapsedMilliseconds, StatusCodes.Status200OK, User.Identity?.IsAuthenticated ?? false,
                entrada.ListaAFPs, entrada.ListaFondos, entrada.ListaFechas, entrada.TipoComision, salida.Count);

            return salida;
        }

        // GET: api/Cuota/DescargarCuotasCSV?listaAFPs=CAPITAL,UNO&listaFondos=A,B&fechaInicio=01/01/2020&fechaFinal=31/12/2020
        [Route("[action]")]
        [HttpGet]
        public async Task<ActionResult> DescargarCuotasCSV(string listaAFPs, string listaFondos, string fechaInicial, string fechaFinal) {
            Stopwatch stopwatch = Stopwatch.StartNew();

            // Si el usuario no está autenticado, se le envía a autenticarse...
            if (User.Identity == null || !User.Identity.IsAuthenticated) {

                logger.LogInformation(
                    "[{Method}] - [{Controller}] - [{Action}] - [{ElapsedTime} ms] - [{StatusCode}] - [Usuario Autenticado: {IsAuthenticated}] - " +
                    "Se redirecciona a challenge dado que usuario no está autenticado - " +
                    "ListaAFPs: {ListaAFPs} - ListaFondos: {ListaFondos} - FechaInicial: {FechaInicial} - FechaFinal: {FechaFinal}.",
                    HttpContext.Request.Method, ControllerContext.ActionDescriptor.ControllerName, ControllerContext.ActionDescriptor.ActionName,
                    stopwatch.ElapsedMilliseconds, StatusCodes.Status401Unauthorized, User.Identity?.IsAuthenticated ?? false,
                    listaAFPs, listaFondos, fechaInicial, fechaFinal);

                return Challenge();
            }

            // Si llegamos a esta URL sin Referer, se asume que llegamos desde pantalla de login, por lo que se redirecciona a pantalla de acceso...
            if (string.IsNullOrEmpty(Request.Headers.Referer.ToString())) {

                logger.LogInformation(
                    "[{Method}] - [{Controller}] - [{Action}] - [{ElapsedTime} ms] - [{StatusCode}] - [Usuario Autenticado: {IsAuthenticated}] - " +
                    "Se redirecciona a pantalla de acceder cuotas dado que no encuentre referer - " +
                    "ListaAFPs: {ListaAFPs} - ListaFondos: {ListaFondos} - FechaInicial: {FechaInicial} - FechaFinal: {FechaFinal}.",
                    HttpContext.Request.Method, ControllerContext.ActionDescriptor.ControllerName, ControllerContext.ActionDescriptor.ActionName,
                    stopwatch.ElapsedMilliseconds, StatusCodes.Status400BadRequest, User.Identity?.IsAuthenticated ?? false,
                    listaAFPs, listaFondos, fechaInicial, fechaFinal);

                return RedirectToAction("Index", "AccederCuotas");
            }

            string[] diaMesAnnoInicio = fechaInicial.Split("/");
            string[] diaMesAnnoFinal = fechaFinal.Split("/");

            DateTime dtFechaInicio = new(
                    int.Parse(diaMesAnnoInicio[2]),
                    int.Parse(diaMesAnnoInicio[1]),
                    int.Parse(diaMesAnnoInicio[0])
            );
            DateTime dtFechaFinal = new(
                    int.Parse(diaMesAnnoFinal[2]),
                    int.Parse(diaMesAnnoFinal[1]),
                    int.Parse(diaMesAnnoFinal[0]
            ));

            if (dtFechaInicio > dtFechaFinal) {
                ModelState.AddModelError(
                    nameof(fechaInicial),
                    "El parámetro fechaInicio no puede representar una fecha posterior al parámetro fechaFinal.");

                logger.LogInformation(
                    "[{Method}] - [{Controller}] - [{Action}] - [{ElapsedTime} ms] - [{StatusCode}] - [Usuario Autenticado: {IsAuthenticated}] - " +
                    "Error en validación de parámetros de entrada, \"fechaInicial\" es mayor a \"fechaFinal\" - " +
                    "ListaAFPs: {ListaAFPs} - ListaFondos: {ListaFondos} - FechaInicial: {FechaInicial} - FechaFinal: {FechaFinal}.",
                    HttpContext.Request.Method, ControllerContext.ActionDescriptor.ControllerName, ControllerContext.ActionDescriptor.ActionName,
                    stopwatch.ElapsedMilliseconds, StatusCodes.Status400BadRequest, User.Identity?.IsAuthenticated ?? false,
                    listaAFPs, listaFondos, fechaInicial, fechaFinal);

                return ValidationProblem();
            }

            List<CuotaUf> retorno = await cuotaUfComisionDAO.ObtenerCuotas(listaAFPs, listaFondos, dtFechaInicio, dtFechaFinal);

            StringBuilder sb = new();
            sb.AppendLine($"AFP;FECHA;FONDO;VALOR_CUOTA;VALOR_UF");
            foreach (CuotaUf cuota in retorno) {
                sb.AppendLine(
                    $"{cuota.Afp};" +
                    $"{cuota.Fecha};" +
                    $"{cuota.Fondo};" +
                    $"{cuota.Valor.ToString(CultureInfo.InvariantCulture).Replace(".", ",")};" +
                    $"{cuota.ValorUf?.ToString(CultureInfo.InvariantCulture).Replace(".", ",")}");
            }

            byte[] bytes = new UTF8Encoding().GetBytes(sb.ToString());

            logger.LogInformation(
                "[{Method}] - [{Controller}] - [{Action}] - [{ElapsedTime} ms] - [{StatusCode}] - [Usuario Autenticado: {IsAuthenticated}] - " +
                "Se retornan exitosamente archivo csv - " +
                "ListaAFPs: {ListaAFPs} - ListaFondos: {ListaFondos} - FechaInicial: {FechaInicial} - FechaFinal: {FechaFinal} - " +
                "Cant. Registros: {CantRegistros} - Cant. Bytes: {CantBytes}.",
                HttpContext.Request.Method, ControllerContext.ActionDescriptor.ControllerName, ControllerContext.ActionDescriptor.ActionName,
                stopwatch.ElapsedMilliseconds, StatusCodes.Status200OK, User.Identity?.IsAuthenticated ?? false,
                listaAFPs, listaFondos, fechaInicial, fechaFinal, retorno.Count, bytes.Length);

            return File(bytes, "text/csv", $"QueTalMiAFP_{DateTime.Now:yyyyMMddHHmmss}.csv");
        }
    }
}
