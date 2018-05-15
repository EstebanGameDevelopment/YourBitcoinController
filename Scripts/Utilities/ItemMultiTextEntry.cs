using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using System.IO;

namespace YourBitcoinController
{

	public class ItemMultiTextEntry
	{
		private List<string> m_items;

		public List<string> Items
		{
			get { return m_items; }
		}

		// -------------------------------------------
		/* 
		 * Constructor
		 */
		public ItemMultiTextEntry(params string[] _list)
		{
			m_items = new List<string>();
			for (int i = 0; i < _list.Length; i++)
			{
				if (_list[i].Length > 0)
				{
					m_items.Add(_list[i]);
				}
			}
		}

		// -------------------------------------------
		/* 
		 * Clone
		 */
		public ItemMultiTextEntry Clone()
		{
			ItemMultiTextEntry output = new ItemMultiTextEntry(m_items.ToArray());
			return output;
		}

		// -------------------------------------------
		/* 
		 * CloneList
		 */
		public static List<ItemMultiTextEntry> CloneList(List<ItemMultiTextEntry> _list)
		{
			List<ItemMultiTextEntry> output = new List<ItemMultiTextEntry>();
			for (int i = 0; i < _list.Count; i++)
			{
				output.Add(_list[i].Clone());
			}
			return output;
		}

		// -------------------------------------------
		/* 
		 * EqualsEntry
		 */
		public bool EqualsEntry(params string[] _list)
		{
			bool output = true;
			for (int i = 0; i < m_items.Count; i++)
			{
				if (i < _list.Length)
				{
					if (m_items[i] != _list[i])
					{
						output = false;
					}
				}
				else
				{
					output = false;
				}
			}
			return output;
		}

		// -------------------------------------------
		/* 
		 * EqualsEntry
		 */
		public bool EqualsEntry(ItemMultiTextEntry _item)
		{
			bool output = true;
			for (int i = 0; i < m_items.Count; i++)
			{
				if (i < _item.Items.Count)
				{
					if (m_items[i] != _item.Items[i])
					{
						output = false;
					}
				}
				else
				{
					output = false;
				}
			}
			return output;
		}

		// -------------------------------------------
		/* 
		 * Package
		 */
		public string Package()
		{
			string output = "";
			for (int i = 0; i < m_items.Count; i++)
			{
				if (i < m_items.Count - 1)
				{
					output += m_items[i] + CommController.TOKEN_SEPARATOR_EVENTS;
				}
				else
				{
					output += m_items[i];
				}
			}
			return output;
		}

		// -------------------------------------------
		/* 
		 * Package
		 */
		public string Package(string _separator)
		{
			string output = "";
			for (int i = 0; i < m_items.Count; i++)
			{
				if (i < m_items.Count - 1)
				{
					output += m_items[i] + _separator;
				}
				else
				{
					output += m_items[i];
				}
			}
			return output;
		}
	}
}