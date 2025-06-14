using MercadoPago.Config;

namespace QueTalMiAFPCdk.Services {
    public class MercadoPagoHelper {
        public string PublicKey { get; set; }

        public string UrlSuccess { get; set; }
        public string UrlFailure { get; set; }
        public string UrlPending { get; set; }

        public MercadoPagoHelper(ParameterStoreHelper parameterStore, SecretManagerHelper secretManager) {
            string accessToken = secretManager.ObtenerSecreto("/QueTalMiAFP").Result.MercadoPagoAccessToken;
            PublicKey = parameterStore.ObtenerParametro("/QueTalMiAFP/MercadoPago/PublicKey").Result;
            UrlSuccess = parameterStore.ObtenerParametro("/QueTalMiAFP/MercadoPago/UrlSuccess").Result;
            UrlFailure = parameterStore.ObtenerParametro("/QueTalMiAFP/MercadoPago/UrlFailure").Result;
            UrlPending = parameterStore.ObtenerParametro("/QueTalMiAFP/MercadoPago/UrlPending").Result;

            MercadoPagoConfig.AccessToken = accessToken;
        }
    }
}
