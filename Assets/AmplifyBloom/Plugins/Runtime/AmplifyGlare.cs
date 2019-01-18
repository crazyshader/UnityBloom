// Amplify Bloom - Advanced Bloom Post-Effect for Unity
// Copyright (c) Amplify Creations, Lda <info@amplify.pt>

using UnityEngine;
using System;
namespace AmplifyBloom
{


	// Glare form library
	public enum GlareLibType
	{
		CheapLens = 0,
		CrossScreen,
		CrossScreenSpectral,
		SnowCross,
		SnowCrossSpectral,
		SunnyCross,
		SunnyCrossSpectral,
		VerticalSlits,
		HorizontalSlits,
		Custom
	};

	[Serializable]
	public class GlareDefData
	{
		public bool FoldoutValue = true;
		[SerializeField]
		private StarLibType m_starType = StarLibType.Cross;
		[SerializeField]
		private float m_starInclination = 0;
		[SerializeField]
		private float m_chromaticAberration = 0;
		[SerializeField]
		private StarDefData m_customStarData = null;

		public GlareDefData()
		{
			m_customStarData = new StarDefData();
		}

		public GlareDefData( StarLibType starType, float starInclination, float chromaticAberration )
		{
			m_starType = starType;
			m_starInclination = starInclination;
			m_chromaticAberration = chromaticAberration;
		}

		public StarLibType StarType
		{
			get
			{
				return m_starType;
			}
			set
			{
				m_starType = value;
			}
		}

		public float StarInclination
		{
			get
			{
				return m_starInclination;
			}
			set
			{
				m_starInclination = value;
			}
		}

		public float StarInclinationDeg
		{
			get
			{
				return m_starInclination * Mathf.Rad2Deg;
			}
			set
			{
				m_starInclination = value * Mathf.Deg2Rad;
			}
		}

		public float ChromaticAberration
		{
			get
			{
				return m_chromaticAberration;
			}
			set
			{
				m_chromaticAberration = value;
			}
		}

		public StarDefData CustomStarData
		{
			get
			{
				return m_customStarData;
			}
			set
			{
				m_customStarData = value;
			}
		}
	}

	[Serializable]
	public sealed class AmplifyGlare : IAmplifyItem
	{
		public const int MaxLineSamples = 8;
		public const int MaxTotalSamples = 16;
		public const int MaxStarLines = 4;
		public const int MaxPasses = 4;
		public const int MaxCustomGlare = 32;

		[SerializeField]
		private GlareDefData[] m_customGlareDef;

		[SerializeField]
		private int m_customGlareDefIdx = 0;

		[SerializeField]
		private int m_customGlareDefAmount = 0;

		[SerializeField]
		private bool m_applyGlare = true;

		[SerializeField]
		private Color _overallTint = Color.white;

		[SerializeField]
		private UnityEngine.Gradient m_cromaticAberrationGrad;

		[SerializeField]
		private int m_glareMaxPassCount = MaxPasses;

		private StarDefData[] m_starDefArr;
		private GlareDefData[] m_glareDefArr;

		private Matrix4x4[] m_weigthsMat;
		private Matrix4x4[] m_offsetsMat;

		private Color m_whiteReference;

		private float m_aTanFoV;

		private AmplifyGlareCache m_amplifyGlareCache;

		[SerializeField]
		private int m_currentWidth;

		[SerializeField]
		private int m_currentHeight;

		[SerializeField]
		private GlareLibType m_currentGlareType = GlareLibType.CheapLens;

		[SerializeField]
		private int m_currentGlareIdx;

		[SerializeField]
		private float m_perPassDisplacement = 4;

		[SerializeField]
		private float m_intensity = 0.17f;

		[SerializeField]
		private float m_overallStreakScale = 1f;

		private bool m_isDirty = true;
		private RenderTexture[] _rtBuffer;

		public AmplifyGlare()
		{
			m_currentGlareIdx = ( int ) m_currentGlareType;

			m_cromaticAberrationGrad = new UnityEngine.Gradient();




			_rtBuffer = new RenderTexture[ MaxStarLines * MaxPasses ];

			m_weigthsMat = new Matrix4x4[ 4 ];
			m_offsetsMat = new Matrix4x4[ 4 ];

			m_amplifyGlareCache = new AmplifyGlareCache();

			m_whiteReference = new Color( 0.63f, 0.63f, 0.63f, 0.0f );
			m_aTanFoV = Mathf.Atan( Mathf.PI / MaxLineSamples );


			m_starDefArr = new StarDefData[] {  new StarDefData(StarLibType.Cross,       "Cross",        2,      4,      1.0f,   0.85f,  0.0f,   0.5f,   -1.0f,      90.0f),
												new StarDefData(StarLibType.Cross_Filter,"CrossFilter",  2,      4,      1.0f,   0.95f,  0.0f,   0.5f,   -1.0f,      90.0f),
												new StarDefData(StarLibType.Snow_Cross,  "snowCross",    3,      4,      1.0f,   0.96f,  0.349f, 0.5f,   -1.0f,      -1),
												new StarDefData(StarLibType.Vertical,    "Vertical",     1,      4,      1.0f,   0.96f,  0.0f,   0.0f,   -1.0f,      -1),
												new StarDefData(StarLibType.Sunny_Cross, "SunnyCross",   4,      4,      1.0f,   0.88f,  0.0f,   0.0f,   0.95f,      45.0f)
			};

			m_glareDefArr = new GlareDefData[] {    new GlareDefData( StarLibType.Cross,        0.00f,  0.5f),//Cheap Lens
													new GlareDefData( StarLibType.Cross_Filter, 0.44f,  0.5f),//Cross Screen
													new GlareDefData( StarLibType.Cross_Filter, 1.22f,  1.5f),//Cross Screen Spectral
													new GlareDefData( StarLibType.Snow_Cross,   0.17f,  0.5f),//Snow Cross
													new GlareDefData( StarLibType.Snow_Cross,   0.70f,  1.5f),//Snow Cross Spectral
													new GlareDefData( StarLibType.Sunny_Cross,  0.00f,  0.5f),//Sunny Cross
													new GlareDefData( StarLibType.Sunny_Cross,  0.79f,  1.5f),//Sunny Cross Spectral
													new GlareDefData( StarLibType.Vertical,     1.57f,  0.5f),//Vertical Slits
													new GlareDefData( StarLibType.Vertical,     0.00f,  0.5f) //Horizontal slits
			};
		}

		public void Init()
		{
			if( m_cromaticAberrationGrad.alphaKeys.Length == 0 && m_cromaticAberrationGrad.colorKeys.Length == 0 )
			{
				UnityEngine.GradientColorKey[] colorKeys = new UnityEngine.GradientColorKey[] { new UnityEngine.GradientColorKey(Color.white,0f),
																							new UnityEngine.GradientColorKey(Color.blue,0.25f),
																							new UnityEngine.GradientColorKey(Color.green,0.5f),
																							new UnityEngine.GradientColorKey(Color.yellow,0.75f),
																							new UnityEngine.GradientColorKey(Color.red,1f)
																};
				UnityEngine.GradientAlphaKey[] alphaKeys = new UnityEngine.GradientAlphaKey[] { new UnityEngine.GradientAlphaKey(1f,0f),
																							new UnityEngine.GradientAlphaKey(1f,0.25f),
																							new UnityEngine.GradientAlphaKey(1f,0.5f),
																							new UnityEngine.GradientAlphaKey(1f,0.75f),
																							new UnityEngine.GradientAlphaKey(1f,1f)
																};
				m_cromaticAberrationGrad.SetKeys( colorKeys, alphaKeys );
			}
		}

		public void Destroy()
		{
			for ( int i = 0; i < m_starDefArr.Length; i++ )
			{
				m_starDefArr[ i ].Destroy();
			}

			m_glareDefArr = null;
			m_weigthsMat = null;
			m_offsetsMat = null;

			for ( int i = 0; i < _rtBuffer.Length; i++ )
			{
				if ( _rtBuffer[ i ] != null )
				{
					AmplifyUtils.ReleaseTempRenderTarget( _rtBuffer[ i ] );
					_rtBuffer[ i ] = null;
				}
			}
			_rtBuffer = null;

			m_amplifyGlareCache.Destroy();
			m_amplifyGlareCache = null;
		}

		public void SetDirty()
		{
			m_isDirty = true;
		}

		public void OnRenderFromCache( RenderTexture source, RenderTexture dest, Material material, float glareIntensity, float cameraRotation )
		{
			// ALLOCATE RENDER TEXTURES
			for ( int i = 0; i < m_amplifyGlareCache.TotalRT; i++ )
			{
				_rtBuffer[ i ] = AmplifyUtils.GetTempRenderTarget( source.width, source.height );
			}

			int rtIdx = 0;
			for ( int d = 0; d < m_amplifyGlareCache.StarDef.StarlinesCount; d++ )
			{
				for ( int p = 0; p < m_amplifyGlareCache.CurrentPassCount; p++ )
				{
					// APPLY SHADER
					UpdateMatrixesForPass( material, m_amplifyGlareCache.Starlines[ d ].Passes[ p ].Offsets, m_amplifyGlareCache.Starlines[ d ].Passes[ p ].Weights, glareIntensity, cameraRotation * m_amplifyGlareCache.StarDef.CameraRotInfluence );

					//CREATED WEIGHTED TEXTURE
					if ( p == 0 )
					{
						Graphics.Blit( source, _rtBuffer[ rtIdx ], material, ( int ) BloomPasses.AnamorphicGlare );
					}
					else
					{
						Graphics.Blit( _rtBuffer[ rtIdx - 1 ], _rtBuffer[ rtIdx ], material, ( int ) BloomPasses.AnamorphicGlare );
					}
					rtIdx += 1;
				}
			}

			//ADD TO MAIN RT
			for ( int i = 0; i < m_amplifyGlareCache.StarDef.StarlinesCount; i++ )
			{
				material.SetVector( AmplifyUtils.AnamorphicGlareWeightsStr[ i ], m_amplifyGlareCache.AverageWeight );
				int idx = ( i + 1 ) * m_amplifyGlareCache.CurrentPassCount - 1;
				material.SetTexture( AmplifyUtils.AnamorphicRTS[ i ], _rtBuffer[ idx ] );
			}


			int passId = ( int ) BloomPasses.WeightedAddPS1 + m_amplifyGlareCache.StarDef.StarlinesCount - 1;
			dest.DiscardContents();
			Graphics.Blit( _rtBuffer[ 0 ], dest, material, passId );

			//RELEASE RT's
			for ( rtIdx = 0; rtIdx < _rtBuffer.Length; rtIdx++ )
			{
				AmplifyUtils.ReleaseTempRenderTarget( _rtBuffer[ rtIdx ] );
				_rtBuffer[ rtIdx ] = null;
			}
		}

		public void UpdateMatrixesForPass( Material material, Vector4[] offsets, Vector4[] weights, float glareIntensity, float rotation )
		{
			float cosRot = Mathf.Cos( rotation );
			float sinRot = Mathf.Sin( rotation );

			for ( int i = 0; i < MaxTotalSamples; i++ )
			{
				int matIdx = i >> 2;
				int vecIdx = i & 3;
				m_offsetsMat[ matIdx ][ vecIdx, 0 ] = offsets[ i ].x * cosRot - offsets[ i ].y * sinRot;
				m_offsetsMat[ matIdx ][ vecIdx, 1 ] = offsets[ i ].x * sinRot + offsets[ i ].y * cosRot;

				m_weigthsMat[ matIdx ][ vecIdx, 0 ] = glareIntensity * weights[ i ].x;
				m_weigthsMat[ matIdx ][ vecIdx, 1 ] = glareIntensity * weights[ i ].y;
				m_weigthsMat[ matIdx ][ vecIdx, 2 ] = glareIntensity * weights[ i ].z;
			}

			for ( int i = 0; i < 4; i++ )
			{
				material.SetMatrix( AmplifyUtils.AnamorphicGlareOffsetsMatStr[ i ], m_offsetsMat[ i ] );
				material.SetMatrix( AmplifyUtils.AnamorphicGlareWeightsMatStr[ i ], m_weigthsMat[ i ] );
			}
		}

		public void OnRenderImage( Material material, RenderTexture source, RenderTexture dest, float cameraRot )
		{
			//NEED TO SET DESTINATION RENDER TARGET TO COMPLETELLY BLACK SO WE CAN SUM ALL THE GLARE/STAR PASSES ON IT
			Graphics.Blit( Texture2D.blackTexture, dest );

			if ( m_isDirty ||
				m_currentWidth != source.width ||
				m_currentHeight != source.height )
			{
				m_isDirty = false;
				m_currentWidth = source.width;
				m_currentHeight = source.height;
			}
			else
			{
				OnRenderFromCache( source, dest, material, m_intensity, cameraRot );
				return;
			}

			GlareDefData glareDef = null;
			bool validCustom = false;
			if ( m_currentGlareType == GlareLibType.Custom )
			{
				if ( m_customGlareDef != null && m_customGlareDef.Length > 0 )
				{
					glareDef = m_customGlareDef[ m_customGlareDefIdx ];
					validCustom = true;
				}
				else
				{
					glareDef = m_glareDefArr[ 0 ];
				}
			}
			else
			{
				glareDef = m_glareDefArr[ m_currentGlareIdx ];
			}


			m_amplifyGlareCache.GlareDef = glareDef;

			float srcW = source.width;
			float srcH = source.height;

			StarDefData starDef = ( validCustom ) ? glareDef.CustomStarData : m_starDefArr[ ( int ) glareDef.StarType ];


			m_amplifyGlareCache.StarDef = starDef;
			int currPassCount = ( m_glareMaxPassCount < starDef.PassCount ) ? m_glareMaxPassCount : starDef.PassCount;
			m_amplifyGlareCache.CurrentPassCount = currPassCount;
			float radOffset = glareDef.StarInclination + starDef.Inclination;

			for ( int p = 0; p < m_glareMaxPassCount; p++ )
			{
				float ratio = ( float ) ( p + 1 ) / ( float ) m_glareMaxPassCount;

				for ( int s = 0; s < MaxLineSamples; s++ )
				{
					Color chromaticAberrColor = _overallTint * Color.Lerp( m_cromaticAberrationGrad.Evaluate( ( float ) s / ( float ) ( MaxLineSamples - 1 ) ), m_whiteReference, ratio );
					m_amplifyGlareCache.CromaticAberrationMat[ p, s ] = Color.Lerp( m_whiteReference, chromaticAberrColor, glareDef.ChromaticAberration );
				}
			}
			m_amplifyGlareCache.TotalRT = starDef.StarlinesCount * currPassCount;

			for ( int i = 0; i < m_amplifyGlareCache.TotalRT; i++ )
			{
				_rtBuffer[ i ] = AmplifyUtils.GetTempRenderTarget( source.width, source.height );
			}

			int rtIdx = 0;
			for ( int d = 0; d < starDef.StarlinesCount; d++ )
			{
				StarLineData starLine = starDef.StarLinesArr[ d ];
				float angle = radOffset + starLine.Inclination;
				float sinAngle = Mathf.Sin( angle );
				float cosAngle = Mathf.Cos( angle );
				Vector2 vtStepUV = new Vector2();
				vtStepUV.x = cosAngle / srcW * ( starLine.SampleLength * m_overallStreakScale );
				vtStepUV.y = sinAngle / srcH * ( starLine.SampleLength * m_overallStreakScale );

				float attnPowScale = ( m_aTanFoV + 0.1f ) * ( 280.0f ) / ( srcW + srcH ) * 1.2f;

				for ( int p = 0; p < currPassCount; p++ )
				{
					for ( int i = 0; i < MaxLineSamples; i++ )
					{
						float lum = Mathf.Pow( starLine.Attenuation, attnPowScale * i );

						m_amplifyGlareCache.Starlines[ d ].Passes[ p ].Weights[ i ] = m_amplifyGlareCache.CromaticAberrationMat[ currPassCount - 1 - p, i ] * lum * ( p + 1.0f ) * 0.5f;

						// OFFSET OF SAMPLING COORDINATE
						m_amplifyGlareCache.Starlines[ d ].Passes[ p ].Offsets[ i ].x = vtStepUV.x * i;
						m_amplifyGlareCache.Starlines[ d ].Passes[ p ].Offsets[ i ].y = vtStepUV.y * i;
						if ( Mathf.Abs( m_amplifyGlareCache.Starlines[ d ].Passes[ p ].Offsets[ i ].x ) >= 0.9f ||
							Mathf.Abs( m_amplifyGlareCache.Starlines[ d ].Passes[ p ].Offsets[ i ].y ) >= 0.9f )
						{
							m_amplifyGlareCache.Starlines[ d ].Passes[ p ].Offsets[ i ].x = 0.0f;
							m_amplifyGlareCache.Starlines[ d ].Passes[ p ].Offsets[ i ].y = 0.0f;
							m_amplifyGlareCache.Starlines[ d ].Passes[ p ].Weights[ i ] *= 0.0f;
						}
					}

					// MIRROR STARLINE
					for ( int i = MaxLineSamples; i < MaxTotalSamples; i++ )
					{
						m_amplifyGlareCache.Starlines[ d ].Passes[ p ].Offsets[ i ] = -m_amplifyGlareCache.Starlines[ d ].Passes[ p ].Offsets[ i - MaxLineSamples ];
						m_amplifyGlareCache.Starlines[ d ].Passes[ p ].Weights[ i ] = m_amplifyGlareCache.Starlines[ d ].Passes[ p ].Weights[ i - MaxLineSamples ];
					}

					// APPLY SHADER
					UpdateMatrixesForPass( material, m_amplifyGlareCache.Starlines[ d ].Passes[ p ].Offsets, m_amplifyGlareCache.Starlines[ d ].Passes[ p ].Weights, m_intensity, starDef.CameraRotInfluence * cameraRot );

					//CREATED WEIGHTED TEXTURE
					if ( p == 0 )
					{
						Graphics.Blit( source, _rtBuffer[ rtIdx ], material, ( int ) BloomPasses.AnamorphicGlare );
					}
					else
					{
						Graphics.Blit( _rtBuffer[ rtIdx - 1 ], _rtBuffer[ rtIdx ], material, ( int ) BloomPasses.AnamorphicGlare );
					}

					rtIdx += 1;
					vtStepUV *= m_perPassDisplacement;
					attnPowScale *= m_perPassDisplacement;
				}
			}

			//ADD TO MAIN RT
			m_amplifyGlareCache.AverageWeight = Vector4.one / starDef.StarlinesCount;
			for ( int i = 0; i < starDef.StarlinesCount; i++ )
			{
				material.SetVector( AmplifyUtils.AnamorphicGlareWeightsStr[ i ], m_amplifyGlareCache.AverageWeight );
				int idx = ( i + 1 ) * currPassCount - 1;
				material.SetTexture( AmplifyUtils.AnamorphicRTS[ i ], _rtBuffer[ idx ] );
			}

			int passId = ( int ) BloomPasses.WeightedAddPS1 + starDef.StarlinesCount - 1;
			dest.DiscardContents();
			Graphics.Blit( _rtBuffer[ 0 ], dest, material, passId );

			//RELEASE RT's
			for ( rtIdx = 0; rtIdx < _rtBuffer.Length; rtIdx++ )
			{
				AmplifyUtils.ReleaseTempRenderTarget( _rtBuffer[ rtIdx ] );
				_rtBuffer[ rtIdx ] = null;
			}
		}

		public GlareLibType CurrentGlare
		{
			get { return m_currentGlareType; }
			set
			{
				if ( m_currentGlareType != value )
				{
					m_currentGlareType = value;
					m_currentGlareIdx = ( int ) value;
					m_isDirty = true;
				}
			}
		}

		public int GlareMaxPassCount
		{
			get { return m_glareMaxPassCount; }
			set
			{
				m_glareMaxPassCount = value;
				m_isDirty = true;
			}
		}

		public float PerPassDisplacement
		{
			get { return m_perPassDisplacement; }
			set
			{
				m_perPassDisplacement = value;
				m_isDirty = true;
			}
		}

		public float Intensity
		{
			get { return m_intensity; }
			set
			{
				m_intensity = value < 0 ? 0 : value;
				m_isDirty = true;
			}
		}

		public Color OverallTint
		{
			get { return _overallTint; }
			set
			{
				_overallTint = value;
				m_isDirty = true;
			}
		}

		public bool ApplyLensGlare
		{
			get { return m_applyGlare; }
			set { m_applyGlare = value; }
		}

		public UnityEngine.Gradient CromaticColorGradient
		{
			get { return m_cromaticAberrationGrad; }
			set
			{
				m_cromaticAberrationGrad = value;
				m_isDirty = true;
			}
		}

		public float OverallStreakScale
		{
			get { return m_overallStreakScale; }
			set
			{
				m_overallStreakScale = value;
				m_isDirty = true;
			}
		}

		public GlareDefData[] CustomGlareDef
		{
			get
			{
				return m_customGlareDef;
			}
			set
			{
				m_customGlareDef = value;
			}
		}

		public int CustomGlareDefIdx
		{
			get
			{
				return m_customGlareDefIdx;
			}

			set
			{
				m_customGlareDefIdx = value;
			}
		}

		public int CustomGlareDefAmount
		{
			get
			{
				return m_customGlareDefAmount;
			}

			set
			{
				if ( value == m_customGlareDefAmount )
				{
					return;
				}

				if ( value == 0 )
				{
					m_customGlareDef = null;
					m_customGlareDefIdx = 0;
					m_customGlareDefAmount = 0;
				}
				else
				{

					GlareDefData[] newArr = new GlareDefData[ value ];
					for ( int i = 0; i < value; i++ )
					{
						if ( i < m_customGlareDefAmount )
						{
							newArr[ i ] = m_customGlareDef[ i ];
						}
						else
						{
							newArr[ i ] = new GlareDefData();
						}
					}
					m_customGlareDefIdx = Mathf.Clamp( m_customGlareDefIdx, 0, value - 1 );
					m_customGlareDef = newArr;
					newArr = null;
					m_customGlareDefAmount = value;
				}
			}
		}
	}
}
