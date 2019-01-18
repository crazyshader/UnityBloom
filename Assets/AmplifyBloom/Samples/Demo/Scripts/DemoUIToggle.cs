// Amplify Bloom - Advanced Bloom Post-Effect for Unity
// Copyright (c) Amplify Creations, Lda <info@amplify.pt>

using UnityEngine.UI;

namespace AmplifyBloom
{

	public sealed class DemoUIToggle : DemoUIElement
	{
		private Toggle m_toggle;
		
		void Start()
		{
			m_toggle = GetComponent<Toggle>();
		}

		public override void DoAction( DemoUIElementAction action, params object[] vars )
		{
			if ( !m_toggle.IsInteractable() )
				return;

			if ( action == DemoUIElementAction.Press )
			{
				m_toggle.isOn = !m_toggle.isOn;
			}
		}
	}
}