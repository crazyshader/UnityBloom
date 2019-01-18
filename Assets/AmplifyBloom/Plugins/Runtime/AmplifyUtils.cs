// Amplify Bloom - Advanced Bloom Post-Effect for Unity
// Copyright (c) Amplify Creations, Lda <info@amplify.pt>

using UnityEngine;
using System.Collections.Generic;

namespace AmplifyBloom
{
	public enum LogType
	{
		Normal,
		Warning,
		Error
	}

	public class AmplifyUtils
	{
		public static int MaskTextureId;
		public static int BlurRadiusId;
		public static string HighPrecisionKeyword = "AB_HIGH_PRECISION";
		public static string ShaderModeTag = "Mode";
		public static string ShaderModeValue = "Full";
		public static string DebugStr = "[AmplifyBloom] ";
		public static int UpscaleContributionId;
		public static int SourceContributionId;
		public static int LensStarburstRTId;
		public static int LensDirtRTId;
		public static int LensFlareRTId;
		public static int LensGlareRTId;
		public static int[] MipResultsRTS;
		public static int[] AnamorphicRTS;
		public static int[] AnamorphicGlareWeightsMatStr;
		public static int[] AnamorphicGlareOffsetsMatStr;
		public static int[] AnamorphicGlareWeightsStr;
		public static int[] UpscaleWeightsStr;
		public static int[] LensDirtWeightsStr;
		public static int[] LensStarburstWeightsStr;
		public static int BloomRangeId;
		public static int LensDirtStrengthId;
		public static int BloomParamsId;
		public static int TempFilterValueId;
		public static int LensFlareStarMatrixId;
		public static int LensFlareStarburstStrengthId;
		public static int LensFlareGhostsParamsId;
		public static int LensFlareLUTId;
		public static int LensFlareHaloParamsId;
		public static int LensFlareGhostChrDistortionId;
		public static int LensFlareHaloChrDistortionId;
		public static int BokehParamsId = -1;
		public static RenderTextureFormat CurrentRTFormat = RenderTextureFormat.DefaultHDR;
		public static FilterMode CurrentFilterMode = FilterMode.Bilinear;
		public static TextureWrapMode CurrentWrapMode = TextureWrapMode.Clamp;
		public static RenderTextureReadWrite CurrentReadWriteMode = RenderTextureReadWrite.sRGB;
		public static bool IsInitialized = false;

		private static List<RenderTexture> _allocatedRT = new List<RenderTexture>();
		public static void InitializeIds()
		{
			IsInitialized = true;
			MaskTextureId = Shader.PropertyToID( "_MaskTex" );

			MipResultsRTS = new int[]{  Shader.PropertyToID( "_MipResultsRTS0" ),
										Shader.PropertyToID( "_MipResultsRTS1" ),
										Shader.PropertyToID( "_MipResultsRTS2" ),
										Shader.PropertyToID( "_MipResultsRTS3" ),
										Shader.PropertyToID( "_MipResultsRTS4" ),
										Shader.PropertyToID( "_MipResultsRTS5" )};

			AnamorphicRTS = new int[]{  Shader.PropertyToID( "_AnamorphicRTS0" ),
										Shader.PropertyToID( "_AnamorphicRTS1" ),
										Shader.PropertyToID( "_AnamorphicRTS2" ),
										Shader.PropertyToID( "_AnamorphicRTS3" ),
										Shader.PropertyToID( "_AnamorphicRTS4" ),
										Shader.PropertyToID( "_AnamorphicRTS5" ),
										Shader.PropertyToID( "_AnamorphicRTS6" ),
										Shader.PropertyToID( "_AnamorphicRTS7" )};

			AnamorphicGlareWeightsMatStr = new int[]{   Shader.PropertyToID( "_AnamorphicGlareWeightsMat0" ),
														Shader.PropertyToID( "_AnamorphicGlareWeightsMat1" ),
														Shader.PropertyToID( "_AnamorphicGlareWeightsMat2" ),
														Shader.PropertyToID( "_AnamorphicGlareWeightsMat3" )};


			AnamorphicGlareOffsetsMatStr = new int[]{   Shader.PropertyToID( "_AnamorphicGlareOffsetsMat0" ),
														Shader.PropertyToID( "_AnamorphicGlareOffsetsMat1" ),
														Shader.PropertyToID( "_AnamorphicGlareOffsetsMat2" ),
														Shader.PropertyToID( "_AnamorphicGlareOffsetsMat3" )};

			AnamorphicGlareWeightsStr = new int[]{  Shader.PropertyToID( "_AnamorphicGlareWeights0" ),
													Shader.PropertyToID( "_AnamorphicGlareWeights1" ),
													Shader.PropertyToID( "_AnamorphicGlareWeights2" ),
													Shader.PropertyToID( "_AnamorphicGlareWeights3" ),
													Shader.PropertyToID( "_AnamorphicGlareWeights4" ),
													Shader.PropertyToID( "_AnamorphicGlareWeights5" ),
													Shader.PropertyToID( "_AnamorphicGlareWeights6" ),
													Shader.PropertyToID( "_AnamorphicGlareWeights7" ),
													Shader.PropertyToID( "_AnamorphicGlareWeights8" ),
													Shader.PropertyToID( "_AnamorphicGlareWeights9" ),
													Shader.PropertyToID( "_AnamorphicGlareWeights10" ),
													Shader.PropertyToID( "_AnamorphicGlareWeights11" ),
													Shader.PropertyToID( "_AnamorphicGlareWeights12" ),
													Shader.PropertyToID( "_AnamorphicGlareWeights13" ),
													Shader.PropertyToID( "_AnamorphicGlareWeights14" ),
													Shader.PropertyToID( "_AnamorphicGlareWeights15" )};

			UpscaleWeightsStr = new int[]{          Shader.PropertyToID( "_UpscaleWeights0" ),
													Shader.PropertyToID( "_UpscaleWeights1" ),
													Shader.PropertyToID( "_UpscaleWeights2" ),
													Shader.PropertyToID( "_UpscaleWeights3" ),
													Shader.PropertyToID( "_UpscaleWeights4" ),
													Shader.PropertyToID( "_UpscaleWeights5" ),
													Shader.PropertyToID( "_UpscaleWeights6" ),
													Shader.PropertyToID( "_UpscaleWeights7" )};

			LensDirtWeightsStr = new int[]{		    Shader.PropertyToID( "_LensDirtWeights0" ),
													Shader.PropertyToID( "_LensDirtWeights1" ),
													Shader.PropertyToID( "_LensDirtWeights2" ),
													Shader.PropertyToID( "_LensDirtWeights3" ),
													Shader.PropertyToID( "_LensDirtWeights4" ),
													Shader.PropertyToID( "_LensDirtWeights5" ),
													Shader.PropertyToID( "_LensDirtWeights6" ),
													Shader.PropertyToID( "_LensDirtWeights7" )};

			LensStarburstWeightsStr= new int[]{     Shader.PropertyToID( "_LensStarburstWeights0" ),
													Shader.PropertyToID( "_LensStarburstWeights1" ),
													Shader.PropertyToID( "_LensStarburstWeights2" ),
													Shader.PropertyToID( "_LensStarburstWeights3" ),
													Shader.PropertyToID( "_LensStarburstWeights4" ),
													Shader.PropertyToID( "_LensStarburstWeights5" ),
													Shader.PropertyToID( "_LensStarburstWeights6" ),
													Shader.PropertyToID( "_LensStarburstWeights7" )};

			BloomRangeId = Shader.PropertyToID( "_BloomRange" );
			LensDirtStrengthId = Shader.PropertyToID( "_LensDirtStrength" );
			BloomParamsId = Shader.PropertyToID( "_BloomParams" );
			TempFilterValueId = Shader.PropertyToID( "_TempFilterValue" );
			LensFlareStarMatrixId = Shader.PropertyToID( "_LensFlareStarMatrix" );
			LensFlareStarburstStrengthId = Shader.PropertyToID( "_LensFlareStarburstStrength" );
			LensFlareGhostsParamsId = Shader.PropertyToID( "_LensFlareGhostsParams" );
			LensFlareLUTId = Shader.PropertyToID( "_LensFlareLUT" );
			LensFlareHaloParamsId = Shader.PropertyToID( "_LensFlareHaloParams" );
			LensFlareGhostChrDistortionId = Shader.PropertyToID( "_LensFlareGhostChrDistortion" );
			LensFlareHaloChrDistortionId = Shader.PropertyToID( "_LensFlareHaloChrDistortion" );
			BokehParamsId = Shader.PropertyToID( "_BokehParams" );
			BlurRadiusId = Shader.PropertyToID( "_BlurRadius" );
			LensStarburstRTId = Shader.PropertyToID( "_LensStarburst" );
			LensDirtRTId = Shader.PropertyToID( "_LensDirt" );
			LensFlareRTId = Shader.PropertyToID( "_LensFlare" );
			LensGlareRTId = Shader.PropertyToID( "_LensGlare" );
			SourceContributionId = Shader.PropertyToID( "_SourceContribution" );
			UpscaleContributionId = Shader.PropertyToID( "_UpscaleContribution" );
		}

		public static void DebugLog( string value , LogType type )
		{
			switch ( type )
			{
				case LogType.Normal: Debug.Log( DebugStr + value ); break;
				case LogType.Warning: Debug.LogWarning( DebugStr + value ); break;
				case LogType.Error: Debug.LogError( DebugStr + value ); break;
			}
		}

		public static RenderTexture GetTempRenderTarget( int width, int height )
		{
			RenderTexture newRT = RenderTexture.GetTemporary( width, height, 0, CurrentRTFormat , CurrentReadWriteMode );
			newRT.filterMode = CurrentFilterMode;
			newRT.wrapMode = CurrentWrapMode;
			_allocatedRT.Add( newRT );
			return newRT;
		}

		public static void ReleaseTempRenderTarget( RenderTexture renderTarget )
		{
			if ( renderTarget != null && _allocatedRT.Remove( renderTarget ) )
			{
				renderTarget.DiscardContents();
				RenderTexture.ReleaseTemporary( renderTarget );
			}
		}

		public static void ReleaseAllRT()
		{
			for ( int i = 0; i < _allocatedRT.Count; i++ )
			{
				_allocatedRT[ i ].DiscardContents();
				RenderTexture.ReleaseTemporary( _allocatedRT[ i ] );
			}
			_allocatedRT.Clear();
		}

		public static void EnsureKeywordEnabled( Material mat, string keyword, bool state )
		{
			if ( mat != null )
			{
				if ( state && !mat.IsKeywordEnabled( keyword ) )
					mat.EnableKeyword( keyword );
				else if ( !state && mat.IsKeywordEnabled( keyword ) )
					mat.DisableKeyword( keyword );
			}
		}
	}
}
