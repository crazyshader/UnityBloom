// Amplify Bloom - Advanced Bloom Post-Effect for Unity
// Copyright (c) Amplify Creations, Lda <info@amplify.pt>

using System;
using System.Collections.Generic;
using UnityEngine;

namespace AmplifyBloom
{
	public enum ApertureShape
	{
		Square,
		Hexagon,
		Octagon,
	}

	[Serializable]
	public class AmplifyBokehData
	{
		internal RenderTexture BokehRenderTexture;
		internal Vector4[] Offsets;

		public AmplifyBokehData( Vector4[] offsets )
		{
			Offsets = offsets;
		}

		public void Destroy()
		{
			if ( BokehRenderTexture != null )
			{
				AmplifyUtils.ReleaseTempRenderTarget( BokehRenderTexture );
				BokehRenderTexture = null;
			}

			Offsets = null;
		}
	}

	[Serializable]
	public sealed class AmplifyBokeh : IAmplifyItem, ISerializationCallbackReceiver
	{
		//CONSTS
		private const int PerPassSampleCount = 8;

		//SERIALIZABLE VARIABLES
		[SerializeField]
		private bool m_isActive = false;

		[SerializeField]
		private bool m_applyOnBloomSource = false;

		[SerializeField]
		private float m_bokehSampleRadius = 0.5f;

		[SerializeField]
		private Vector4 m_bokehCameraProperties = new Vector4( 0.05f, 0.018f, 1.34f, 0.18f ); // x - aperture y - Focal Length z - Focal Distance w - Max CoC Diameter

		[SerializeField]
		private float m_offsetRotation = 0;

		[SerializeField]
		private ApertureShape m_apertureShape = ApertureShape.Hexagon;

		private List<AmplifyBokehData> m_bokehOffsets;

		public AmplifyBokeh()
		{
			m_bokehOffsets = new List<AmplifyBokehData>();
			CreateBokehOffsets( ApertureShape.Hexagon );
		}

		public void Destroy()
		{
			for ( int i = 0; i < m_bokehOffsets.Count; i++ )
			{
				m_bokehOffsets[ i ].Destroy();
			}
		}

		void CreateBokehOffsets( ApertureShape shape )
		{
			m_bokehOffsets.Clear();
			switch ( shape )
			{
				case ApertureShape.Square:
				{
					m_bokehOffsets.Add( new AmplifyBokehData( CalculateBokehSamples( PerPassSampleCount, m_offsetRotation ) ) );
					m_bokehOffsets.Add( new AmplifyBokehData( CalculateBokehSamples( PerPassSampleCount, m_offsetRotation + 90 ) ) );
				}
				break;
				case ApertureShape.Hexagon:
				{

					m_bokehOffsets.Add( new AmplifyBokehData( CalculateBokehSamples( PerPassSampleCount, m_offsetRotation ) ) );
					m_bokehOffsets.Add( new AmplifyBokehData( CalculateBokehSamples( PerPassSampleCount, m_offsetRotation - 75 ) ) );
					m_bokehOffsets.Add( new AmplifyBokehData( CalculateBokehSamples( PerPassSampleCount, m_offsetRotation + 75 ) ) );
				}
				break;
				case ApertureShape.Octagon:
				{
					m_bokehOffsets.Add( new AmplifyBokehData( CalculateBokehSamples( PerPassSampleCount, m_offsetRotation ) ) );
					m_bokehOffsets.Add( new AmplifyBokehData( CalculateBokehSamples( PerPassSampleCount, m_offsetRotation + 65 ) ) );
					m_bokehOffsets.Add( new AmplifyBokehData( CalculateBokehSamples( PerPassSampleCount, m_offsetRotation + 90 ) ) );
					m_bokehOffsets.Add( new AmplifyBokehData( CalculateBokehSamples( PerPassSampleCount, m_offsetRotation + 115 ) ) );

				}
				break;
			}
		}

		Vector4[] CalculateBokehSamples( int sampleCount, float angle )
		{
			Vector4[] bokehSamples = new Vector4[ sampleCount ];
			float angleRad = Mathf.Deg2Rad * angle;
			float aspectRatio = ( float ) Screen.width / ( float ) Screen.height;
			Vector4 samplePoint = new Vector4( m_bokehSampleRadius * Mathf.Cos( angleRad ), m_bokehSampleRadius * Mathf.Sin( angleRad ) );
			samplePoint.x /= aspectRatio;
			for ( int i = 0; i < sampleCount; i++ )
			{
				float sampleInterp = ( float ) i / ( ( float ) sampleCount - 1.0f );
				bokehSamples[ i ] = Vector4.Lerp( -samplePoint, samplePoint, sampleInterp );
			}
			return bokehSamples;
		}

		public void ApplyBokehFilter( RenderTexture source, Material material )
		{
			// ALLOCATE RENDER TEXTURES
			for ( int i = 0; i < m_bokehOffsets.Count; i++ )
			{
				m_bokehOffsets[ i ].BokehRenderTexture = AmplifyUtils.GetTempRenderTarget( source.width, source.height );
			}

			// SET CAMERA PARAMS AND APPLY EACH ROTATIONAL WEIGHTS
			material.SetVector( AmplifyUtils.BokehParamsId, m_bokehCameraProperties );

			for ( int bId = 0; bId < m_bokehOffsets.Count; bId++ )
			{
				for ( int i = 0; i < PerPassSampleCount; i++ )
				{
					material.SetVector( AmplifyUtils.AnamorphicGlareWeightsStr[ i ], m_bokehOffsets[ bId ].Offsets[ i ] );
				}
				Graphics.Blit( source, m_bokehOffsets[ bId ].BokehRenderTexture, material, ( int ) BloomPasses.BokehWeightedBlur );
			}

			for ( int bId = 0; bId < m_bokehOffsets.Count - 1; bId++ )
			{
				material.SetTexture( AmplifyUtils.AnamorphicRTS[ bId ], m_bokehOffsets[ bId ].BokehRenderTexture );

			}

			// FINAL COMPOSITION
			source.DiscardContents();
			Graphics.Blit( m_bokehOffsets[ m_bokehOffsets.Count - 1 ].BokehRenderTexture, source, material, ( int ) BloomPasses.BokehComposition2S + ( m_bokehOffsets.Count - 2 ) );

			//RELEASE RENDER TEXTURES
			for ( int i = 0; i < m_bokehOffsets.Count; i++ )
			{
				AmplifyUtils.ReleaseTempRenderTarget( m_bokehOffsets[ i ].BokehRenderTexture );
				m_bokehOffsets[ i ].BokehRenderTexture = null;
			}
		}

		public void OnAfterDeserialize()
		{
			CreateBokehOffsets( m_apertureShape );
		}

		public void OnBeforeSerialize()
		{

		}

		public ApertureShape ApertureShape
		{
			get { return m_apertureShape; }
			set
			{
				if ( m_apertureShape != value )
				{
					m_apertureShape = value;
					CreateBokehOffsets( value );
				}
			}
		}

		public bool ApplyBokeh
		{
			get { return m_isActive; }
			set { m_isActive = value; }
		}


		public bool ApplyOnBloomSource
		{
			get { return m_applyOnBloomSource; }
			set { m_applyOnBloomSource = value; }
		}

		public float BokehSampleRadius
		{
			get { return m_bokehSampleRadius; }
			set { m_bokehSampleRadius = value; }
		}

		public float OffsetRotation
		{
			get { return m_offsetRotation; }
			set { m_offsetRotation = value; }
		}

		public Vector4 BokehCameraProperties
		{
			get { return m_bokehCameraProperties; }
			set { m_bokehCameraProperties = value; }
		}

		public float Aperture
		{
			get { return m_bokehCameraProperties.x; }
			set { m_bokehCameraProperties.x = value; }
		}

		public float FocalLength
		{
			get { return m_bokehCameraProperties.y; }
			set { m_bokehCameraProperties.y = value; }
		}

		public float FocalDistance
		{
			get { return m_bokehCameraProperties.z; }
			set { m_bokehCameraProperties.z = value; }
		}

		public float MaxCoCDiameter
		{
			get { return m_bokehCameraProperties.w; }
			set { m_bokehCameraProperties.w = value; }
		}
	}
}
