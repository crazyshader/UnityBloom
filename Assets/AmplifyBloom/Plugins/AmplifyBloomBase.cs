// Amplify Bloom - Advanced Bloom Post-Effect for Unity
// Copyright (c) Amplify Creations, Lda <info@amplify.pt>

using UnityEngine;
using UnityEngine.Rendering;

namespace AmplifyBloom
{
	public enum PrecisionModes
	{
		Low = 0,
		High
	}

	public enum BloomPasses
	{
		Threshold = 0,
		ThresholdMask = 1,
		AnamorphicGlare = 2,
		LensFlare0 = 3,
		LensFlare1 = 4,
		LensFlare2 = 5,
		LensFlare3 = 6,
		LensFlare4 = 7,
		LensFlare5 = 8,
		DownsampleNoWeightedAvg = 9,
		DownsampleWithKaris = 10,
		DownsampleWithoutKaris = 11,
		DownsampleWithTempFilterWithKaris = 12,
		DownsampleWithTempFilterWithoutKaris = 13,
		HorizontalBlur = 14,
		VerticalBlur = 15,
		VerticalBlurWithTempFilter = 16,
		UpscaleFirstPass = 17,
		Upscale = 18,
		WeightedAddPS1 = 19,
		WeightedAddPS2 = 20,
		WeightedAddPS3 = 21,
		WeightedAddPS4 = 22,
		WeightedAddPS5 = 23,
		WeightedAddPS6 = 24,
		WeightedAddPS7 = 25,
		WeightedAddPS8 = 26,
		BokehWeightedBlur = 27,
		BokehComposition2S = 28,
		BokehComposition3S = 29,
		BokehComposition4S = 30,
		BokehComposition5S = 31,
		BokehComposition6S = 32,
		Decode = 33,
		TotalPasses
	};

	public enum UpscaleQualityEnum
	{
		Realistic,
		Natural
	}

	public enum DebugToScreenEnum
	{
		None,
		Bloom,
		MainThreshold,
		FeaturesThreshold,
		TemporalFilter,
		BokehFilter,
		LensFlare,
		LensGlare,
		LensDirt,
		LensStarburst
	}

	public enum MainThresholdSizeEnum
	{
		Full = 0,
		Half,
		Quarter

	}

	[System.Serializable]
	[AddComponentMenu( "" )]
	public class AmplifyBloomBase : MonoBehaviour
	{
		//CONSTS
		public const int MaxGhosts = 5;
		public const int MinDownscales = 1;
		public const int MaxDownscales = 6;
		public const int MaxGaussian = 8;
		private const float MaxDirtIntensity = 1;
		private const float MaxStarburstIntensity = 1;

		// SERIALIZABLE VARIABLES

		[SerializeField]
		private Texture m_maskTexture = null;
		[SerializeField]
		private RenderTexture m_targetTexture = null;

		[SerializeField]
		private bool m_showDebugMessages = true;

		[SerializeField]
		private int m_softMaxdownscales = MaxDownscales;

		[SerializeField]
		private DebugToScreenEnum m_debugToScreen = DebugToScreenEnum.None;

		[SerializeField]
		private bool m_highPrecision = false;

		[SerializeField]
		private Vector4 m_bloomRange = new Vector4( 500, 1, 0, 0 );

		[SerializeField]
		private float m_overallThreshold = 0.53f;

		[SerializeField]
		private Vector4 m_bloomParams = new Vector4( 0.8f, 1, 1, 1 ); // x - overallIntensity, y - threshold, z - blur radius w - bloom scale

		[SerializeField]
		private bool m_temporalFilteringActive = false;

		[SerializeField]
		private float m_temporalFilteringValue = 0.05f;

		[SerializeField]
		private int m_bloomDownsampleCount = 6;

		[SerializeField]
		private AnimationCurve m_temporalFilteringCurve;

		[SerializeField]
		private bool m_separateFeaturesThreshold = false;

		[SerializeField]
		private float m_featuresThreshold = 0.05f;

		[SerializeField]
		private AmplifyLensFlare m_lensFlare = new AmplifyLensFlare();

		[SerializeField]
		private bool m_applyLensDirt = true;

		[SerializeField]
		private float m_lensDirtStrength = 2f;

		[SerializeField]
		private Texture m_lensDirtTexture;

		[SerializeField]
		private bool m_applyLensStardurst = true;

		[SerializeField]
		private Texture m_lensStardurstTex;

		[SerializeField]
		private float m_lensStarburstStrength = 2f;

		[SerializeField]
		private AmplifyGlare m_anamorphicGlare = new AmplifyGlare();

		[SerializeField]
		private AmplifyBokeh m_bokehFilter = new AmplifyBokeh();

		[SerializeField]
		private float[] m_upscaleWeights = new float[ MaxDownscales ] { 0.0842f, 0.1282f, 0.1648f, 0.2197f, 0.2197f, 0.1831f };

		[SerializeField]
		private float[] m_gaussianRadius = new float[ MaxDownscales ] { 1, 1, 1, 1, 1, 1 };

		[SerializeField]
		private int[] m_gaussianSteps = new int[ MaxDownscales ] { 1, 1, 1, 1, 1, 1 };

		[SerializeField]
		private float[] m_lensDirtWeights = new float[ MaxDownscales ] { 0.0670f, 0.1020f, 0.1311f, 0.1749f, 0.2332f, 0.3f };

		[SerializeField]
		private float[] m_lensStarburstWeights = new float[ MaxDownscales ] { 0.0670f, 0.1020f, 0.1311f, 0.1749f, 0.2332f, 0.3f };

		[SerializeField]
		private bool[] m_downscaleSettingsFoldout = new bool[ MaxDownscales ] { false, false, false, false, false, false };

		[SerializeField]
		private int m_featuresSourceId = 0;

		[SerializeField]
		private UpscaleQualityEnum m_upscaleQuality = UpscaleQualityEnum.Realistic;

		[SerializeField]
		private MainThresholdSizeEnum m_mainThresholdSize = MainThresholdSizeEnum.Full;

		// Internal private variables
		private Transform m_cameraTransform;
		private Matrix4x4 m_starburstMat;

		private Shader m_bloomShader;
		private Material m_bloomMaterial;

		private Shader m_finalCompositionShader;
		private Material m_finalCompositionMaterial;

		private RenderTexture m_tempFilterBuffer;
		private Camera m_camera;
		RenderTexture[] m_tempUpscaleRTs = new RenderTexture[ MaxDownscales ];
		RenderTexture[] m_tempAuxDownsampleRTs = new RenderTexture[ MaxDownscales ];
		Vector2[] m_tempDownsamplesSizes = new Vector2[ MaxDownscales ];

		private bool silentError = false;

#if TRIAL
		private Texture2D watermark = null;
#endif

		void Awake()
		{
			bool nullDev = ( SystemInfo.graphicsDeviceType == GraphicsDeviceType.Null );
			if ( nullDev )
			{

				AmplifyUtils.DebugLog( "Null graphics device detected. Skipping effect silently.", LogType.Error );
				silentError = true;
				return;
			}

			if ( !AmplifyUtils.IsInitialized )
				AmplifyUtils.InitializeIds();

			m_anamorphicGlare.Init();
			m_lensFlare.Init();

			for ( int i = 0; i < MaxDownscales; i++ )
			{
				m_tempDownsamplesSizes[ i ] = new Vector2( 0, 0 );
			}
			m_cameraTransform = transform;
			m_tempFilterBuffer = null;
			m_starburstMat = Matrix4x4.identity;

			if ( m_temporalFilteringCurve == null )
				m_temporalFilteringCurve = new AnimationCurve( new Keyframe( 0, 0 ), new Keyframe( 1, 0.999f ) );

			m_bloomShader = Shader.Find( "Hidden/AmplifyBloom" );
			if ( m_bloomShader != null )
			{
				m_bloomMaterial = new Material( m_bloomShader );
				m_bloomMaterial.hideFlags = HideFlags.DontSave;
			}
			else
			{
				AmplifyUtils.DebugLog( "Main Bloom shader not found", LogType.Error );
				gameObject.SetActive( false );
			}

			m_finalCompositionShader = Shader.Find( "Hidden/BloomFinal" );
			if ( m_finalCompositionShader != null )
			{
				m_finalCompositionMaterial = new Material( m_finalCompositionShader );
				if ( !m_finalCompositionMaterial.GetTag( AmplifyUtils.ShaderModeTag, false ).Equals( AmplifyUtils.ShaderModeValue ) )
				{
					if ( m_showDebugMessages )
						AmplifyUtils.DebugLog( "Amplify Bloom is running on a limited hardware and may lead to a decrease on its visual quality.", LogType.Warning );
				}
				else
				{
					m_softMaxdownscales = MaxDownscales;
				}

				m_finalCompositionMaterial.hideFlags = HideFlags.DontSave;
				if ( m_lensDirtTexture == null )
				{
					m_lensDirtTexture = m_finalCompositionMaterial.GetTexture( AmplifyUtils.LensDirtRTId );
				}

				if ( m_lensStardurstTex == null )
				{
					m_lensStardurstTex = m_finalCompositionMaterial.GetTexture( AmplifyUtils.LensStarburstRTId );
				}
			}
			else
			{
				AmplifyUtils.DebugLog( "Bloom Composition shader not found", LogType.Error );
				gameObject.SetActive( false );
			}

			m_camera = GetComponent<Camera>();
			m_camera.depthTextureMode |= DepthTextureMode.Depth;
			m_lensFlare.CreateLUTexture();

#if TRIAL
			watermark = new Texture2D( 4, 4 ) { hideFlags = HideFlags.HideAndDontSave };
			watermark.LoadImage( AmplifyBloom.Watermark.ImageData );
#endif
		}

		void OnDestroy()
		{
			if ( m_bokehFilter != null )
			{
				m_bokehFilter.Destroy();
				m_bokehFilter = null;
			}

			if ( m_anamorphicGlare != null )
			{
				m_anamorphicGlare.Destroy();
				m_anamorphicGlare = null;
			}

			if ( m_lensFlare != null )
			{
				m_lensFlare.Destroy();
				m_lensFlare = null;
			}

#if TRIAL
			if ( watermark != null )
			{
				DestroyImmediate( watermark );
				watermark = null;
			}
#endif
		}

		void ApplyGaussianBlur( RenderTexture renderTexture, int amount, float radius = 1.0f, bool applyTemporal = false )
		{
			if ( amount == 0 )
				return;

			m_bloomMaterial.SetFloat( AmplifyUtils.BlurRadiusId, radius );
			RenderTexture blurRT = AmplifyUtils.GetTempRenderTarget( renderTexture.width, renderTexture.height );
			for ( int i = 0; i < amount; i++ )
			{
				blurRT.DiscardContents();
				Graphics.Blit( renderTexture, blurRT, m_bloomMaterial, ( int ) BloomPasses.HorizontalBlur );

				if ( m_temporalFilteringActive && applyTemporal && i == ( amount - 1 ) )
				{
					if ( m_tempFilterBuffer != null && m_temporalFilteringActive )
					{
						float filterVal = m_temporalFilteringCurve.Evaluate( m_temporalFilteringValue );
						m_bloomMaterial.SetFloat( AmplifyUtils.TempFilterValueId, filterVal );
						m_bloomMaterial.SetTexture( AmplifyUtils.AnamorphicRTS[ 0 ], m_tempFilterBuffer );
						renderTexture.DiscardContents();
						Graphics.Blit( blurRT, renderTexture, m_bloomMaterial, ( int ) BloomPasses.VerticalBlurWithTempFilter );
					}
					else
					{
						renderTexture.DiscardContents();
						Graphics.Blit( blurRT, renderTexture, m_bloomMaterial, ( int ) BloomPasses.VerticalBlur );
					}

					bool createRT = false;
					if ( m_tempFilterBuffer != null )
					{
						if ( m_tempFilterBuffer.format != renderTexture.format ||
								m_tempFilterBuffer.width != renderTexture.width ||
								m_tempFilterBuffer.height != renderTexture.height )
						{
							CleanTempFilterRT();
							createRT = true;
						}
					}
					else
					{
						createRT = true;
					}

					if ( createRT )
					{
						CreateTempFilterRT( renderTexture );
					}
					m_tempFilterBuffer.DiscardContents();
					Graphics.Blit( renderTexture, m_tempFilterBuffer );
				}
				else
				{
					renderTexture.DiscardContents();
					Graphics.Blit( blurRT, renderTexture, m_bloomMaterial, ( int ) BloomPasses.VerticalBlur );
				}
			}
			AmplifyUtils.ReleaseTempRenderTarget( blurRT );
		}

		void CreateTempFilterRT( RenderTexture source )
		{
			if ( m_tempFilterBuffer != null )
			{
				CleanTempFilterRT();
			}

			m_tempFilterBuffer = new RenderTexture( source.width, source.height, 0, source.format, AmplifyUtils.CurrentReadWriteMode );
			m_tempFilterBuffer.filterMode = AmplifyUtils.CurrentFilterMode;
			m_tempFilterBuffer.wrapMode = AmplifyUtils.CurrentWrapMode;
			m_tempFilterBuffer.Create();
		}

		void CleanTempFilterRT()
		{
			if ( m_tempFilterBuffer != null )
			{
				RenderTexture.active = null;
				m_tempFilterBuffer.Release();
				DestroyImmediate( m_tempFilterBuffer );
				m_tempFilterBuffer = null;
			}
		}

		void OnRenderImage( RenderTexture src, RenderTexture dest )
		{
			if ( silentError )
				return;

			if ( !AmplifyUtils.IsInitialized )
				AmplifyUtils.InitializeIds();

			if ( m_highPrecision )
			{
				AmplifyUtils.EnsureKeywordEnabled( m_bloomMaterial, AmplifyUtils.HighPrecisionKeyword, true );
				AmplifyUtils.EnsureKeywordEnabled( m_finalCompositionMaterial, AmplifyUtils.HighPrecisionKeyword, true );
				AmplifyUtils.CurrentRTFormat = RenderTextureFormat.DefaultHDR;
			}
			else
			{
				AmplifyUtils.EnsureKeywordEnabled( m_bloomMaterial, AmplifyUtils.HighPrecisionKeyword, false );
				AmplifyUtils.EnsureKeywordEnabled( m_finalCompositionMaterial, AmplifyUtils.HighPrecisionKeyword, false );
				AmplifyUtils.CurrentRTFormat = RenderTextureFormat.Default;
			}

			float totalCamRot = Mathf.Acos( Vector3.Dot( m_cameraTransform.right, Vector3.right ) );
			if ( Vector3.Cross( m_cameraTransform.right, Vector3.right ).y > 0 )
				totalCamRot = -totalCamRot;


			RenderTexture lensFlareRT = null;
			RenderTexture lensGlareRT = null;

			if ( !m_highPrecision )
			{
				m_bloomRange.y = 1 / m_bloomRange.x;

				m_bloomMaterial.SetVector( AmplifyUtils.BloomRangeId, m_bloomRange );
				m_finalCompositionMaterial.SetVector( AmplifyUtils.BloomRangeId, m_bloomRange );
			}
			m_bloomParams.y = m_overallThreshold;

			m_bloomMaterial.SetVector( AmplifyUtils.BloomParamsId, m_bloomParams );
			m_finalCompositionMaterial.SetVector( AmplifyUtils.BloomParamsId, m_bloomParams );

			int thresholdResDiv = 1;
			switch ( m_mainThresholdSize )
			{
				case MainThresholdSizeEnum.Half: thresholdResDiv = 2; break;
				case MainThresholdSizeEnum.Quarter: thresholdResDiv = 4; break;
			}

			// CALCULATE THRESHOLD
			RenderTexture thresholdRT = AmplifyUtils.GetTempRenderTarget( src.width / thresholdResDiv, src.height / thresholdResDiv );
			if ( m_maskTexture != null )
			{
				m_bloomMaterial.SetTexture( AmplifyUtils.MaskTextureId, m_maskTexture );
				Graphics.Blit( src, thresholdRT, m_bloomMaterial, ( int ) BloomPasses.ThresholdMask );
			}
			else
			{
				Graphics.Blit( src, thresholdRT, m_bloomMaterial, ( int ) BloomPasses.Threshold );
			}

			if ( m_debugToScreen == DebugToScreenEnum.MainThreshold )
			{
				Graphics.Blit( thresholdRT, dest, m_bloomMaterial, ( int ) BloomPasses.Decode );
				AmplifyUtils.ReleaseAllRT();
				return;
			}

			// DOWNSAMPLE
			bool applyGaussian = true;
			RenderTexture downsampleRT = thresholdRT;
			if ( m_bloomDownsampleCount > 0 )
			{
				applyGaussian = false;
				int tempW = thresholdRT.width;
				int tempH = thresholdRT.height;
				for ( int i = 0; i < m_bloomDownsampleCount; i++ )
				{

					m_tempDownsamplesSizes[ i ].x = tempW;
					m_tempDownsamplesSizes[ i ].y = tempH;
					tempW = ( tempW + 1 ) >> 1;
					tempH = ( tempH + 1 ) >> 1;
					m_tempAuxDownsampleRTs[ i ] = AmplifyUtils.GetTempRenderTarget( tempW, tempH );
					if ( i == 0 )
					{
						if ( !m_temporalFilteringActive || m_gaussianSteps[ i ] != 0 )
						{
							if ( m_upscaleQuality == UpscaleQualityEnum.Realistic )
							{
								Graphics.Blit( downsampleRT, m_tempAuxDownsampleRTs[ i ], m_bloomMaterial, ( int ) BloomPasses.DownsampleWithKaris );
							}
							else
							{
								Graphics.Blit( downsampleRT, m_tempAuxDownsampleRTs[ i ], m_bloomMaterial, ( int ) BloomPasses.DownsampleWithoutKaris );
							}
						}
						else
						{
							if ( m_tempFilterBuffer != null && m_temporalFilteringActive )
							{
								float filterVal = m_temporalFilteringCurve.Evaluate( m_temporalFilteringValue );
								m_bloomMaterial.SetFloat( AmplifyUtils.TempFilterValueId, filterVal );
								m_bloomMaterial.SetTexture( AmplifyUtils.AnamorphicRTS[ 0 ], m_tempFilterBuffer );
								if ( m_upscaleQuality == UpscaleQualityEnum.Realistic )
								{
									Graphics.Blit( downsampleRT, m_tempAuxDownsampleRTs[ i ], m_bloomMaterial, ( int ) BloomPasses.DownsampleWithTempFilterWithKaris );
								}
								else
								{
									Graphics.Blit( downsampleRT, m_tempAuxDownsampleRTs[ i ], m_bloomMaterial, ( int ) BloomPasses.DownsampleWithTempFilterWithoutKaris );
								}
							}
							else
							{
								if ( m_upscaleQuality == UpscaleQualityEnum.Realistic )
								{
									Graphics.Blit( downsampleRT, m_tempAuxDownsampleRTs[ i ], m_bloomMaterial, ( int ) BloomPasses.DownsampleWithKaris );
								}
								else
								{
									Graphics.Blit( downsampleRT, m_tempAuxDownsampleRTs[ i ], m_bloomMaterial, ( int ) BloomPasses.DownsampleWithoutKaris );
								}
							}

							bool createRT = false;
							if ( m_tempFilterBuffer != null )
							{
								if ( m_tempFilterBuffer.format != m_tempAuxDownsampleRTs[ i ].format ||
										m_tempFilterBuffer.width != m_tempAuxDownsampleRTs[ i ].width ||
										m_tempFilterBuffer.height != m_tempAuxDownsampleRTs[ i ].height )
								{
									CleanTempFilterRT();
									createRT = true;
								}
							}
							else
							{
								createRT = true;
							}

							if ( createRT )
							{
								CreateTempFilterRT( m_tempAuxDownsampleRTs[ i ] );
							}

							m_tempFilterBuffer.DiscardContents();
							Graphics.Blit( m_tempAuxDownsampleRTs[ i ], m_tempFilterBuffer );
							if ( m_debugToScreen == DebugToScreenEnum.TemporalFilter )
							{
								Graphics.Blit( m_tempAuxDownsampleRTs[ i ], dest );
								AmplifyUtils.ReleaseAllRT();
								return;
							}
						}
					}
					else
					{
						Graphics.Blit( m_tempAuxDownsampleRTs[ i - 1 ], m_tempAuxDownsampleRTs[ i ], m_bloomMaterial, ( int ) BloomPasses.DownsampleNoWeightedAvg );
					}

					if ( m_gaussianSteps[ i ] > 0 )
					{
						ApplyGaussianBlur( m_tempAuxDownsampleRTs[ i ], m_gaussianSteps[ i ], m_gaussianRadius[ i ], i == 0 );
						if ( m_temporalFilteringActive && m_debugToScreen == DebugToScreenEnum.TemporalFilter )
						{
							Graphics.Blit( m_tempAuxDownsampleRTs[ i ], dest );
							AmplifyUtils.ReleaseAllRT();
							return;
						}
					}

				}

				downsampleRT = m_tempAuxDownsampleRTs[ m_featuresSourceId ];
				AmplifyUtils.ReleaseTempRenderTarget( thresholdRT );
			}

			// BOKEH FILTER
			if ( m_bokehFilter.ApplyBokeh && m_bokehFilter.ApplyOnBloomSource )
			{
				m_bokehFilter.ApplyBokehFilter( downsampleRT, m_bloomMaterial );
				if ( m_debugToScreen == DebugToScreenEnum.BokehFilter )
				{
					Graphics.Blit( downsampleRT, dest );
					AmplifyUtils.ReleaseAllRT();
					return;
				}
			}

			// FEATURES THRESHOLD
			RenderTexture featuresRT = null;
			bool releaseFeaturesRT = false;
			if ( m_separateFeaturesThreshold )
			{
				m_bloomParams.y = m_featuresThreshold;
				m_bloomMaterial.SetVector( AmplifyUtils.BloomParamsId, m_bloomParams );
				m_finalCompositionMaterial.SetVector( AmplifyUtils.BloomParamsId, m_bloomParams );
				featuresRT = AmplifyUtils.GetTempRenderTarget( downsampleRT.width, downsampleRT.height );
				releaseFeaturesRT = true;
				Graphics.Blit( downsampleRT, featuresRT, m_bloomMaterial, ( int ) BloomPasses.Threshold );
				if ( m_debugToScreen == DebugToScreenEnum.FeaturesThreshold )
				{
					Graphics.Blit( featuresRT, dest );
					AmplifyUtils.ReleaseAllRT();
					return;
				}
			}
			else
			{
				featuresRT = downsampleRT;
			}

			if ( m_bokehFilter.ApplyBokeh && !m_bokehFilter.ApplyOnBloomSource )
			{
				if ( !releaseFeaturesRT )
				{
					releaseFeaturesRT = true;
					featuresRT = AmplifyUtils.GetTempRenderTarget( downsampleRT.width, downsampleRT.height );
					Graphics.Blit( downsampleRT, featuresRT );
				}

				m_bokehFilter.ApplyBokehFilter( featuresRT, m_bloomMaterial );
				if ( m_debugToScreen == DebugToScreenEnum.BokehFilter )
				{
					Graphics.Blit( featuresRT, dest );
					AmplifyUtils.ReleaseAllRT();
					return;
				}
			}

			// LENS FLARE
			if ( m_lensFlare.ApplyLensFlare && m_debugToScreen != DebugToScreenEnum.Bloom )
			{
				lensFlareRT = m_lensFlare.ApplyFlare( m_bloomMaterial, featuresRT );
				ApplyGaussianBlur( lensFlareRT, m_lensFlare.LensFlareGaussianBlurAmount );
				if ( m_debugToScreen == DebugToScreenEnum.LensFlare )
				{
					Graphics.Blit( lensFlareRT, dest );
					AmplifyUtils.ReleaseAllRT();
					return;
				}
			}

			//ANAMORPHIC GLARE
			if ( m_anamorphicGlare.ApplyLensGlare && m_debugToScreen != DebugToScreenEnum.Bloom )
			{
				lensGlareRT = AmplifyUtils.GetTempRenderTarget( downsampleRT.width, downsampleRT.height );

				m_anamorphicGlare.OnRenderImage( m_bloomMaterial, featuresRT, lensGlareRT, totalCamRot );
				if ( m_debugToScreen == DebugToScreenEnum.LensGlare )
				{
					Graphics.Blit( lensGlareRT, dest );
					AmplifyUtils.ReleaseAllRT();
					return;
				}
			}

			if ( releaseFeaturesRT )
			{
				AmplifyUtils.ReleaseTempRenderTarget( featuresRT );
			}

			//BLUR
			if ( applyGaussian )
			{
				ApplyGaussianBlur( downsampleRT, m_gaussianSteps[ 0 ], m_gaussianRadius[ 0 ] );
			}

			//UPSAMPLE

			if ( m_bloomDownsampleCount > 0 )
			{
				if ( m_bloomDownsampleCount == 1 )
				{
					if ( m_upscaleQuality == UpscaleQualityEnum.Realistic )
					{
						ApplyUpscale();
						m_finalCompositionMaterial.SetTexture( AmplifyUtils.MipResultsRTS[ 0 ], m_tempUpscaleRTs[ 0 ] );
					}
					else
					{
						m_finalCompositionMaterial.SetTexture( AmplifyUtils.MipResultsRTS[ 0 ], m_tempAuxDownsampleRTs[ 0 ] );
					}
					m_finalCompositionMaterial.SetFloat( AmplifyUtils.UpscaleWeightsStr[ 0 ], m_upscaleWeights[ 0 ] );
				}
				else
				{

					if ( m_upscaleQuality == UpscaleQualityEnum.Realistic )
					{
						ApplyUpscale();
						for ( int i = 0; i < m_bloomDownsampleCount; i++ )
						{
							int id = m_bloomDownsampleCount - i - 1;
							m_finalCompositionMaterial.SetTexture( AmplifyUtils.MipResultsRTS[ id ], m_tempUpscaleRTs[ i ] );
							m_finalCompositionMaterial.SetFloat( AmplifyUtils.UpscaleWeightsStr[ id ], m_upscaleWeights[ i ] );
						}
					}
					else
					{
						for ( int i = 0; i < m_bloomDownsampleCount; i++ )
						{
							int id = m_bloomDownsampleCount - 1 - i;
							m_finalCompositionMaterial.SetTexture( AmplifyUtils.MipResultsRTS[ id ], m_tempAuxDownsampleRTs[ id ] );
							m_finalCompositionMaterial.SetFloat( AmplifyUtils.UpscaleWeightsStr[ id ], m_upscaleWeights[ i ] );
						}
					}
				}
			}
			else
			{
				m_finalCompositionMaterial.SetTexture( AmplifyUtils.MipResultsRTS[ 0 ], downsampleRT );
				m_finalCompositionMaterial.SetFloat( AmplifyUtils.UpscaleWeightsStr[ 0 ], 1 );
			}

			if ( m_debugToScreen == DebugToScreenEnum.Bloom )
			{
				m_finalCompositionMaterial.SetFloat( AmplifyUtils.SourceContributionId, 0 );
				FinalComposition( 0, 1, src, dest, 0 );
				return;
			}


			// FINAL COMPOSITION
			// LENS FLARE
			if ( m_bloomDownsampleCount > 1 )
			{
				for ( int i = 0; i < m_bloomDownsampleCount; i++ )
				{
					m_finalCompositionMaterial.SetFloat( AmplifyUtils.LensDirtWeightsStr[ m_bloomDownsampleCount - i - 1 ], m_lensDirtWeights[ i ] );
					m_finalCompositionMaterial.SetFloat( AmplifyUtils.LensStarburstWeightsStr[ m_bloomDownsampleCount - i - 1 ], m_lensStarburstWeights[ i ] );
				}
			}
			else
			{
				m_finalCompositionMaterial.SetFloat( AmplifyUtils.LensDirtWeightsStr[ 0 ], m_lensDirtWeights[ 0 ] );
				m_finalCompositionMaterial.SetFloat( AmplifyUtils.LensStarburstWeightsStr[ 0 ], m_lensStarburstWeights[ 0 ] );
			}
			if ( m_lensFlare.ApplyLensFlare )
			{
				m_finalCompositionMaterial.SetTexture( AmplifyUtils.LensFlareRTId, lensFlareRT );
			}

			//LENS GLARE
			if ( m_anamorphicGlare.ApplyLensGlare )
			{
				m_finalCompositionMaterial.SetTexture( AmplifyUtils.LensGlareRTId, lensGlareRT );
			}

			// LENS DIRT
			if ( m_applyLensDirt )
			{
				m_finalCompositionMaterial.SetTexture( AmplifyUtils.LensDirtRTId, m_lensDirtTexture );
				m_finalCompositionMaterial.SetFloat( AmplifyUtils.LensDirtStrengthId, m_lensDirtStrength * MaxDirtIntensity );

				if ( m_debugToScreen == DebugToScreenEnum.LensDirt )
				{
					FinalComposition( 0, 0, src, dest, 2 );
					return;
				}
			}

			// LENS STARBURST
			if ( m_applyLensStardurst )
			{
				m_starburstMat[ 0, 0 ] = Mathf.Cos( totalCamRot );
				m_starburstMat[ 0, 1 ] = -Mathf.Sin( totalCamRot );
				m_starburstMat[ 1, 0 ] = Mathf.Sin( totalCamRot );
				m_starburstMat[ 1, 1 ] = Mathf.Cos( totalCamRot );

				m_finalCompositionMaterial.SetMatrix( AmplifyUtils.LensFlareStarMatrixId, m_starburstMat );
				m_finalCompositionMaterial.SetFloat( AmplifyUtils.LensFlareStarburstStrengthId, m_lensStarburstStrength * MaxStarburstIntensity );
				m_finalCompositionMaterial.SetTexture( AmplifyUtils.LensStarburstRTId, m_lensStardurstTex );

				if ( m_debugToScreen == DebugToScreenEnum.LensStarburst )
				{
					FinalComposition( 0, 0, src, dest, 1 );
					return;
				}
			}

			if ( m_targetTexture != null )
			{
				m_targetTexture.DiscardContents();
				FinalComposition( 0, 1, src, m_targetTexture, -1 );
				Graphics.Blit( src, dest );
			}
			else
			{
				FinalComposition( 1, 1, src, dest, -1 );
			}
		}


		void FinalComposition( float srcContribution, float upscaleContribution, RenderTexture src, RenderTexture dest, int forcePassId )
		{
			m_finalCompositionMaterial.SetFloat( AmplifyUtils.SourceContributionId, srcContribution );
			m_finalCompositionMaterial.SetFloat( AmplifyUtils.UpscaleContributionId, upscaleContribution );

			int passCount = 0;
			if ( forcePassId > -1 )
			{
				passCount = forcePassId;
			}
			else
			{
				if ( LensFlareInstance.ApplyLensFlare )
				{
					passCount = passCount | 8;
				}

				if ( LensGlareInstance.ApplyLensGlare )
				{
					passCount = passCount | 4;
				}

				if ( m_applyLensDirt )
				{
					passCount = passCount | 2;
				}

				if ( m_applyLensStardurst )
				{
					passCount = passCount | 1;
				}
			}
			passCount += ( m_bloomDownsampleCount - 1 ) * 16;
			Graphics.Blit( src, dest, m_finalCompositionMaterial, passCount );
			AmplifyUtils.ReleaseAllRT();
		}

		void ApplyUpscale()
		{
			int beginIdx = ( m_bloomDownsampleCount - 1 );
			int upscaleIdx = 0;
			for ( int downscaleIdx = beginIdx; downscaleIdx > -1; downscaleIdx-- )
			{
				m_tempUpscaleRTs[ upscaleIdx ] = AmplifyUtils.GetTempRenderTarget( ( int ) m_tempDownsamplesSizes[ downscaleIdx ].x, ( int ) m_tempDownsamplesSizes[ downscaleIdx ].y );
				if ( downscaleIdx == beginIdx )
				{
					Graphics.Blit( m_tempAuxDownsampleRTs[ beginIdx ], m_tempUpscaleRTs[ upscaleIdx ], m_bloomMaterial, ( int ) BloomPasses.UpscaleFirstPass );
				}
				else
				{
					m_bloomMaterial.SetTexture( AmplifyUtils.AnamorphicRTS[ 0 ], m_tempUpscaleRTs[ upscaleIdx - 1 ] );
					Graphics.Blit( m_tempAuxDownsampleRTs[ downscaleIdx ], m_tempUpscaleRTs[ upscaleIdx ], m_bloomMaterial, ( int ) BloomPasses.Upscale );
				}
				upscaleIdx++;
			}
		}

		public AmplifyGlare LensGlareInstance
		{
			get { return m_anamorphicGlare; }
		}

		public AmplifyBokeh BokehFilterInstance
		{
			get { return m_bokehFilter; }
		}

		public AmplifyLensFlare LensFlareInstance
		{
			get { return m_lensFlare; }
		}

		public bool ApplyLensDirt
		{
			get { return m_applyLensDirt; }
			set { m_applyLensDirt = value; }
		}

		public float LensDirtStrength
		{
			get { return m_lensDirtStrength; }
			set
			{
				m_lensDirtStrength = value < 0 ? 0 : value;
			}
		}

		public Texture LensDirtTexture
		{
			get { return m_lensDirtTexture; }
			set { m_lensDirtTexture = value; }
		}

		public bool ApplyLensStardurst
		{
			get { return m_applyLensStardurst; }
			set { m_applyLensStardurst = value; }
		}

		public Texture LensStardurstTex
		{
			get { return m_lensStardurstTex; }
			set { m_lensStardurstTex = value; }
		}

		public float LensStarburstStrength
		{
			get { return m_lensStarburstStrength; }
			set { m_lensStarburstStrength = value < 0 ? 0 : value; }
		}

		public PrecisionModes CurrentPrecisionMode
		{
			get
			{
				if ( m_highPrecision )
					return PrecisionModes.High;

				return PrecisionModes.Low;
			}
			set
			{
				HighPrecision = value == PrecisionModes.High;
			}
		}

		public bool HighPrecision
		{
			get { return m_highPrecision; }
			set
			{
				if ( m_highPrecision != value )
				{
					m_highPrecision = value;
					CleanTempFilterRT();
				}
			}
		}

		public float BloomRange
		{
			get { return m_bloomRange.x; }
			set
			{
				m_bloomRange.x = value < 0 ? 0 : value;
			}
		}

		public float OverallThreshold
		{
			get { return m_overallThreshold; }
			set
			{
				m_overallThreshold = value < 0 ? 0 : value;
			}
		}

		public Vector4 BloomParams
		{
			get { return m_bloomParams; }
			set { m_bloomParams = value; }
		}

		public float OverallIntensity
		{
			get { return m_bloomParams.x; }
			set
			{
				m_bloomParams.x = value < 0 ? 0 : value;
			}
		}

		public float BloomScale
		{
			get { return m_bloomParams.w; }
			set
			{
				m_bloomParams.w = value < 0 ? 0 : value;
			}
		}

		public float UpscaleBlurRadius
		{
			get { return m_bloomParams.z; }
			set { m_bloomParams.z = value; }
		}

		public bool TemporalFilteringActive
		{
			get { return m_temporalFilteringActive; }
			set
			{
				if ( m_temporalFilteringActive != value )
				{
					CleanTempFilterRT();
				}
				m_temporalFilteringActive = value;
			}
		}

		public float TemporalFilteringValue
		{
			get { return m_temporalFilteringValue; }
			set { m_temporalFilteringValue = value; }
		}

		public int SoftMaxdownscales
		{
			get { return m_softMaxdownscales; }
		}
		public int BloomDownsampleCount
		{
			get { return m_bloomDownsampleCount; }
			set { m_bloomDownsampleCount = Mathf.Clamp( value, MinDownscales, m_softMaxdownscales ); }
		}

		public int FeaturesSourceId
		{
			get { return m_featuresSourceId; }
			set { m_featuresSourceId = Mathf.Clamp( value, 0, m_bloomDownsampleCount - 1 ); }
		}

		public bool[] DownscaleSettingsFoldout
		{
			get { return m_downscaleSettingsFoldout; }
		}

		public float[] UpscaleWeights
		{
			get { return m_upscaleWeights; }
		}

		public float[] LensDirtWeights
		{
			get { return m_lensDirtWeights; }
		}

		public float[] LensStarburstWeights
		{
			get { return m_lensStarburstWeights; }
		}

		public float[] GaussianRadius
		{
			get { return m_gaussianRadius; }
		}

		public int[] GaussianSteps
		{
			get { return m_gaussianSteps; }
		}

		public AnimationCurve TemporalFilteringCurve
		{
			get { return m_temporalFilteringCurve; }
			set { m_temporalFilteringCurve = value; }
		}

		public bool SeparateFeaturesThreshold
		{
			get { return m_separateFeaturesThreshold; }
			set { m_separateFeaturesThreshold = value; }
		}

		public float FeaturesThreshold
		{
			get { return m_featuresThreshold; }
			set { m_featuresThreshold = value < 0 ? 0 : value; }
		}

		public DebugToScreenEnum DebugToScreen
		{
			get { return m_debugToScreen; }
			set { m_debugToScreen = value; }
		}

		public UpscaleQualityEnum UpscaleQuality
		{
			get { return m_upscaleQuality; }
			set { m_upscaleQuality = value; }
		}

		public bool ShowDebugMessages
		{
			get { return m_showDebugMessages; }
			set { m_showDebugMessages = value; }
		}

		public MainThresholdSizeEnum MainThresholdSize
		{
			get { return m_mainThresholdSize; }
			set { m_mainThresholdSize = value; }
		}

		public RenderTexture TargetTexture
		{
			get { return m_targetTexture; }
			set { m_targetTexture = value; }
		}

		public Texture MaskTexture
		{
			get { return m_maskTexture; }
			set { m_maskTexture = value; }
		}

		public bool ApplyBokehFilter
		{
			get { return m_bokehFilter.ApplyBokeh; }
			set { m_bokehFilter.ApplyBokeh = value; }
		}

		public bool ApplyLensFlare
		{
			get { return m_lensFlare.ApplyLensFlare; }
			set { m_lensFlare.ApplyLensFlare = value; }
		}

		public bool ApplyLensGlare
		{
			get { return m_anamorphicGlare.ApplyLensGlare; }
			set { m_anamorphicGlare.ApplyLensGlare = value; }
		}
#if TRIAL
		void OnGUI()
		{
			if ( !silentError && watermark != null )
				GUI.DrawTexture( new Rect( Screen.width - watermark.width - 15, Screen.height - watermark.height - 12, watermark.width, watermark.height ), watermark );
		}
#endif
	}
}
