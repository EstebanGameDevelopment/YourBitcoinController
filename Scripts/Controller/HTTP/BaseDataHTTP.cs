using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace YourBitcoinController
{

	public class BaseDataHTTP
	{
		public const int METHOD_GET = 0;
		public const int METHOD_POST = 1;

		protected string m_code;
		protected string m_jsonResponse;
		protected int m_method = METHOD_GET;
		protected WWWForm m_formPost;

		public BaseDataHTTP()
		{
		}

		public int Method
		{
			get { return m_method; }
		}

		public WWWForm FormPost
		{
			get { return m_formPost; }
		}

		public virtual Dictionary<string, string> GetHeaders()
		{
			Dictionary<string, string> headers = new Dictionary<string, string>();
			return headers;
		}

		private string CleanUndesiredTags(string _data)
		{
			string output = _data;
			if ((output.IndexOf("<br>") != -1) ||
				(output.IndexOf("<br/>") != -1) ||
				(output.IndexOf("<br />") != -1))
			{
				output = output.Replace("<br>", "");
				output = output.Replace("<br/>", "");
				output = output.Replace("<br />", "");
			}
			return output;
		}

		public bool ResponseCode(byte[] _response)
		{
			m_jsonResponse = Encoding.ASCII.GetString(_response);
			m_jsonResponse = CleanUndesiredTags(m_jsonResponse);
#if DEBUG_MODE_DISPLAY_LOG
            Debug.Log("BaseDataJSON::ResponseCode(BYTE)=" + m_jsonResponse);
#endif
			return (m_jsonResponse.IndexOf("Error::") == -1);
		}

		public bool ResponseBase64Code(string _response)
		{
			m_jsonResponse = _response;

#if DEBUG_MODE_DISPLAY_LOG
        Debug.Log("BaseDataJSON::ResponseBase64Code(BYTE)=" + m_jsonResponse);
#endif
			return (m_jsonResponse.IndexOf("Error::") == -1);
		}

		public bool ResponseCode(string _response)
		{
			m_jsonResponse = _response;
			m_jsonResponse = CleanUndesiredTags(m_jsonResponse);
#if DEBUG_MODE_DISPLAY_LOG
        Debug.Log("BaseDataJSON::ResponseCode(STRING)=" + m_jsonResponse);
#endif
			return (m_jsonResponse.IndexOf("Error::") == -1);
		}

		public virtual void Response(byte[] _response)
		{
		}

		public virtual void Response(string _response)
		{
		}
	}
}