using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using System.IO;

namespace YourBitcoinController
{
	public class ItemMultiObjects
	{
		private List<object> m_objects;

		public List<object> Objects
		{
			get { return m_objects; }
		}

		// -------------------------------------------
		/* 
		 * Constructor
		 */
		public ItemMultiObjects(params object[] _list)
		{
			m_objects = new List<object>();
			for (int i = 0; i < _list.Length; i++)
			{
				if (_list[i] != null)
				{
					m_objects.Add(_list[i]);
				}
			}
		}

		// -------------------------------------------
		/* 
		 * EqualsEntry
		 */
		public bool EqualsEntry(params object[] _list)
		{
			bool output = true;
			for (int i = 0; i < m_objects.Count; i++)
			{
				if (i < _list.Length)
				{
					if (m_objects[i] != _list[i])
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
		public bool EqualsEntry(ItemMultiObjects _item)
		{
			bool output = true;
			for (int i = 0; i < m_objects.Count; i++)
			{
				if (i < _item.Objects.Count)
				{
					if (m_objects[i] != _item.Objects[i])
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
	}
}