using System.Configuration;
using System.Security.Cryptography.X509Certificates;
using MercadoPago.Config;

namespace QueTalMiAFPCdk.Services {
    public class MercadoPagoHelper {
        public string PublicKey { get; set; }

        public MercadoPagoHelper(IConfiguration configuration) {
            string accessToken = configuration.GetValue<string>("MercadoPago:AccessToken")!;
            PublicKey = configuration.GetValue<string>("MercadoPago:PublicKey")!;

            MercadoPagoConfig.AccessToken = accessToken;
        }
    }
}
