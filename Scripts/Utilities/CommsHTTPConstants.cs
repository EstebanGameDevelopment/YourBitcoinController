using YourCommonTools;

namespace YourBitcoinController
{
	public class CommsHTTPConstants
	{
		// ----------------------------------------------
		// COMM EVENTS
		// ----------------------------------------------	
		public const string EVENT_COMM_CONFIGURATION_PARAMETERS = "YourSharingEconomyApp.GetConfigurationServerParametersHTTP";
		// ----------------------------------------------
		// COMM EVENTS
		// ----------------------------------------------	
		public const string EVENT_COMM_BITCOIN_EXCHANGE_INFO = "YourBitcoinController.BitcoinExchangeHTTP";
		public const string EVENT_COMM_BITCOIN_JSON_EXCHANGE_TABLE = "YourBitcoinController.BitcoinJSONExchangeTableHTTP";
		public const string EVENT_COMM_BITCOIN_JSON_TRANSACTION_FEE = "YourBitcoinController.BitcoinJSONFeeHTTP";

		// -------------------------------------------
		/* 
		 * DisplayLog
		 */
		public static void DisplayLog(string _data)
		{
			CommController.Instance.DisplayLog(_data);
		}

		// -------------------------------------------
		/* 
		 * CheckAccessDataByCountryname
		 */
		public static void GetBitcoinExchangeFromCurrency(string _currency, int _value)
		{
			CommController.Instance.Request(EVENT_COMM_BITCOIN_EXCHANGE_INFO, false, _currency, _value.ToString());
		}

		// -------------------------------------------
		/* 
		 * GetBitcoinExchangeFromCurrency
		 */
		public static void GetBitcoinExchangeRatesTable()
		{
			CommController.Instance.Request(EVENT_COMM_BITCOIN_JSON_EXCHANGE_TABLE, false);
		}

		// -------------------------------------------
		/* 
		 * GetBitcoinTransactionFee
		 */
		public static void GetBitcoinTransactionFee()
		{
			CommController.Instance.Request(EVENT_COMM_BITCOIN_JSON_TRANSACTION_FEE, false);
		}
	}
}
