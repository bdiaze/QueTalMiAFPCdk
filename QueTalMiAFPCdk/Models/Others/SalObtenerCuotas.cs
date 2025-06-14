using System;

namespace QueTalMiAFPCdk.Models.Others {
	public class SalObtenerCuotas {
		public required string S3Url { get; set; }
		public List<CuotaUf>? ListaCuotas { get; set; }
	}
}
