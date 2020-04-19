using System.Collections.Generic;
using UnityEngine;
using YourCommonTools;

namespace YourBitcoinController
{

	public class BitcoinJSONFeeHTTP : BaseDataHTTP, IHTTPComms
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
#if DEBUG_MODE_DISPLAY_LOG
            Debug.LogError("BitcoinJSONFeeHTTP::Build::REQUESTING FEES++");
#endif

            string phpFile = "^https://bitcoinfees.earn.com/api/v1/fees/recommended^";
#if !ENABLE_MY_OFUSCATION || UNITY_EDITOR
			phpFile = phpFile.Replace("^", "");
#endif
			m_urlRequest = phpFile;

			return "";
		}

		public override void Response(string _response)
		{
			ResponseCode(_response);

#if DEBUG_MODE_DISPLAY_LOG
            Debug.LogError("BitcoinJSONFeeHTTP::Response::RETRIEVED FEES--");
#endif

            BitcoinEventController.Instance.DelayBasicEvent(BitCoinController.EVENT_BITCOINCONTROLLER_JSON_FEE_TABLE, 0.1f, m_jsonResponse);
		}
	}
}

