using System;

namespace QueTalMiAFPCdk.Models.Others {
	public class ExceptionQueTalMiAFP : Exception {
		public ExceptionQueTalMiAFP() {

		}

		public ExceptionQueTalMiAFP(string message) : base(message) {

		}

		public ExceptionQueTalMiAFP(string message, Exception inner) : base(message, inner) {

		}
	}
}
