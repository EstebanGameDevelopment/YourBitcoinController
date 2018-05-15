using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using NBitcoin;
using QBitNinja.Client;
using System.Text;
using QBitNinja.Client.Models;
using NBitcoin.Policy;
using UnityEngine.Networking;

namespace YourBitcoinController
{
	/******************************************
	 * 
	 * BasicManager
	 * 
	 * Basic example to test the most transaction basic functionalities
	 * 
	 * 	To get Bitcoins in the Main Network:
	 *  
	 *  https://buy.blockexplorer.com/
	 *  
	 *  Or in the TestNet Network:
	 *  
	 *  https://testnet.manu.backend.hamburg/faucet
	 *
	 * @author Esteban Gallardo
	 */
	public class BasicManager : MonoBehaviour
	{

		public const string PRIVATE_ROOT_KEY = "cTbnpYYGcHx3EjxXrBrF7dbgGXjJtuqtsh7k5KJZsz8G3hDgb981";

		public readonly string[] PRIVATE_KEY_TOTAL =   {
												"cTbnpYYGcHx3EjxXrBrF7dbgGXjJtuqtsh7k5KJZsz8G3hDgb981", // ROOT_KEY
												"cTEA1jLF1xm61L1Kjn5jV8NxDHpAnPTPJAZH58pBbCxhY1G2tSgT",
												"cVdcbibCsBWSyP27D9ZnN7EKtVTyDa82ssW7ezNCt29zuPtqJZVm",
												"cS2anGVL3KS2mBCHkVHCKCUB5fP1EXYScB3VFWUsb5tYxrxwgbKM",
												"cQ1oNFRaKJY7qcnfuEgdCDXeAfjLTQc82nC5VYR8JqJ2Rj8ZjU2b"
											};

		public readonly string[] PUBLIC_DESTINATIONS_KEYS =
													{
												"migfekTLcGCgko4dDhWECPuik1s5nR4wq5", // ROOT_KEY
												"ms2wLCiHNdPirguvxp6TC7fPuKtT9KfnBs",
												"mjggYbhy5iMRE7bAok3t18JbYX85cEggt6",
												"mnBgzAVysXiibaviXJsMUezdZMHtCSFgsR",
												"mudCKeLa1V5iG9WFdLVhhMSb3whyGzc7QS"
											};

		public readonly string[] CURRENCIES = {
											"USD",
											"EUR",
											"RUB",
											"GBP",
											"JPY"
										};

		// ----------------------------------------------
		// PUBLIC MEMBERS
		// ----------------------------------------------
		public GUISkin SkinUI;

		// ----------------------------------------------
		// PRIVATE MEMBERS
		// ----------------------------------------------
		private List<string> m_displayMessages = new List<string>();
		private Vector2 m_scrollPosition = Vector2.zero;
		private bool m_activateTextArea = false;
		private string m_amountToTransfer = "0.001";

		private int m_retryPush = 0;
		private string m_transactionHex;
		private string m_finalFeeAmount = "0.00001";
		private string m_initialFinalFeeAmount = "";
		private int m_indexCurrency = 0;
		private string m_currency = "USD";
		private decimal m_exchangeValue = -1;
		private string m_titleOfTransfer = "Title of transfer";

		private List<float> m_balanceWallets = new List<float>();

		// -------------------------------------------
		/* 
		 * Runs as soon as the object is active
		 */
		void Start()
		{
			BitCoinController.Instance.Init(BitCoinController.OPTION_NETWORK_TEST);

			BasicEventController.Instance.BasicEvent += new BasicEventHandler(OnBasicEvent);

			for (int i = 0; i < PRIVATE_KEY_TOTAL.Length; i++)
			{
				m_balanceWallets.Add(0);
			}
		}

		// -------------------------------------------
		/* 
		 * Destroy
		 */
		void OnDestroy()
		{
			Destroy();
		}


		// -------------------------------------------
		/* 
		* Destroy
		*/
		public void Destroy()
		{
			BasicEventController.Instance.BasicEvent -= OnBasicEvent;
		}

		// -------------------------------------------
		/* 
		* Display messages on screen and buttons on screen
		*/
		void OnGUI()
		{
			GUI.skin = SkinUI;

			float fontSize = 1.2f * 15;

			if (m_initialFinalFeeAmount.Length == 0)
			{
				GUI.Label(new Rect(new Vector2(10, (Screen.height / 2) - fontSize), new Vector2(Screen.width - 20, 2 * fontSize)), "Calculating fee. Wait...");
				return;
			}

			// NETWORK (MAIN or TESTNET)
			float yGlobalPosition = 10;
			float xPosSetParameters = 10;
			float widthSetParameters = ((Screen.width - 20) / 4);
			GUI.Label(new Rect(new Vector2(xPosSetParameters, yGlobalPosition), new Vector2(widthSetParameters, 2 * fontSize)), "NETWORK ++[" + BitCoinController.Instance.Network.ToString() + "]++");
			xPosSetParameters += widthSetParameters;
			if (GUI.Button(new Rect(new Vector2(xPosSetParameters, yGlobalPosition), new Vector2(3 * widthSetParameters, 2 * fontSize)), "Clear Log"))
			{
				m_activateTextArea = false;
				m_displayMessages.Clear();
			}
			yGlobalPosition += 2.2f * fontSize;

			// CURRENT INFO
			xPosSetParameters = 10;
			widthSetParameters = ((Screen.width - 20) / 6);
			GUI.Label(new Rect(new Vector2((int)(xPosSetParameters), yGlobalPosition), new Vector2(widthSetParameters, 2 * fontSize)), "CURRENCY");
			xPosSetParameters += widthSetParameters;
			GUI.Label(new Rect(new Vector2((int)(xPosSetParameters), yGlobalPosition), new Vector2(widthSetParameters / 3, 2 * fontSize)), m_currency);
			xPosSetParameters += widthSetParameters / 3;
			if (GUI.Button(new Rect(new Vector2((int)(xPosSetParameters), yGlobalPosition), new Vector2(2 * widthSetParameters / 3, 2 * fontSize)), "CHANGE"))
			{
				m_indexCurrency++;
				if (m_indexCurrency >= CURRENCIES.Length)
				{
					m_indexCurrency = 0;
				}
				m_currency = CURRENCIES[m_indexCurrency];
				m_exchangeValue = BitCoinController.Instance.CurrenciesExchange[m_currency];
			}
			xPosSetParameters += 2 * widthSetParameters / 3;
			GUI.Label(new Rect(new Vector2((int)(xPosSetParameters), yGlobalPosition), new Vector2(widthSetParameters, 2 * fontSize)), "EXCHANGE");
			xPosSetParameters += widthSetParameters;
			GUI.Label(new Rect(new Vector2((int)(xPosSetParameters), yGlobalPosition), new Vector2(widthSetParameters, 2 * fontSize)), m_exchangeValue.ToString());
			xPosSetParameters += widthSetParameters/2;
			GUI.Label(new Rect(new Vector2((int)(xPosSetParameters), yGlobalPosition), new Vector2(1.5f * widthSetParameters, 2 * fontSize)), "HOUR FEE");
			xPosSetParameters += 1.5f * widthSetParameters;
			float hourFeeInCurrency = (float)(BitCoinController.Instance.FeesTransactions[BitCoinController.FEE_LABEL_HOUR] * (decimal)m_exchangeValue);
			GUI.Label(new Rect(new Vector2((int)(xPosSetParameters), yGlobalPosition), new Vector2(widthSetParameters, 2 * fontSize)), hourFeeInCurrency + " " + m_currency);
			float minEstimatedFeeInCurrency = (float)(BitCoinController.Instance.FeesTransactions[BitCoinController.FEE_LABEL_MIN_ESTIMATED] * (decimal)m_exchangeValue);
			yGlobalPosition += 1.1f * fontSize;
			xPosSetParameters -= 1.5f * widthSetParameters;
			GUI.Label(new Rect(new Vector2((int)(xPosSetParameters), yGlobalPosition), new Vector2(1.5f * widthSetParameters, 2 * fontSize)), "MIN. ESTIMATED FEE");
			xPosSetParameters += 1.5f * widthSetParameters;
			GUI.Label(new Rect(new Vector2((int)(xPosSetParameters), yGlobalPosition), new Vector2(widthSetParameters, 2 * fontSize)), minEstimatedFeeInCurrency + " " + m_currency);
			yGlobalPosition += 1.1f * fontSize;

			// CHECK BALANCE ORIGIN
			float xPosLocalCheck = 10;
			float widthButtonCheck = ((Screen.width - 20) / PRIVATE_KEY_TOTAL.Length);
			for (int l = 0; l < PRIVATE_KEY_TOTAL.Length; l++)
			{
				if (GUI.Button(new Rect(new Vector2((int)xPosLocalCheck, yGlobalPosition), new Vector2(widthButtonCheck, 2 * fontSize)), "BAL[" + l + "]=" + (m_balanceWallets[l] * (float)m_exchangeValue) + " " + m_currency))
				{
					m_activateTextArea = false;
					m_balanceWallets[l] = (float)CheckBalanceOrigin(l);
				}
				xPosLocalCheck += widthButtonCheck;
			}
			yGlobalPosition += 2.2f * fontSize;

			// TEXTFIELD AMOUNT
			xPosSetParameters = 10;
			widthSetParameters = ((Screen.width - 20) / 8);
			m_titleOfTransfer = GUI.TextField(new Rect(new Vector2((int)(xPosSetParameters), yGlobalPosition), new Vector2(widthSetParameters, 2 * fontSize)), m_titleOfTransfer);
			xPosSetParameters += widthSetParameters;
			GUI.Label(new Rect(new Vector2((int)(xPosSetParameters), yGlobalPosition), new Vector2(widthSetParameters, 2 * fontSize)), "AMOUNT");
			xPosSetParameters += widthSetParameters;
			m_amountToTransfer = GUI.TextField(new Rect(new Vector2((int)(xPosSetParameters), yGlobalPosition), new Vector2(widthSetParameters, 2 * fontSize)), m_amountToTransfer);
			xPosSetParameters += widthSetParameters;
			GUI.Label(new Rect(new Vector2((int)(xPosSetParameters), yGlobalPosition), new Vector2(widthSetParameters, 2 * fontSize)), (float.Parse(m_amountToTransfer) * (float)m_exchangeValue).ToString() + " " + m_currency);
			xPosSetParameters += widthSetParameters;
			GUI.Label(new Rect(new Vector2((int)(xPosSetParameters), yGlobalPosition), new Vector2(widthSetParameters, 2 * fontSize)), "SET YOUR FEE");
			xPosSetParameters += widthSetParameters;
			m_finalFeeAmount = GUI.TextField(new Rect(new Vector2((int)(xPosSetParameters), yGlobalPosition), new Vector2(widthSetParameters, 2 * fontSize)), m_finalFeeAmount);
			xPosSetParameters += widthSetParameters;
			GUI.Label(new Rect(new Vector2((int)(xPosSetParameters), yGlobalPosition), new Vector2(widthSetParameters, 2 * fontSize)), (float.Parse(m_finalFeeAmount) * (float)m_exchangeValue).ToString() + " " + m_currency);
			xPosSetParameters += widthSetParameters;
			if (GUI.Button(new Rect(new Vector2(xPosSetParameters, yGlobalPosition), new Vector2(widthSetParameters, 2 * fontSize)), "RESET"))
			{
				m_finalFeeAmount = m_initialFinalFeeAmount;
				m_amountToTransfer = "0.001";
			}
			yGlobalPosition += 2.2f * fontSize;

			// SEND MONEY TO MINIONS TRANSACTION SIMPLE
			xPosLocalCheck = 10 + widthButtonCheck;
			for (int l = 1; l < PUBLIC_DESTINATIONS_KEYS.Length; l++)
			{
				if (GUI.Button(new Rect(new Vector2((int)xPosLocalCheck, yGlobalPosition), new Vector2(widthButtonCheck, 2 * fontSize)), "SEND[0->" + l + "]"))
				{
					m_activateTextArea = false;
					RunTransaction(PRIVATE_ROOT_KEY, PUBLIC_DESTINATIONS_KEYS[l], decimal.Parse(m_amountToTransfer), decimal.Parse(m_finalFeeAmount), m_titleOfTransfer);
				}
				xPosLocalCheck += widthButtonCheck;
			}
			yGlobalPosition += 2.2f * fontSize;

			// RETURN MONEY TO ROOT TRANSACTION SIMPLE
			xPosLocalCheck = 10 + widthButtonCheck;
			for (int l = 1; l < PUBLIC_DESTINATIONS_KEYS.Length; l++)
			{
				if (GUI.Button(new Rect(new Vector2((int)xPosLocalCheck, yGlobalPosition), new Vector2(widthButtonCheck, 2 * fontSize)), "RETURN[" + l + "->0]"))
				{
					m_activateTextArea = false;
					RunTransaction(PRIVATE_KEY_TOTAL[l], PUBLIC_DESTINATIONS_KEYS[0], decimal.Parse(m_amountToTransfer), decimal.Parse(m_finalFeeAmount), m_titleOfTransfer);
				}
				xPosLocalCheck += widthButtonCheck;
			}
			yGlobalPosition += 2.2f * fontSize;

			// GET HISTORY TRANSACTIONS
			xPosLocalCheck = 10;
			for (int l = 0; l < PUBLIC_DESTINATIONS_KEYS.Length; l++)
			{
				if (GUI.Button(new Rect(new Vector2((int)xPosLocalCheck, yGlobalPosition), new Vector2(widthButtonCheck, 2 * fontSize)), "HISTORY[" + l + "]"))
				{
					m_activateTextArea = false;
					GetAllTransactions(PUBLIC_DESTINATIONS_KEYS[l]);
				}
				xPosLocalCheck += widthButtonCheck;
			}
			yGlobalPosition += 2.2f * fontSize;

			// LOG DISPLAY
			GUI.Label(new Rect(0, yGlobalPosition, Screen.width - 20, fontSize), "**PROGRAM LOG**");
			yGlobalPosition += 1.2f * fontSize;
			int linesTextArea = 10;
			if (m_activateTextArea)
			{
				linesTextArea = 10;
			}
			else
			{
				linesTextArea = 2;
			}
			float finalHeighArea = linesTextArea * fontSize;
			m_scrollPosition = GUI.BeginScrollView(new Rect(10, yGlobalPosition, Screen.width - 20, Screen.height - yGlobalPosition), m_scrollPosition, new Rect(0, 0, 200, m_displayMessages.Count * finalHeighArea));
			float yPosition = 0;
			for (int i = 0; i < m_displayMessages.Count; i++)
			{
				string message = m_displayMessages[i];
				GUI.TextArea(new Rect(0, yPosition, Screen.width, finalHeighArea), message);
				yPosition += finalHeighArea;
			}
			GUI.EndScrollView();
		}

		// -------------------------------------------
		/* 
		 * Add Log message
		 */
		private void AddLog(string _message)
		{
			m_displayMessages.Add(_message);
			Debug.Log(_message);
		}

		// -------------------------------------------
		/* 
		* CheckBalanceOrigin
		*/
		private decimal CheckBalanceOrigin(int _index)
		{
			decimal balance = BitCoinController.Instance.GetBalance(PRIVATE_KEY_TOTAL[_index], false);
			AddLog("++++CURRENT BALANCE[" + _index + "][" + balance + "][" + (m_exchangeValue * balance) + " " + m_currency + "]++++");
			return balance;
		}

		// -------------------------------------------
		/* 
		* GetAllTransactions
		*/
		private void GetAllTransactions(string _publicKeyAdress)
		{
			BitCoinController.Instance.GetAllInformation(_publicKeyAdress);

			AddLog("++INPUT TRANSACTIONS[" + BitCoinController.Instance.InTransactionsHistory.Count + "]++");
			for (int i = 0; i < BitCoinController.Instance.InTransactionsHistory.Count; i++)
			{
				ItemMultiObjectEntry transaction = BitCoinController.Instance.InTransactionsHistory[i];
				AddLog(BitCoinController.ToStringTransaction(transaction));
			}

			AddLog("--OUTPUT TRANSACTIONS[" + BitCoinController.Instance.OutTransactionsHistory.Count + "]--");
			for (int i = 0; i < BitCoinController.Instance.OutTransactionsHistory.Count; i++)
			{
				ItemMultiObjectEntry transaction = BitCoinController.Instance.OutTransactionsHistory[i];
				AddLog(BitCoinController.ToStringTransaction(transaction));
			}
		}

		// -------------------------------------------
		/* 
		 * Runs the most basic transaction possible
		 */
		private void RunTransaction(string _privateKeyOrigin, string _publicKeyDestination, decimal _amountToTransfer, decimal _finalFeeAmount, string _titleOfTransfer)
		{
			if (BitCoinController.Instance.ValidatePrivateKey(_privateKeyOrigin))
			{
				BitCoinController.Instance.Pay(_privateKeyOrigin, _publicKeyDestination, _titleOfTransfer, _amountToTransfer, _finalFeeAmount);
			}
		}

		// -------------------------------------------
		/* 
		 * OnBasicEvent
		 */
		private void OnBasicEvent(string _nameEvent, params object[] _list)
		{
			if (_nameEvent == BitCoinController.EVENT_BITCOINCONTROLLER_ALL_DATA_COLLECTED)
			{
				m_exchangeValue = BitCoinController.Instance.CurrenciesExchange[m_currency];
				m_finalFeeAmount = BitCoinController.Instance.FeesTransactions[BitCoinController.FEE_LABEL_MIN_ESTIMATED].ToString();
				m_initialFinalFeeAmount = m_finalFeeAmount;
			}
			if (_nameEvent == BitCoinController.EVENT_BITCOINCONTROLLER_TRANSACTION_COMPLETED)
			{
				if ((bool)_list[0])
				{
					AddLog("+++TRANSACTION SUCCESS::PRESS ON BALANCE TO CHECK IF UPDATED, IT COULD TAKE A WHILE, BE PATIENCE");
				}
				else
				{
					string info = ((_list.Length > 1) ? (string)_list[1] : "");
					AddLog("---TRANSACTION FAILED::info=" + info);
				}
			}
		}
	}
}