using System.Configuration;
using MercadoPago.Config;

namespace QueTalMiAFPCdk.Services {
    public class MercadoPagoHelper {
        public string PublicKey { get; set; }

        public string UrlSuccess { get; set; }
        public string UrlFailure { get; set; }
        public string UrlPending { get; set; }

        public MercadoPagoHelper(IConfiguration configuration) {
            string accessToken = configuration.GetValue<string>("MercadoPago:AccessToken")!;
            PublicKey = configuration.GetValue<string>("MercadoPago:PublicKey")!;
            UrlSuccess = configuration.GetValue<string>("MercadoPago:UrlSuccess")!;
            UrlFailure = configuration.GetValue<string>("MercadoPago:UrlFailure")!;
            UrlPending = configuration.GetValue<string>("MercadoPago:UrlPending")!;

            MercadoPagoConfig.AccessToken = accessToken;
        }
    }
}
