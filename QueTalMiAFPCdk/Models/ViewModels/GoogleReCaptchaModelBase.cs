﻿using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using QueTalMiAFPCdk.Models.ValidationAttributes;

namespace QueTalMiAFPCdk.Models.ViewModels {
	public abstract class GoogleReCaptchaModelBase {
		[Required(ErrorMessage = "Debes validar que no eres un robot.")]
		[GoogleReCaptchaValidation]
		[BindProperty(Name = "g-recaptcha-response")]
		public required string GoogleReCaptchaResponse { get; set; }
	}
}
