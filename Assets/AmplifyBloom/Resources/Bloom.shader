// Amplify Bloom - Advanced Bloom Post-Effect for Unity
// Copyright (c) Amplify Creations, Lda <info@amplify.pt>

Shader "Hidden/AmplifyBloom"
{
	Properties
	{
		_MainTex ( " ", 2D ) = "black" {}
		_AnamorphicRTS0 ( " ",2D ) = "black"{}
		_AnamorphicRTS1 ( " ",2D ) = "black"{}
		_AnamorphicRTS2 ( " ",2D ) = "black"{}
		_AnamorphicRTS3 ( " ",2D ) = "black"{}
		_AnamorphicRTS4 ( " ",2D ) = "black"{}
		_AnamorphicRTS5 ( " ",2D ) = "black"{}
		_AnamorphicRTS6 ( " ",2D ) = "black"{}
		_AnamorphicRTS7 ( " ",2D ) = "black"{}
		_LensFlareLUT( " ",2D ) = "black"{}
	}

	CGINCLUDE
		#pragma target 3.0

		#include "UnityCG.cginc"
		#include "BloomLib.cginc"
		#pragma multi_compile __ AB_HIGH_PRECISION

		uniform float4		_MainTex_TexelSize;// x - 1/width y - 1/height z- width w - height

		struct v2f_img_custom
		{
			float4 pos : SV_POSITION;
			float2 uv   : TEXCOORD0;
	#if UNITY_UV_STARTS_AT_TOP
			float4 uv2 : TEXCOORD1;
	#endif
		};

		v2f_img_custom vert_img_custom ( appdata_img v )
		{
			v2f_img_custom o;

			o.pos = CustomObjectToClipPos( v.vertex );
			o.uv = float4( v.texcoord.xy, 1, 1 );

	#ifdef UNITY_HALF_TEXEL_OFFSET
			o.uv.y += _MainTex_TexelSize.y;
	#endif

	#if UNITY_UV_STARTS_AT_TOP
			o.uv2 = float4( v.texcoord.xy, 1, 1 );
			if ( _MainTex_TexelSize.y < 0.0 )
				o.uv.y = 1.0 - o.uv.y;
	#endif

			return o;
		}

	#include "BloomFrag.cginc"

	ENDCG

	SubShader
	{
		ZTest Always Cull Off ZWrite Off

		Pass//0
		{
			Name "frag_threshold"

			CGPROGRAM
			#pragma vertex vert_img
			#pragma fragment frag_threshold
			ENDCG
		}

		Pass//1
		{
			Name "frag_thresholdMask"

			CGPROGRAM
			#pragma vertex vert_img
			#pragma fragment frag_thresholdMask
			ENDCG
		}

		Pass//2
		{
			Name "frag_anamorphicGlare"
			CGPROGRAM
			#pragma vertex vert_img
			#pragma fragment frag_anamorphicGlare
			ENDCG
		}

		Pass//3
		{
			Name "frag_lensFlare0"
			CGPROGRAM
			#pragma vertex vert_img
			#pragma fragment frag_lensFlare0
			ENDCG
		}

		Pass//4
		{
			Name "frag_lensFlare1"
			CGPROGRAM
			#pragma vertex vert_img
			#pragma fragment frag_lensFlare1
			ENDCG
		}

		Pass//5
		{
			Name "frag_lensFlare2"
			CGPROGRAM
			#pragma vertex vert_img
			#pragma fragment frag_lensFlare2
			ENDCG
		}

		Pass//6
		{
			Name "frag_lensFlare3"
			CGPROGRAM
			#pragma vertex vert_img
			#pragma fragment frag_lensFlare3
			ENDCG
		}

		Pass//7
		{
			Name "frag_lensFlare4"
			CGPROGRAM
			#pragma vertex vert_img
			#pragma fragment frag_lensFlare4
			ENDCG
		}

		Pass//8
		{
			Name "frag_lensFlare5"
			CGPROGRAM
			#pragma vertex vert_img
			#pragma fragment frag_lensFlare5
			ENDCG
		}

		Pass//9
		{
			Name "frag_downsamplerNoWeightedAvg"
			CGPROGRAM
			#pragma vertex vert_img
			#pragma fragment frag_downsamplerNoWeightedAvg
			ENDCG
		}
		Pass//10
		{
			Name "frag_downsampler_with_karis"
			CGPROGRAM
			#pragma vertex vert_img
			#pragma fragment frag_downsampler_with_karis
			ENDCG
		}


		Pass//11
		{
			Name "frag_downsampler_without_karis"
			CGPROGRAM
			#pragma vertex vert_img
			#pragma fragment frag_downsampler_without_karis
			ENDCG
		}

		Pass//12
		{
			Name "frag_downsampler_temp_filter_with_karis"
			CGPROGRAM
			#pragma vertex vert_img
			#pragma fragment frag_downsampler_temp_filter_with_karis
			ENDCG
		}


		Pass//13
		{
			Name "frag_downsampler_temp_filter_without_karis"
			CGPROGRAM
			#pragma vertex vert_img
			#pragma fragment frag_downsampler_temp_filter_without_karis
			ENDCG
		}



		Pass//14
		{
			Name "frag_horizontal_gaussian_blur"
			CGPROGRAM
			#pragma vertex vert_img
			#pragma fragment frag_horizontal_gaussian_blur
			ENDCG
		}


		Pass//15
		{
			Name "frag_vertical_gaussian_blur"
			CGPROGRAM
			#pragma vertex vert_img
			#pragma fragment frag_vertical_gaussian_blur
			ENDCG
		}

		Pass//16
		{
			Name "frag_vertical_gaussian_blur_temp_filter"
			CGPROGRAM
			#pragma vertex vert_img
			#pragma fragment frag_vertical_gaussian_blur_temp_filter
			ENDCG
		}

		Pass//17
		{
			Name "frag_upscaleTentFirstPass"
			CGPROGRAM
			#pragma vertex vert_img
			#pragma fragment frag_upscaleTentFirstPass
			ENDCG
		}

		Pass//18
		{
			Name "frag_upscaleTent"
			CGPROGRAM
			#pragma vertex vert_img
			#pragma fragment frag_upscaleTent
			ENDCG
		}

		Pass//19
		{
			Name "frag_weightedAddPS1"
			CGPROGRAM
			#pragma vertex vert_img
			#pragma fragment frag_weightedAddPS1
			ENDCG
		}

		Pass//20
		{
			Name "frag_weightedAddPS2"
			CGPROGRAM
			#pragma vertex vert_img
			#pragma fragment frag_weightedAddPS2
			ENDCG
		}

		Pass//21
		{
			Name "frag_weightedAddPS3"
			CGPROGRAM
			#pragma vertex vert_img
			#pragma fragment frag_weightedAddPS3
			ENDCG
		}

		Pass//22
		{
			Name "frag_weightedAddPS4"
			CGPROGRAM
			#pragma vertex vert_img
			#pragma fragment frag_weightedAddPS4
			ENDCG
		}

		Pass//23
		{
			Name "frag_weightedAddPS5"
			CGPROGRAM
			#pragma vertex vert_img
			#pragma fragment frag_weightedAddPS5
			ENDCG
		}

		Pass//24
		{
			Name "frag_weightedAddPS6"
			CGPROGRAM
			#pragma vertex vert_img
			#pragma fragment frag_weightedAddPS6
			ENDCG
		}

		Pass//25
		{
			Name "frag_weightedAddPS7"
			CGPROGRAM
			#pragma vertex vert_img
			#pragma fragment frag_weightedAddPS7
			ENDCG
		}

		Pass//26
		{
			Name "frag_weightedAddPS8"
			CGPROGRAM
			#pragma vertex vert_img
			#pragma fragment frag_weightedAddPS8
			ENDCG
		}

		Pass//27
		{
			Name "frag_BokehFiltering"
			CGPROGRAM
			#pragma vertex vert_img
			#pragma fragment frag_BokehFiltering
			ENDCG
		}

		Pass//28
		{
			Name "frag_BokehComposition2S"
			CGPROGRAM
			#pragma vertex vert_img
			#pragma fragment frag_BokehComposition2S
			ENDCG
		}

		Pass//29
		{
			Name "frag_BokehComposition3S"
			CGPROGRAM
			#pragma vertex vert_img
			#pragma fragment frag_BokehComposition3S
			ENDCG
		}

		Pass//30
		{
			Name "frag_BokehComposition4S"
			CGPROGRAM
			#pragma vertex vert_img
			#pragma fragment frag_BokehComposition4S
			ENDCG
		}

		Pass//31
		{
			Name "frag_BokehComposition5S"
			CGPROGRAM
			#pragma vertex vert_img
			#pragma fragment frag_BokehComposition5S
			ENDCG
		}

		Pass//32
		{
			Name "frag_BokehComposition6S"
			CGPROGRAM
			#pragma vertex vert_img
			#pragma fragment frag_BokehComposition6S
			ENDCG
		}

		Pass//33
		{
			Name "frag_decode"
			CGPROGRAM
			#pragma vertex vert_img
			#pragma fragment frag_decode
			ENDCG
		}
	}
}
