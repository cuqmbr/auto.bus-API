using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace LiqPayIntegration
{
	public class PaymentResponse
	{
		/// <summary>
		/// Id платежу в системі LiqPay
		/// </summary>
		[JsonPropertyName("payment_id")]
		public long PaymentId { get; set; }
		/// <summary>
		/// Тип операції.
		/// Можливі значення: <c>pay</c> - платіж,
		/// <c>hold</c> - блокування коштів на рахунку відправника,
		/// <c>paysplit</c> - розщеплення платежу,
		/// <c>subscribe</c> - створення регулярного платежу,
		/// <c>paydonate</c> - пожертвування,
		/// <c>auth</c> - предавторізація картки,
		/// <c>regular</c> - регулярний платіж
		/// </summary>
		[JsonPropertyName("action")]
		public string Action { get; set; }
		/// <summary>
		/// Статус платежу.
		/// Можливі значення:
		/// </summary>
		[JsonPropertyName("status")]
		public string Status { get; set; }
		/// <summary>
		/// Версія API. Поточне значення - <c>3</c>
		/// </summary>
		[JsonPropertyName("version")]
		public int Version { get; set; }
		/// <summary>
		/// Тип платежу
		/// </summary>
		[JsonPropertyName("type")]
		public string Type { get; set; }
		/// <summary>
		/// Спосіб оплати.
		/// Можливі значення:
		/// </summary>
		[JsonPropertyName("paytype")]
		public string PayType { get; set; }
		/// <summary>
		/// Публічний ключ магазину
		/// </summary>
		[JsonPropertyName("public_key")]
		public string PublicKey { get; set; }
		/// <summary>
		///	ID еквайера
		/// </summary>
		[JsonPropertyName("acq_id")]
		public int AcqId { get; set; }
		/// <summary>
		/// Order_id платежу
		/// </summary>
		[JsonPropertyName("order_id")]
		public string OrderId { get; set; }
		/// <summary>
		/// Order_id платежу в системі LiqPay
		/// </summary>
		[JsonPropertyName("liqpay_order_id")]
		public string LiqPayOrderId { get; set; }
		/// <summary>
		/// Коментар до платежу
		/// </summary>
		[JsonPropertyName("description")]
		public string Description { get; set; }
		/// <summary>
		/// Карта відправника
		/// </summary>
		[JsonPropertyName("sender_card_mask2")]
		public string SenderCardMask2 { get; set; }
		/// <summary>
		/// Банк відправника
		/// </summary>
		[JsonPropertyName("sender_card_bank")]
		public string SenderCardBank { get; set; }
		/// <summary>
		/// Тип картки відправника MC/Visa
		/// </summary>
		[JsonPropertyName("sender_card_type")]
		public string SenderCardType { get; set; }
		/// <summary>
		/// Країна картки відправника. Цифровий <see href="https://uk.wikipedia.org/wiki/ISO_3166-1">ISO 3166-1 код</see>
		/// </summary>
		[JsonPropertyName("sender_card_country")]
		public int SenderCardCountry { get; set; }
		/// <summary>
		///
		/// </summary>
		[JsonPropertyName("ip")]
		public string Ip { get; set; }
		/// <summary>
		/// Сума платежу
		/// </summary>
		[JsonPropertyName("amount")]
		public double Amount { get; set; }
		/// <summary>
		/// Валюта платежу
		/// </summary>
		[JsonPropertyName("currency")]
		public string Currency { get; set; }
		/// <summary>
		/// Комісія з відправника у валюті платежу
		/// </summary>
		[JsonPropertyName("sender_commission")]
		public double SenderCommission { get; set; }
		/// <summary>
		/// Комісія з одержувача у валюті платежу
		/// </summary>
		[JsonPropertyName("receiver_commission")]
		public double ReceiverCommission { get; set; }
		/// <summary>
		/// Комісія агента в валюті платежу
		/// </summary>
		[JsonPropertyName("agent_commission")]
		public double AgentCommission { get; set; }
		/// <summary>
		/// Сума транзакції debit у валюті <see cref="CurrencyDebit">currency_debit</see>
		/// </summary>
		[JsonPropertyName("amount_debit")]
		public double AmountDebit { get; set; }
		/// <summary>
		/// Сума транзакції credit в валюті <see cref="CurrencyCredit">amount_credit</see>
		/// </summary>
		[JsonPropertyName("amount_credit")]
		public double AmountCredit { get; set; }
		/// <summary>
		/// Комісія з відправника у валюті <see cref="CurrencyDebit">currency_debit</see>
		/// </summary>
		[JsonPropertyName("commission_debit")]
		public double CommissionDebit { get; set; }
		/// <summary>
		/// Комісія з одержувача у валюті <see cref="CurrencyCredit">currency_credit</see>
		/// </summary>
		[JsonPropertyName("commission_credit")]
		public double CommissionCredit { get; set; }
		/// <summary>
		/// Валюта транзакції debit
		/// </summary>
		[JsonPropertyName("currency_debit")]
		public string CurrencyDebit { get; set; }
		/// <summary>
		/// Валюта транзакції credit
		/// </summary>
		[JsonPropertyName("currency_credit")]
		public string CurrencyCredit { get; set; }
		/// <summary>
		/// Бонус відправника у валюті платежу
		/// </summary>
		[JsonPropertyName("sender_bonus")]
		public double SenderBonus { get; set; }
		/// <summary>
		/// Бонус відправника у валюті платежу debit
		/// </summary>
		[JsonPropertyName("amount_bonus")]
		public double AmountBonus { get; set; }
		/// <summary>
		/// Можливі значення: 5 - транзакція пройшла з 3DS (емітент і еквайєр підтримують технологію 3D-Secure), 6 - емітент картки платника не підтримує технологію 3D-Secure, 7 - операція пройшла без 3D-Secure
		/// </summary>
		[JsonPropertyName("mpi_eci")]
		public string MpiEci { get; set; }
		/// <summary>
		/// Можливі значення:
		/// <c>true</c> - транзакція пройшла з 3DS перевіркою,
		/// <c>false</c> - транзакція пройшла без 3DS перевірки
		/// </summary>
		[JsonPropertyName("is_3ds")]
		public bool Is3ds { get; set; }
		/// <summary>
		///
		/// </summary>
		[JsonPropertyName("language")]
		public string Language { get; set; }
		/// <summary>
		/// Дата створення платежу
		/// </summary>
		[JsonPropertyName("create_date")]
		public long CreateDate { get; set; }
		/// <summary>
		/// Дата завершення/зміни платежу
		/// </summary>
		[JsonPropertyName("end_date")]
		public long EndDate { get; set; }
		/// <summary>
		/// Id транзакції в системі LiqPay
		/// </summary>
		[JsonPropertyName("transaction_id")]
		public long TransactionId { get; set; }
	}
}