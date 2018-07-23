#if ENABLE_BITCOIN
using NBitcoin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

namespace YourBitcoinController
{

	/******************************************
	 * 
	 * PaymentModel
	 * 
	 * @author Esteban Gallardo
	 */
	public class PaymentModel
	{
		// ----------------------------------------------
		// PRIVATE MEMBERS
		// ----------------------------------------------
		private BitcoinAddress m_bitcoinAddress;
		private decimal m_amountToPay;
		private Money m_moneyToPay;

		public BitcoinAddress PublicKeyAddress
		{
			get { return m_bitcoinAddress; }
		}
		public decimal AmountToPay
		{
			get { return m_amountToPay; }
		}
		public Money MoneyToPay
		{
			get { return m_moneyToPay; }
		}

		// -------------------------------------------
		/* 
		 * Constructor
		 */
		public PaymentModel(string _publicKeyAddress, NBitcoin.Network _network, decimal _amountToPay)
		{
			m_bitcoinAddress = BitcoinAddress.Create(_publicKeyAddress, _network);
			m_amountToPay = _amountToPay;
			m_moneyToPay = new Money(m_amountToPay, MoneyUnit.BTC);
		}
	}
}
#endif