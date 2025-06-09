using HtmlAgilityPack;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using QueTalMiAFP.Models.Entities;
using QueTalMiAFP.Models.Others;
using QueTalMiAFP.Services;
using QueTalMiAFP.Services.Exceptions;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Xml;

namespace QueTalMiAFPCdk.Services {
	public class Extractor(IConfiguration configuration) {
		public const string NOMBRE_CAPITAL = "CAPITAL";
		public const string NOMBRE_CUPRUM = "CUPRUM";
		public const string NOMBRE_HABITAT = "HABITAT";
		public const string NOMBRE_MODELO = "MODELO";
		public const string NOMBRE_PLANVITAL = "PLANVITAL";
		public const string NOMBRE_PROVIDA = "PROVIDA";
		public const string NOMBRE_UNO = "UNO";

		public static readonly HashSet<string> AFPs = [
			NOMBRE_CAPITAL, NOMBRE_CUPRUM, NOMBRE_HABITAT,
			NOMBRE_MODELO, NOMBRE_PLANVITAL, NOMBRE_PROVIDA,
			NOMBRE_UNO ];

		public List<Log> Logs = [];

		public readonly List<Uf> UFS_NO_ENCONTRADAS = [
			new Uf() { Fecha = new DateTime(2015, 12, 12), Valor = 25629.09M },
			new Uf() { Fecha = new DateTime(2015, 12, 31), Valor = 25629.09M }
		];

		public readonly List<Uf> UFS_ERRONEAS = [
			new Uf() { Fecha = new DateTime(2014, 12, 29), Valor = 24627.10M },
			new Uf() { Fecha = new DateTime(2014, 12, 30), Valor = 24627.10M }
		];

		private readonly string _afpModeloUrlApiBase = configuration.GetValue<string>("Extractor:AFPModelo:UrlApiBase")!;
        private readonly string _afpModeloV2UrlApiBase = configuration.GetValue<string>("Extractor:AFPModeloV2:UrlApiBase")!;
        private readonly string _afpModeloV3UrlApiBase = configuration.GetValue<string>("Extractor:AFPModeloV3:UrlApiBase")!;
        private readonly string _afpModeloV3Base64Key = configuration.GetValue<string>("Extractor:AFPModeloV3:Base64Key")!;
		private readonly string _afpModeloV3Base64IV = configuration.GetValue<string>("Extractor:AFPModeloV3:Base64IV")!;
        private readonly string _afpCuprumUrlApiBase = configuration.GetValue<string>("Extractor:AFPCuprum:UrlApiBase")!;
        private readonly string _afpCapitalUrlApiBase = configuration.GetValue<string>("Extractor:AFPCapital:UrlApiBase")!;
        private readonly string _afpHabitatUrlApiBase = configuration.GetValue<string>("Extractor:AFPHabitat:UrlApiBase")!;
        private readonly string _afpPlanvitalUrlApiBase = configuration.GetValue<string>("Extractor:AFPPlanvital:UrlApiBase")!;
        private readonly string _afpProvidaUrlApiBase = configuration.GetValue<string>("Extractor:AFPProvida:UrlApiBase")!;
        private readonly string _afpUnoUrlApiBase = configuration.GetValue<string>("Extractor:AFPUno:UrlApiBase")!;
        private readonly string _valoresUfUrlApiBase = configuration.GetValue<string>("Extractor:ValoresUf:UrlApiBase")!;
        private readonly string _comisionesUrlApiBase = configuration.GetValue<string>("Extractor:Comisiones:UrlApiBase")!;
        private readonly string _comisionesCavUrlApiBase = configuration.GetValue<string>("Extractor:ComisionesCav:UrlApiBase")!;

        public async Task<List<Cuota>> ObtenerCuotasModelo(DateTime fechaInicio, DateTime fechaFinal, string? fondo) {
			try {
				RegistrarLog(string.Format("Extrayendo valores cuota de {0} - Fondo {1} - Fecha Inicio {2} - Fecha Final {3}",
					NOMBRE_MODELO,
                    fondo ?? "*",
					fechaInicio.ToString("dd/MM/yyyy"),
					fechaFinal.ToString("dd/MM/yyyy")));

				string url_api_base = _afpModeloUrlApiBase;
				string url_api = string.Format(url_api_base,
					fondo == "A" || fondo == null ? 1 : 0,
					fondo == "B" || fondo == null ? 1 : 0,
					fondo == "C" || fondo == null ? 1 : 0,
					fondo == "D" || fondo == null ? 1 : 0,
					fondo == "E" || fondo == null ? 1 : 0,
					fechaInicio.ToString("yyyy-MM-dd"),
					fechaFinal.ToString("yyyy-MM-dd"));

				HttpWebRequest request = WebRequest.CreateHttp(url_api);
				request.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate | DecompressionMethods.Brotli;
                using HttpWebResponse response = (HttpWebResponse)await request.GetResponseAsync();
                using Stream stream = response.GetResponseStream();
                using StreamReader reader = new(stream);
                string contenido = await reader.ReadToEndAsync();
                JArray arreglo = JArray.Parse(contenido);

                List<Cuota> retorno = [];
                foreach (JObject nodoFondo in arreglo.Cast<JObject>()) {
                    string tipoFondo = nodoFondo["name"]!.Value<string>()!.Replace("Fondo ", "").Trim();
                    foreach (JArray valorCuota in nodoFondo["data"]!.Cast<JArray>()) {
                        DateTime fecha = DateTimeOffset.FromUnixTimeMilliseconds((long)valorCuota[0].Value<decimal>()).UtcDateTime;
                        decimal valor = valorCuota[1].Value<decimal>();
                        retorno.Add(new Cuota() {
                            Afp = NOMBRE_MODELO,
                            Fecha = fecha,
                            Fondo = tipoFondo,
                            Valor = valor
                        });
                    }
                }

                RegistrarLog(string.Format("Terminó extracción de {0} - Fondo {1} - Fecha Inicio {2} - Fecha Final {3} - {4} Valores Cuota",
                    NOMBRE_MODELO,
                    fondo ?? "*",
                    fechaInicio.ToString("dd/MM/yyyy"),
                    fechaFinal.ToString("dd/MM/yyyy"),
                    retorno.Count));

                return retorno;
            } catch (Exception ex) {
				throw new ExcepcionValorCuota(ex) {
					Afp = NOMBRE_MODELO,
					FechaInicio = fechaInicio,
					FechaFinal = fechaFinal,
					Fondo = fondo
				};
			}
		}

		public async Task<List<Cuota>> ObtenerCuotasModeloV2(DateTime fechaInicio, DateTime fechaFinal, string? fondo) {
            try {
				RegistrarLog(string.Format("Extrayendo valores cuota de {0} - Fondo {1} - Fecha Inicio {2} - Fecha Final {3}",
					NOMBRE_MODELO,
                    fondo ?? "*",
					fechaInicio.ToString("dd/MM/yyyy"),
					fechaFinal.ToString("dd/MM/yyyy")));

				string url_api_base = _afpModeloV2UrlApiBase;

				string parametros = string.Format("{{\"fechaIni\":\"{0}\",\"fechaFin\":\"{1}\"}}",
					fechaInicio.AddDays(-7).ToString("yyyy-MM-dd"),
					fechaFinal.AddDays(7).ToString("yyyy-MM-dd"));

				HttpWebRequest request = WebRequest.CreateHttp(url_api_base);
				request.Method = "POST";
				request.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate | DecompressionMethods.Brotli;
				request.ContentType = "application/json;charset=UTF-8";
				byte[] byteArray = Encoding.UTF8.GetBytes(parametros);
				request.ContentLength = byteArray.Length;
				using (Stream dataStream = await request.GetRequestStreamAsync()) {
					await dataStream.WriteAsync(byteArray);
                }

                using HttpWebResponse response = (HttpWebResponse)await request.GetResponseAsync();
                using Stream stream = response.GetResponseStream();
                using StreamReader reader = new(stream);
                string contenido = await reader.ReadToEndAsync();
                JObject json = JObject.Parse(contenido);

                List<string> listaFondos;
                if (fondo != null) {
                    listaFondos = [fondo];
                } else {
                    listaFondos = ["A", "B", "C", "D", "E"];
                }

                List<Cuota> retorno = [];
                foreach (string tipoFondo in listaFondos) {
                    foreach (JObject fechaValor in json["wmValoresCuotaAFPResponse"]!["wmValoresCuotaAFPResult"]!["diffgram"]!["NewDataSet"]!["TABLA_VALORES_CUOTA_AFP"]!.Cast<JObject>()) {
                        DateTime fecha = fechaValor["FECHA"]!.Value<DateTime>().ToUniversalTime().Date;
                        if (fechaInicio <= fecha && fecha <= fechaFinal) {
                            decimal valor = decimal.Parse(fechaValor["FONDO_" + tipoFondo]!.Value<string>()!.Replace(",", "."), CultureInfo.InvariantCulture);
                            retorno.Add(new Cuota() {
                                Afp = NOMBRE_MODELO,
                                Fecha = fecha,
                                Fondo = tipoFondo,
                                Valor = valor
                            });
                        }
                    }
                }

                RegistrarLog(string.Format("Terminó extracción de {0} - Fondo {1} - Fecha Inicio {2} - Fecha Final {3} - {4} Valores Cuota",
                    NOMBRE_MODELO,
                    fondo ?? "*",
                    fechaInicio.ToString("dd/MM/yyyy"),
                    fechaFinal.ToString("dd/MM/yyyy"),
                    retorno.Count));

                return retorno;

            } catch (Exception ex) {
				throw new ExcepcionValorCuota(ex) {
					Afp = NOMBRE_MODELO,
					FechaInicio = fechaInicio,
					FechaFinal = fechaFinal,
					Fondo = fondo
				};
			}
		}

        public async Task<List<Cuota>> ObtenerCuotasModeloV3(DateTime fechaInicio, DateTime fechaFinal, string? fondo) {
            try {
                RegistrarLog(string.Format("Extrayendo valores cuota de {0} - Fondo {1} - Fecha Inicio {2} - Fecha Final {3}",
                    NOMBRE_MODELO,
                    fondo ?? "*",
                    fechaInicio.ToString("dd/MM/yyyy"),
                    fechaFinal.ToString("dd/MM/yyyy")));

                string url_api_base = _afpModeloV3UrlApiBase;
				string base64Key = _afpModeloV3Base64Key;
                string base64IV = _afpModeloV3Base64IV;

                string parametros = string.Format("{{\"fechaIni\":\"{0}\",\"fechaFin\":\"{1}\"}}",
                    fechaInicio.AddDays(-7).ToString("yyyy-MM-dd"),
                    fechaFinal.AddDays(7).ToString("yyyy-MM-dd"));
				parametros = Aes256Cbc.Encriptar(parametros, base64Key, base64IV);
				parametros = string.Format("{{\"enc\":\"{0}\"}}", parametros);

                HttpWebRequest request = WebRequest.CreateHttp(url_api_base);
                request.Method = "POST";
                request.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate | DecompressionMethods.Brotli;
                request.ContentType = "application/json;charset=UTF-8";
                byte[] byteArray = Encoding.UTF8.GetBytes(parametros);
                request.ContentLength = byteArray.Length;
                using (Stream dataStream = await request.GetRequestStreamAsync()) {
                    await dataStream.WriteAsync(byteArray);
                }

                using HttpWebResponse response = (HttpWebResponse)await request.GetResponseAsync();
                using Stream stream = response.GetResponseStream();
                using StreamReader reader = new(stream);
                string contenido = await reader.ReadToEndAsync();
                JObject json = JObject.Parse(contenido);

                string respuestaEncriptada = json["respuesta"]!.Value<string>()!;
                string respuesta = Aes256Cbc.Desencriptar(respuestaEncriptada, base64Key, base64IV);
                json = JObject.Parse(respuesta);

                List<string> listaFondos;
                if (fondo != null) {
                    listaFondos = [fondo];
                } else {
                    listaFondos = ["A", "B", "C", "D", "E"];
                }

                List<Cuota> retorno = [];
                foreach (string tipoFondo in listaFondos) {
                    foreach (JObject fechaValor in json["wmValoresCuotaAFPResponse"]!["wmValoresCuotaAFPResult"]!["diffgram"]!["NewDataSet"]!["TABLA_VALORES_CUOTA_AFP"]!.Cast<JObject>()) {
                        DateTime fecha = fechaValor["FECHA"]!.Value<DateTime>().ToUniversalTime().Date;
                        if (fechaInicio <= fecha && fecha <= fechaFinal) {
                            decimal valor = decimal.Parse(fechaValor["FONDO_" + tipoFondo]!.Value<string>()!.Replace(",", "."), CultureInfo.InvariantCulture);
                            retorno.Add(new Cuota() {
                                Afp = NOMBRE_MODELO,
                                Fecha = fecha,
                                Fondo = tipoFondo,
                                Valor = valor
                            });
                        }
                    }
                }

                RegistrarLog(string.Format("Terminó extracción de {0} - Fondo {1} - Fecha Inicio {2} - Fecha Final {3} - {4} Valores Cuota",
                    NOMBRE_MODELO,
                    fondo ?? "*",
                    fechaInicio.ToString("dd/MM/yyyy"),
                    fechaFinal.ToString("dd/MM/yyyy"),
                    retorno.Count));

                return retorno;

            } catch (Exception ex) {
                throw new ExcepcionValorCuota(ex) {
                    Afp = NOMBRE_MODELO,
                    FechaInicio = fechaInicio,
                    FechaFinal = fechaFinal,
                    Fondo = fondo
                };
            }
        }

        public async Task<List<Cuota>> ObtenerCuotasCuprum(DateTime fechaInicio, DateTime fechaFinal, string? fondo) {
			try {
				RegistrarLog(string.Format("Extrayendo valores cuota de {0} - Fondo {1} - Fecha Inicio {2} - Fecha Final {3}",
					NOMBRE_CUPRUM,
                    fondo ?? "*",
					fechaInicio.ToString("dd/MM/yyyy"),
					fechaFinal.ToString("dd/MM/yyyy")));

				string url_api_base = _afpCuprumUrlApiBase;

				HttpWebRequest request = WebRequest.CreateHttp(url_api_base);
                request.UserAgent = GetRandomUserAgent();
                request.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate | DecompressionMethods.Brotli;
                using HttpWebResponse response = (HttpWebResponse)await request.GetResponseAsync();
                using Stream stream = response.GetResponseStream();
                using StreamReader reader = new(stream);
                string contenido = await reader.ReadToEndAsync();
                JObject json = JObject.Parse(contenido);

                List<string> listaFondos;
                if (fondo != null) {
                    listaFondos = [fondo];
                } else {
                    listaFondos = ["A", "B", "C", "D", "E"];
                }

                List<Cuota> retorno = [];
                foreach (string tipoFondo in listaFondos) {
                    foreach (JArray fechaValor in json[tipoFondo]!.Cast<JArray>()) {
                        DateTime fecha = DateTimeOffset.FromUnixTimeMilliseconds((long)fechaValor[0].Value<decimal>()).UtcDateTime;
                        if (fechaInicio <= fecha && fecha <= fechaFinal) {
                            retorno.Add(new Cuota() {
                                Afp = NOMBRE_CUPRUM,
                                Fecha = fecha,
                                Fondo = tipoFondo,
                                Valor = fechaValor[1].Value<decimal>()
                            });
                        }
                    }
                }

                RegistrarLog(string.Format("Terminó extracción de {0} - Fondo {1} - Fecha Inicio {2} - Fecha Final {3} - {4} Valores Cuota",
                    NOMBRE_CUPRUM,
                    fondo ?? "*",
                    fechaInicio.ToString("dd/MM/yyyy"),
                    fechaFinal.ToString("dd/MM/yyyy"),
                    retorno.Count));

                return retorno;
            } catch (Exception ex) {
				throw new ExcepcionValorCuota(ex) {
					Afp = NOMBRE_CUPRUM,
					FechaInicio = fechaInicio,
					FechaFinal = fechaFinal,
					Fondo = fondo
				};
			}
		}

		public async Task<List<Cuota>> ObtenerCuotasCapital(DateTime fechaInicio, DateTime fechaFinal, string? fondo) {
			try {
				RegistrarLog(string.Format("Extrayendo valores cuota de {0} - Fondo {1} - Fecha Inicio {2} - Fecha Final {3}",
					NOMBRE_CAPITAL,
                    fondo ?? "*",
					fechaInicio.ToString("dd/MM/yyyy"),
					fechaFinal.ToString("dd/MM/yyyy")));

				string url_api_base = _afpCapitalUrlApiBase;
				string url_api = string.Format(url_api_base,
					HttpUtility.UrlEncode(fondo ?? "A;B;C;D;E"),
					HttpUtility.UrlEncode(fechaInicio.ToString("dd/MM/yyyy")),
					HttpUtility.UrlEncode(fechaFinal.ToString("dd/MM/yyyy")));

				HttpWebRequest request = WebRequest.CreateHttp(url_api);
				request.ContentType = "application/x-www-form-urlencoded; charset=UTF-8";
				request.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate | DecompressionMethods.Brotli;
                using HttpWebResponse response = (HttpWebResponse)await request.GetResponseAsync();
                using Stream stream = response.GetResponseStream();
                using StreamReader reader = new(stream);
                string contenido = await reader.ReadToEndAsync();
                contenido = contenido.Replace("\\\"", "\"");
                contenido = contenido[1..^1];
                JObject json = JObject.Parse(contenido);

                List<string> tipoFondos;
                if (fondo == null) {
                    tipoFondos = ["A", "B", "C", "D", "E"];
                } else {
                    tipoFondos = [fondo];
                }

                List<Cuota> retorno = [];
                foreach (string tipoFondo in tipoFondos) {
                    foreach (JObject nodoFondo in json["fondo" + tipoFondo]!.Cast<JObject>()) {
                        string fondoAux = nodoFondo["Serie"]!.Value<string>()!;
                        DateTime fecha = DateTimeOffset.FromUnixTimeMilliseconds(
                            long.Parse(nodoFondo["Dia"]!.Value<string>()!.Replace("\\/Date(", "").Replace(")\\/", ""))
                            ).UtcDateTime;
                        fecha = new DateTime(fecha.Year, fecha.Month, fecha.Day);
                        decimal valor = nodoFondo["ValorCuota"]!.Value<decimal>();
                        retorno.Add(new Cuota() {
                            Afp = NOMBRE_CAPITAL,
                            Fecha = fecha,
                            Fondo = fondoAux,
                            Valor = valor
                        });
                    }
                }

                RegistrarLog(string.Format("Terminó extracción de {0} - Fondo {1} - Fecha Inicio {2} - Fecha Final {3} - {4} Valores Cuota",
                    NOMBRE_CAPITAL,
                    fondo,
                    fechaInicio.ToString("dd/MM/yyyy"),
                    fechaFinal.ToString("dd/MM/yyyy"),
                    retorno.Count));

                return retorno;
            } catch (Exception ex) {
				throw new ExcepcionValorCuota(ex) {
					Afp = NOMBRE_CAPITAL,
					FechaInicio = fechaInicio,
					FechaFinal = fechaFinal,
					Fondo = fondo
				};
			}
		}

		public async Task<List<Cuota>> ObtenerCuotasHabitat(DateTime fechaInicio, DateTime fechaFinal, string? fondo) {
			try {
				RegistrarLog(string.Format("Extrayendo valores cuota de {0} - Fondo {1} - Fecha Inicio {2} - Fecha Final {3}",
					NOMBRE_HABITAT,
                    fondo ?? "*",
					fechaInicio.ToString("dd/MM/yyyy"),
					fechaFinal.ToString("dd/MM/yyyy")));

				string url_api_base = _afpHabitatUrlApiBase;
				string parametros = string.Format("mesdesde={0}&anodesde={1}&meshasta={2}&anohasta={3}&fondoa={4}&fondob={5}&fondoc={6}&fondod={7}&fondoe={8}",
					fechaInicio.Month,
					fechaInicio.Year,
					fechaFinal.Month,
					fechaFinal.Year,
					fondo == "A" || fondo == null ? "A" : "",
					fondo == "B" || fondo == null ? "B" : "",
					fondo == "C" || fondo == null ? "C" : "",
					fondo == "D" || fondo == null ? "D" : "",
					fondo == "E" || fondo == null ? "E" : "");

				HttpWebRequest request = WebRequest.CreateHttp(url_api_base);
				request.Method = "POST";
				request.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate | DecompressionMethods.Brotli;
				request.ContentType = "application/x-www-form-urlencoded";
				byte[] byteArray = Encoding.UTF8.GetBytes(parametros);
				request.ContentLength = byteArray.Length;
				using (Stream dataStream = await request.GetRequestStreamAsync()) {
					await dataStream.WriteAsync(byteArray);
				}
                using HttpWebResponse response = (HttpWebResponse)await request.GetResponseAsync();
                using Stream stream = response.GetResponseStream();
                using StreamReader reader = new(stream);
                string contenido = await reader.ReadToEndAsync();

                JArray arreglo;
                try {
                    arreglo = JArray.Parse(contenido);
                } catch (JsonReaderException) {
                    JObject objeto = JObject.Parse(contenido);
                    if (objeto["6"] != null) {
                        string mensajeError = objeto["6"]!.Value<string>()!;
                        string[] mensajeSeparado = mensajeError.Split("-");
                        if (mensajeSeparado.Length >= 2 && mensajeSeparado[0].Length >= 2) {
                            int ultimoDia = int.Parse(mensajeSeparado[0][^2..]);
                            int ultimoMes = int.Parse(mensajeSeparado[1]);

                            DateTime ultimaFecha = new(
                                fechaFinal.Year,
                                ultimoMes,
                                ultimoDia
                            );
                            while (ultimaFecha > fechaFinal) {
                                ultimaFecha = ultimaFecha.AddYears(-1);
                            }
                            if (fechaInicio > ultimaFecha) {
                                fechaInicio = ultimaFecha;
                            }
                            return await ObtenerCuotasHabitat(fechaInicio, ultimaFecha, fondo);
                        }
                    }
                    throw;
                }

                List<Cuota> retorno = [];
                int contFondo = 0;
                for (int indFondo = 0; indFondo < 5; indFondo++) {
                    if (arreglo[indFondo] != null) {
                        JArray infoFondo = arreglo[indFondo].ToObject<JArray>()!;
                        string tipoFondo = infoFondo[0].Value<string>()!.Replace("FONDO ", "");
                        JArray valoresFondo = infoFondo;
                        valoresFondo.RemoveAt(0);
                        if (tipoFondo == fondo || fondo == null) {
                            JArray listaFechas = arreglo[6][contFondo]!.ToObject<JArray>()!;
                            for (int indValor = 0; indValor < valoresFondo.Count; indValor++) {
                                decimal valor = decimal.Parse(valoresFondo[indValor].Value<string>()!, CultureInfo.InvariantCulture);
                                string[] diaMesAnno = listaFechas[indValor].Value<string>()!.Split("-");
                                DateTime fecha = new(
                                    int.Parse(diaMesAnno[2]),
                                    int.Parse(diaMesAnno[1]),
                                    int.Parse(diaMesAnno[0]));
                                if (fechaInicio <= fecha && fecha <= fechaFinal) {
                                    retorno.Add(new Cuota() {
                                        Afp = NOMBRE_HABITAT,
                                        Fecha = fecha,
                                        Fondo = tipoFondo,
                                        Valor = valor
                                    });
                                }
                            }
                        }
                        contFondo++;
                    }
                }

                RegistrarLog(string.Format("Terminó extracción de {0} - Fondo {1} - Fecha Inicio {2} - Fecha Final {3} - {4} Valores Cuota",
                        NOMBRE_HABITAT,
                        fondo ?? "*",
                        fechaInicio.ToString("dd/MM/yyyy"),
                        fechaFinal.ToString("dd/MM/yyyy"),
                        retorno.Count));

                return retorno;
            } catch (Exception ex) {
				throw new ExcepcionValorCuota(ex) {
					Afp = NOMBRE_HABITAT,
					FechaInicio = fechaInicio,
					FechaFinal = fechaFinal,
					Fondo = fondo
				};
			}
		}

		public async Task<List<Cuota>> ObtenerCuotasPlanvital(DateTime fechaInicio, DateTime fechaFinal, string? fondo) {
			try {
				RegistrarLog(string.Format("Extrayendo valores cuota de {0} - Fondo {1} - Fecha Inicio {2} - Fecha Final {3}", 
					NOMBRE_PLANVITAL,
                    fondo ?? "*",
					fechaInicio.ToString("dd/MM/yyyy"),
					fechaFinal.ToString("dd/MM/yyyy")));

				string url_api_base = _afpPlanvitalUrlApiBase;
				string url_api = string.Format(url_api_base,
					fechaInicio.ToString("yyyy-MM-dd"),
					fechaFinal.ToString("yyyy-MM-dd"));

				HttpWebRequest request = WebRequest.CreateHttp(url_api);
				request.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate | DecompressionMethods.Brotli;
                using HttpWebResponse response = (HttpWebResponse)await request.GetResponseAsync();
                using Stream stream = response.GetResponseStream();
                using StreamReader reader = new(stream);
                string contenido = await reader.ReadToEndAsync();
                JObject json = JObject.Parse(contenido);

                List<Cuota> retorno = [];
                foreach (JObject valoresFecha in json["quotaValues"]!.Cast<JObject>()) {
                    DateTime fecha = valoresFecha["date"]!.Value<DateTime>().ToUniversalTime();
                    fecha = new DateTime(fecha.Year, fecha.Month, fecha.Day);
                    List<string> tipoFondos;
                    if (fondo == null) {
                        tipoFondos = ["A", "B", "C", "D", "E"];
                    } else {
                        tipoFondos = [fondo];
                    }

                    foreach (string tipoFondo in tipoFondos) {
                        decimal valor = valoresFecha["fund" + tipoFondo]!.Value<decimal>();
                        retorno.Add(new Cuota() {
                            Afp = NOMBRE_PLANVITAL,
                            Fecha = fecha,
                            Fondo = tipoFondo,
                            Valor = valor
                        });
                    }
                }

                RegistrarLog(string.Format("Terminó extracción de {0} - Fondo {1} - Fecha Inicio {2} - Fecha Final {3} - {4} Valores Cuota",
                        NOMBRE_PLANVITAL,
                        fondo ?? "*",
                        fechaInicio.ToString("dd/MM/yyyy"),
                        fechaFinal.ToString("dd/MM/yyyy"),
                        retorno.Count));

                return retorno;
            } catch (Exception ex) {
				throw new ExcepcionValorCuota(ex) {
					Afp = NOMBRE_PLANVITAL,
					FechaInicio = fechaInicio,
					FechaFinal = fechaFinal,
					Fondo = fondo
				};
			}
		}

		public async Task<List<Cuota>> ObtenerCuotasProvida(DateTime mesAnno, DateTime fechaInicio, DateTime fechaFinal, string? fondo) {
			try {
				RegistrarLog(string.Format("Extrayendo valores cuota de {0} - Fondo {1} - Mes/Año {2} - Fecha Inicio {3} - Fecha Final {4}",
					NOMBRE_PROVIDA,
                    fondo ?? "*",
					mesAnno.ToString("MM/yyyy"),
					fechaInicio.ToString("dd/MM/yyyy"),
					fechaFinal.ToString("dd/MM/yyyy")));

				string url_api_base = _afpProvidaUrlApiBase;

				string body = @"
				<soapenv:Envelope xmlns:soapenv=""http://schemas.xmlsoap.org/soap/envelope/"" xmlns:tem=""http://tempuri.org/"">
					<soapenv:Header/>
					<soapenv:Body>
						<tem:histquotavalue>
							<tem:requirement>
								<![CDATA[<req><data>{0}</data></req>]]>
							</tem:requirement>
						</tem:histquotavalue>
					</soapenv:Body>
				</soapenv:Envelope>";
				body = string.Format(body, mesAnno.ToString("yyyyMM"));

				HttpWebRequest request = WebRequest.CreateHttp(url_api_base);
				request.Method = "POST";
				request.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate | DecompressionMethods.Brotli;
				request.ContentType = "text/xml; charset=utf-8";
				request.Headers.Add("x-csrf-token", Guid.NewGuid().ToString());
				byte[] byteArray = Encoding.UTF8.GetBytes(body);
				request.ContentLength = byteArray.Length;
				using (Stream dataStream = await request.GetRequestStreamAsync()) {
					await dataStream.WriteAsync(byteArray);
				}
                using HttpWebResponse response = (HttpWebResponse)await request.GetResponseAsync();
                using Stream stream = response.GetResponseStream();
                using StreamReader reader = new(stream);
                string contenido = await reader.ReadToEndAsync();
                XmlDocument xml = new();
                xml.LoadXml(contenido);

                XmlNamespaceManager nsmgr = new(xml.NameTable);
                nsmgr.AddNamespace("soap", "http://schemas.xmlsoap.org/soap/envelope/");
                nsmgr.AddNamespace("xsi", "http://www.w3.org/2001/XMLSchema-instance");
                nsmgr.AddNamespace("xsd", "http://www.w3.org/2001/XMLSchema");

                string strXmlData = xml.SelectSingleNode(".", nsmgr)!.InnerText;
                XmlDocument xmlData = new();
                xmlData.LoadXml(strXmlData);

                List<Cuota> retorno = [];
                foreach (XmlNode nodoFund in xmlData.SelectNodes("//resp/data/fund")!) {
                    string[] diaMesAnno = nodoFund.SelectSingleNode("@date")!.InnerText.Split("-");
                    DateTime fecha = new(
                        int.Parse(diaMesAnno[2]),
                        int.Parse(diaMesAnno[1]),
                        int.Parse(diaMesAnno[0]));
                    string tipoFondo = nodoFund.SelectSingleNode("@type")!.InnerText.ToUpper();
                    decimal valor = decimal.Parse(nodoFund.SelectSingleNode("value")!.InnerText.Replace(".", "").Replace(",", "."), CultureInfo.InvariantCulture);
                    if (tipoFondo == fondo || fondo == null) {
                        if (fechaInicio <= fecha && fecha <= fechaFinal) {
                            retorno.Add(new Cuota() {
                                Afp = NOMBRE_PROVIDA,
                                Fecha = fecha,
                                Fondo = tipoFondo,
                                Valor = valor
                            });
                        }
                    }
                }

                RegistrarLog(string.Format("Terminó extracción de {0} - Fondo {1} - Mes/Año {2} - Fecha Inicio {3} - Fecha Final {4} - {5} Valores Cuota",
                    NOMBRE_PROVIDA,
                    fondo ?? "*",
                    mesAnno.ToString("MM/yyyy"),
                    fechaInicio.ToString("dd/MM/yyyy"),
                    fechaFinal.ToString("dd/MM/yyyy"),
                    retorno.Count));

                return retorno;
            } catch (Exception ex) {
				throw new ExcepcionValorCuota(ex) {
					Afp = NOMBRE_PROVIDA,
					FechaInicio = fechaInicio,
					FechaFinal = fechaFinal,
					Fondo = fondo
				};
			}
		}

		public async Task<List<Cuota>> ObtenerCuotasUno(DateTime fechaInicio, DateTime fechaFinal, string? fondo) {
			try {
				RegistrarLog(string.Format("Extrayendo valores cuota de {0} - Fondo {1} - Fecha Inicio {2} - Fecha Final {3}",
					NOMBRE_UNO,
                    fondo ?? "*",
					fechaInicio.ToString("dd/MM/yyyy"),
					fechaFinal.ToString("dd/MM/yyyy")));

				string url_api_base = _afpUnoUrlApiBase;
				string parametros = string.Format("{{\"fechaInicio\":\"{0}\",\"fechaFin\":\"{1}\"}}",
					fechaInicio.AddDays(-7).ToString("yyyy-MM-dd"),
					fechaFinal.AddDays(7).ToString("yyyy-MM-dd"));
				HttpWebRequest request = WebRequest.CreateHttp(url_api_base);
				request.Method = "POST";
				request.UserAgent = GetRandomUserAgent();
				request.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate | DecompressionMethods.Brotli;
				request.ContentType = "application/json;charset=UTF-8";
				byte[] byteArray = Encoding.UTF8.GetBytes(parametros);
				request.ContentLength = byteArray.Length;
				using (Stream dataStream = await request.GetRequestStreamAsync()) {
					await dataStream.WriteAsync(byteArray);
				}
                using HttpWebResponse response = (HttpWebResponse)await request.GetResponseAsync();
                using Stream stream = response.GetResponseStream();
                using StreamReader reader = new(stream);
                string contenido = await reader.ReadToEndAsync();
                JObject json = JObject.Parse(contenido);
                List<string> tiposFondo;
                if (fondo == null) {
                    tiposFondo = ["A", "B", "C", "D", "E"];
                } else {
                    tiposFondo = [fondo];
                }

                List<Cuota> retorno = [];
                foreach (JObject fondoCuota in json["data"]!.Cast<JObject>()) {
                    DateTime fecha = fondoCuota["FECHA"]!.Value<DateTime>().ToUniversalTime();
                    fecha = new DateTime(fecha.Year, fecha.Month, fecha.Day);
                    if (fechaInicio <= fecha && fecha <= fechaFinal) {
                        foreach (string tipoFondo in tiposFondo) {
                            decimal valor = decimal.Parse(fondoCuota["FONDO_" + tipoFondo]!.Value<string>()!.Replace(",", "."), CultureInfo.InvariantCulture);
                            retorno.Add(new Cuota() {
                                Afp = NOMBRE_UNO,
                                Fecha = fecha,
                                Fondo = tipoFondo,
                                Valor = valor
                            });
                        }
                    }
                }

                RegistrarLog(string.Format("Terminó extracción de {0} - Fondo {1} - Fecha Inicio {2} - Fecha Final {3} - {4} Valores Cuota",
                    NOMBRE_UNO,
                    fondo ?? "*",
                    fechaInicio.ToString("dd/MM/yyyy"),
                    fechaFinal.ToString("dd/MM/yyyy"),
                    retorno.Count));

                return retorno;
            } catch (Exception ex) {
				throw new ExcepcionValorCuota(ex) {
					Afp = NOMBRE_UNO,
					FechaInicio = fechaInicio,
					FechaFinal = fechaFinal,
					Fondo = fondo
				};
			}
		}

		public async Task<HashSet<Uf>> ObtenerValoresUF(DateTime anno, DateTime fechaInicio, DateTime fechaFinal) {			
			try {
				RegistrarLog(string.Format("Extrayendo valores UF de Mindicador - Año: {0} - Fecha Inicio {1} - Fecha Final {2}",
					anno.Year,
					fechaInicio.ToString("dd/MM/yyyy"),
					fechaFinal.ToString("dd/MM/yyyy")));

				string url_api_base = _valoresUfUrlApiBase;
				string url_api = string.Format(url_api_base, anno.Year);

				HttpWebRequest request = WebRequest.CreateHttp(url_api);
				request.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate | DecompressionMethods.Brotli;
                using HttpWebResponse response = (HttpWebResponse)await request.GetResponseAsync();
                using Stream stream = response.GetResponseStream();
                using StreamReader reader = new(stream);
                string contenido = await reader.ReadToEndAsync();
                JObject json = JObject.Parse(contenido);

                HashSet<Uf> retorno = [];
                foreach (JObject valorUf in json["serie"]!.Cast<JObject>()) {
                    DateTime fecha = valorUf["fecha"]!.Value<DateTime>().ToUniversalTime();
                    fecha = new DateTime(fecha.Year, fecha.Month, fecha.Day);
                    decimal valor = valorUf["valor"]!.Value<decimal>();

                    foreach (Uf uf in UFS_ERRONEAS) {
                        if (fecha == uf.Fecha) {
                            valor = uf.Valor;
                        }
                    }

                    if (fechaInicio <= fecha && fecha <= fechaFinal) {
                        retorno.Add(new Uf() {
                            Fecha = fecha,
                            Valor = valor
                        });
                    }
                }

                foreach (Uf uf in UFS_NO_ENCONTRADAS) {
                    if (fechaInicio <= uf.Fecha && uf.Fecha <= fechaFinal) {
                        retorno.Add(uf);
                    }
                }

                RegistrarLog(string.Format("Terminó extracción de Mindicador - Año {0} - Fecha Inicio {1} - Fecha Final {2} - {3} Valores UF",
                    anno.Year,
                    fechaInicio.ToString("dd/MM/yyyy"),
                    fechaFinal.ToString("dd/MM/yyyy"),
                    retorno.Count));

                return retorno;
            } catch (Exception ex) {
				throw new ExcepcionUf(ex) {
					FechaInicio = fechaInicio,
					FechaFinal = fechaFinal
				};
			}
		}

		public async Task<List<Comision>> ObtenerComisiones(string? Afp, DateTime mesAnno) {
			try {
				RegistrarLog(string.Format("Extrayendo comisiones de SPensiones - {0} - Mes/Año {1}",
						Afp ?? "Todas las AFPs",
						mesAnno.ToString("MM/yyyy")));

				await RandomWait();

				string url_api_base = _comisionesUrlApiBase; // 20210201
				string url_api = string.Format(url_api_base, mesAnno.ToString("yyyyMMdd"));

                HttpClient request = new(new HttpClientHandler {
                    AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate | DecompressionMethods.Brotli
                });
                request.DefaultRequestHeaders.Add("User-Agent", GetRandomUserAgent());
                request.DefaultRequestHeaders.Add("Referer", GetRandomReferer(1));
                request.DefaultRequestHeaders.Add("Accept", "text/html,application/xhtml+xml,application/xml;q=0.9,image/avif,image/webp,image/apng,*/*;q=0.8,application/signed-exchange;v=b3;q=0.9");
                request.DefaultRequestHeaders.Add("Accept-Encoding", "gzip, deflate, br");
                request.DefaultRequestHeaders.Add("Accept-Language", "es-MX,es;q=0.9,es-419;q=0.8,en;q=0.7");
                request.DefaultRequestHeaders.Add("Cache-Control", "max-age=0");
                using HttpResponseMessage response = await request.GetAsync(url_api);
                using Stream stream = await response.Content.ReadAsStreamAsync();
				using StreamReader reader = new(stream);
				string contenido = await reader.ReadToEndAsync();
				HtmlDocument htmlDoc = new();
				htmlDoc.LoadHtml(contenido);

				List<Comision> retorno = [];
				foreach (HtmlNode tabla in htmlDoc.DocumentNode.SelectNodes("//table")!) {
					if (tabla.InnerHtml.Contains("dep&oacute;sito", StringComparison.CurrentCultureIgnoreCase)) {
						HtmlNodeCollection filas = tabla.SelectNodes("./tr")!;
						if (filas.Count == 0) {
							filas = tabla.SelectNodes("./tbody/tr")!;
						}

						foreach (HtmlNode fila in filas) {
							HtmlNode columnaAfp = fila.SelectSingleNode("./td[@class='ITEM']")!;
							HtmlNode columnaComision = fila.SelectSingleNode("./td[@align='CENTER']")!;
							if (columnaAfp != null && columnaComision != null) {
								string nombreAfp = columnaAfp.InnerText.Trim().ToUpper();
								string valorComision = columnaComision.InnerText.Trim();
								if (nombreAfp == "SANTA MARIA") {
									nombreAfp = NOMBRE_CAPITAL;
								}

								if (AFPs.Contains(nombreAfp)) {
									if (Afp == null || Afp == nombreAfp) {
										retorno.Add(new Comision() {
											TipoComision = Comision.TIPO_COMIS_DEPOS_COTIZ_OBLIG,
											Afp = nombreAfp,
											Fecha = mesAnno,
											Valor = decimal.Parse(valorComision, CultureInfo.InvariantCulture),
											TipoValor = Comision.TIPO_VALOR_PORCENTAJE,
										});
									}
								}
							}
						}
						break;
					}
				}
				RegistrarLog(string.Format("Terminó extracción de SPensiones - {0} - Mes/Año {1} - {2} Comisiones AFP",
								Afp ?? "Todas las AFPs",
								mesAnno.ToString("MM/yyyy"),
								retorno.Count));

				return retorno;
			} catch (Exception ex) {
				throw new ExcepcionComision(ex) {
					Afp = Afp,
					MesAnno = mesAnno
				};
			}
		}

		public async Task<List<Comision>> ObtenerComisionesCAV(string? Afp, DateTime mesAnno) {
			try {
				RegistrarLog(string.Format("Extrayendo comisiones CAV de SPensiones - {0} - Mes/Año {1}",
						Afp ?? "Todas las AFPs",
						mesAnno.ToString("MM/yyyy")));

				await RandomWait();

				string url_api_base = _comisionesCavUrlApiBase; // 202102
				string url_api = string.Format(url_api_base, mesAnno.ToString("yyyyMM"));

				HttpClient request = new(new HttpClientHandler {
					AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate | DecompressionMethods.Brotli
                });
				request.DefaultRequestHeaders.Add("User-Agent", GetRandomUserAgent());
                request.DefaultRequestHeaders.Add("Referer", GetRandomReferer(2));
                request.DefaultRequestHeaders.Add("Accept", "text/html,application/xhtml+xml,application/xml;q=0.9,image/avif,image/webp,image/apng,*/*;q=0.8,application/signed-exchange;v=b3;q=0.9");
                request.DefaultRequestHeaders.Add("Accept-Encoding", "gzip, deflate, br");
                request.DefaultRequestHeaders.Add("Accept-Language", "es-MX,es;q=0.9,es-419;q=0.8,en;q=0.7");
                request.DefaultRequestHeaders.Add("Cache-Control", "max-age=0");
                using HttpResponseMessage response = await request.GetAsync(url_api);
				using Stream stream = await response.Content.ReadAsStreamAsync();
				using StreamReader reader = new(stream);
				string contenido = await reader.ReadToEndAsync();
				HtmlDocument htmlDoc = new();
				htmlDoc.LoadHtml(contenido);

				List<Comision> retorno = [];
				foreach (HtmlNode tabla in htmlDoc.DocumentNode.SelectNodes("//table")!) {
					if (tabla.InnerHtml.Contains("comisiones de la cuenta de ahorro voluntario", StringComparison.CurrentCultureIgnoreCase)) {
						HtmlNodeCollection filas = tabla.SelectNodes("./tr")!;
						if (filas.Count == 0) {
							filas = tabla.SelectNodes("./tbody/tr")!;
						}

						foreach (HtmlNode fila in filas) {
							HtmlNode columnaAfp = fila.SelectSingleNode("./td[@class='ITEM']")!;
							HtmlNode columnaComision = fila.SelectSingleNode("./td[@align='right']")!;
							if (columnaAfp != null && columnaComision != null) {
								string nombreAfp = columnaAfp.InnerText.Trim().ToUpper();
								string valorComision = columnaComision.InnerText
									.Replace(",", ".")
									.Replace("-", "")
									.Replace("—", "")
									.Trim();
								if (nombreAfp == "SANTA MARIA") {
									nombreAfp = NOMBRE_CAPITAL;
								}

								if (AFPs.Contains(nombreAfp) && valorComision.Length > 0) {
									if (Afp == null || Afp == nombreAfp) {
										retorno.Add(new Comision() {
											TipoComision = Comision.TIPO_COMIS_ADMIN_CTA_AHO_VOL,
											Afp = nombreAfp,
											Fecha = mesAnno,
											Valor = decimal.Parse(valorComision, CultureInfo.InvariantCulture),
											TipoValor = Comision.TIPO_VALOR_PORCENTAJE,
										});
									}
								}
							}
						}
						break;
					}
				}
				RegistrarLog(string.Format("Terminó extracción CAV - SPensiones - {0} - Mes/Año {1} - {2} Comisiones CAV AFP",
								Afp ?? "Todas las AFPs",
								mesAnno.ToString("MM/yyyy"),
								retorno.Count));

				return retorno;
			} catch (Exception ex) {
				throw new ExcepcionComision(ex) {
					Afp = Afp,
					MesAnno = mesAnno
				};
			}
		}

		public void RegistrarLog(string mensaje, int tipo = 0) {
			DateTime fecha = DateTime.Now;
			Logs.Add(new Log() { 
				Fecha = fecha,
				Mensaje = mensaje,
				Tipo = tipo
			});
		}

		private static string GetRandomReferer(int i = 1) {
			List<string> list = [
				"https://www.spensiones.cl/portal/institucional/594/w3-propertyvalue-9847.html",
				"https://www.spensiones.cl/portal/institucional/594/w3-propertyvalue-9598.html"
			];

			if (i == 1) {
				list.Add("https://www.spensiones.cl/apps/estcom/estcom.php");
				list.Add("https://www.spensiones.cl/apps/estcom/estcom.php?fecha=20210601");
				list.Add("https://www.spensiones.cl/apps/estcom/estcom.php?fecha=20210501");
				list.Add("https://www.spensiones.cl/apps/estcom/estcom.php?fecha=20210401");
				list.Add("https://www.spensiones.cl/apps/estcom/estcom.php?fecha=20210301");
				list.Add("https://www.spensiones.cl/apps/estcom/estcom.php?fecha=20210201");
				list.Add("https://www.spensiones.cl/apps/estcom/estcom.php?fecha=20210101");
			} else if (i == 2) {
				list.Add("https://www.spensiones.cl/apps/comisiones/getComisAPV.php");
				list.Add("https://www.spensiones.cl/apps/comisiones/getComisAPV.php?fecha=202104");
				list.Add("https://www.spensiones.cl/apps/comisiones/getComisAPV.php?fecha=202103");
				list.Add("https://www.spensiones.cl/apps/comisiones/getComisAPV.php?fecha=202102");
				list.Add("https://www.spensiones.cl/apps/comisiones/getComisAPV.php?fecha=202101");
			}

			Random random = new(Guid.NewGuid().GetHashCode());
			int index = random.Next(list.Count);
			return list[index];
		}

		private static string GetRandomUserAgent() {
			List<string> list = [
				"Mozilla/5.0 (Windows NT 10.0; Win64; x64; rv:87.0) Gecko/20100101 Firefox/87.0",
				"Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/89.0.4389.114 Safari/537.36",
				"Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/42.0.2311.135 Safari/537.36 Edge/12.246",
				"Mozilla/5.0 (Windows NT 6.1; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/47.0.2526.111 Safari/537.36",
				"Mozilla/5.0 (X11; CrOS x86_64 8172.45.0) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/51.0.2704.64 Safari/537.36",
				"Mozilla/5.0 (X11; Ubuntu; Linux x86_64; rv:15.0) Gecko/20100101 Firefox/15.0.1",
				"Mozilla/5.0 (Macintosh; Intel Mac OS X 10_11_2) AppleWebKit/601.3.9 (KHTML, like Gecko) Version/9.0.2 Safari/601.3.9",
				"Mozilla/5.0 (Apple-iPhone7C2/1202.466; U; CPU like Mac OS X; en) AppleWebKit/420+ (KHTML, like Gecko) Version/3.0 Mobile/1A543 Safari/419.3",
				"Mozilla/5.0 (iPhone9,4; U; CPU iPhone OS 10_0_1 like Mac OS X) AppleWebKit/602.1.50 (KHTML, like Gecko) Version/10.0 Mobile/14A403 Safari/602.1",
				"Mozilla/5.0 (iPhone9,3; U; CPU iPhone OS 10_0_1 like Mac OS X) AppleWebKit/602.1.50 (KHTML, like Gecko) Version/10.0 Mobile/14A403 Safari/602.1",
				"Mozilla/5.0 (iPhone; CPU iPhone OS 11_0 like Mac OS X) AppleWebKit/604.1.38 (KHTML, like Gecko) Version/11.0 Mobile/15A5370a Safari/604.1",
				"Mozilla/5.0 (iPhone; CPU iPhone OS 11_0 like Mac OS X) AppleWebKit/604.1.34 (KHTML, like Gecko) Version/11.0 Mobile/15A5341f Safari/604.1",
				"Mozilla/5.0 (iPhone; CPU iPhone OS 11_0 like Mac OS X) AppleWebKit/604.1.38 (KHTML, like Gecko) Version/11.0 Mobile/15A372 Safari/604.1",
				"Mozilla/5.0 (iPhone; CPU iPhone OS 12_0 like Mac OS X) AppleWebKit/605.1.15 (KHTML, like Gecko) FxiOS/13.2b11866 Mobile/16A366 Safari/605.1.15",
				"Mozilla/5.0 (iPhone; CPU iPhone OS 12_0 like Mac OS X) AppleWebKit/605.1.15 (KHTML, like Gecko) CriOS/69.0.3497.105 Mobile/15E148 Safari/605.1",
				"Mozilla/5.0 (iPhone; CPU iPhone OS 12_0 like Mac OS X) AppleWebKit/605.1.15 (KHTML, like Gecko) Version/12.0 Mobile/15E148 Safari/604.1",
				"Mozilla/5.0 (Linux; Android 8.0.0; SM-G960F Build/R16NW) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/62.0.3202.84 Mobile Safari/537.36",
				"Mozilla/5.0 (Linux; Android 7.0; SM-G892A Build/NRD90M; wv) AppleWebKit/537.36 (KHTML, like Gecko) Version/4.0 Chrome/60.0.3112.107 Mobile Safari/537.36",
				"Mozilla/5.0 (Linux; Android 7.0; SM-G930VC Build/NRD90M; wv) AppleWebKit/537.36 (KHTML, like Gecko) Version/4.0 Chrome/58.0.3029.83 Mobile Safari/537.36",
				"Mozilla/5.0 (Linux; Android 6.0.1; SM-G935S Build/MMB29K; wv) AppleWebKit/537.36 (KHTML, like Gecko) Version/4.0 Chrome/55.0.2883.91 Mobile Safari/537.36",
				"Mozilla/5.0 (Linux; Android 6.0.1; SM-G920V Build/MMB29K) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/52.0.2743.98 Mobile Safari/537.36",
				"Mozilla/5.0 (Linux; Android 5.1.1; SM-G928X Build/LMY47X) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/47.0.2526.83 Mobile Safari/537.36",
				"Mozilla/5.0 (Linux; Android 6.0.1; Nexus 6P Build/MMB29P) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/47.0.2526.83 Mobile Safari/537.36",
				"Mozilla/5.0 (Linux; Android 7.1.1; G8231 Build/41.2.A.0.219; wv) AppleWebKit/537.36 (KHTML, like Gecko) Version/4.0 Chrome/59.0.3071.125 Mobile Safari/537.36",
				"Mozilla/5.0 (Linux; Android 6.0.1; E6653 Build/32.2.A.0.253) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/52.0.2743.98 Mobile Safari/537.36",
				"Mozilla/5.0 (Linux; Android 6.0; HTC One X10 Build/MRA58K; wv) AppleWebKit/537.36 (KHTML, like Gecko) Version/4.0 Chrome/61.0.3163.98 Mobile Safari/537.36",
				"Mozilla/5.0 (Linux; Android 6.0; HTC One M9 Build/MRA58K) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/52.0.2743.98 Mobile Safari/537.3"
			];

			Random random = new(Guid.NewGuid().GetHashCode());
			int index = random.Next(list.Count);
			return list[index];
		}

		private static Task RandomWait() {
			Random random = new(Guid.NewGuid().GetHashCode());
			int milliseconds = random.Next(5000);
			return Task.Delay(milliseconds);
		}
	}
}
