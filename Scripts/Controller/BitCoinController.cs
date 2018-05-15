using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using NBitcoin;
using QBitNinja.Client;
using System.Text;
using QBitNinja.Client.Models;
using UnityEngine.Networking;
using System.Security.Cryptography;

namespace YourBitcoinController
{
	public static class ArrayExtensions
	{
		public static T[] SubArray<T>(this T[] data, int index, int length)
		{
			var result = new T[length];
			Array.Copy(data, index, result, 0, length);
			return result;
		}
	}

	/******************************************
	 * 
	 * BitCoinController
	 * 
	 * Steps:
	 *  1 - NuGet Packages NBitCoin: Install-Package NBitcoin
	 *  2 - NuGet Packages NBitCoin: Install-Package QBitNinja.Client -Version 1.0.3.42
	 *  3 - Attach the script to a game object and you are good to go (tested on desktop and Android)
	 * 
	 * The code of this example comes from: https://github.com/ProgrammingBlockchain
	 * 
	 * @author Esteban Gallardo
	 */
	public class BitCoinController : MonoBehaviour
	{
		public const float ESTIMATED_SIZE_BLOCK = 1800;

		// ----------------------------------------------
		// EVENTS
		// ----------------------------------------------	
		public const string EVENT_BITCOINCONTROLLER_BALANCE_WALLET			= "EVENT_BITCOINCONTROLLER_BALANCE_WALLET";
		public const string EVENT_BITCOINCONTROLLER_PAYMENTS_DONE			= "EVENT_BITCOINCONTROLLER_PAYMENTS_DONE";
		public const string EVENT_BITCOINCONTROLLER_TRANSACTION_DONE		= "EVENT_BITCOINCONTROLLER_TRANSACTION_DONE";
		public const string EVENT_BITCOINCONTROLLER_TRANSACTION_COMPLETED	= "EVENT_BITCOINCONTROLLER_TRANSACTION_COMPLETED";
		public const string EVENT_BITCOINCONTROLLER_EXCHANGE_DATA			= "EVENT_BITCOINCONTROLLER_EXCHANGE_DATA";
		public const string EVENT_BITCOINCONTROLLER_JSON_EXCHANGE_TABLE		= "EVENT_BITCOINCONTROLLER_JSON_EXCHANGE_TABLE";
		public const string EVENT_BITCOINCONTROLLER_JSON_FEE_TABLE			= "EVENT_BITCOINCONTROLLER_JSON_FEE_TABLE";
		public const string EVENT_BITCOINCONTROLLER_ALL_DATA_COLLECTED		= "EVENT_BITCOINCONTROLLER_ALL_DATA_COLLECTED";
		public const string EVENT_BITCOINCONTROLLER_SELECTED_PRIVATE_KEY	= "EVENT_BITCOINCONTROLLER_SELECTED_PRIVATE_KEY";
		public const string EVENT_BITCOINCONTROLLER_SELECTED_PUBLIC_KEY		= "EVENT_BITCOINCONTROLLER_SELECTED_PUBLIC_KEY";
		public const string EVENT_BITCOINCONTROLLER_BALANCE_UPDATED			= "EVENT_BITCOINCONTROLLER_BALANCE_UPDATED";
		public const string EVENT_BITCOINCONTROLLER_NEW_CURRENCY_SELECTED	= "EVENT_BITCOINCONTROLLER_NEW_CURRENCY_SELECTED";
		public const string EVENT_BITCOINCONTROLLER_CURRENCY_CHANGED		= "EVENT_BITCOINCONTROLLER_CURRENCY_CHANGED";
		public const string EVENT_BITCOINCONTROLLER_UPDATE_ACCOUNT_DATA		= "EVENT_BITCOINCONTROLLER_UPDATE_ACCOUNT_DATA";

		public const string NETWORK_TEST = "TEST_";
		public const string NETWORK_MAIN = "MAIN_";

		public const string BITCOIN_PRIVATE_KEYS		= "BITCOIN_PRIVATE_KEYS";
		public const string BITCOIN_DEFAULT_CURRENCY	= "BITCOIN_DEFAULT_CURRENCY";
		public const string BITCOIN_PRIVATE_KEY_SELECTED = "BITCOIN_PRIVATE_KEY_SELECTED";
		public const string BITCOIN_ADDRESSES_LIST		= "BITCOIN_ADDRESSES_LIST";

		public const char SEPARATOR_ITEMS	= ';';
		public const char SEPARATOR_COMA = ',';

#if !ENABLE_MY_OFUSCATION || UNITY_EDITOR
		public const string ENCRYPTION_KEY = "ps22NwtKe521rLuQwr4752IcoREhuP26";  // CRITICAL!!! CHANGE THIS KEY TO YOUR OWN AND DO NOT SHARE IT, PLEASE!!
#else
	public const string ENCRYPTION_KEY = "^ps22NwtKe521rLuQwr4752IcoREhuP26^";  // CRITICAL!!! CHANGE THIS KEY TO YOUR OWN AND DO NOT SHARE IT, PLEASE!!
#endif

		public const string NBITCOIN_API_MAIN_NETWORK = "http://api.qbit.ninja/";
		public const string NBITCOIN_API_TEST_NETWORK = "http://tapi.qbit.ninja/";
		
		public const string FEE_FASTEST		= "fastestFee";
		public const string FEE_HALFHOUR	= "halfHourFee";
		public const string FEE_HOUR		= "hourFee";

		public const string FEE_LABEL_FASTEST		= "Fastest Fee";
		public const string FEE_LABEL_HALFHOUR		= "HalfHour Fee";
		public const string FEE_LABEL_HOUR			= "Hour Fee";
		public const string FEE_LABEL_MIN_ESTIMATED = "Min. Estimated";

		public static readonly string[] FEES_SUGGESTED = { FEE_LABEL_FASTEST, FEE_LABEL_HALFHOUR, FEE_LABEL_HOUR, FEE_LABEL_MIN_ESTIMATED };

		public const string CODE_DOLLAR = "USD";
		public const string CODE_EURO = "EUR";
		public const string CODE_YEN = "JPY";
		public const string CODE_RUBLO = "RUB";
		public const string CODE_POUND = "GBP";
		public const string CODE_BITCOIN = "BTC";

		public static readonly string[] CURRENCY_CODE = { CODE_DOLLAR, CODE_EURO, CODE_YEN, CODE_RUBLO, CODE_POUND, CODE_BITCOIN };

		public const string OPTION_NETWORK_COOKIE = "OPTION_NETWORK_COOKIE";

		public const string OPTION_NETWORK_TEST = "Test Network";
		public const string OPTION_NETWORK_MAIN = "Main Network";

		public static readonly string[] OPTIONS_NETWORK = { OPTION_NETWORK_TEST, OPTION_NETWORK_MAIN };

		// ----------------------------------------------
		// SINGLETON
		// ----------------------------------------------	
		private static BitCoinController _instance;

		public static BitCoinController Instance
		{
			get
			{
				if (!_instance)
				{
					_instance = GameObject.FindObjectOfType(typeof(BitCoinController)) as BitCoinController;
					if (!_instance)
					{
						GameObject container = new GameObject();
						container.name = "BitCoinController";
						_instance = container.AddComponent(typeof(BitCoinController)) as BitCoinController;
					}
				}
				return _instance;
			}
		}

		// ----------------------------------------------
		// PRIVATE MEMBERS
		// ----------------------------------------------
		private bool m_initialized = false;
		private NBitcoin.Network m_network;
		private bool m_isMainNetwork = false;
		private Dictionary<string,decimal> m_privateKeys = new Dictionary<string, decimal>();
		private Dictionary<string, string> m_publicKeys = new Dictionary<string, string>();
		private string m_currentPrivateKey = "";
		private string m_currentPublicKey = "";
		private string m_backupCurrentPrivateKey = "";
		private string m_currentCurrency = CODE_DOLLAR;

		private string m_titleTransaction;
		private string m_publicKeyTarget;

		private bool m_requestedToPay = false;
		private bool m_requestedToPayConfirmationFee = false;
		private decimal m_finalValueBitcoins = -1m;
		private decimal m_finalFeeAmount = -1m;

		private decimal m_balanceWallet = -1;
		private float m_paymentsWallet = -1;

		private List<ItemMultiObjectEntry> m_allTransactionsHistory = new List<ItemMultiObjectEntry>();
		private List<ItemMultiObjectEntry> m_inTransactionsHistory = new List<ItemMultiObjectEntry>();
		private List<ItemMultiObjectEntry> m_outTransactionsHistory = new List<ItemMultiObjectEntry>();

		private Dictionary<string, decimal> m_walletBalanceCurrencies = new Dictionary<string, decimal>();
		private Dictionary<string, decimal> m_currenciesExchange = new Dictionary<string, decimal>();
		private Dictionary<string, decimal> m_feesTransactions = new Dictionary<string, decimal>();

		private Dictionary<string, string> m_addressesList = new Dictionary<string, string>();

		public bool IsMainNetwork
		{
			get { return m_isMainNetwork; }
			set
			{
				m_isMainNetwork = value;
				PlayerPrefs.SetString(OPTION_NETWORK_COOKIE, (m_isMainNetwork?OPTION_NETWORK_MAIN:OPTION_NETWORK_TEST));
			}
		}
		public NBitcoin.Network Network
		{
			get { return m_network; }
		}
		public string NetworkAPI
		{
			get
			{
				if (m_isMainNetwork)
				{
					return NBITCOIN_API_MAIN_NETWORK;
				}
				else
				{
					return NBITCOIN_API_TEST_NETWORK;
				}
			}
		}
		public Dictionary<string, decimal> WalletBalanceCurrencies
		{
			get { return m_walletBalanceCurrencies; }
		}
		public List<ItemMultiObjectEntry> AllTransactionsHistory
		{
			get { return m_allTransactionsHistory; }
		}
		public List<ItemMultiObjectEntry> InTransactionsHistory
		{
			get { return m_inTransactionsHistory; }
		}
		public List<ItemMultiObjectEntry> OutTransactionsHistory
		{
			get { return m_outTransactionsHistory; }
		}
		public Dictionary<string,decimal> CurrenciesExchange
		{
			get { return m_currenciesExchange; }
		}
		public Dictionary<string, decimal> FeesTransactions
		{
			get { return m_feesTransactions; }
		}
		public Dictionary<string, decimal> PrivateKeys
		{
			get { return m_privateKeys; }
		}
		public Dictionary<string, string> PublicKeys
		{
			get { return m_publicKeys; }
		}		
		public string CodeNetwork
		{
			get
			{
				if (m_isMainNetwork)
				{
					return NETWORK_MAIN;
				}
				else
				{
					return NETWORK_TEST;
				}
			}
		}
		public string CurrentPrivateKey
		{
			get { return m_currentPrivateKey; }
			set {
				if (m_currentPrivateKey.Length > 0) m_backupCurrentPrivateKey = "" + m_currentPrivateKey;
				m_currentPrivateKey = value;
				if (m_currentPrivateKey.Length != 0)
				{
					m_currentPublicKey = GetPublicKey(m_currentPrivateKey);
					PlayerPrefs.SetString(CodeNetwork + BITCOIN_PRIVATE_KEY_SELECTED, RJEncryptor.EncryptStringWithKey(m_currentPrivateKey, ENCRYPTION_KEY));
					BitcoinEventController.Instance.DispatchBitcoinEvent(EVENT_BITCOINCONTROLLER_SELECTED_PRIVATE_KEY);
				}
			}
		}
		public string CurrentPublicKey
		{
			get { return m_currentPublicKey; }
		}
		public string BackupCurrentPrivateKey
		{
			set
			{
				m_backupCurrentPrivateKey = value;
				if (m_backupCurrentPrivateKey.Length == 0)
				{
					m_currentPrivateKey = "";
				}
				BitcoinEventController.Instance.DispatchBitcoinEvent(EVENT_BITCOINCONTROLLER_SELECTED_PRIVATE_KEY);
			}
		}
		public string CurrentCurrency
		{
			get { return m_currentCurrency; }
			set { m_currentCurrency = value;
				PlayerPrefs.SetString(CodeNetwork + BITCOIN_DEFAULT_CURRENCY, m_currentCurrency);
			}
		}
		public Dictionary<string, string> AddressesList
		{
			get { return m_addressesList; }
		}

		// -------------------------------------------
		/* 
		 * Initialitzation
		 */
		public void Init(params string[] _list)
		{
			if (m_initialized) return;
			m_initialized = true;

			System.Globalization.CultureInfo customCulture = (System.Globalization.CultureInfo)System.Threading.Thread.CurrentThread.CurrentCulture.Clone();
			customCulture.NumberFormat.NumberDecimalSeparator = ".";
			System.Threading.Thread.CurrentThread.CurrentCulture = customCulture;

			QBitNinjaClient.SetCompression(false);
#if DEBUG_MODE_DISPLAY_LOG
			Debug.Log("BitCoinController Initialized");
#endif
			string currentNetworkUsed = PlayerPrefs.GetString(OPTION_NETWORK_COOKIE, OPTION_NETWORK_TEST);
			m_isMainNetwork = (currentNetworkUsed == OPTION_NETWORK_MAIN);
			if ((_list != null) && (_list.Length > 0))
			{
				m_isMainNetwork = (_list[0] == OPTION_NETWORK_MAIN);
			}
			if (m_isMainNetwork)
			{
				m_network = NBitcoin.Network.Main; 
			}
			else
			{
				m_network = NBitcoin.Network.TestNet;
			}			
			BitcoinEventController.Instance.BitcoinEvent += new BitcoinEventHandler(OnBasicEvent);

			m_currentCurrency = PlayerPrefs.GetString(CodeNetwork + BITCOIN_DEFAULT_CURRENCY, CODE_DOLLAR);

			CommController.Instance.GetBitcoinExchangeRatesTable();
		}

		// -------------------------------------------
		/* 
		 * Destroy
		 */
		public void Destroy()
		{
			BitcoinEventController.Instance.BitcoinEvent -= OnBasicEvent;
			Destroy(_instance.gameObject);
			_instance = null;
		}

		// -------------------------------------------
		/* 
		* RestoreCurrentPrivateKey
		*/
		public void RestoreCurrentPrivateKey()
		{
			if ((m_currentPrivateKey.Length == 0) && (m_backupCurrentPrivateKey.Length > 0))
			{
				CurrentPrivateKey = m_backupCurrentPrivateKey;
			}
		}

		// -------------------------------------------
		/* 
		* ClearListDataAddresses
		*/
		public void ClearListDataAddresses()
		{
			m_addressesList.Clear();
		}

		// -------------------------------------------
		/* 
		* LoadDataAddresses
		*/
		public void LoadDataAddresses()
		{
			string encryptedAddresses = PlayerPrefs.GetString(CodeNetwork + BITCOIN_ADDRESSES_LIST, "");
			if ((encryptedAddresses == null) || (encryptedAddresses == "")) return;

			string dataAddresses = RJEncryptor.DecryptStringWithKey(encryptedAddresses, ENCRYPTION_KEY);
			m_addressesList.Clear();

			string[] arrayAddresses = dataAddresses.Split(SEPARATOR_ITEMS);
			for (int i = 0; i < arrayAddresses.Length; i++)
			{
				string[] addressWithLabel = arrayAddresses[i].Split(SEPARATOR_COMA);
				if (addressWithLabel.Length == 2)
				{
					string address = addressWithLabel[0];
					string label = addressWithLabel[1];
					m_addressesList.Add(address, label);
#if DEBUG_MODE_DISPLAY_LOG
					Debug.Log("address[" + address + "]::label[" + label + "]");
#endif
				}
			}
		}

		// -------------------------------------------
		/* 
		* SaveAddresses
		*/
		public void SaveAddresses(string _publicKeyAddress, string _labelAddress)
		{
			LoadDataAddresses();

			m_addressesList.Remove(_publicKeyAddress);
			m_addressesList.Add(_publicKeyAddress, _labelAddress);

			SaveAddressesStorage();
		}

		// -------------------------------------------
		/* 
		* SaveAddresses
		*/
		private void SaveAddressesStorage()
		{
			string dataAddresses = "";
			foreach (KeyValuePair<string, string> publicAddress in m_addressesList)
			{
				if (dataAddresses.Length > 0)
				{
					dataAddresses += SEPARATOR_ITEMS;
				}
				dataAddresses += publicAddress.Key + SEPARATOR_COMA + publicAddress.Value;
			}
			PlayerPrefs.SetString(CodeNetwork + BITCOIN_ADDRESSES_LIST, RJEncryptor.EncryptStringWithKey(dataAddresses, ENCRYPTION_KEY));
		}

		// -------------------------------------------
		/* 
		* ContainsAddress
		*/
		public bool ContainsAddress(string _publicKeyAddress)
		{
			LoadDataAddresses();
			return m_addressesList.Keys.Contains(_publicKeyAddress);
		}

		// -------------------------------------------
		/* 
		* GetListDataAddresses
		*/
		public List<ItemMultiObjectEntry> GetListDataAddresses(bool _excludeOwnerAccounts, params string[] _excludeAddresses)
		{
			LoadDataAddresses();
			List<ItemMultiObjectEntry> output = new List<ItemMultiObjectEntry>();
			foreach (KeyValuePair<string, string> address in m_addressesList)
			{
				bool shouldBeIncluded = true;
				for (int i = 0; i < _excludeAddresses.Length; i++)
				{
					if (_excludeAddresses[i] == address.Key)
					{
						shouldBeIncluded = false;
					}
				}
				if (_excludeOwnerAccounts)
				{
					if (ContainsPublicKey(address.Key))
					{
						shouldBeIncluded = false;
					}
				}
				if (shouldBeIncluded)
				{
					output.Add(new ItemMultiObjectEntry(address.Key, address.Value));
				}				
			}
			return output;
		}

		// -------------------------------------------
		/* 
		* AddressToLabel
		*/
		public string AddressToLabel(params string[] _publicKeyAddress)
		{
			LoadDataAddresses();
			string labelAddresses = "";
			string originalAddress = ""; 
			for (int i = 0; i < _publicKeyAddress.Length; i++)
			{
				if (originalAddress.Length > 0)
				{
					originalAddress += ":";
				}
				originalAddress += _publicKeyAddress[i];
				if (m_addressesList.Keys.Contains(_publicKeyAddress[i]))
				{
					if (labelAddresses.Length > 0)
					{
						labelAddresses += ":";
					}
					string labelAddress = "";
					if (m_addressesList.TryGetValue(_publicKeyAddress[i], out labelAddress))
					{
						labelAddresses += labelAddress;
					}
				}
			}
			if (labelAddresses.Length > 0)
			{
				return labelAddresses;
			}
			else
			{
				return originalAddress;
			}
		}

		// -------------------------------------------
		/* 
		* LoadPrivateKeys
		*/
		public void LoadPrivateKeys(bool _getBalance)
		{
			string currentPrivateKeySelected = PlayerPrefs.GetString(CodeNetwork + BITCOIN_PRIVATE_KEY_SELECTED, "");
			if (currentPrivateKeySelected.Length > 0)
			{
				m_currentPrivateKey = RJEncryptor.DecryptStringWithKey(currentPrivateKeySelected, ENCRYPTION_KEY);
				m_currentPublicKey = GetPublicKey(m_currentPrivateKey);
			}			

			string encryptedKeys = PlayerPrefs.GetString(CodeNetwork + BITCOIN_PRIVATE_KEYS, "");
			if ((encryptedKeys == null) || (encryptedKeys == "")) return;

			string dataKeys = RJEncryptor.DecryptStringWithKey(encryptedKeys, ENCRYPTION_KEY);
			m_privateKeys.Clear();

			string[] arrayKeys = dataKeys.Split(SEPARATOR_ITEMS);
			for (int i = 0; i < arrayKeys.Length; i++)
			{
				string[] keyWithBalance = arrayKeys[i].Split(SEPARATOR_COMA);
				if (keyWithBalance.Length == 2)
				{
					string key = keyWithBalance[0];
					decimal balance = decimal.Parse(keyWithBalance[1]);
					if (_getBalance)
					{
						balance = GetBalance(keyWithBalance[0]);
					}
					m_privateKeys.Add(key, balance);
					m_publicKeys.Add(key, GetPublicKey(key));
#if DEBUG_MODE_DISPLAY_LOG
					Debug.Log("key[" + key + "]::BALANCE[" + balance + "]");
#endif
				}				
			}
		}

		// -------------------------------------------
		/* 
		* RefreshBalancePrivateKeys
		*/
		public void RefreshBalancePrivateKeys()
		{
			Dictionary<string,decimal> newDataPrivateKeys = new Dictionary<string, decimal>();
			m_publicKeys.Clear();
			foreach (KeyValuePair<string, decimal> privateKey in m_privateKeys)
			{
				decimal newBalance = GetBalance(privateKey.Key);
				newDataPrivateKeys.Add(privateKey.Key, newBalance);
				BitcoinEventController.Instance.DispatchBitcoinEvent(EVENT_BITCOINCONTROLLER_BALANCE_UPDATED, privateKey.Key, newBalance);
				m_publicKeys.Add(privateKey.Key, GetPublicKey(privateKey.Key));
			}
			m_privateKeys.Clear();
			m_privateKeys = newDataPrivateKeys;
		}

		// -------------------------------------------
		/* 
		* SavePrivateKeys
		*/
		public void SavePrivateKeys()
		{
			string dataKeys = "";
			foreach (KeyValuePair<string, decimal> privateKey in m_privateKeys)
			{
				if (dataKeys.Length > 0)
				{
					dataKeys += SEPARATOR_ITEMS;
				}
				dataKeys += privateKey.Key + SEPARATOR_COMA + privateKey.Value;
			}
			PlayerPrefs.SetString(CodeNetwork + BITCOIN_PRIVATE_KEYS, RJEncryptor.EncryptStringWithKey(dataKeys, ENCRYPTION_KEY));
		}

		// -------------------------------------------
		/* 
		* AddPrivateKey
		*/
		public void AddPrivateKey(string _privateKey, bool _dispatchEvent)
		{
			string finalKey = _privateKey;
			if (!m_privateKeys.Keys.Contains(_privateKey))
			{				
				m_privateKeys.Add(_privateKey, GetBalance(finalKey, _dispatchEvent));
				m_publicKeys.Add(_privateKey, GetPublicKey(_privateKey));
			}
			else
			{
				m_privateKeys[_privateKey] = GetBalance(finalKey, _dispatchEvent);
			}
		}

		// -------------------------------------------
		/* 
		* ContainsPrivateKey
		*/
		public bool ContainsPrivateKey(string _privateKey)
		{
			return m_privateKeys.Keys.Contains(_privateKey);
		}

		// -------------------------------------------
		/* 
		* ContainsPublicKeysKey
		*/
		public bool ContainsPublicKey(string _publicKey)
		{
			return m_publicKeys.Values.Contains(_publicKey);
		}		

		// -------------------------------------------
		/* 
		* RemovePrivateKey
		*/
		public bool RemovePrivateKey(string _privateKey)
		{
			string finalKey = _privateKey;
			if (m_privateKeys.Remove(_privateKey))
			{
				if (m_addressesList.Remove(m_publicKeys[_privateKey]))
				{
					SaveAddressesStorage();
				}
				if (m_publicKeys.Remove(_privateKey))
				{
					SavePrivateKeys();
				}				
				return true;
			}
			else
			{
				return false;
			}
		}

		// -------------------------------------------
		/* 
		* GetListPrivateKeys
		*/
		public List<ItemMultiObjectEntry> GetListPrivateKeys(params string[] _excludePrivateKeys)
		{
			SortedDictionary<string, ItemMultiObjectEntry> orderedKeys = new SortedDictionary<string, ItemMultiObjectEntry>();
			foreach (KeyValuePair<string, decimal> privateKey in m_privateKeys)
			{
				bool includeKey = true;
				if ((_excludePrivateKeys != null) && (_excludePrivateKeys.Length > 0))
				{
					for (int i = 0; i < _excludePrivateKeys.Length; i++)
					{
						if (_excludePrivateKeys[i] == privateKey.Key)
						{
							includeKey = false;
						}
					}
				}
				if (includeKey)
				{
					orderedKeys.Add(AddressToLabel(m_publicKeys[privateKey.Key]), new ItemMultiObjectEntry(privateKey.Key, privateKey.Value));
				}
			}

			List<ItemMultiObjectEntry> output = new List<ItemMultiObjectEntry>();
			foreach (KeyValuePair<string, ItemMultiObjectEntry> itemPair in orderedKeys)
			{
				output.Add(itemPair.Value);
			}

			return output;
		}

		// -------------------------------------------
		/* 
		 * ValidatePrivateKey
		 */
		public bool ValidatePrivateKey(string _encryptedKey)
		{
			if (_encryptedKey == null) return false;
			if (_encryptedKey == "") return false;
			if (_encryptedKey == "null") return false;

			try
			{				
				BitcoinSecret bitcoinPrivateKey = new BitcoinSecret(_encryptedKey, m_network);
#if DEBUG_MODE_DISPLAY_LOG
				Debug.Log("ValidatePrivateKey::NETWORK["+m_network+"]::bitcoinPrivateKey.GetAddress()=" + bitcoinPrivateKey.GetAddress());
#endif
				GetBalance(_encryptedKey, true);
				return true;
			}
			catch (Exception err)
			{
#if DEBUG_MODE_DISPLAY_LOG
				Debug.Log("ValidatePrivateKey::ERROR[" + err.Message + "]==========" + err.StackTrace);
#endif
			}
			return false;
		}

		// -------------------------------------------
		/* 
		 * ValidatePublicKey
		 */
		public bool ValidatePublicKey(string _publicKey)
		{
			try
			{
				if (m_isMainNetwork)
				{
					return ValidateBitcoinAddress(_publicKey);
				}
				else
				{
					BitcoinPubKeyAddress btkAddress = new BitcoinPubKeyAddress(_publicKey, BitCoinController.Instance.Network);
					string publicKeyVerification = btkAddress.ScriptPubKey.GetDestinationAddress(BitCoinController.Instance.Network).ToString();
					return publicKeyVerification == _publicKey;
				}
			} catch (Exception err)
			{
#if DEBUG_MODE_DISPLAY_LOG
				Debug.Log("ValidatePublicKey::ERROR[" + err.Message + "]==========" + err.StackTrace);
#endif
				return false;
			}
		}

		// -------------------------------------------
		/* 
		* ValidateBitcoinAddress
		*/
		public static bool ValidateBitcoinAddress(string _address)
		{
			if (_address.Length < 26 || _address.Length > 35) return false;
			byte[] decoded = DecodeBase58(_address);
			var d1 = Hash(decoded.SubArray(0, 21));
			var d2 = Hash(d1);
			if (!decoded.SubArray(21, 4).SequenceEqual(d2.SubArray(0, 4))) return false;
			return true;
		}

		private const string Alphabet = "123456789ABCDEFGHJKLMNPQRSTUVWXYZabcdefghijkmnopqrstuvwxyz";
		private const int Size = 25;

		// -------------------------------------------
		/* 
		* DecodeBase58
		*/
		private static byte[] DecodeBase58(string _input)
		{
			var output = new byte[Size];
			foreach (var t in _input)
			{
				var p = Alphabet.IndexOf(t);
				if (p == -1) throw new Exception("invalid character found");
				var j = Size;
				while (--j > 0)
				{
					p += 58 * output[j];
					output[j] = (byte)(p % 256);
					p /= 256;
				}
				if (p != 0) throw new Exception("address too long");
			}
			return output;
		}

		// -------------------------------------------
		/* 
		* Hash
		*/
		private static byte[] Hash(byte[] _bytes)
		{
			var hasher = new SHA256Managed();
			return hasher.ComputeHash(_bytes);
		}

		// -------------------------------------------
		/* 
		* GetPublicKey
		*/
		public string GetPublicKey(string _dataKey)
		{
			try
			{
				BitcoinSecret bitcoinPrivateKey = new BitcoinSecret(_dataKey, m_network);
#if DEBUG_MODE_DISPLAY_LOG
				Debug.Log("GetPublicKey::bitcoinPrivateKey.GetAddress()=" + bitcoinPrivateKey.GetAddress());
#endif
				return bitcoinPrivateKey.GetAddress().ToString();
			}
			catch (Exception err)
			{
#if DEBUG_MODE_DISPLAY_LOG
				Debug.Log("ValidatePrivateKey::ERROR[" + err.Message + "]======" + err.StackTrace);
#endif
			}
			return "";
		}

		// -------------------------------------------
		/* 
		* GetBalance
		*/
		public decimal GetBalance(string _privateKey, bool _dispatchEvent = false)
		{
#if DEBUG_MODE_DISPLAY_LOG
			Debug.Log("GetBalance::TRYING TO GET BALANCE::m_network="+ m_network.ToString());
#endif
			BitcoinSecret bitcoinUserPrivateKey = new BitcoinSecret(_privateKey, m_network);
			List<Coin> unspentCoins = GetUnspentCoins(bitcoinUserPrivateKey.GetAddress(), _dispatchEvent);
			if (unspentCoins == null)
			{
				return 0;
			}
			else
			{
				return unspentCoins.Sum(x => x.Amount.ToDecimal(MoneyUnit.BTC));
			}				
		}

		// -------------------------------------------
		/* 
		* GetAllInformation
		*/
		public void GetAllInformation(string _publicKeyAdress)
		{
#if DEBUG_MODE_DISPLAY_LOG
			Debug.Log("GetBalance::TRYING TO GET BALANCE");
#endif
			BitcoinAddress address = BitcoinAddress.Create(_publicKeyAdress, m_network);
			GetSummaryAccount(address);
		}

		// -------------------------------------------
		/* 
		* GetUnspentCoins
		*/
		public List<Coin> GetUnspentCoins(BitcoinAddress _address, bool _dispatchEvent)
		{
			QBitNinjaClient clientQBitNinja = new QBitNinjaClient(NetworkAPI, m_network);
			var balanceModel = clientQBitNinja.GetBalance(_address, true).Result;
			if (balanceModel.Operations.Count == 0)
			{
#if DEBUG_MODE_DISPLAY_LOG
				Debug.Log("THERE ARE NO OPERATIONS FOR THIS ACCOUNT");
#endif
				if (_dispatchEvent) BitcoinEventController.Instance.DelayBasicEvent(EVENT_BITCOINCONTROLLER_BALANCE_WALLET, 0.2f, 0f, true);
				return null;
			}
			List<Coin> unspentCoins = new List<Coin>();
			foreach (var operation in balanceModel.Operations)
			{
				unspentCoins.AddRange(operation.ReceivedCoins.Select(coin => coin as Coin));
			}
			var balance = unspentCoins.Sum(x => x.Amount.ToDecimal(MoneyUnit.BTC));
#if DEBUG_MODE_DISPLAY_LOG
			Debug.Log("CURRENT BALANCE[" + balance + "]::DISPATCHED INFO[" + balance + "]");
#endif
			bool getTablesExchanges = false;
			if (m_balanceWallet != balance)
			{
				getTablesExchanges = true;
			}
			m_balanceWallet = balance;
			if (_dispatchEvent) BitcoinEventController.Instance.DelayBasicEvent(EVENT_BITCOINCONTROLLER_BALANCE_WALLET, 0.2f, (float)balance, getTablesExchanges);
			return unspentCoins;
		}

		// -------------------------------------------
		/* 
		* GetSummaryAccount
		*/
		private void GetSummaryAccount(BitcoinAddress _address)
		{
			QBitNinjaClient clientQBitNinja = new QBitNinjaClient(NetworkAPI, m_network);
			var balanceModel = clientQBitNinja.GetBalance(_address).Result;

			m_allTransactionsHistory = new List<ItemMultiObjectEntry>();
			m_inTransactionsHistory = new List<ItemMultiObjectEntry>();
			m_outTransactionsHistory = new List<ItemMultiObjectEntry>();
			List<Coin> unspentCoins = new List<Coin>();
			foreach (var operation in balanceModel.Operations)
			{
				string transactionID = operation.TransactionId.ToString();
				int transactionHeight = operation.Height;
				DateTimeOffset transactionDate = operation.FirstSeen;
				decimal transactionAmount = operation.Amount.ToDecimal(MoneyUnit.BTC);

				// CALCULATE FOR THE CURRENT BALANCE
				unspentCoins.AddRange(operation.ReceivedCoins.Select(coin => coin as Coin));

				// GET OUTPUT TRANSACTIONS INFO
				var transactionId = uint256.Parse(transactionID);
				GetTransactionResponse transactionResponse = clientQBitNinja.GetTransaction(transactionId).Result;
				decimal transactionFee = transactionResponse.Transaction.GetFee(transactionResponse.SpentCoins.ToArray()).ToUnit(MoneyUnit.BTC);
				if (transactionAmount < 0) transactionAmount += transactionFee;
				var outputs = transactionResponse.Transaction.Outputs;
				List<ItemMultiTextEntry> transactionsAddresses = new List<ItemMultiTextEntry>();
				string transactionMessage = "";
				foreach (TxOut output in outputs)
				{
					Money amount = output.Value;
					decimal valueCurrentTransaction = amount.ToDecimal(MoneyUnit.BTC);

					if (amount != 0)
					{
						BitcoinAddress address = output.ScriptPubKey.GetDestinationAddress(m_network);
						if (address != null)
						{
							if (address != _address)
							{
								transactionsAddresses.Add(new ItemMultiTextEntry(amount.ToString(), address.ToString()));
							}
						}
					}
					else
					{						
						byte[][] messageBytes = TxNullDataTemplate.Instance.ExtractScriptPubKeyParameters(output.ScriptPubKey);
						transactionMessage = Encoding.UTF8.GetString(messageBytes[0]);
					}
				}

				if (transactionAmount > 0)
				{
					m_inTransactionsHistory.Add(new ItemMultiObjectEntry(transactionID, transactionHeight, transactionDate, transactionAmount, transactionFee, transactionMessage, transactionsAddresses));
				}
				else
				{
					m_outTransactionsHistory.Add(new ItemMultiObjectEntry(transactionID, transactionHeight, transactionDate, transactionAmount, transactionFee, transactionMessage, transactionsAddresses));
				}
				m_allTransactionsHistory.Add(new ItemMultiObjectEntry(transactionID, transactionHeight, transactionDate, transactionAmount, transactionFee, transactionMessage, transactionsAddresses));
			}
		}

		// -------------------------------------------
		/* 
		* ToStringTransaction
		*/
		public static string ToStringTransaction(ItemMultiObjectEntry _transaction)
		{
			string transactionID = (string)_transaction.Objects[0];
			int transactionHeight = (int)_transaction.Objects[1];
			DateTimeOffset transactionDate = (DateTimeOffset)_transaction.Objects[2];
			decimal transactionAmount = (decimal)_transaction.Objects[3];
			decimal transactionFee = (decimal)_transaction.Objects[4];
			string transactionMessage = (string)_transaction.Objects[5];
			List<ItemMultiTextEntry> transactionScriptPubKey = (List<ItemMultiTextEntry>)_transaction.Objects[6];

			string addresses = "";
			for (int i = 0; i < transactionScriptPubKey.Count; i++)
			{
				ItemMultiTextEntry item = transactionScriptPubKey[i];
				if (addresses.Length > 0)
				{
					addresses += "::";
				}				

				addresses += item.Items[1];
			}

			return "DATE["+transactionDate.ToString() + "];AMOUNT["+ transactionAmount + "];FEE["+ transactionFee + "];MESSAGE["+ transactionMessage + "];ADDRESSES["+ addresses + "]";
		}

		// -------------------------------------------
		/* 
		* GetTransactionInputCoins
		*/
		private List<Coin> GetTransactionInputCoins(BitcoinAddress _customerAddress, List<Coin> _coinsCustomer, decimal _totalAmount)
		{
			List<Coin> coinsToSpend = new List<Coin>();
			decimal acumulated = 0;
#if DEBUG_MODE_DISPLAY_LOG
			Debug.Log("TOTAL COINS TO ANALIZE[" + _coinsCustomer.Count + "]");
#endif
			for (int i = _coinsCustomer.Count - 1; i >= 0; i--)
			{
				var coin = _coinsCustomer[i];
				if (acumulated < _totalAmount)
				{
					if (coin.TxOut.ScriptPubKey == _customerAddress.ScriptPubKey)
					{
						decimal realAmountCoin = coin.Amount.ToUnit(MoneyUnit.BTC);
						acumulated += realAmountCoin;
						coinsToSpend.Add(coin);
					}
				}
			}
			if (coinsToSpend.Count == 0)
			{
#if DEBUG_MODE_DISPLAY_LOG
				Debug.Log("TxOut doesn't contain our ScriptPubKey");
#endif
				return null;
			}
			else
			{
#if DEBUG_MODE_DISPLAY_LOG
				Debug.Log("We want to spend " + coinsToSpend.Count + " outpoints");

				for (int k = 0; k < coinsToSpend.Count; k++)
				{
					Debug.Log("    [COIN:" + k + "][AMOUNT:" + coinsToSpend[k].Amount.ToUnit(MoneyUnit.BTC) + "][OUTPOINT:" + coinsToSpend[k].Outpoint.N + "]");
				}
#endif
				return coinsToSpend;
			}
		}

		// -------------------------------------------
		/* 
		* AddInputs
		*/
		private bool AddInputs(Transaction _transactionCustomer, BitcoinSecret _customerPrivateKey, List<Coin> _coinsToSpend, decimal _amountTotal)
		{
			if ((_coinsToSpend == null) || (_coinsToSpend.Count == 0))
			{
#if DEBUG_MODE_DISPLAY_LOG
				Debug.Log("++ERROR++ CONSUMER HAS NO COINS");
#endif
				BitcoinEventController.Instance.DispatchBitcoinEvent(EVENT_BITCOINCONTROLLER_TRANSACTION_DONE, false, "text.locations.you.have.no.coins");
				return false;
			}

			// FILL THE INPUT TRANSACTIONS
			for (int i = 0; i < _coinsToSpend.Count; i++)
			{
				_transactionCustomer.Inputs.Add(new TxIn()
				{
					PrevOut = _coinsToSpend[i].Outpoint
				});
			}
			return true;
		}

		// -------------------------------------------
		/* 
		* AddOutputs
		*/
		private void AddOutputs(Transaction _transactionCustomer,
								BitcoinSecret _customerPrivateKey,
								decimal _finalFeeAmount,
								PaymentModel[] _payments,
								decimal _totalAmountToSpend,
								List<Coin> _coinsToSpend)
		{
			// GET THE MONEY TO SPEND
			if ((_coinsToSpend == null) || (_coinsToSpend.Count == 0))
			{
#if DEBUG_MODE_DISPLAY_LOG
				Debug.Log("++ERROR++ THERE IS NO COIN TO SELECT");
#endif
				BitcoinEventController.Instance.DispatchBitcoinEvent(EVENT_BITCOINCONTROLLER_TRANSACTION_DONE, false, "text.locations.you.have.no.coins");
				return;
			}
			Money txInAmount = _coinsToSpend[0].Amount;
			for (int i = 1; i < _coinsToSpend.Count; i++)
			{
				txInAmount += _coinsToSpend[i].Amount;
			}
#if DEBUG_MODE_DISPLAY_LOG
			Debug.Log("(ADD OUTPUTS) TOTAL AMOUNT TO PAY = " + txInAmount.ToUnit(MoneyUnit.BTC));
#endif
			for (int i = 0; i < _payments.Length; i++)
			{
				PaymentModel paymentItem = _payments[i];
#if DEBUG_MODE_DISPLAY_LOG
				Debug.Log("    PAY TO[" + i + "][" + paymentItem.PublicKeyAddress + "][" + paymentItem.AmountToPay + "]");
#endif
			}

			// CALCULATE THE MONEY TO RETURN TO ORIGIN (THE TRANSACTION SHOULD CONSUME ALL THE COINS, THAT'S WHY IT'S LIKE THIS)
			Money changeBackAmount = txInAmount;
			for (int i = 0; i < _payments.Length; i++)
			{
				changeBackAmount -= _payments[i].MoneyToPay;
			}

			// SET MINERS FEE
			changeBackAmount -= new Money(_finalFeeAmount, MoneyUnit.BTC);

#if DEBUG_MODE_DISPLAY_LOG
			Debug.Log("    CHANGE BACK[" + changeBackAmount.ToUnit(MoneyUnit.BTC) + "]");
#endif

			// ADD TXOUT OF DESTINATIONS
			for (int i = 0; i < _payments.Length; i++)
			{
				TxOut destinationTxOut = new TxOut()
				{
					Value = _payments[i].MoneyToPay,
					ScriptPubKey = _payments[i].PublicKeyAddress.ScriptPubKey
				};
				_transactionCustomer.Outputs.Add(destinationTxOut);
			}

			// ADD TXOUT OF CHANGE BACK
			TxOut originTxOut = new TxOut()
			{
				Value = changeBackAmount,
				ScriptPubKey = _customerPrivateKey.ScriptPubKey
			};
			_transactionCustomer.Outputs.Add(originTxOut);
		}


		// -------------------------------------------
		/* 
		* AddOutputMessage
		*/
		private void AddOutputMessage(Transaction _transactionCustomer, string _text)
		{
			// INFORMATION RELATED WITH THE TRANSACTION NAME (MAXIMUM NUMBER OF 40 CHARACTERS)
			string message = _text;
			if (message.Length >= 40) message = message.Substring(0, 39);
			var bytes = Encoding.UTF8.GetBytes(message);
			_transactionCustomer.Outputs.Add(new TxOut()
			{
				Value = Money.Zero,
				ScriptPubKey = TxNullDataTemplate.Instance.GenerateScriptPubKey(bytes)
			});
		}

		// -------------------------------------------
		/* 
		* SignTransaction
		*/
		private void SignTransaction(Transaction _transactionCustomer, BitcoinSecret _customerPrivateKey)
		{
			for (int i = 0; i < _transactionCustomer.Inputs.Count; i++)
			{
				_transactionCustomer.Inputs[i].ScriptSig = _customerPrivateKey.ScriptPubKey;
			}
			_transactionCustomer.Sign(_customerPrivateKey, false);
		}

		// -------------------------------------------
		/* 
		* BroadcastTransaction
		*/
		private void BroadcastTransaction(QBitNinjaClient _clientQBitNinja, Transaction _customerTransaction)
		{
			// BROADCAST TRANSACTION
			BroadcastResponse broadcastResponse = _clientQBitNinja.Broadcast(_customerTransaction).Result;
			if (broadcastResponse.Success)
			{
				// IT IS RETURNING ALWAYS TRUE BUT THE TRANSACTION IS NOWHERE
#if DEBUG_MODE_DISPLAY_LOG
				Debug.Log("Success! You can check out the hash of the transaciton in any block explorer:");
				Debug.Log("HASH");
				Debug.Log(_customerTransaction.GetHash().ToString());
				Debug.Log("HEXADECIMAL");
				Debug.Log(_customerTransaction.ToHex());
#endif
				BitcoinEventController.Instance.DelayBasicEvent(EVENT_BITCOINCONTROLLER_TRANSACTION_DONE, 0.1f, true);
			}
			else
			{
#if DEBUG_MODE_DISPLAY_LOG
				Debug.Log("ErrorCode: " + broadcastResponse.Error.ErrorCode);
				Debug.Log("Error message: " + broadcastResponse.Error.Reason);
#endif

				BitcoinEventController.Instance.DispatchBitcoinEvent(EVENT_BITCOINCONTROLLER_TRANSACTION_DONE, false, "Transaction failed to be broadcasted to the network");
			}
		}

		// -------------------------------------------
		/* 
		* PushTransactionToNetwork
		*/
		private void PushTransactionToNetwork()
		{
			/*
			UnityWebRequest www = UnityWebRequest.Post("https://blockchain.info/pushtx?cors=true", "{tx:" + m_customerTransactionHex + "}");
			www.SetRequestHeader("Content-Type", "application/x-www-form-urlencoded");
			StartCoroutine(WaitForRequest(www));
			*/
		}

		// -------------------------------------------
		/* 
		* WaitForRequest
		*/
		System.Collections.IEnumerator WaitForRequest(UnityWebRequest www)
		{
			yield return www.SendWebRequest();
		}

		// -------------------------------------------
		/* 
		* TotalAmountToPay
		*/
		private decimal TotalAmountToPay(PaymentModel[] _payments)
		{
			decimal output = 0;
			for (int i = 0; i < _payments.Length; i++)
			{
				output += _payments[i].AmountToPay;
			}
			return output;
		}

		// -------------------------------------------
		/* 
		* PayExperience
		*/
		private void ExecuteTransaction(string _title, decimal _finalFeeAmount, params PaymentModel[] _payments)
		{
			decimal totalAmountToPay = TotalAmountToPay(_payments) + _finalFeeAmount;

#if DEBUG_MODE_DISPLAY_LOG
			Debug.Log("*****************************************************************************");
			Debug.Log("START PayExperience::PRICE[" + totalAmountToPay + "]::FEE["+ _finalFeeAmount + "]");
			Debug.Log("*****************************************************************************");
#endif

			// Create a client
			QBitNinjaClient clientQBitNinja = new QBitNinjaClient(NetworkAPI, m_network);

			// CUSTOMER PRIVATE KEY
			var customerPrivateKey = new BitcoinSecret(m_currentPrivateKey);
			List<Coin> coinsInCustomerWallet = GetUnspentCoins(customerPrivateKey.GetAddress(), false);

			// GET THE COINS WE HAVE TO SPEND FROM THE CUSTOMER WALLET
			List<Coin> coinsToSpendInVideo = GetTransactionInputCoins(customerPrivateKey.GetAddress(), coinsInCustomerWallet, totalAmountToPay);

			// TRANSACTION
			Transaction customerTransaction = new Transaction();

			// ADD INPUTS
			AddInputs(customerTransaction, customerPrivateKey, coinsToSpendInVideo, totalAmountToPay);

			// ADD OUTPUTS
			AddOutputs(customerTransaction, customerPrivateKey, _finalFeeAmount, _payments, totalAmountToPay, coinsToSpendInVideo);

			// ADD MESSAGE
			AddOutputMessage(customerTransaction, _title);

			// SIGN MESSAGE
			SignTransaction(customerTransaction, customerPrivateKey);

			// BROADCAST TRANSACTION
			BroadcastTransaction(clientQBitNinja, customerTransaction);
		}

		// -------------------------------------------
		/* 
		* GetBalanceForCurrency
		*/
		public decimal GetBalanceForCurrency(string _currency)
		{
			decimal currencyForBalance = -1;
			m_walletBalanceCurrencies.TryGetValue(_currency, out currencyForBalance);
			return currencyForBalance;
		}

		// -------------------------------------------
		/* 
		* Main call to make the payment
		*/
		public void Pay(string _currentPrivateKey,  string _publicKey, string _title, decimal _finalValueBitcoins, decimal _finalFeeAmount)
		{
			m_titleTransaction = _title;
			m_currentPrivateKey = _currentPrivateKey;
			m_publicKeyTarget = _publicKey;
			m_finalValueBitcoins = _finalValueBitcoins;
			m_finalFeeAmount = _finalFeeAmount;			

			// CUSTOMER HAS ENOUGH FUNDS?
			var customerPrivateKey = new BitcoinSecret(m_currentPrivateKey);
			decimal balanceCurrent = GetBalance(m_currentPrivateKey);
			if (balanceCurrent < m_finalValueBitcoins + m_finalFeeAmount)
			{
				string balanceCurrencyTrimmed = Utilities.Trim((m_currenciesExchange[m_currentCurrency] * balanceCurrent).ToString());
				balanceCurrencyTrimmed += " " + m_currentCurrency;
				string transactionCurrencyTrimmed = Utilities.Trim((m_currenciesExchange[m_currentCurrency] * m_finalValueBitcoins).ToString());
				transactionCurrencyTrimmed += " " + m_currentCurrency;
				BitcoinEventController.Instance.DispatchBitcoinEvent(EVENT_BITCOINCONTROLLER_TRANSACTION_DONE, false, "There is not enough balance to perform the transaction. Current balance="+ balanceCurrencyTrimmed + " and Transaction requires=" + transactionCurrencyTrimmed);
				return;
			}

			// EXECUTE PAYMENT
			PaymentModel paymentForProvider = new PaymentModel(m_publicKeyTarget, m_network, m_finalValueBitcoins);
			ExecuteTransaction(m_titleTransaction, _finalFeeAmount, paymentForProvider);
		}

		// -------------------------------------------
		/* 
		* Manager of global events
		*/
		private void OnBasicEvent(string _nameEvent, params object[] _list)
		{
			if (_nameEvent == EVENT_BITCOINCONTROLLER_JSON_EXCHANGE_TABLE)
			{
				m_walletBalanceCurrencies.Clear();
				m_currenciesExchange.Clear();
				JSONNode jsonExchangeTable = JSON.Parse((string)_list[0]);
#if DEBUG_MODE_DISPLAY_LOG
				Debug.Log("BITCOINS IN WALLET[" + m_balanceWallet + "]");
#endif
				for (int i = 0; i < CURRENCY_CODE.Length; i++)
				{
					string currencyCode = CURRENCY_CODE[i];
					if (currencyCode == CODE_BITCOIN)
					{
						m_walletBalanceCurrencies.Add(currencyCode, m_balanceWallet * 1);
						m_currenciesExchange.Add(currencyCode, 1);
					}
					else
					{
						decimal exchangeValue = decimal.Parse(jsonExchangeTable[currencyCode]["sell"]);
						m_walletBalanceCurrencies.Add(currencyCode, m_balanceWallet * exchangeValue);
						m_currenciesExchange.Add(currencyCode, exchangeValue);
#if DEBUG_MODE_DISPLAY_LOG
						Debug.Log("BALANCE IN[" + currencyCode + "] IS[" + (m_balanceWallet * exchangeValue) + "]");
#endif
					}
				}
				CommController.Instance.GetBitcoinTransactionFee();
			}
			if (_nameEvent == EVENT_BITCOINCONTROLLER_JSON_FEE_TABLE)
			{
#if DEBUG_MODE_DISPLAY_LOG
				Debug.Log("FEES TABLE: " + (string)_list[0]);
#endif
				JSONNode jsonFee = JSON.Parse((string)_list[0]);
				int finalFeeAmount = 5;
				m_feesTransactions.Clear();
				if (int.TryParse(jsonFee[FEE_FASTEST], out finalFeeAmount))
				{
					decimal fastFeeAmount = (((decimal)finalFeeAmount / (decimal)MoneyUnit.BTC) * (decimal)BitCoinController.ESTIMATED_SIZE_BLOCK);					
					m_feesTransactions.Add(FEE_LABEL_FASTEST, fastFeeAmount);
#if DEBUG_MODE_DISPLAY_LOG
					Debug.Log("FASTEST FEE: " + m_feesTransactions[FEE_LABEL_FASTEST]);
#endif
				}
				if (int.TryParse(jsonFee[FEE_HALFHOUR], out finalFeeAmount))
				{
					decimal halfHourFeeAmount = (((decimal)finalFeeAmount / (decimal)MoneyUnit.BTC) * (decimal)BitCoinController.ESTIMATED_SIZE_BLOCK);
					m_feesTransactions.Add(FEE_LABEL_HALFHOUR, halfHourFeeAmount);
#if DEBUG_MODE_DISPLAY_LOG
					Debug.Log("HALFHOUR FEE: " + m_feesTransactions[FEE_LABEL_HALFHOUR]);
#endif
				}
				if (int.TryParse(jsonFee[FEE_HOUR], out finalFeeAmount))
				{
					decimal hourFeeAmount = (((decimal)finalFeeAmount / (decimal)MoneyUnit.BTC) * (decimal)BitCoinController.ESTIMATED_SIZE_BLOCK);
					m_feesTransactions.Add(FEE_LABEL_HOUR, hourFeeAmount);
#if DEBUG_MODE_DISPLAY_LOG
					Debug.Log("HOUR FEE: " + m_feesTransactions[FEE_LABEL_HOUR]);
#endif
				}

				decimal twentyCents = 0.20m;
				decimal minEstimated = twentyCents / (decimal)m_currenciesExchange[CODE_DOLLAR];
				m_feesTransactions.Add(FEE_LABEL_MIN_ESTIMATED, minEstimated);

#if DEBUG_MODE_DISPLAY_LOG
				Debug.Log("MIN ESTIMATED FEE: " + m_feesTransactions[FEE_LABEL_MIN_ESTIMATED]);
#endif

				BitcoinEventController.Instance.DispatchBitcoinEvent(EVENT_BITCOINCONTROLLER_ALL_DATA_COLLECTED);
			}
			if (_nameEvent == EVENT_BITCOINCONTROLLER_TRANSACTION_DONE)
			{
				if (!(bool)_list[0])
				{
					BitcoinEventController.Instance.DispatchBitcoinEvent(EVENT_BITCOINCONTROLLER_TRANSACTION_COMPLETED, false, (string)_list[1]);
				}
				else
				{
					BitcoinEventController.Instance.DispatchBitcoinEvent(EVENT_BITCOINCONTROLLER_TRANSACTION_COMPLETED, true);

					// UPDATE WALLET
					AddPrivateKey(m_currentPrivateKey, false);
				}
			}
		}
	}

}