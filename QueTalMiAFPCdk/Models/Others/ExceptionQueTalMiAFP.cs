using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace QueTalMiAFP.Models.Others {
	public class ExceptionQueTalMiAFP : Exception {
		public ExceptionQueTalMiAFP() {

		}

		public ExceptionQueTalMiAFP(string message) : base(message) {

		}

		public ExceptionQueTalMiAFP(string message, Exception inner) : base(message, inner) {

		}
	}
}
