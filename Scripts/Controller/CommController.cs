using UnityEngine;
using System;
using System.Reflection;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using System.IO;
using System.Text;
using UnityEngine.Networking;
using YourCommonTools;

namespace YourBitcoinController
{
	/******************************************
	 * 
	 * CommController
	 * 
	 * It manages all the communications with the server
	 * 
	 * @author Esteban Gallardo
	 */
	public class CommController : StateManager
	{
		// ----------------------------------------------
		// CONSTANTS
		// ----------------------------------------------	
		public const char TOKEN_SEPARATOR_COMA = ',';
		public const string TOKEN_SEPARATOR_EVENTS = "<par>";
		public const string TOKEN_SEPARATOR_LINES = "<line>";

		// ----------------------------------------------
		// COMM EVENTS
		// ----------------------------------------------	
		public const string EVENT_COMM_BITCOIN_EXCHANGE_INFO		= "EVENT_COMM_BITCOIN_EXCHANGE_INFO";
		public const string EVENT_COMM_BITCOIN_JSON_EXCHANGE_TABLE	= "EVENT_COMM_BITCOIN_JSON_EXCHANGE_TABLE";
		public const string EVENT_COMM_BITCOIN_JSON_TRANSACTION_FEE = "EVENT_COMM_BITCOIN_JSON_TRANSACTION_FEE";

		public const int STATE_IDLE = 0;
		public const int STATE_COMMUNICATION = 1;

		// ----------------------------------------------
		// SINGLETON
		// ----------------------------------------------	

		private static CommController _instance;

		public static CommController Instance
		{
			get
			{
				if (!_instance)
				{
					_instance = GameObject.FindObjectOfType(typeof(CommController)) as CommController;
					if (!_instance)
					{
						GameObject container = new GameObject();
						string finalSingletonName = "^CommController^";
#if !ENABLE_MY_OFUSCATION || UNITY_EDITOR
						finalSingletonName = finalSingletonName.Replace("^", "");
#endif
						container.name = finalSingletonName;
						_instance = container.AddComponent(typeof(CommController)) as CommController;
					}
				}
				return _instance;
			}
		}

		// ----------------------------------------------
		// MEMBERS
		// ----------------------------------------------
		private string m_event;
		private IHTTPComms m_commRequest;
		private List<TimedEventData> m_listTimedEvents = new List<TimedEventData>();
		private List<TimedEventData> m_listQueuedEvents = new List<TimedEventData>();
		private List<TimedEventData> m_priorityQueuedEvents = new List<TimedEventData>();

		public bool ReloadXML = false;

		// -------------------------------------------
		/* 
		 * Will delete from the text introduced by the user any special token that can break the comunication
		 */
		public static string FilterSpecialTokens(string _text)
		{
			string output = _text;

			string[] arrayEvents = output.Split(new string[] { TOKEN_SEPARATOR_EVENTS }, StringSplitOptions.None);
			output = "";
			for (int i = 0; i < arrayEvents.Length; i++)
			{
				output += arrayEvents[i];
				if (i + 1 < arrayEvents.Length)
				{
					output += " ";
				}
			}


			string[] arrayLines = output.Split(new string[] { TOKEN_SEPARATOR_LINES }, StringSplitOptions.None);
			output = "";
			for (int i = 0; i < arrayLines.Length; i++)
			{
				output += arrayLines[i];
				if (i + 1 < arrayLines.Length)
				{
					output += " ";
				}
			}

			return output;
		}

		// ----------------------------------------------
		// CONSTRUCTOR
		// ----------------------------------------------	
		// -------------------------------------------
		/* 
		 * Constructor
		 */
		private CommController()
		{
			ChangeState(STATE_IDLE);
		}

		// -------------------------------------------
		/* 
		 * Start
		 */
		void Start()
		{
			ChangeState(STATE_IDLE);
		}

		// -------------------------------------------
		/* 
		 * Init
		 */
		public void Init()
		{
			ChangeState(STATE_IDLE);
		}


		// -------------------------------------------
		/* 
		 * CheckAccessDataByCountryname
		 */
		public void GetBitcoinExchangeFromCurrency(string _currency, int _value)
		{
			Request(EVENT_COMM_BITCOIN_EXCHANGE_INFO, _currency, _value.ToString());
		}

		// -------------------------------------------
		/* 
		 * GetBitcoinExchangeFromCurrency
		 */
		public void GetBitcoinExchangeRatesTable()
		{
			Request(EVENT_COMM_BITCOIN_JSON_EXCHANGE_TABLE);
		}

		// -------------------------------------------
		/* 
		 * GetBitcoinTransactionFee
		 */
		public void GetBitcoinTransactionFee()
		{
			Request(EVENT_COMM_BITCOIN_JSON_TRANSACTION_FEE);
		}

		// -------------------------------------------
		/* 
		 * Destroy
		 */
		public void Destroy()
		{
			Destroy(_instance.gameObject);
			_instance = null;
		}

		// -------------------------------------------
		/* 
		 * Request
		 */
		public void Request(string _event, params object[] _list)
		{
			if (m_state != STATE_IDLE)
			{
				QueuedRequest(_event, _list);
				return;
			}

			RequestReal(_event, _list);
		}

		// -------------------------------------------
		/* 
		 * RequestPriority
		 */
		public void RequestPriority(string _event, params object[] _list)
		{
			if (m_state != STATE_IDLE)
			{
				InsertRequest(_event, _list);
				return;
			}

			RequestReal(_event, _list);
		}

		// -------------------------------------------
		/* 
		 * RequestNoQueue
		 */
		public void RequestNoQueue(string _event, params object[] _list)
		{
			if (m_state != STATE_IDLE)
			{
				return;
			}

			RequestReal(_event, _list);
		}

		// -------------------------------------------
		/* 
		 * RequestReal
		 */
		private void RequestReal(string _event, params object[] _list)
		{
			m_event = _event;
			bool isBinaryResponse = true;

			switch (m_event)
			{
				case EVENT_COMM_BITCOIN_EXCHANGE_INFO:
					isBinaryResponse = false;
					m_commRequest = new BitcoinExchangeHTTP();
					break;

				case EVENT_COMM_BITCOIN_JSON_EXCHANGE_TABLE:
					isBinaryResponse = false;
					m_commRequest = new BitcoinJSONExchangeTableHTTP();
					break;

				case EVENT_COMM_BITCOIN_JSON_TRANSACTION_FEE:
					isBinaryResponse = false;
					m_commRequest = new BitcoinJSONFeeHTTP();
					break;
			}

			ChangeState(STATE_COMMUNICATION);
			string data = m_commRequest.Build(_list);
#if DEBUG_MODE_DISPLAY_LOG
            Debug.Log("CommController::RequestReal:URL=" + m_commRequest.UrlRequest);
            Debug.Log("CommController::RequestReal:data=" + data);
#endif
			if (m_commRequest.Method == BaseDataHTTP.METHOD_GET)
			{
#if UNITY_ANDROID && !UNITY_EDITOR
        WWW www = new WWW(m_commRequest.UrlRequest + data, null, m_commRequest.GetHeaders());
#else
				WWW www = new WWW(m_commRequest.UrlRequest + data);
#endif
				if (isBinaryResponse)
				{
					StartCoroutine(WaitForRequest(www));
				}
				else
				{
					StartCoroutine(WaitForStringRequest(www));
				}
			}
			else
			{
#if UNITY_ANDROID && !UNITY_EDITOR
            WWW www = new WWW(m_commRequest.UrlRequest, m_commRequest.FormPost.data, m_commRequest.GetHeaders());
#else
				WWW www = new WWW(m_commRequest.UrlRequest, m_commRequest.FormPost.data, m_commRequest.FormPost.headers);
#endif

				if (isBinaryResponse)
				{
					StartCoroutine(WaitForRequest(www));
				}
				else
				{
					StartCoroutine(WaitForStringRequest(www));
				}
			}
		}

		// -------------------------------------------
		/* 
		 * DelayRequest
		 */
		public void DelayRequest(string _nameEvent, float _time, params object[] _list)
		{
			m_listTimedEvents.Add(new TimedEventData(_nameEvent, _time, true, _list));
		}

		// -------------------------------------------
		/* 
		 * QueuedRequest
		 */
		public void QueuedRequest(string _nameEvent, params object[] _list)
		{
			m_listQueuedEvents.Add(new TimedEventData(_nameEvent, 0, _list));
		}

		// -------------------------------------------
		/* 
		 * InsertRequest
		 */
		public void InsertRequest(string _nameEvent, params object[] _list)
		{
			m_priorityQueuedEvents.Insert(0, new TimedEventData(_nameEvent, 0, _list));
		}

		// -------------------------------------------
		/* 
		* WaitForRequest
		*/
		IEnumerator WaitForRequest(WWW www)
		{
			yield return www;

			// check for errors
			if (www.error == null)
			{
#if DEBUG_MODE_DISPLAY_LOG
            Debug.Log("WWW Ok!: " + www.text);
#endif
				m_commRequest.Response(www.bytes);
			}
			else
			{
#if DEBUG_MODE_DISPLAY_LOG
            Debug.LogError("WWW Error: " + www.error);
#endif
				m_commRequest.Response(Encoding.ASCII.GetBytes(www.error));

			}

			ChangeState(STATE_IDLE);
			ProcesQueuedComms();
		}

		// -------------------------------------------
		/* 
		* WaitForRequest
		*/
		IEnumerator WaitForStringRequest(WWW www)
		{
			yield return www;

			// check for errors
			if (www.error == null)
			{
#if DEBUG_MODE_DISPLAY_LOG
            Debug.Log("WWW Ok!: " + www.text);
#endif
				m_commRequest.Response(www.text);
			}
			else
			{
#if DEBUG_MODE_DISPLAY_LOG
            Debug.LogError("WWW Error: " + www.error);
#endif
				m_commRequest.Response(Encoding.ASCII.GetBytes(www.error));
			}

			ChangeState(STATE_IDLE);
			ProcesQueuedComms();
		}

		// -------------------------------------------
		/* 
		 * ProcessTimedEvents
		 */
		private void ProcessTimedEvents()
		{
			switch (m_state)
			{
				case STATE_IDLE:
					for (int i = 0; i < m_listTimedEvents.Count; i++)
					{
						TimedEventData eventData = m_listTimedEvents[i];
						eventData.Time -= Time.deltaTime;
						if (eventData.Time <= 0)
						{
							m_listTimedEvents.RemoveAt(i);
							Request(eventData.NameEvent, eventData.List);
							eventData.Destroy();
							break;
						}
					}
					break;
			}
		}

		// -------------------------------------------
		/* 
		 * ProcesQueuedComms
		 */
		private void ProcesQueuedComms()
		{
			// PRIORITY QUEUE
			if (m_priorityQueuedEvents.Count > 0)
			{
				int i = 0;
				TimedEventData eventData = m_priorityQueuedEvents[i];
				m_priorityQueuedEvents.RemoveAt(i);
				Request(eventData.NameEvent, eventData.List);
				eventData.Destroy();
				return;
			}
			// NORMAL QUEUE
			if (m_listQueuedEvents.Count > 0)
			{
				int i = 0;
				TimedEventData eventData = m_listQueuedEvents[i];
				m_listQueuedEvents.RemoveAt(i);
				Request(eventData.NameEvent, eventData.List);
				eventData.Destroy();
				return;
			}
		}

		// -------------------------------------------
		/* 
		 * ProcessQueueEvents
		 */
		private void ProcessQueueEvents()
		{
			switch (m_state)
			{
				case STATE_IDLE:
					break;

				case STATE_COMMUNICATION:
					break;
			}
		}

		// -------------------------------------------
		/* 
		 * ProcessQueueEvents
		 */
		public void DisplayLog(string _message)
		{
#if DEBUG_MODE_DISPLAY_LOG
        Debug.Log(_message);
#endif
		}

		// -------------------------------------------
		/* 
		 * Update
		 */
		public void Update()
		{
			Logic();

			ProcessTimedEvents();
			ProcessQueueEvents();
		}
	}
}
