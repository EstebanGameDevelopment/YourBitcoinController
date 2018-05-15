using System;
using UnityEngine;
using System.Collections;

namespace YourBitcoinController
{

	/******************************************
	 * 
	 * Class that handles states
	 * 
	 * @author Esteban Gallardo
	 */
	public class StateManager : MonoBehaviour
	{
		protected int m_state;
		protected int m_lastState;
		protected int m_iterator;
		protected float m_timeAcum;
		protected float m_scale;

		// -------------------------------------------
		/* 
		 * Constructor		
		 */
		public StateManager()
		{
			m_iterator = 0;
			m_state = -1;
		}

		// ----------------------------------------------
		// GETTERS/SETTERS
		// ----------------------------------------------		
		public int State
		{
			get { return m_state; }
			set { m_state = value; }
		}
		public int LastState
		{
			get { return m_lastState; }
			set { m_lastState = value; }
		}

		// -------------------------------------------
		/* 
		 * Change the state of the object		
		 */
		public virtual void ChangeState(int newState)
		{
			SetState(newState);
		}

		// -------------------------------------------
		/* 
		 * Change the state of the object		
		 */
		protected virtual void SetState(int _newState)
		{
			m_lastState = m_state;
			m_iterator = 0;
			m_state = _newState;
			m_timeAcum = 0;
		}

		// -------------------------------------------
		/* 
		 * Update		
		 */
		public virtual void Logic()
		{
			if (m_iterator < 100000) m_iterator++;
		}
	}
}