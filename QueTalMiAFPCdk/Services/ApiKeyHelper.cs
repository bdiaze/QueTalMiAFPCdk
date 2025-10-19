using Amazon.APIGateway;
using Amazon.APIGateway.Model;
using Amazon.SecurityToken;
using Amazon.SecurityToken.Model;
using System.Configuration;

namespace QueTalMiAFPCdk.Services {
    public class ApiKeyHelper(IConfiguration configuration) {
        
        private readonly Dictionary<string, string> apiKeys = [];

        public async Task<string> ObtenerApiKey(string apiKeyId) {
            if (!apiKeys.TryGetValue(apiKeyId, out string? value)) {
                string assumeRole = configuration.GetValue<string>("AssumeRoleArn")!;

                using AmazonSecurityTokenServiceClient client = new();
                AssumeRoleRequest request = new() {
                    RoleSessionName = "QueTalMiAFP-GetApiKeyValue-Session",
                    RoleArn = assumeRole,
                };

                AssumeRoleResponse responseAssumeRole = await client.AssumeRoleAsync(request);

                AmazonAPIGatewayClient apiClient = new(
                    responseAssumeRole.Credentials.AccessKeyId,
                    responseAssumeRole.Credentials.SecretAccessKey,
                    responseAssumeRole.Credentials.SessionToken
                );

                GetApiKeyResponse response = await apiClient.GetApiKeyAsync(new GetApiKeyRequest {
                    ApiKey = apiKeyId,
                    IncludeValue = true
                });

                if (response == null || response.Value == null) {
                    throw new Exception("No se pudo rescatar correctamente el api key");
                }

                value = response.Value;
                apiKeys[apiKeyId] = value;
            }

            return value;
        }
    }
}
