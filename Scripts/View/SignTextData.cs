using UnityEngine;
using System.Collections.Generic;
#if ENABLE_BITCOIN
using NBitcoin;
#endif

namespace YourBitcoinController
{
	/******************************************
	 * 
	 * SignTextData
	 * 
	 * Allows to sign the data with your private key
	 * in order to prove that the data is yours
	 * 
	 * @author Esteban Gallardo
	 */
	public class SignTextData : MonoBehaviour
	{
		// ----------------------------------------------
		// PUBLIC MEMBERS
		// ----------------------------------------------
		public GUISkin SkinUI;

		// ----------------------------------------------
		// PRIVATE CONSTANTS MEMBERS
		// ----------------------------------------------
		private const string PRIVATE_ROOT_KEY = "cTbnpYYGcHx3EjxXrBrF7dbgGXjJtuqtsh7k5KJZsz8G3hDgb981";
		private const string PUBLICK_ROOT_KEY = "migfekTLcGCgko4dDhWECPuik1s5nR4wq5";
		
		// ----------------------------------------------
		// PRIVATE MEMBERS
		// ----------------------------------------------
		private List<string> m_displayMessages = new List<string>();
		private Vector2 m_scrollPosition = Vector2.zero;
		private bool m_activateTextArea = false;

		private string m_textData = "";
		private string m_textSigned = "";

		// -------------------------------------------
		/* 
		 * Runs as soon as the object is active
		 */
		void Start()
		{
			BitCoinController.Instance.Init(BitCoinController.OPTION_NETWORK_TEST);
		}

		// -------------------------------------------
		/* 
		* Display messages on screen and buttons on screen
		*/
		void OnGUI()
		{
			GUI.skin = SkinUI;

			float fontSize = 1.2f * 15;

			// BUTTON CLEAR LOG
			float yGlobalPosition = 10;
			if (GUI.Button(new Rect(new Vector2(10, yGlobalPosition), new Vector2(Screen.width - 20, 2 * fontSize)), "Clear Log"))
			{
				m_activateTextArea = false;
				m_displayMessages.Clear();
			}
			yGlobalPosition += 2.2f * fontSize;

			// TITLE
			GUI.Label(new Rect(new Vector2(10, yGlobalPosition), new Vector2(Screen.width - 20, 2 * fontSize)), "Write the text data you want to sign:");
			yGlobalPosition += 2.2f * fontSize;

			// TEXTFIELD
			m_textData = GUI.TextField(new Rect(new Vector2(10, yGlobalPosition), new Vector2(Screen.width - 20, 3 * fontSize)), m_textData);
			yGlobalPosition += 3.2f * fontSize;

			// SIGN DATA
			if (GUI.Button(new Rect(new Vector2(10, yGlobalPosition), new Vector2(Screen.width - 20, 2 * fontSize)), "Sign the data with you key"))
			{
				if (m_textData.Length == 0)
				{
					AddLog("There is no text data to sign");
				}
				else
				{
#if ENABLE_BITCOIN
					AddLog("DATA SIGNED=" + BitCoinController.Instance.SignTextData(m_textData, PRIVATE_ROOT_KEY));
#endif
				}
			}
			yGlobalPosition += 2.2f * fontSize;

			// TITLE
			GUI.Label(new Rect(new Vector2(10, yGlobalPosition), new Vector2(Screen.width - 20, 2 * fontSize)), "Verify the data:");
			yGlobalPosition += 2.2f * fontSize;

			// TEXTFIELD
			m_textSigned = GUI.TextField(new Rect(new Vector2(10, yGlobalPosition), new Vector2(Screen.width - 20, 3 * fontSize)), m_textSigned);
			yGlobalPosition += 3.2f * fontSize;

			// SIGN DATA
			if (GUI.Button(new Rect(new Vector2(10, yGlobalPosition), new Vector2(Screen.width - 20, 2 * fontSize)), "Verify the signed data"))
			{
				if (m_textSigned.Length == 0)
				{
					AddLog("There is no text data verify");
				}
				else
				{
#if ENABLE_BITCOIN
					AddLog("IS DATA SIGNED BY ME? " + BitCoinController.Instance.VerifySignedData(m_textData, m_textSigned, PUBLICK_ROOT_KEY));
#endif
				}
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
	}
}