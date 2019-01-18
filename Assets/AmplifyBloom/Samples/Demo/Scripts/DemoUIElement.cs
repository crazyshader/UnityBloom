// Amplify Bloom - Advanced Bloom Post-Effect for Unity
// Copyright (c) Amplify Creations, Lda <info@amplify.pt>

using UnityEngine;
using UnityEngine.UI;

namespace AmplifyBloom
{
	public enum DemoUIElementAction
	{
		Press = 0,
		Slide
	};

	public class DemoUIElement : MonoBehaviour
	{
		private bool m_isSelected = false;
		private Text m_text;
		private Color m_selectedColor = new Color( 1, 1, 1 );
		private Color m_unselectedColor;

		public void Init()
		{
			m_text = transform.GetComponentInChildren<Text>();
			m_unselectedColor = m_text.color;
		}

		virtual public void DoAction( DemoUIElementAction action, params object[] vars )
		{

		}

		virtual public void Idle()
		{
		}

		public bool Select
		{
			get { return m_isSelected; }
			set
			{
				m_isSelected = value;
				m_text.color = value ? m_selectedColor : m_unselectedColor;
			}
		}
		
	}
}