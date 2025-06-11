using Amazon.S3;
using Amazon.S3.Model;

namespace QueTalMiAFPCdk.Services {
    public class S3BucketHelper(IAmazonS3 amazonS3, IConfiguration configuration) {

        public async Task<string> GetFile(string keyName) {
            string bucketName = configuration.GetValue<string>("AWSGatewayAPIKey:s3BucketName")!;

            GetObjectResponse response = await amazonS3.GetObjectAsync(bucketName, keyName);
            if (response.HttpStatusCode == System.Net.HttpStatusCode.OK) {
                using Stream stream = response.ResponseStream;
                using StreamReader reader = new(stream);
                return await reader.ReadToEndAsync();
            }

            throw new Exception("Ocurrió un error al bajar la respuesta del bucket de S3.");            
        }
    }
}
