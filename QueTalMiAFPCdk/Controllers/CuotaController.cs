using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using Newtonsoft.Json;
using QueTalMiAFP.Models.Others;
using QueTalMiAFP.Repositories;
using QueTalMiAFP.Services;
using System;
using System.Globalization;
using System.Net;
using System.Text;
using System.Text.Json;
using JsonSerializer = System.Text.Json.JsonSerializer;

namespace QueTalMiAFP.Controllers {
    [Route("api/[controller]")]
    [ApiController]
    public class CuotaController : ControllerBase {
        private readonly IConfiguration _configuration;
        private readonly ICuotaUfComisionDAO _cuotaUfComisionDAO;

        private readonly string _baseUrl;
        private readonly string _xApiKey;

        public CuotaController(IConfiguration configuration, ICuotaUfComisionDAO cuotaUfComisionDAO) {
            _configuration = configuration;
            _cuotaUfComisionDAO = cuotaUfComisionDAO;

            _baseUrl = _configuration.GetValue<string>("AWSGatewayAPIKey:api-url")!;
            _xApiKey = _configuration.GetValue<string>("AWSGatewayAPIKey:x-api-key")!;
        }

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
                fechaFinal = DateTime.Now.ToString("dd/MM/yyyy");
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

            Dictionary<string, string?> parameters = new Dictionary<string, string?>() {
                { "listaAFPs", listaAFPs },
                { "listaFondos", listaFondos },
                { "fechaInicial", dtFechaInicio.ToString("dd/MM/yyyy") },
                { "fechaFinal", dtFechaFinal.ToString("dd/MM/yyyy") }
            };

			using HttpClient client = new HttpClient(new RetryHandler(new HttpClientHandler(), _configuration));
			client.DefaultRequestHeaders.Add("x-api-key", _xApiKey);
			string requestUri = QueryHelpers.AddQueryString(_baseUrl + "CuotaUfComision/ObtenerCuotas", parameters);
			HttpResponseMessage response = await client.GetAsync(requestUri);
            using Stream responseStream = await response.Content.ReadAsStreamAsync();
            JsonSerializerOptions options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            SalObtenerCuotas? retornoConsulta = await JsonSerializer.DeserializeAsync<SalObtenerCuotas>(responseStream, options);

            if (retornoConsulta!.S3Url == null) {
                return retornoConsulta!.ListaCuotas!;
			} else {
                HttpResponseMessage responseS3 = await client.GetAsync(retornoConsulta.S3Url);
                using Stream responseS3Stream = await responseS3.Content.ReadAsStreamAsync();
                JsonSerializerOptions optionsS3 = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                return (await JsonSerializer.DeserializeAsync<IEnumerable<CuotaUf>>(responseS3Stream, optionsS3))!.ToList();
			}
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
                } catch (ArgumentOutOfRangeException) {
                    ModelState.AddModelError(
                        nameof(entrada.ListaFechas),
                        "Una de las fechas en el parámetro listaFechas no corresponden a una fecha válida.");
                    return ValidationProblem();
                }
            }

            using HttpClient client = new HttpClient(new RetryHandler(new HttpClientHandler(), _configuration));
            client.DefaultRequestHeaders.Add("x-api-key", _xApiKey);
            var response = await client.PostAsync(_baseUrl + "CuotaUfComision/ObtenerUltimaCuota", new StringContent(WebUtility.HtmlEncode(JsonConvert.SerializeObject(entrada)), Encoding.UTF8, "application/json"));
            using Stream responseStream = await response.Content.ReadAsStreamAsync();
            JsonSerializerOptions options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            return (await JsonSerializer.DeserializeAsync<IEnumerable<SalObtenerUltimaCuota>>(responseStream, options))!.ToList();
        }

        // GET: api/Cuota/ObtenerRentabilidadRealUltimoAnno
        [Route("[action]")]
        [HttpGet]
        public async Task<ActionResult<List<RentabilidadReal>>> ObtenerRentabilidadRealUltimoAnno() {
            DateTime fechaFinal = await _cuotaUfComisionDAO.UltimaFechaTodas();
            DateTime fechaInicial = fechaFinal.AddYears(-1);

            return await _cuotaUfComisionDAO.ObtenerRentabilidadReal(
                "CAPITAL,CUPRUM,HABITAT,MODELO,PLANVITAL,PROVIDA,UNO",
                "A,B,C,D,E",
                fechaInicial,
                fechaFinal);
        }

        // GET: api/Cuota/DescargarCuotasCSV?listaAFPs=CAPITAL,UNO&listaFondos=A,B&fechaInicio=01/01/2020&fechaFinal=31/12/2020
        [Route("[action]")]
        [HttpGet]
        public async Task<FileContentResult> DescargarCuotasCSV(string listaAFPs, string listaFondos, string fechaInicial, string fechaFinal) {
            ActionResult<List<CuotaUf>> retorno = await ObtenerCuotas(listaAFPs, listaFondos, fechaInicial, fechaFinal);

            StringBuilder sb = new StringBuilder();
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
