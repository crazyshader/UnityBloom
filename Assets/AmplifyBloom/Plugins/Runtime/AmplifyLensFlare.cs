// Amplify Bloom - Advanced Bloom Post-Effect for Unity
// Copyright (c) Amplify Creations, Lda <info@amplify.pt>
using UnityEngine;

namespace AmplifyBloom
{
	[System.Serializable]
	public class AmplifyLensFlare : IAmplifyItem
	{
		//CONSTS
		private const int LUTTextureWidth = 256;

		//SERIALIZABLE VARIABLES
		[SerializeField]
		private float m_overallIntensity = 1f;

		[SerializeField]
		private float m_normalizedGhostIntensity = 0.8f;

		[SerializeField]
		private float m_normalizedHaloIntensity = 0.1f;

		[SerializeField]
		private bool m_applyLensFlare = true;

		[SerializeField]
		private int m_lensFlareGhostAmount = 3;

		[SerializeField]
		private Vector4 m_lensFlareGhostsParams = new Vector4( 0.8f, 0.228f, 1, 4 );// x - intensity y - Dispersal z - Power Factor w - Power Falloff

		[SerializeField]
		private float m_lensFlareGhostChrDistortion = 2;

		[SerializeField]
		private UnityEngine.Gradient m_lensGradient;

		[SerializeField]
		private Texture2D m_lensFlareGradTexture;

		private Color[] m_lensFlareGradColor = new Color[ LUTTextureWidth ];

		[SerializeField]
		private Vector4 m_lensFlareHaloParams = new Vector4( 0.1f, 0.573f, 1, 128 ); // x - Intensity y - Width z - Power Factor w - Power Falloff

		[SerializeField]
		private float m_lensFlareHaloChrDistortion = 1.51f;

		[SerializeField]
		private int m_lensFlareGaussianBlurAmount = 1;

		public AmplifyLensFlare()
		{
			m_lensGradient = new UnityEngine.Gradient();
		}

		public void Init()
		{
			if( m_lensGradient.alphaKeys.Length == 0 && m_lensGradient.colorKeys.Length == 0 )
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
				m_lensGradient.SetKeys( colorKeys, alphaKeys );
			}
		}

		public void Destroy()
		{
			if ( m_lensFlareGradTexture != null )
			{
				GameObject.DestroyImmediate( m_lensFlareGradTexture );
				m_lensFlareGradTexture = null;
			}
		}

		public void CreateLUTexture()
		{
			m_lensFlareGradTexture = new Texture2D( LUTTextureWidth, 1, TextureFormat.ARGB32, false );
			m_lensFlareGradTexture.filterMode = FilterMode.Bilinear;
			TextureFromGradient();
		}

		public RenderTexture ApplyFlare( Material material, RenderTexture source )
		{
			RenderTexture dest = AmplifyUtils.GetTempRenderTarget( source.width, source.height );
			material.SetVector( AmplifyUtils.LensFlareGhostsParamsId, m_lensFlareGhostsParams );
			material.SetTexture( AmplifyUtils.LensFlareLUTId, m_lensFlareGradTexture );
			material.SetVector( AmplifyUtils.LensFlareHaloParamsId, m_lensFlareHaloParams );
			material.SetFloat( AmplifyUtils.LensFlareGhostChrDistortionId, m_lensFlareGhostChrDistortion );
			material.SetFloat( AmplifyUtils.LensFlareHaloChrDistortionId, m_lensFlareHaloChrDistortion );
			Graphics.Blit( source, dest, material, ( int ) BloomPasses.LensFlare0 + m_lensFlareGhostAmount );
			return dest;
		}

		public void TextureFromGradient()
		{
			for ( int i = 0; i < LUTTextureWidth; i++ )
			{
				m_lensFlareGradColor[ i ] = m_lensGradient.Evaluate( ( float ) i / ( float ) ( LUTTextureWidth - 1 ) );
			}
			m_lensFlareGradTexture.SetPixels( m_lensFlareGradColor );
			m_lensFlareGradTexture.Apply();
		}

		public bool ApplyLensFlare
		{
			get { return m_applyLensFlare; }
			set { m_applyLensFlare = value; }
		}

		public float OverallIntensity
		{
			get { return m_overallIntensity; }
			set
			{
				m_overallIntensity = value < 0 ? 0 : value;
				m_lensFlareGhostsParams.x = value * m_normalizedGhostIntensity;
				m_lensFlareHaloParams.x = value * m_normalizedHaloIntensity;
			}
		}
		public int LensFlareGhostAmount
		{
			get { return m_lensFlareGhostAmount; }
			set { m_lensFlareGhostAmount = value; }
		}

		public Vector4 LensFlareGhostsParams
		{
			get { return m_lensFlareGhostsParams; }
			set { m_lensFlareGhostsParams = value; }
		}

		public float LensFlareNormalizedGhostsIntensity
		{
			get { return m_normalizedGhostIntensity; }
			set
			{
				m_normalizedGhostIntensity = value < 0 ? 0 : value;
				m_lensFlareGhostsParams.x = m_overallIntensity * m_normalizedGhostIntensity;
			}
		}

		public float LensFlareGhostsIntensity
		{
			get { return m_lensFlareGhostsParams.x; }
			set
			{
				m_lensFlareGhostsParams.x = value < 0 ? 0 : value;
			}
		}

		public float LensFlareGhostsDispersal
		{
			get { return m_lensFlareGhostsParams.y; }
			set { m_lensFlareGhostsParams.y = value; }
		}

		public float LensFlareGhostsPowerFactor
		{
			get { return m_lensFlareGhostsParams.z; }
			set { m_lensFlareGhostsParams.z = value; }
		}

		public float LensFlareGhostsPowerFalloff
		{
			get { return m_lensFlareGhostsParams.w; }
			set { m_lensFlareGhostsParams.w = value; }
		}

		public UnityEngine.Gradient LensFlareGradient
		{
			get { return m_lensGradient; }
			set { m_lensGradient = value; }
		}

		public Vector4 LensFlareHaloParams
		{
			get { return m_lensFlareHaloParams; }
			set { m_lensFlareHaloParams = value; }
		}

		public float LensFlareNormalizedHaloIntensity
		{
			get { return m_normalizedHaloIntensity; }
			set
			{
				m_normalizedHaloIntensity = value < 0 ? 0 : value;
				m_lensFlareHaloParams.x = m_overallIntensity * m_normalizedHaloIntensity;
			}
		}

		public float LensFlareHaloIntensity
		{
			get { return m_lensFlareHaloParams.x; }
			set
			{
				m_lensFlareHaloParams.x = value < 0 ? 0 : value;
			}
		}

		public float LensFlareHaloWidth
		{
			get { return m_lensFlareHaloParams.y; }
			set { m_lensFlareHaloParams.y = value; }
		}

		public float LensFlareHaloPowerFactor
		{
			get { return m_lensFlareHaloParams.z; }
			set { m_lensFlareHaloParams.z = value; }
		}

		public float LensFlareHaloPowerFalloff
		{
			get { return m_lensFlareHaloParams.w; }
			set { m_lensFlareHaloParams.w = value; }
		}

		public float LensFlareGhostChrDistortion
		{
			get { return m_lensFlareGhostChrDistortion; }
			set { m_lensFlareGhostChrDistortion = value; }
		}

		public float LensFlareHaloChrDistortion
		{
			get { return m_lensFlareHaloChrDistortion; }
			set { m_lensFlareHaloChrDistortion = value; }
		}

		public int LensFlareGaussianBlurAmount
		{
			get { return m_lensFlareGaussianBlurAmount; }
			set { m_lensFlareGaussianBlurAmount = value; }
		}
	}
}
