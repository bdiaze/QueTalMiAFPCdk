using Amazon.SecretsManager;
using Amazon.SecretsManager.Model;
using Amazon.SecurityToken;
using Amazon.SecurityToken.Model;
using Newtonsoft.Json;
using System.Configuration;

namespace QueTalMiAFPCdk.Services {
    public class SecretManagerHelper(IConfiguration configuration) {

        private readonly Dictionary<string, dynamic> secretsValues = [];

        public async Task<dynamic> ObtenerSecreto(string secretName) {
            if (!secretsValues.TryGetValue(secretName, out dynamic? value)) {
                string assumeRole = configuration.GetValue<string>("AssumeRoleArn")!;

                using AmazonSecurityTokenServiceClient client = new();
                AssumeRoleRequest request = new() {
                    RoleSessionName = "QueTalMiAFP-GetSecretValue-Session",
                    RoleArn = assumeRole,
                };

                AssumeRoleResponse responseAssumeRole = await client.AssumeRoleAsync(request);

                AmazonSecretsManagerClient secretsManager = new(
                    responseAssumeRole.Credentials.AccessKeyId,
                    responseAssumeRole.Credentials.SecretAccessKey,
                    responseAssumeRole.Credentials.SessionToken
                );

                GetSecretValueResponse response = await secretsManager.GetSecretValueAsync(new GetSecretValueRequest {
                    SecretId = secretName
                });

                if (response == null || response.SecretString == null) {
                    throw new Exception("No se pudo rescatar correctamente el secreto");
                }

                value = JsonConvert.DeserializeObject(response.SecretString)!;
                secretsValues[secretName] = value;
            }

            return value;
        }
    }
}
