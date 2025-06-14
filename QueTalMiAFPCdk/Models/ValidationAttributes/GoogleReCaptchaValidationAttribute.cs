using System.ComponentModel.DataAnnotations;
using System.Text;
using System.Net;
using Newtonsoft.Json.Linq;
using QueTalMiAFPCdk.Services;

namespace QueTalMiAFPCdk.Models.ValidationAttributes {
	public class GoogleReCaptchaValidationAttribute: ValidationAttribute {
		protected override ValidationResult? IsValid(object? value, ValidationContext validationContext) {
			string strError = "Nos reservamos el derecho de admisión, por lo que los robots no están autorizados para ingresar en esta página.";
			byte[] bytesError = Encoding.UTF8.GetBytes(strError);
			string binaryStrError = "";
			foreach (byte byteError in bytesError) {
				binaryStrError += Convert.ToString(byteError, 2).PadLeft(8, '0');
			}
			Lazy<ValidationResult> errorResult = new(() => new ValidationResult(binaryStrError, [validationContext.MemberName!]));
			if (value == null || string.IsNullOrWhiteSpace(value?.ToString())) {
				return errorResult.Value;
			}

			SecretManagerHelper secretManager = (SecretManagerHelper)validationContext!.GetService(typeof(SecretManagerHelper))!;
			string reCaptchaResponse = value!.ToString()!;
			string reCaptchaSecret = secretManager.ObtenerSecreto("/QueTalMiAFP").Result.GoogleRecaptchaSecretKey;

			HttpClient httpClient = new();
			HttpResponseMessage httpResponse = httpClient.GetAsync($"https://www.google.com/recaptcha/api/siteverify?secret={reCaptchaSecret}&response={reCaptchaResponse}").Result;
			if (httpResponse.StatusCode != HttpStatusCode.OK) {
				return errorResult.Value;
			}

			string jsonResponse = httpResponse.Content.ReadAsStringAsync().Result;
			dynamic jsonData = JObject.Parse(jsonResponse);
			if (!jsonData.success.ToString().Equals("true", StringComparison.CurrentCultureIgnoreCase)) {
				return errorResult.Value;
			}

			return ValidationResult.Success!;
		}
	}
}
