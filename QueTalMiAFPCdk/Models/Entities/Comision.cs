using System;

namespace QueTalMiAFPCdk.Models.Entities {
	public class Comision {
		public const byte TIPO_VALOR_PORCENTAJE = 1;
		public const byte TIPO_VALOR_PESO_CHILENO = 2;

		public const byte TIPO_COMIS_DEPOS_COTIZ_OBLIG = 1; // Por deposito de la cotización obligatoria
		public const byte TIPO_COMIS_ADMIN_CTA_AHO_VOL = 2; // Por administración de la cuenta de ahorro voluntaria
		public const byte TIPO_COMIS_ADMIN_AHO_PRE_VOL = 3; // Por administración del ahorro previsional voluntaria
		public const byte TIPO_COMIS_TRANS_AHO_PRE_VOL = 4; // Por transferencia de depósitos del ahorro previsional voluntario

		public long Id { get; set; }

		public string? Afp { get; set; }

		public DateOnly Fecha { get; set; }

		public decimal Valor { get; set; }

		public byte TipoComision { get; set; }

		public byte TipoValor { get; set; }
	}
}
