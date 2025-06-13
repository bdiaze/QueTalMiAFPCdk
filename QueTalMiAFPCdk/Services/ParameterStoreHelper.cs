using Amazon.SecretsManager;
using Amazon.SecretsManager.Model;
using Amazon.SecurityToken;
using Amazon.SecurityToken.Model;
using Amazon.SimpleSystemsManagement;
using Amazon.SimpleSystemsManagement.Model;
using Newtonsoft.Json;

namespace QueTalMiAFPCdk.Services {
    public class ParameterStoreHelper(IConfiguration configuration) {

        private readonly Dictionary<string, string> parametersValues = [];

        public async Task<string> ObtenerParametro(string parameterName) {
            if (!parametersValues.TryGetValue(parameterName, out string? value)) {
                string assumeRole = configuration.GetValue<string>("AWSGatewayAPIKey:AssumeRoleArn")!;

                using AmazonSecurityTokenServiceClient client = new();
                AssumeRoleRequest request = new() {
                    RoleSessionName = "QueTalMiAFP-SSMGetParameter-Session",
                    RoleArn = assumeRole,
                };

                AssumeRoleResponse responseAssumeRole = await client.AssumeRoleAsync(request);

                AmazonSimpleSystemsManagementClient parameterStore = new(
                    responseAssumeRole.Credentials.AccessKeyId,
                    responseAssumeRole.Credentials.SecretAccessKey,
                    responseAssumeRole.Credentials.SessionToken
                );

                GetParameterResponse response = await parameterStore.GetParameterAsync(new GetParameterRequest {
                    Name = parameterName
                });

                if (response == null || response.Parameter == null) {
                    throw new Exception("No se pudo rescatar correctamente el parámetro");
                }

                value = response.Parameter.Value!;
                parametersValues[parameterName] = value;
            }

            return value;
        }

    }
}
