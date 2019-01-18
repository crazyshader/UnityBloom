// Amplify Bloom - Advanced Bloom Post-Effect for Unity
// Copyright (c) Amplify Creations, Lda <info@amplify.pt>
using UnityEngine;
using UnityEngine.UI;

namespace AmplifyBloom
{
	public sealed class DemoUISlider : DemoUIElement
	{
		public bool SingleStep = false;
		private Slider m_slider;
		private bool m_lastStep = false;
		void Start()
		{
			m_slider = GetComponent<Slider>();
		}

		public override void DoAction( DemoUIElementAction action, params object[] vars )
		{
			if ( !m_slider.IsInteractable() )
				return;

			if ( action == DemoUIElementAction.Slide )
			{
				float slideAmount = ( float ) vars[ 0 ];
				if ( SingleStep )
				{
					if ( m_lastStep )
					{
						return;
					}
					m_lastStep = true;
				}

				if ( m_slider.wholeNumbers )
				{
					if ( slideAmount > 0 )
					{
						m_slider.value += 1;
					}
					else if ( slideAmount < 0 )
					{
						m_slider.value -= 1;
					}
				}
				else
				{
					m_slider.value += slideAmount;
				}
			}
		}

		public override void Idle()
		{
			m_lastStep = false;
		}
	}
}