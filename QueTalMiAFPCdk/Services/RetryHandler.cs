using System.Net;

namespace QueTalMiAFPCdk.Services {
	public class RetryHandler(HttpMessageHandler innerHandler, ParameterStoreHelper parameterStore) : DelegatingHandler(innerHandler) {
		private readonly int MaxRetries = int.Parse(parameterStore.ObtenerParametro("/QueTalMiAFP/Api/MaxRetries").Result);

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken) {
			HttpResponseMessage? response = null;
			for (int i = 0; i < MaxRetries; i++) {
				response = await base.SendAsync(request, cancellationToken);
				
				string? errorTypeHeader = null;
				if (response.Headers.Contains("ErrorType")) {
                    errorTypeHeader = response.Headers.GetValues("ErrorType").FirstOrDefault();
                }

				if (response.IsSuccessStatusCode && errorTypeHeader == null || 
					response.StatusCode == HttpStatusCode.BadRequest) {
					return response;
				}
			}

			return response!;
		}
	}
}
