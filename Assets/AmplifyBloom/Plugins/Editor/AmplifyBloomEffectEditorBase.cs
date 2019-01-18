// Amplify Bloom - Advanced Bloom Post-Effect for Unity
// Copyright (c) Amplify Creations, Lda <info@amplify.pt>

#if UNITY_5_0 || UNITY_5_1 || UNITY_5_2
#define UNITY_PRE_5_3
#endif

using UnityEngine;
using UnityEditor;
#if !UNITY_PRE_5_3
using UnityEditor.SceneManagement;
#endif

namespace AmplifyBloom
{
	[System.Serializable]
	public class AmplifyBloomEffectEditorBase : Editor
	{
		private const string IntensityStr = "Intensity";
		private const string AdvancedSettingsStr = "Advanced Settings";
		private Rect TemporalCurveRanges = new Rect( 0, 0, 1, 0.999f );
		private Color TemporalCurveColor = new Color( 0, 1, 0, 1 );
		[SerializeField]
		private bool m_bokehAdvancedSettingsFoldout = false;
		[SerializeField]
		private bool m_ghostsAdvancedSettingsFoldout = false;
		[SerializeField]
		private bool m_haloAdvancedSettingsFoldout = false;
		[SerializeField]
		private bool m_lensGlareAdvancedSettingsFoldout = false;
		[SerializeField]
		private bool m_bloomFoldout = true;
		[SerializeField]
		private bool m_temporalFilterFoldout = false;
		[SerializeField]
		private bool m_featuresFoldout = false;
		[SerializeField]
		private bool m_bokehFilterFoldout = false;
		[SerializeField]
		private bool m_lensFlareFoldout = false;
		[SerializeField]
		private bool m_ghostsFoldout = false;
		[SerializeField]
		private bool m_haloFoldout = false;
		[SerializeField]
		private bool m_lensGlareFoldout = false;
		[SerializeField]
		private bool m_lensDirtFoldout = false;
		[SerializeField]
		private bool m_lensStarburstFoldout = false;
		[SerializeField]
		private bool m_mipSettingsFoldout = false;
		[SerializeField]
		private bool m_lensDirtWeightsFoldout = false;

		[SerializeField]
		private bool m_lensStarburstWeightsFoldout = false;
		[SerializeField]
		private bool m_bloomWeightsFoldout = false;


		private GUIStyle m_mainFoldoutStyle;
		private GUIContent m_highPrecisionGC = new GUIContent( "Precision", "Switch between HDR and LDR Render Texture formats." );
		private GUIContent m_debugToScreenGC = new GUIContent( "Debug", "Debug each bloom/feature stage to screen." );
		private GUIContent m_ldrRangeGC = new GUIContent( "Range", "LDR Tweakable range. Use to match HDR results." );
		private GUIContent m_overallIntensityGC = new GUIContent( IntensityStr, "Overall bloom intensity. Affects all the effects bellow." );
		private GUIContent m_thresholdGC = new GUIContent( "Threshold", "Luminance threshold to setup what should generate bloom." );
		private GUIContent m_blurStepGC = new GUIContent( "Blur Step", "Number of blur passes done on bloom results. Higher number provides smoother results but decreases performance." );
		private GUIContent m_blurRadiusGC = new GUIContent( "Blur Radius", "Blur radius amount" );
		private GUIContent m_upscaleWeightGC = new GUIContent( "Weight", "Influence of the selected Mip. Only valid when Mip Amount greater than 0." );
		private GUIContent m_featuresSourceIdGC = new GUIContent( "Features Source Id", "Mip source which will be used to generate features." );
		private GUIContent m_upscaleQualityGC = new GUIContent( "Technique", "Method which will be used to upscale results. Realistic is visually more robust but less efficient." );
		private GUIContent m_downscaleAmountGC = new GUIContent( "Mip Count", "Number of resizes done on main RT before performing bloom. Increasing its the number provides finer tweaking but at the loss at performance." );
		private GUIContent m_upscaleScaleRadiusGC = new GUIContent( "Upscale Blur Radius", "Radius used on the tent filter when upscaling to source size." );
		private GUIContent m_filterCurveGC = new GUIContent( "Filter Curve", "Range of values which defines temporal filter behaviour." );
		private GUIContent m_filterValueGC = new GUIContent( "Filter Value", "Position on the filter curve." );
		private GUIContent m_separateThresholdGC = new GUIContent( "Threshold", "Threshold value for second threshold layer." );
		private GUIContent m_bokehApplyOnBloomSourceGC = new GUIContent( "Apply on Bloom Source", "Bokeh filtering can either be applied on the bloom source and visually affect it or only affect features (lens flare/glare/dirt/starburst)." );
		private GUIContent m_bokehApertureShapeGC = new GUIContent( "Aperture Shape", "Type of bokeh filter which will reshape bloom results." );
		private GUIContent m_bokehSampleRadiusGC = new GUIContent( "Sample Radius", "Bokeh imaginary camera DOF's radius." );
		private GUIContent m_bokehRotationGC = new GUIContent( "Rotation", "Filter overall rotation." );
		private GUIContent m_bokehApertureGC = new GUIContent( "Aperture", "Bokeh imaginary camera DOF's aperture." );
		private GUIContent m_bokehFocalLengthGC = new GUIContent( "Focal Length", "Bokeh imaginary camera DOF's focal length." );
		private GUIContent m_bokehFocalDistanceGC = new GUIContent( "Focal Distance", "Bokeh imaginary camera DOF's focal distance." );
		private GUIContent m_bokehMaxCoCDiameterGC = new GUIContent( "Max CoC Diameter", "Bokeh imaginary camera DOF's Max Circle of Confusion diameter." );
		private GUIContent m_lensFlareIntensityGC = new GUIContent( IntensityStr, "Overall intensity for both halo and ghosts." );
		private GUIContent m_lensFlareBlurAmountGC = new GUIContent( "Blur amount", "Amount of blur applied on generated halo and ghosts." );
		private GUIContent m_lensFlareRadialTintGC = new GUIContent( "Radial Tint", "Dynamic tint color applied to halo and ghosts according to its screen position. Left most color on gradient corresponds to screen center." );
		private GUIContent m_lensFlareGhostsInstensityGC = new GUIContent( IntensityStr, "Ghosts intensity." );
		private GUIContent m_lensFlareGhostAmountGC = new GUIContent( "Count", "Amount of ghosts generated from each bloom area." );
		private GUIContent m_lensFlareGhostDispersalGC = new GUIContent( "Dispersal", "Distance between ghost generated from the same bloom area." );
		private GUIContent m_lensFlareGhostChrmDistortGC = new GUIContent( "Chromatic Distortion", "Amount of chromatic distortion applied on each ghost." );
		private GUIContent m_lensFlareGhostPowerFactorGC = new GUIContent( "Power Factor", "Base on ghost fade power function." );
		private GUIContent m_lensFlareGhostPowerFalloffGC = new GUIContent( "Power Falloff", "Exponent on ghost fade power function." );
		private GUIContent m_lensFlareHalosIntensityGC = new GUIContent( IntensityStr, "Halo intensity." );
		private GUIContent m_lensFlareHaloWidthGC = new GUIContent( "Width", "Width/Radius of the generated halo." );
		private GUIContent m_lensFlareHaloChrmDistGC = new GUIContent( "Chromatic Distortion", "Amount of chromatic distortion applied on halo." );
		private GUIContent m_lensFlareHaloPowerFactorGC = new GUIContent( "Power Factor", "Base on halo fade power function." );
		private GUIContent m_lensFlareHaloPowerFalloffGC = new GUIContent( "Power Falloff", "Exponent on halo fade power function." );
		private GUIContent m_lensGlareIntensityGC = new GUIContent( IntensityStr, "Lens Glare intensity." );
		private GUIContent m_lensGlareOverallStreakScaleGC = new GUIContent( "Streak Scale", "Overall glare streak length modifier." );
		private GUIContent m_lensGlareOverallTintGC = new GUIContent( "Overall Tint", "Tint applied uniformly across each type of glare." );
		private GUIContent m_lensGlareTypeGC = new GUIContent( "Type", "Type of glare." );
		private GUIContent m_lensGlareTintAlongGlareGC = new GUIContent( "Tint Along Glare", "Tint for spectral types along each ray.Leftmost color on the gradient corresponds to sample near bloom source." );
		private GUIContent m_lensGlarePerPassDispGC = new GUIContent( "Per Pass Displacement", "Distance between samples when creating each ray." );
		private GUIContent m_lensGlareMaxPerRayGC = new GUIContent( "Max Per Ray Passes", "Max amount of passes used to build each ray. More passes means more defined and propagated rays but decreases performance." );
		private GUIContent m_lensDirtIntensityGC = new GUIContent( IntensityStr, "Lens Dirt Intensity." );
		private GUIContent m_lensDirtTextureGC = new GUIContent( "Dirt Texture", "Mask from which dirt is going to be created." );
		private GUIContent m_lensStarburstIntensityGC = new GUIContent( IntensityStr, "Lens Starburst Intensity." );
		private GUIContent m_lensStarburstTextureGC = new GUIContent( "Starburst Texture", "Mask from which starburst is going to be created." );
		private GUIContent m_bloomFoldoutGC = new GUIContent( " Bloom", "Settings for bloom generation, will affect all features." );
		private GUIContent m_bokehFilterFoldoutGC = new GUIContent( " Bokeh Filter", "Settings for Bokeh filter generation." );
		private GUIContent m_lensFlareFoldoutGC = new GUIContent( " Lens Flare", "Overall settings for Lens Flare (Halo/Ghosts) generation." );
		private GUIContent m_ghostsFoldoutGC = new GUIContent( "Ghosts", "Settings for Ghosts generation." );
		private GUIContent m_halosFoldoutGC = new GUIContent( "Halo", "Settings for Halo generation." );
		private GUIContent m_lensGlareFoldoutGC = new GUIContent( " Lens Glare", "Settings for Anamorphic Lens Glare generation." );
		private GUIContent m_lensDirtFoldoutGC = new GUIContent( " Lens Dirt", "Settings for Lens Dirt composition." );
		private GUIContent m_lensStarburstFoldoutGC = new GUIContent( " Lens Starburst", "Settings for Lens Starburts composition." );
		private GUIContent m_temporalFilterFoldoutGC = new GUIContent( " Temporal Filter", "Settings for temporal filtering configuration." );
		private GUIContent m_sepFeaturesThresholdFoldoutGC = new GUIContent( " Features Threshold", "Settings for features threshold." );
		private GUIContent m_advancedSettingsBokehFoldoutGC = new GUIContent( AdvancedSettingsStr, "Advanced settings for Bokeh filter." );
		private GUIContent m_advancedSettingsGhostsFoldoutGC = new GUIContent( AdvancedSettingsStr, "Advanced settings for Ghosts." );
		private GUIContent m_advancedSettingsHalosFoldoutGC = new GUIContent( AdvancedSettingsStr, "Advanced settings for Halo." );
		private GUIContent m_advancedSettingsLensGlareFoldoutGC = new GUIContent( AdvancedSettingsStr, "Advanced settings for Lens Glare." );
		private GUIContent m_customGlareIdxGC = new GUIContent( "Current", "Current selected custom glare from array bellow." );
		private GUIContent m_customGlareSizeGC = new GUIContent( "Size", "Amount of customizable glare definitions." );
		private GUIContent m_customGlareNameGC = new GUIContent( "Name", "Custom glare name." );
		private GUIContent m_customGlareStarInclinationGC = new GUIContent( "Initial Offset", "Star angle initial offset." );
		private GUIContent m_customGlareChromaticAberrationGC = new GUIContent( "Chromatic Amount", "Amount of influence from chromatic gradient." );
		private GUIContent m_customGlareStarlinesCountGC = new GUIContent( "Star Lines Count", "Amount of generated rays." );
		private GUIContent m_customGlarePassCountGC = new GUIContent( "Pass Count", "Amount of passes used to generate rays." );
		private GUIContent m_customGlareSampleLengthGC = new GUIContent( "Sample Length", "Spacing between each sample when generating rays." );
		private GUIContent m_customGlareAttenuationGC = new GUIContent( "Attenuation", "Attenuation factor along ray." );
		private GUIContent m_customGlareRotationGC = new GUIContent( "Camera Influence", "Amount of influence camera rotation has on rays." );
		private GUIContent m_customGlareCustomIncrementGC = new GUIContent( "Custom Increment", "Custom angle increment between rays. They will be evenly rotated if specified a value equal to 0." );
		private GUIContent m_customGlareLongAttenuationGC = new GUIContent( "Long Attenuation", "Second attenuation factor. Rays will alternate between Attenuation ( Odd numbers) and Long Attenuation ( Even numbers). Only active if specified value is greater than 0." );
		private GUIContent m_customGlareFoldoutGC = new GUIContent( "Custom Label", "Properties for hovered custom glare." );
		private GUIContent m_mipSettingGC = new GUIContent( "Mip Settings", "Configurable settings for each mip" );
		private GUIContent m_lensWeightsFoldoutGC = new GUIContent( "Mip Weights", "Each mip contribution to the Lens Dirt and Starburts feature." );
		private GUIContent m_lensWeightGC = new GUIContent( "Mip ", "Influence of the selected Mip. Only valid when Mip Amount greater than 0." );
		private GUIContent[] m_lensWeightGCArr;
		private GUIContent m_bloomWeightsFoldoutGC = new GUIContent( "Mip Weights", "Each mip contribution to Bloom." );
		private GUIContent m_showDebugMessagesGC = new GUIContent( "Show Warnings", "Show relevant Amplify Bloom Warnings to Console." );
		private GUIContent m_mainThresholdSizeGC = new GUIContent( "Source Downscale", "Initial render texture scale on which the Main Threshold will be written." );
		private GUIContent m_targetTextureGC = new GUIContent( "Target Texture", "Render Bloom to a render texture instead of destination" );
		private GUIContent m_maskTextureGC = new GUIContent( "Mask Texture", "Render Bloom only on certain areas specifed by mask." );
		private GUIStyle m_foldoutClosed;
		private GUIStyle m_foldoutOpened;
		private GUIStyle m_toggleStyle;
		private GUIStyle m_mainLabelStyle;
		private GUIStyle m_disabledToggleStyle;


		private GUIContent[] m_bloomWeightsLabelGCArr;
		void Awake()
		{
			m_bloomWeightsLabelGCArr = new GUIContent[ AmplifyBloomBase.MaxDownscales ];
			for ( int i = 0; i < m_bloomWeightsLabelGCArr.Length; i++ )
			{
				m_bloomWeightsLabelGCArr[ i ] = new GUIContent( "Mip " + ( i + 1 ), m_bloomWeightsFoldoutGC.tooltip );
			}

			m_mainLabelStyle = new GUIStyle( EditorStyles.label );
			m_mainLabelStyle.fontStyle = FontStyle.Bold;

			m_disabledToggleStyle = new GUIStyle( EditorStyles.toggle );
			m_disabledToggleStyle.normal = m_disabledToggleStyle.onActive;

			m_foldoutClosed = new GUIStyle( EditorStyles.foldout );
			m_foldoutClosed.fontStyle = FontStyle.Bold;

			m_foldoutOpened = new GUIStyle( EditorStyles.foldout );
			m_foldoutOpened.normal = m_foldoutOpened.onActive;
			m_foldoutOpened.fontStyle = FontStyle.Bold;

			m_mainFoldoutStyle = new GUIStyle( EditorStyles.foldout );
			m_mainFoldoutStyle.fontStyle = FontStyle.Bold;

			m_toggleStyle = new GUIStyle( EditorStyles.toggle );
			m_toggleStyle.fontStyle = FontStyle.Bold;

			m_lensWeightGCArr = new GUIContent[ AmplifyBloomBase.MaxDownscales ];
			for ( int i = 0; i < m_lensWeightGCArr.Length; i++ )
			{
				m_lensWeightGCArr[ i ] = new GUIContent( m_lensWeightGC );
				m_lensWeightGCArr[ i ].text += ( i + 1 ).ToString();
			}
			AmplifyBloomBase bloom = ( AmplifyBloomBase ) target;
			if ( bloom.LensDirtTexture == null )
			{
				bloom.LensDirtTexture = AssetDatabase.LoadAssetAtPath<Texture>( "Assets/AmplifyBloom/Samples/Textures/Dirt/DirtHighContrast.png" );
			}

			if ( bloom.LensStardurstTex == null )
			{
				bloom.LensStardurstTex = AssetDatabase.LoadAssetAtPath<Texture>( "Assets/AmplifyBloom/Samples/Textures/Starburst/Starburst.png" );
			}
		}

		bool CustomFoldout( bool value , GUIContent content, int space = 4)
		{
			GUIStyle foldoutStyle = value ? m_foldoutOpened : m_foldoutClosed;
			m_mainLabelStyle.fontStyle = FontStyle.Normal;
			EditorGUILayout.BeginHorizontal();
			{
				GUILayout.Space( space );
				if ( GUILayout.Button( string.Empty, foldoutStyle, GUILayout.Width( 10 ) ) )
				{
					value = !value;

				}

				if ( GUILayout.Button( content, m_mainLabelStyle ) )
				{
					value = !value;
				}

			}
			EditorGUILayout.EndHorizontal();
			return value;
		}

		void ToggleFoldout( GUIContent content, ref bool foldoutValue, ref bool toggleValue, bool applyBold, int space = -8, bool specialToggle = false )
		{
			GUIStyle foldoutStyle = foldoutValue ? m_foldoutOpened : m_foldoutClosed;
			m_toggleStyle.fontStyle = foldoutStyle.fontStyle = applyBold ? FontStyle.Bold : FontStyle.Normal;
			m_mainLabelStyle.fontStyle = m_toggleStyle.fontStyle;
			EditorGUILayout.BeginHorizontal();
			{
				GUILayout.Space( space );
				if ( GUILayout.Button( string.Empty, foldoutStyle, GUILayout.Width( 10 ) ) )
				{
					foldoutValue = !foldoutValue;
				}


				if ( specialToggle )
				{
					GUI.enabled = false;
					GUILayout.Button( string.Empty, m_disabledToggleStyle, GUILayout.Width( 10 ) );
					GUI.enabled = true;
				}
				else
				{
					toggleValue = GUILayout.Toggle( toggleValue, content, m_toggleStyle, GUILayout.Width( 10 ) );
				}

				if ( GUILayout.Button( content, m_mainLabelStyle ) )
				{
					foldoutValue = !foldoutValue;
				}

			}
			EditorGUILayout.EndHorizontal();
		}

		override public void OnInspectorGUI()
		{
			Undo.RecordObject( target, "AmplifyBloomInspector" );
			AmplifyBloomBase bloom = ( AmplifyBloomBase ) target;
			SerializedObject bloomObj = new SerializedObject( bloom );
			GUILayout.BeginVertical();
			{
				EditorGUI.BeginChangeCheck();
				EditorGUILayout.Separator();
				bool applyBloom = true;
				ToggleFoldout( m_bloomFoldoutGC, ref m_bloomFoldout, ref applyBloom, true, -8, true );
				if ( m_bloomFoldout )
				{
					bloom.UpscaleQuality = ( UpscaleQualityEnum ) EditorGUILayout.EnumPopup( m_upscaleQualityGC, bloom.UpscaleQuality );
					bloom.MainThresholdSize = ( MainThresholdSizeEnum ) EditorGUILayout.EnumPopup( m_mainThresholdSizeGC, bloom.MainThresholdSize );

					GUILayout.BeginHorizontal();
					float originalLabelWidth = EditorGUIUtility.labelWidth;

					bloom.CurrentPrecisionMode = ( PrecisionModes ) EditorGUILayout.EnumPopup( m_highPrecisionGC, bloom.CurrentPrecisionMode );
					GUI.enabled = !bloom.HighPrecision;
					{
						EditorGUIUtility.labelWidth = 45;
						bloom.BloomRange = EditorGUILayout.FloatField( m_ldrRangeGC, bloom.BloomRange, GUILayout.MaxWidth( 1300 ) );
					}
					EditorGUIUtility.labelWidth = originalLabelWidth;
					GUI.enabled = true;

					GUILayout.EndHorizontal();

					bloom.OverallIntensity = EditorGUILayout.FloatField( m_overallIntensityGC, bloom.OverallIntensity );

					bloom.OverallThreshold = EditorGUILayout.FloatField( m_thresholdGC, bloom.OverallThreshold );

					SerializedProperty maskTextureField = bloomObj.FindProperty( "m_maskTexture" );
					EditorGUI.BeginChangeCheck();
					EditorGUILayout.PropertyField( maskTextureField, m_maskTextureGC );
					if ( EditorGUI.EndChangeCheck() )
					{
						bloomObj.ApplyModifiedProperties();
					}

					SerializedProperty targetTextureField = bloomObj.FindProperty( "m_targetTexture" );
					EditorGUI.BeginChangeCheck();
					EditorGUILayout.PropertyField( targetTextureField, m_targetTextureGC );
					if ( EditorGUI.EndChangeCheck() )
					{
						bloomObj.ApplyModifiedProperties();
					}
					//bloom.TargetTexture = EditorGUILayout.ObjectField( m_targetTextureGC, bloom.TargetTexture, typeof( RenderTexture ),false ) as RenderTexture;
					bloom.DebugToScreen = ( DebugToScreenEnum ) EditorGUILayout.EnumPopup( m_debugToScreenGC, bloom.DebugToScreen );
					bloom.ShowDebugMessages = EditorGUILayout.Toggle( m_showDebugMessagesGC, bloom.ShowDebugMessages );
					int weightMaxDowsampleCount = Mathf.Max( 1, bloom.BloomDownsampleCount );
					{
						EditorGUI.indentLevel++;
						m_mipSettingsFoldout = CustomFoldout( m_mipSettingsFoldout, m_mipSettingGC );
						if ( m_mipSettingsFoldout )
						{
							EditorGUI.indentLevel++;
							bloom.BloomDownsampleCount = EditorGUILayout.IntSlider( m_downscaleAmountGC, bloom.BloomDownsampleCount, AmplifyBloomBase.MinDownscales, bloom.SoftMaxdownscales );
							bool guiState = bloom.BloomDownsampleCount != 0;

							GUI.enabled = ( bloom.UpscaleQuality == UpscaleQualityEnum.Realistic ) && guiState;
							{
								bloom.UpscaleBlurRadius = EditorGUILayout.Slider( m_upscaleScaleRadiusGC, bloom.UpscaleBlurRadius, 1f, 3.0f );
							}
							GUI.enabled = guiState;

							int featuresSourceId = bloom.FeaturesSourceId + 1;
							featuresSourceId = EditorGUILayout.IntSlider( m_featuresSourceIdGC, featuresSourceId, 1, bloom.BloomDownsampleCount );
							bloom.FeaturesSourceId = featuresSourceId - 1;
							EditorGUI.indentLevel--;
						}
						GUI.enabled = true;

						m_bloomWeightsFoldout = CustomFoldout( m_bloomWeightsFoldout, m_bloomWeightsFoldoutGC );
						if ( m_bloomWeightsFoldout )
						{
							EditorGUI.indentLevel++;

							float blurStepSize = 15;
							float blurRadiusSize = 15;
							float weightSize = 25;

							GUILayout.BeginHorizontal();
							GUILayout.Space( 41 );
							EditorGUILayout.LabelField( m_blurStepGC, GUILayout.MinWidth( blurStepSize ) );
							GUILayout.Space( -26 );
							EditorGUILayout.LabelField( m_blurRadiusGC, GUILayout.MinWidth( blurRadiusSize ) );
							GUILayout.Space( -27 );
							EditorGUILayout.LabelField( m_upscaleWeightGC, GUILayout.MinWidth( weightSize ) );
							GUILayout.EndHorizontal();
							for ( int i = 0; i < weightMaxDowsampleCount; i++ )
							{
								GUILayout.BeginHorizontal();
								EditorGUILayout.LabelField( m_bloomWeightsLabelGCArr[ i ], GUILayout.Width( 65 ) );
								GUILayout.Space( -30 );

								bloom.GaussianSteps[ i ] = EditorGUILayout.IntField( string.Empty, bloom.GaussianSteps[ i ], GUILayout.MinWidth( blurStepSize ) );
								bloom.GaussianSteps[ i ] = Mathf.Clamp( bloom.GaussianSteps[ i ], 0, AmplifyBloomBase.MaxGaussian );

								GUILayout.Space( -27 );
								bloom.GaussianRadius[ i ] = EditorGUILayout.FloatField( string.Empty, bloom.GaussianRadius[ i ], GUILayout.MinWidth( blurRadiusSize ) );
								bloom.GaussianRadius[ i ] = Mathf.Max( bloom.GaussianRadius[ i ], 0f );

								GUILayout.Space( -27 );
								int id = weightMaxDowsampleCount - 1 - i;
								bloom.UpscaleWeights[ id ] = EditorGUILayout.FloatField( string.Empty, bloom.UpscaleWeights[ id ], GUILayout.MinWidth( weightSize ) );
								bloom.UpscaleWeights[ id ] = Mathf.Max( bloom.UpscaleWeights[ id ], 0f );

								GUILayout.EndHorizontal();
							}
							EditorGUI.indentLevel--;
						}

					}
					GUI.enabled = true;

					bool applyTemporalFilter = bloom.TemporalFilteringActive;
					ToggleFoldout( m_temporalFilterFoldoutGC, ref m_temporalFilterFoldout, ref applyTemporalFilter, false, 4 );
					bloom.TemporalFilteringActive = applyTemporalFilter;
					if ( m_temporalFilterFoldout )
					{
						GUI.enabled = bloom.TemporalFilteringActive;
						{
							bloom.TemporalFilteringCurve = EditorGUILayout.CurveField( m_filterCurveGC, bloom.TemporalFilteringCurve, TemporalCurveColor, TemporalCurveRanges );
							bloom.TemporalFilteringValue = EditorGUILayout.Slider( m_filterValueGC, bloom.TemporalFilteringValue, 0.01f, 1f );
						}
						GUI.enabled = true;
					}

					bool applySeparateFeatures = bloom.SeparateFeaturesThreshold;
					ToggleFoldout( m_sepFeaturesThresholdFoldoutGC, ref m_featuresFoldout, ref applySeparateFeatures, false, 4 );
					bloom.SeparateFeaturesThreshold = applySeparateFeatures;

					if ( m_featuresFoldout )
					{
						GUI.enabled = bloom.SeparateFeaturesThreshold;
						{
							bloom.FeaturesThreshold = EditorGUILayout.FloatField( m_separateThresholdGC, bloom.FeaturesThreshold );
						}
						GUI.enabled = true;
					}
					EditorGUI.indentLevel--;

				}

				bool applyLensDirt = bloom.ApplyLensDirt;
				ToggleFoldout( m_lensDirtFoldoutGC, ref m_lensDirtFoldout, ref applyLensDirt, true );
				bloom.ApplyLensDirt = applyLensDirt;
				if ( m_lensDirtFoldout )
				{
					GUI.enabled = bloom.ApplyLensDirt;
					bloom.LensDirtStrength = EditorGUILayout.FloatField( m_lensDirtIntensityGC, bloom.LensDirtStrength );


					EditorGUI.indentLevel++;
					m_lensDirtWeightsFoldout = CustomFoldout( m_lensDirtWeightsFoldout, m_lensWeightsFoldoutGC );
					if ( m_lensDirtWeightsFoldout )
					{
						for ( int i = 0; i < bloom.BloomDownsampleCount; i++ )
						{
							int id = bloom.BloomDownsampleCount - 1 - i;
							bloom.LensDirtWeights[ id ] = EditorGUILayout.FloatField( m_lensWeightGCArr[ i ], bloom.LensDirtWeights[ id ] );
							bloom.LensDirtWeights[ id ] = Mathf.Max( bloom.LensDirtWeights[ id ], 0f );
						}
					}
					EditorGUI.indentLevel--;
					bloom.LensDirtTexture = EditorGUILayout.ObjectField( m_lensDirtTextureGC, bloom.LensDirtTexture, typeof( Texture ), false ) as Texture;
					GUI.enabled = true;
				}

				bool applyStarburst = bloom.ApplyLensStardurst;
				ToggleFoldout( m_lensStarburstFoldoutGC, ref m_lensStarburstFoldout, ref applyStarburst, true );
				bloom.ApplyLensStardurst = applyStarburst;
				if ( m_lensStarburstFoldout )
				{
					GUI.enabled = bloom.ApplyLensStardurst;
					{
						bloom.LensStarburstStrength = EditorGUILayout.FloatField( m_lensStarburstIntensityGC, bloom.LensStarburstStrength );
						EditorGUI.indentLevel++;
						m_lensStarburstWeightsFoldout = CustomFoldout( m_lensStarburstWeightsFoldout, m_lensWeightsFoldoutGC );
						if ( m_lensStarburstWeightsFoldout )
						{
							for ( int i = 0; i < bloom.BloomDownsampleCount; i++ )
							{
								int id = bloom.BloomDownsampleCount - 1 - i;
								bloom.LensStarburstWeights[ id ] = EditorGUILayout.FloatField( m_lensWeightGCArr[ i ], bloom.LensStarburstWeights[ id ] );
								bloom.LensStarburstWeights[ id ] = Mathf.Max( bloom.LensStarburstWeights[ id ], 0f );
							}

						}
						EditorGUI.indentLevel--;
						bloom.LensStardurstTex = EditorGUILayout.ObjectField( m_lensStarburstTextureGC, bloom.LensStardurstTex, typeof( Texture ), false ) as Texture;
					}
					GUI.enabled = true;
				}

				bool applyBokeh = bloom.BokehFilterInstance.ApplyBokeh;
				ToggleFoldout( m_bokehFilterFoldoutGC, ref m_bokehFilterFoldout, ref applyBokeh, true );
				bloom.BokehFilterInstance.ApplyBokeh = applyBokeh;
				if ( m_bokehFilterFoldout )
				{
					GUI.enabled = bloom.BokehFilterInstance.ApplyBokeh;
					{
						bloom.BokehFilterInstance.ApplyOnBloomSource = EditorGUILayout.Toggle( m_bokehApplyOnBloomSourceGC, bloom.BokehFilterInstance.ApplyOnBloomSource );
						bloom.BokehFilterInstance.ApertureShape = ( ApertureShape ) EditorGUILayout.EnumPopup( m_bokehApertureShapeGC, bloom.BokehFilterInstance.ApertureShape );
						EditorGUI.indentLevel++;
						m_bokehAdvancedSettingsFoldout = CustomFoldout( m_bokehAdvancedSettingsFoldout, m_advancedSettingsBokehFoldoutGC );
						if ( m_bokehAdvancedSettingsFoldout )
						{
							bloom.BokehFilterInstance.OffsetRotation = EditorGUILayout.Slider( m_bokehRotationGC, bloom.BokehFilterInstance.OffsetRotation, 0, 360 );
							bloom.BokehFilterInstance.BokehSampleRadius = EditorGUILayout.Slider( m_bokehSampleRadiusGC, bloom.BokehFilterInstance.BokehSampleRadius, 0.01f, 1f );
							bloom.BokehFilterInstance.Aperture = EditorGUILayout.Slider( m_bokehApertureGC, bloom.BokehFilterInstance.Aperture, 0.01f, 0.05f );
							bloom.BokehFilterInstance.FocalLength = EditorGUILayout.Slider( m_bokehFocalLengthGC, bloom.BokehFilterInstance.FocalLength, 0.018f, 0.055f );
							bloom.BokehFilterInstance.FocalDistance = EditorGUILayout.Slider( m_bokehFocalDistanceGC, bloom.BokehFilterInstance.FocalDistance, 0.055f, 3f );
							bloom.BokehFilterInstance.MaxCoCDiameter = EditorGUILayout.Slider( m_bokehMaxCoCDiameterGC, bloom.BokehFilterInstance.MaxCoCDiameter, 0f, 2f );
						}
						EditorGUI.indentLevel--;
					}
					GUI.enabled = true;
				}

				bool applyLensFlare = bloom.LensFlareInstance.ApplyLensFlare;
				ToggleFoldout( m_lensFlareFoldoutGC, ref m_lensFlareFoldout, ref applyLensFlare, true );
				bloom.LensFlareInstance.ApplyLensFlare = applyLensFlare;
				if ( m_lensFlareFoldout )
				{
					GUI.enabled = bloom.LensFlareInstance.ApplyLensFlare;
					{
						bloom.LensFlareInstance.OverallIntensity = EditorGUILayout.FloatField( m_lensFlareIntensityGC, bloom.LensFlareInstance.OverallIntensity );
						bloom.LensFlareInstance.LensFlareGaussianBlurAmount = EditorGUILayout.IntSlider( m_lensFlareBlurAmountGC, bloom.LensFlareInstance.LensFlareGaussianBlurAmount, 0, 3 );

						EditorGUI.BeginChangeCheck();
						SerializedProperty gradientField = bloomObj.FindProperty( "m_lensFlare.m_lensGradient" );
						EditorGUILayout.PropertyField( gradientField, m_lensFlareRadialTintGC );
						if ( EditorGUI.EndChangeCheck() )
						{
							bloomObj.ApplyModifiedProperties();
							bloom.LensFlareInstance.TextureFromGradient();
						}

						EditorGUI.indentLevel++;
						m_ghostsFoldout = CustomFoldout( m_ghostsFoldout, m_ghostsFoldoutGC );
						if ( m_ghostsFoldout )
						{
							bloom.LensFlareInstance.LensFlareNormalizedGhostsIntensity = EditorGUILayout.FloatField( m_lensFlareGhostsInstensityGC, bloom.LensFlareInstance.LensFlareNormalizedGhostsIntensity );
							bloom.LensFlareInstance.LensFlareGhostAmount = EditorGUILayout.IntSlider( m_lensFlareGhostAmountGC, bloom.LensFlareInstance.LensFlareGhostAmount, 0, AmplifyBloomBase.MaxGhosts );
							bloom.LensFlareInstance.LensFlareGhostsDispersal = EditorGUILayout.Slider( m_lensFlareGhostDispersalGC, bloom.LensFlareInstance.LensFlareGhostsDispersal, 0.01f, 1.0f );
							bloom.LensFlareInstance.LensFlareGhostChrDistortion = EditorGUILayout.Slider( m_lensFlareGhostChrmDistortGC, bloom.LensFlareInstance.LensFlareGhostChrDistortion, 0, 10 );
							EditorGUI.indentLevel++;
							m_ghostsAdvancedSettingsFoldout = CustomFoldout( m_ghostsAdvancedSettingsFoldout, m_advancedSettingsGhostsFoldoutGC ,19);
							if ( m_ghostsAdvancedSettingsFoldout )
							{
								bloom.LensFlareInstance.LensFlareGhostsPowerFactor = EditorGUILayout.Slider( m_lensFlareGhostPowerFactorGC, bloom.LensFlareInstance.LensFlareGhostsPowerFactor, 0, 2 );
								bloom.LensFlareInstance.LensFlareGhostsPowerFalloff = EditorGUILayout.Slider( m_lensFlareGhostPowerFalloffGC, bloom.LensFlareInstance.LensFlareGhostsPowerFalloff, 1, 128 );
							}
							EditorGUI.indentLevel--;
						}

						m_haloFoldout = CustomFoldout( m_haloFoldout, m_halosFoldoutGC );
						if ( m_haloFoldout )
						{
							bloom.LensFlareInstance.LensFlareNormalizedHaloIntensity = EditorGUILayout.FloatField( m_lensFlareHalosIntensityGC, bloom.LensFlareInstance.LensFlareNormalizedHaloIntensity );
							bloom.LensFlareInstance.LensFlareHaloWidth = EditorGUILayout.Slider( m_lensFlareHaloWidthGC, bloom.LensFlareInstance.LensFlareHaloWidth, 0, 1 );
							bloom.LensFlareInstance.LensFlareHaloChrDistortion = EditorGUILayout.Slider( m_lensFlareHaloChrmDistGC, bloom.LensFlareInstance.LensFlareHaloChrDistortion, 0, 10 );
							EditorGUI.indentLevel++;
							m_haloAdvancedSettingsFoldout = CustomFoldout( m_haloAdvancedSettingsFoldout, m_advancedSettingsHalosFoldoutGC ,19);
							if ( m_haloAdvancedSettingsFoldout )
							{
								bloom.LensFlareInstance.LensFlareHaloPowerFactor = EditorGUILayout.Slider( m_lensFlareHaloPowerFactorGC, bloom.LensFlareInstance.LensFlareHaloPowerFactor, 1, 2 );
								bloom.LensFlareInstance.LensFlareHaloPowerFalloff = EditorGUILayout.Slider( m_lensFlareHaloPowerFalloffGC, bloom.LensFlareInstance.LensFlareHaloPowerFalloff, 1, 128 );
							}
							EditorGUI.indentLevel--;
						}
						EditorGUI.indentLevel--;
					}
					GUI.enabled = true;
				}

				bool applyGlare = bloom.LensGlareInstance.ApplyLensGlare;
				ToggleFoldout( m_lensGlareFoldoutGC, ref m_lensGlareFoldout, ref applyGlare, true );
				bloom.LensGlareInstance.ApplyLensGlare = applyGlare;
				if ( m_lensGlareFoldout )
				{
					GUI.enabled = bloom.LensGlareInstance.ApplyLensGlare;
					{
						bloom.LensGlareInstance.Intensity = EditorGUILayout.FloatField( m_lensGlareIntensityGC, bloom.LensGlareInstance.Intensity );
						bloom.LensGlareInstance.OverallStreakScale = EditorGUILayout.Slider( m_lensGlareOverallStreakScaleGC, bloom.LensGlareInstance.OverallStreakScale, 0, 2 );
						bloom.LensGlareInstance.OverallTint = EditorGUILayout.ColorField( m_lensGlareOverallTintGC, bloom.LensGlareInstance.OverallTint );

						EditorGUI.BeginChangeCheck();
						SerializedProperty gradientField = bloomObj.FindProperty( "m_anamorphicGlare.m_cromaticAberrationGrad" );
						EditorGUILayout.PropertyField( gradientField, m_lensGlareTintAlongGlareGC );
						if ( EditorGUI.EndChangeCheck() )
						{
							bloomObj.ApplyModifiedProperties();
							bloom.LensGlareInstance.SetDirty();
						}

						bloom.LensGlareInstance.CurrentGlare = ( GlareLibType ) EditorGUILayout.EnumPopup( m_lensGlareTypeGC, bloom.LensGlareInstance.CurrentGlare );
						if ( bloom.LensGlareInstance.CurrentGlare == GlareLibType.Custom )
						{
							EditorGUI.indentLevel++;
							bloom.LensGlareInstance.CustomGlareDefAmount = EditorGUILayout.IntSlider( m_customGlareSizeGC, bloom.LensGlareInstance.CustomGlareDefAmount, 0, AmplifyGlare.MaxCustomGlare );
							if ( bloom.LensGlareInstance.CustomGlareDefAmount > 0 )
							{
								bloom.LensGlareInstance.CustomGlareDefIdx = EditorGUILayout.IntSlider( m_customGlareIdxGC, bloom.LensGlareInstance.CustomGlareDefIdx, 0, bloom.LensGlareInstance.CustomGlareDef.Length - 1 );
								for ( int i = 0; i < bloom.LensGlareInstance.CustomGlareDef.Length; i++ )
								{
									EditorGUI.indentLevel++;
									m_customGlareFoldoutGC.text = "[" + i + "] " + bloom.LensGlareInstance.CustomGlareDef[ i ].CustomStarData.StarName;
									bloom.LensGlareInstance.CustomGlareDef[ i ].FoldoutValue = CustomFoldout( bloom.LensGlareInstance.CustomGlareDef[ i ].FoldoutValue, m_customGlareFoldoutGC );
									if ( bloom.LensGlareInstance.CustomGlareDef[ i ].FoldoutValue )
									{
										bloom.LensGlareInstance.CustomGlareDef[ i ].CustomStarData.StarName = EditorGUILayout.TextField( m_customGlareNameGC, bloom.LensGlareInstance.CustomGlareDef[ i ].CustomStarData.StarName );
										bloom.LensGlareInstance.CustomGlareDef[ i ].StarInclinationDeg = EditorGUILayout.Slider( m_customGlareStarInclinationGC, bloom.LensGlareInstance.CustomGlareDef[ i ].StarInclinationDeg, 0, 180 );
										bloom.LensGlareInstance.CustomGlareDef[ i ].ChromaticAberration = EditorGUILayout.Slider( m_customGlareChromaticAberrationGC, bloom.LensGlareInstance.CustomGlareDef[ i ].ChromaticAberration, 0, 1 );
										bloom.LensGlareInstance.CustomGlareDef[ i ].CustomStarData.StarlinesCount = EditorGUILayout.IntSlider( m_customGlareStarlinesCountGC, bloom.LensGlareInstance.CustomGlareDef[ i ].CustomStarData.StarlinesCount, 1, AmplifyGlare.MaxStarLines );
										bloom.LensGlareInstance.CustomGlareDef[ i ].CustomStarData.PassCount = EditorGUILayout.IntSlider( m_customGlarePassCountGC, bloom.LensGlareInstance.CustomGlareDef[ i ].CustomStarData.PassCount, 1, AmplifyGlare.MaxPasses );
										bloom.LensGlareInstance.CustomGlareDef[ i ].CustomStarData.SampleLength = EditorGUILayout.Slider( m_customGlareSampleLengthGC, bloom.LensGlareInstance.CustomGlareDef[ i ].CustomStarData.SampleLength, 0, 2 );
										bloom.LensGlareInstance.CustomGlareDef[ i ].CustomStarData.Attenuation = EditorGUILayout.Slider( m_customGlareAttenuationGC, bloom.LensGlareInstance.CustomGlareDef[ i ].CustomStarData.Attenuation, 0, 1 );
										bloom.LensGlareInstance.CustomGlareDef[ i ].CustomStarData.CameraRotInfluence = EditorGUILayout.FloatField( m_customGlareRotationGC, bloom.LensGlareInstance.CustomGlareDef[ i ].CustomStarData.CameraRotInfluence ); ;
										bloom.LensGlareInstance.CustomGlareDef[ i ].CustomStarData.CustomIncrement = EditorGUILayout.Slider( m_customGlareCustomIncrementGC, bloom.LensGlareInstance.CustomGlareDef[ i ].CustomStarData.CustomIncrement, 0, 180 );
										bloom.LensGlareInstance.CustomGlareDef[ i ].CustomStarData.LongAttenuation = EditorGUILayout.Slider( m_customGlareLongAttenuationGC, bloom.LensGlareInstance.CustomGlareDef[ i ].CustomStarData.LongAttenuation, 0, 1 );
									}
									EditorGUI.indentLevel--;
								}
							}
							EditorGUI.indentLevel--;
						}

						EditorGUI.indentLevel++;
						m_lensGlareAdvancedSettingsFoldout = CustomFoldout( m_lensGlareAdvancedSettingsFoldout, m_advancedSettingsLensGlareFoldoutGC );
						if ( m_lensGlareAdvancedSettingsFoldout )
						{
							bloom.LensGlareInstance.PerPassDisplacement = EditorGUILayout.Slider( m_lensGlarePerPassDispGC, bloom.LensGlareInstance.PerPassDisplacement, 1, 8 );
							bloom.LensGlareInstance.GlareMaxPassCount = EditorGUILayout.IntSlider( m_lensGlareMaxPerRayGC, bloom.LensGlareInstance.GlareMaxPassCount, 1, AmplifyGlare.MaxPasses );

						}
						EditorGUI.indentLevel--;
					}
					GUI.enabled = true;
				}

				if ( EditorGUI.EndChangeCheck() )
				{
					EditorUtility.SetDirty( bloom );
#if !UNITY_PRE_5_3
					if ( !Application.isPlaying )
					{
						EditorSceneManager.MarkSceneDirty( bloom.gameObject.scene );
					}
#endif
				}
			}
			GUILayout.EndVertical();
		}
	}
}
