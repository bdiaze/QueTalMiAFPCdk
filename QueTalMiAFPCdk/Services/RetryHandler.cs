using Microsoft.Extensions.Configuration;
using QueTalMiAFPCdk.Services;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace QueTalMiAFP.Services {
	public class RetryHandler(HttpMessageHandler innerHandler, ParameterStoreHelper parameterStore) : DelegatingHandler(innerHandler) {
		private readonly int MaxRetries = int.Parse(parameterStore.ObtenerParametro("/QueTalMiAFP/Api/MaxRetries").Result);

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken) {
			HttpResponseMessage? response = null;
			for (int i = 0; i < MaxRetries; i++) {
				response = await base.SendAsync(request, cancellationToken);
				
				string? errorTypeHeader;
				try {
					errorTypeHeader = response.Headers.GetValues("ErrorType").FirstOrDefault();
				} catch (Exception) {
					errorTypeHeader = null;
				}

				if ((response.IsSuccessStatusCode && errorTypeHeader == null) || 
					response.StatusCode == HttpStatusCode.BadRequest) {
					return response;
				}
			}

			return response!;
		}
	}
}
