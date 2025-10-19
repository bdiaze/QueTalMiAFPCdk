using Amazon.S3;
using Amazon.S3.Model;
using Amazon.SecurityToken;
using Amazon.SecurityToken.Model;
using System.Configuration;

namespace QueTalMiAFPCdk.Services {
    public class S3BucketHelper(ParameterStoreHelper parameterStore, IConfiguration configuration) {

        public async Task<string> GetFile(string keyName) {
            string assumeRole = configuration.GetValue<string>("AssumeRoleArn")!;
            string bucketName = parameterStore.ObtenerParametro("/QueTalMiAFP/S3/BucketName").Result;

            using (AmazonSecurityTokenServiceClient client = new()) {
                AssumeRoleRequest request = new() {
                    RoleSessionName = "QueTalMiAFP-S3GetObject-Session",
                    RoleArn = assumeRole,
                };

                AssumeRoleResponse responseAssumeRole = await client.AssumeRoleAsync(request);

                AmazonS3Client amazonS3 = new(
                    responseAssumeRole.Credentials.AccessKeyId,
                    responseAssumeRole.Credentials.SecretAccessKey,
                    responseAssumeRole.Credentials.SessionToken
                );

                GetObjectResponse response = await amazonS3.GetObjectAsync(bucketName, keyName);
                if (response.HttpStatusCode == System.Net.HttpStatusCode.OK) {
                    using Stream stream = response.ResponseStream;
                    using StreamReader reader = new(stream);
                    return await reader.ReadToEndAsync();
                }
            }

            throw new Exception("Ocurrió un error al bajar la respuesta del bucket de S3.");            
        }
    }
}
