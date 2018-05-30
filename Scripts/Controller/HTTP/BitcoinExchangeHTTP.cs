using System.Collections.Generic;
using YourCommonTools;

namespace YourBitcoinController
{

	public class BitcoinExchangeHTTP : BaseDataHTTP, IHTTPComms
	{
		private string m_urlRequest;

		private string m_currency;
		private string m_valueItem;


		public string UrlRequest
		{
			get { return m_urlRequest; }
		}

		public override Dictionary<string, string> GetHeaders()
		{
			Dictionary<string, string> headers = new Dictionary<string, string>();
			return headers;
		}

		public string Build(params object[] _list)
		{
			string phpFile = "^https://blockchain.info/tobtc^";
#if !ENABLE_MY_OFUSCATION || UNITY_EDITOR
			phpFile = phpFile.Replace("^", "");
#endif
			m_urlRequest = phpFile;

			m_currency = (string)_list[0];
			m_valueItem = (string)_list[1];

			return "?currency=" + m_currency + "&value=" + m_valueItem;
		}

		public override void Response(string _response)
		{
			ResponseCode(_response);
			BitcoinEventController.Instance.DispatchBitcoinEvent(BitCoinController.EVENT_BITCOINCONTROLLER_EXCHANGE_DATA, m_jsonResponse);
		}
	}
}

