// Amplify Bloom - Advanced Bloom Post-Effect for Unity
// Copyright (c) Amplify Creations, Lda <info@amplify.pt>

Shader "Hidden/BloomFinal"
{
	Properties
	{
		_LensDirt ( "Lens Dirt Texture",2D ) = "black"{}
		_LensStarburst ( "Lens Starburst Texture",2D ) = "black"{}
		_MainTex ( " ", 2D ) = "black" {}
		_LensFlare ( " ",2D ) = "black"{}
		_LensGlare ( " ",2D ) = "black"{}
		_MipResultsRTS0 ( " ",2D ) = "black"{}
		_MipResultsRTS1 ( " ",2D ) = "black"{}
		_MipResultsRTS2 ( " ",2D ) = "black"{}
		_MipResultsRTS3 ( " ",2D ) = "black"{}
		_MipResultsRTS4 ( " ",2D ) = "black"{}
		_MipResultsRTS5 ( " ",2D ) = "black"{}
	}

	CGINCLUDE
		#pragma vertex vert_img_custom
		#pragma fragment frag
		#pragma target 3.0

		#include "UnityCG.cginc"
		#include "BloomLib.cginc"
		#pragma multi_compile __  AB_HIGH_PRECISION
		uniform half4		_MainTex_TexelSize;// x - 1/width y - 1/height z- width w - height

		uniform sampler2D	_MainTex;
		uniform sampler2D	_MipResultsRTS0;
		uniform sampler2D	_MipResultsRTS1;
		uniform sampler2D	_MipResultsRTS2;
		uniform sampler2D	_MipResultsRTS3;
		uniform sampler2D	_MipResultsRTS4;
		uniform sampler2D	_MipResultsRTS5;

		uniform half		_UpscaleWeights0;
		uniform half		_UpscaleWeights1;
		uniform half		_UpscaleWeights2;
		uniform half		_UpscaleWeights3;
		uniform half		_UpscaleWeights4;
		uniform half		_UpscaleWeights5;

		uniform half		_LensStarburstWeights0;
		uniform half		_LensStarburstWeights1;
		uniform half		_LensStarburstWeights2;
		uniform half		_LensStarburstWeights3;
		uniform half		_LensStarburstWeights4;
		uniform half		_LensStarburstWeights5;

		uniform half		_LensDirtWeights0;
		uniform half		_LensDirtWeights1;
		uniform half		_LensDirtWeights2;
		uniform half		_LensDirtWeights3;
		uniform half		_LensDirtWeights4;
		uniform half		_LensDirtWeights5;

		uniform sampler2D	_LensDirt;
		uniform sampler2D	_LensStarburst;
		uniform sampler2D	_LensFlare;
		uniform sampler2D	_LensGlare;

		uniform half		_LensDirtStrength;
		uniform half4x4		_LensFlareStarMatrix;
		uniform half		_LensFlareStarburstStrength;

		uniform half _SourceContribution;
		uniform half _UpscaleContribution;
		uniform half4 _BloomParams;// x - overallIntensity y - threshold, z - blur radius w - bloom scale

		struct v2f_img_custom
		{
			float4 pos : SV_POSITION;
			half2 uv   : TEXCOORD0;
			half2 stereoUV : TEXCOORD2;
		#if UNITY_UV_STARTS_AT_TOP
			half4 uv2 : TEXCOORD1;
			half4 stereoUV2 : TEXCOORD3;
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
			o.stereoUV2 = UnityStereoScreenSpaceUVAdjust ( o.uv2, _MainTex_ST );

			if ( _MainTex_TexelSize.y < 0.0 )
				o.uv.y = 1.0 - o.uv.y;
		#endif
			o.stereoUV = UnityStereoScreenSpaceUVAdjust ( o.uv, _MainTex_ST );
			return o;
		}

		inline half3 CalculateStarburst ( half4 bloomColor, half2 uv ,half3 weightedStarburstColor )
		{
			half2 imageCenter = half2( 0.5, 0.5 );
			half4 starburstColor = tex2D ( _LensStarburst, uv );
			half2 starburstUV = uv - imageCenter;
			starburstUV = mul ( _LensFlareStarMatrix, half4( starburstUV, 0, 1 ) ).xy;
			starburstUV += imageCenter;
			starburstColor += tex2D ( _LensStarburst, starburstUV );
			bloomColor.rgb += weightedStarburstColor*starburstColor.rgb*_LensFlareStarburstStrength;
			return bloomColor;
		}

		inline half4 FinalComposition( v2f_img_custom input, const int count, const bool flare, const bool glare, const bool dirt, const bool starburst )
		{
#ifdef UNITY_UV_STARTS_AT_TOP
			half2 uv = input.uv2;
			half2 stereoUV = input.stereoUV2;
#else
			half2 uv = input.uv;
			half2 stereoUV = input.stereoUV;
#endif

			half3 upscaleColor = half3( 0, 0, 0 );
			half3 weightedDirtColor = half3( 0, 0, 0 );
			half3 weightedStarburstColor = half3( 0, 0, 0 );

			half3 b0 = 0;
			half3 b1 = 0;
			half3 b2 = 0;
			half3 b3 = 0;
			half3 b4 = 0;
			half3 b5 = 0;

			b0 =  DecodeColor ( tex2D ( _MipResultsRTS0, input.stereoUV ) );
			if ( count > 1 ) b1 = DecodeColor ( tex2D ( _MipResultsRTS1, input.stereoUV ) );
			if ( count > 2 ) b2 = DecodeColor ( tex2D ( _MipResultsRTS2, input.stereoUV ) );
			if ( count > 3 ) b3 = DecodeColor ( tex2D ( _MipResultsRTS3, input.stereoUV ) );
			if ( count > 4 ) b4 = DecodeColor ( tex2D ( _MipResultsRTS4, input.stereoUV ) );
			if ( count > 5 ) b5 = DecodeColor ( tex2D ( _MipResultsRTS5, input.stereoUV ) );

			upscaleColor = _UpscaleWeights0 * b0;
			if ( count > 1 ) upscaleColor += _UpscaleWeights1 * b1;
			if ( count > 2 ) upscaleColor += _UpscaleWeights2 * b2;
			if ( count > 3 ) upscaleColor += _UpscaleWeights3 * b3;
			if ( count > 4 ) upscaleColor += _UpscaleWeights4 * b4;
			if ( count > 5 ) upscaleColor += _UpscaleWeights5 * b5;

			if ( dirt )
			{
				weightedDirtColor = _LensDirtWeights0 * b0;
				if ( count > 1 ) weightedDirtColor += _LensDirtWeights1 * b1;
				if ( count > 2 ) weightedDirtColor += _LensDirtWeights2 * b2;
				if ( count > 3 ) weightedDirtColor += _LensDirtWeights3 * b3;
				if ( count > 4 ) weightedDirtColor += _LensDirtWeights4 * b4;
				if ( count > 5 ) weightedDirtColor += _LensDirtWeights5 * b5;
			}

			if ( starburst )
			{
				weightedStarburstColor = _LensStarburstWeights0 * b0;
				if ( count > 1 ) weightedStarburstColor += _LensStarburstWeights1 * b1;
				if ( count > 2 ) weightedStarburstColor += _LensStarburstWeights2 * b2;
				if ( count > 3 ) weightedStarburstColor += _LensStarburstWeights3 * b3;
				if ( count > 4 ) weightedStarburstColor += _LensStarburstWeights4 * b4;
				if ( count > 5 ) weightedStarburstColor += _LensStarburstWeights5 * b5;
			}

			half4 color = _SourceContribution*tex2D ( _MainTex, stereoUV );
			half4 originalUpscaleColor = half4( upscaleColor, 1 );
			half4 bloomColor = _UpscaleContribution*originalUpscaleColor*_BloomParams.x;

			if ( flare )
			{
				half4 lensFlareColor = tex2D ( _LensFlare, input.stereoUV );
				bloomColor += lensFlareColor;
				originalUpscaleColor += lensFlareColor;
			}

			if ( glare )
			{
				half4 lensGlareColor = tex2D ( _LensGlare, input.stereoUV );
				bloomColor += lensGlareColor;
				originalUpscaleColor += lensGlareColor;
			}

			if ( dirt )
				bloomColor.rgb += weightedDirtColor*_LensDirtStrength*tex2D ( _LensDirt, input.uv ).rgb;

			if ( starburst )
				bloomColor.rgb = CalculateStarburst ( bloomColor, input.uv, weightedStarburstColor );

			return half4( ( color + bloomColor ).rgb, color.a );
		}

	ENDCG

	// HIGH END DEVICES WITH MORE THAN 11 TEXTURE LOOKUP CAPABILITIES
	SubShader
	{
		ZTest Always Cull Off ZWrite Off
		Tags{ "Mode"="Full" }
		// count 1
		Pass{ CGPROGRAM half4 frag ( v2f_img_custom i ) : SV_Target { return FinalComposition( i, 1, false, false, false, false ); } ENDCG }
		Pass{ CGPROGRAM half4 frag ( v2f_img_custom i ) : SV_Target{ return FinalComposition ( i, 1, false, false, false, true ); } ENDCG }
		Pass{ CGPROGRAM half4 frag ( v2f_img_custom i ) : SV_Target{ return FinalComposition ( i, 1, false, false, true, false ); } ENDCG }
		Pass{ CGPROGRAM half4 frag ( v2f_img_custom i ) : SV_Target{ return FinalComposition ( i, 1, false, false, true, true ); } ENDCG }
		Pass{ CGPROGRAM half4 frag ( v2f_img_custom i ) : SV_Target{ return FinalComposition ( i, 1, false, true, false, false ); } ENDCG }
		Pass{ CGPROGRAM half4 frag ( v2f_img_custom i ) : SV_Target{ return FinalComposition ( i, 1, false, true, false, true ); } ENDCG }
		Pass{ CGPROGRAM half4 frag ( v2f_img_custom i ) : SV_Target{ return FinalComposition ( i, 1, false, true, true, false ); } ENDCG }
		Pass{ CGPROGRAM half4 frag ( v2f_img_custom i ) : SV_Target{ return FinalComposition ( i, 1, false, true, true, true ); } ENDCG }
		Pass{ CGPROGRAM half4 frag ( v2f_img_custom i ) : SV_Target{ return FinalComposition ( i, 1, true, false, false, false ); } ENDCG }
		Pass{ CGPROGRAM half4 frag ( v2f_img_custom i ) : SV_Target{ return FinalComposition ( i, 1, true, false, false, true ); } ENDCG }
		Pass{ CGPROGRAM half4 frag ( v2f_img_custom i ) : SV_Target{ return FinalComposition ( i, 1, true, false, true, false ); } ENDCG }
		Pass{ CGPROGRAM half4 frag ( v2f_img_custom i ) : SV_Target{ return FinalComposition ( i, 1, true, false, true, true ); } ENDCG }
		Pass{ CGPROGRAM half4 frag ( v2f_img_custom i ) : SV_Target{ return FinalComposition ( i, 1, true, true, false, false ); } ENDCG }
		Pass{ CGPROGRAM half4 frag ( v2f_img_custom i ) : SV_Target{ return FinalComposition ( i, 1, true, true, false, true ); } ENDCG }
		Pass{ CGPROGRAM half4 frag ( v2f_img_custom i ) : SV_Target{ return FinalComposition ( i, 1, true, true, true, false ); } ENDCG }
		Pass{ CGPROGRAM half4 frag ( v2f_img_custom i ) : SV_Target{ return FinalComposition ( i, 1, true, true, true, true ); } ENDCG }
		// count 2
		Pass{ CGPROGRAM half4 frag ( v2f_img_custom i ) : SV_Target{ return FinalComposition ( i, 2, false, false, false, false ); } ENDCG }
		Pass{ CGPROGRAM half4 frag ( v2f_img_custom i ) : SV_Target{ return FinalComposition ( i, 2, false, false, false, true ); } ENDCG }
		Pass{ CGPROGRAM half4 frag ( v2f_img_custom i ) : SV_Target{ return FinalComposition ( i, 2, false, false, true, false ); } ENDCG }
		Pass{ CGPROGRAM half4 frag ( v2f_img_custom i ) : SV_Target{ return FinalComposition ( i, 2, false, false, true, true ); } ENDCG }
		Pass{ CGPROGRAM half4 frag ( v2f_img_custom i ) : SV_Target{ return FinalComposition ( i, 2, false, true, false, false ); } ENDCG }
		Pass{ CGPROGRAM half4 frag ( v2f_img_custom i ) : SV_Target{ return FinalComposition ( i, 2, false, true, false, true ); } ENDCG }
		Pass{ CGPROGRAM half4 frag ( v2f_img_custom i ) : SV_Target{ return FinalComposition ( i, 2, false, true, true, false ); } ENDCG }
		Pass{ CGPROGRAM half4 frag ( v2f_img_custom i ) : SV_Target{ return FinalComposition ( i, 2, false, true, true, true ); } ENDCG }
		Pass{ CGPROGRAM half4 frag ( v2f_img_custom i ) : SV_Target{ return FinalComposition ( i, 2, true, false, false, false ); } ENDCG }
		Pass{ CGPROGRAM half4 frag ( v2f_img_custom i ) : SV_Target{ return FinalComposition ( i, 2, true, false, false, true ); } ENDCG }
		Pass{ CGPROGRAM half4 frag ( v2f_img_custom i ) : SV_Target{ return FinalComposition ( i, 2, true, false, true, false ); } ENDCG }
		Pass{ CGPROGRAM half4 frag ( v2f_img_custom i ) : SV_Target{ return FinalComposition ( i, 2, true, false, true, true ); } ENDCG }
		Pass{ CGPROGRAM half4 frag ( v2f_img_custom i ) : SV_Target{ return FinalComposition ( i, 2, true, true, false, false ); } ENDCG }
		Pass{ CGPROGRAM half4 frag ( v2f_img_custom i ) : SV_Target{ return FinalComposition ( i, 2, true, true, false, true ); } ENDCG }
		Pass{ CGPROGRAM half4 frag ( v2f_img_custom i ) : SV_Target{ return FinalComposition ( i, 2, true, true, true, false ); } ENDCG }
		Pass{ CGPROGRAM half4 frag ( v2f_img_custom i ) : SV_Target{ return FinalComposition ( i, 2, true, true, true, true ); } ENDCG }
		// count 3
		Pass{ CGPROGRAM half4 frag ( v2f_img_custom i ) : SV_Target{ return FinalComposition ( i, 3, false, false, false, false ); } ENDCG }
		Pass{ CGPROGRAM half4 frag ( v2f_img_custom i ) : SV_Target{ return FinalComposition ( i, 3, false, false, false, true ); } ENDCG }
		Pass{ CGPROGRAM half4 frag ( v2f_img_custom i ) : SV_Target{ return FinalComposition ( i, 3, false, false, true, false ); } ENDCG }
		Pass{ CGPROGRAM half4 frag ( v2f_img_custom i ) : SV_Target{ return FinalComposition ( i, 3, false, false, true, true ); } ENDCG }
		Pass{ CGPROGRAM half4 frag ( v2f_img_custom i ) : SV_Target{ return FinalComposition ( i, 3, false, true, false, false ); } ENDCG }
		Pass{ CGPROGRAM half4 frag ( v2f_img_custom i ) : SV_Target{ return FinalComposition ( i, 3, false, true, false, true ); } ENDCG }
		Pass{ CGPROGRAM half4 frag ( v2f_img_custom i ) : SV_Target{ return FinalComposition ( i, 3, false, true, true, false ); } ENDCG }
		Pass{ CGPROGRAM half4 frag ( v2f_img_custom i ) : SV_Target{ return FinalComposition ( i, 3, false, true, true, true ); } ENDCG }
		Pass{ CGPROGRAM half4 frag ( v2f_img_custom i ) : SV_Target{ return FinalComposition ( i, 3, true, false, false, false ); } ENDCG }
		Pass{ CGPROGRAM half4 frag ( v2f_img_custom i ) : SV_Target{ return FinalComposition ( i, 3, true, false, false, true ); } ENDCG }
		Pass{ CGPROGRAM half4 frag ( v2f_img_custom i ) : SV_Target{ return FinalComposition ( i, 3, true, false, true, false ); } ENDCG }
		Pass{ CGPROGRAM half4 frag ( v2f_img_custom i ) : SV_Target{ return FinalComposition ( i, 3, true, false, true, true ); } ENDCG }
		Pass{ CGPROGRAM half4 frag ( v2f_img_custom i ) : SV_Target{ return FinalComposition ( i, 3, true, true, false, false ); } ENDCG }
		Pass{ CGPROGRAM half4 frag ( v2f_img_custom i ) : SV_Target{ return FinalComposition ( i, 3, true, true, false, true ); } ENDCG }
		Pass{ CGPROGRAM half4 frag ( v2f_img_custom i ) : SV_Target{ return FinalComposition ( i, 3, true, true, true, false ); } ENDCG }
		Pass{ CGPROGRAM half4 frag ( v2f_img_custom i ) : SV_Target{ return FinalComposition ( i, 3, true, true, true, true ); } ENDCG }
		// count 4
		Pass{ CGPROGRAM half4 frag ( v2f_img_custom i ) : SV_Target{ return FinalComposition ( i, 4, false, false, false, false ); } ENDCG }
		Pass{ CGPROGRAM half4 frag ( v2f_img_custom i ) : SV_Target{ return FinalComposition ( i, 4, false, false, false, true ); } ENDCG }
		Pass{ CGPROGRAM half4 frag ( v2f_img_custom i ) : SV_Target{ return FinalComposition ( i, 4, false, false, true, false ); } ENDCG }
		Pass{ CGPROGRAM half4 frag ( v2f_img_custom i ) : SV_Target{ return FinalComposition ( i, 4, false, false, true, true ); } ENDCG }
		Pass{ CGPROGRAM half4 frag ( v2f_img_custom i ) : SV_Target{ return FinalComposition ( i, 4, false, true, false, false ); } ENDCG }
		Pass{ CGPROGRAM half4 frag ( v2f_img_custom i ) : SV_Target{ return FinalComposition ( i, 4, false, true, false, true ); } ENDCG }
		Pass{ CGPROGRAM half4 frag ( v2f_img_custom i ) : SV_Target{ return FinalComposition ( i, 4, false, true, true, false ); } ENDCG }
		Pass{ CGPROGRAM half4 frag ( v2f_img_custom i ) : SV_Target{ return FinalComposition ( i, 4, false, true, true, true ); } ENDCG }
		Pass{ CGPROGRAM half4 frag ( v2f_img_custom i ) : SV_Target{ return FinalComposition ( i, 4, true, false, false, false ); } ENDCG }
		Pass{ CGPROGRAM half4 frag ( v2f_img_custom i ) : SV_Target{ return FinalComposition ( i, 4, true, false, false, true ); } ENDCG }
		Pass{ CGPROGRAM half4 frag ( v2f_img_custom i ) : SV_Target{ return FinalComposition ( i, 4, true, false, true, false ); } ENDCG }
		Pass{ CGPROGRAM half4 frag ( v2f_img_custom i ) : SV_Target{ return FinalComposition ( i, 4, true, false, true, true ); } ENDCG }
		Pass{ CGPROGRAM half4 frag ( v2f_img_custom i ) : SV_Target{ return FinalComposition ( i, 4, true, true, false, false ); } ENDCG }
		Pass{ CGPROGRAM half4 frag ( v2f_img_custom i ) : SV_Target{ return FinalComposition ( i, 4, true, true, false, true ); } ENDCG }
		Pass{ CGPROGRAM half4 frag ( v2f_img_custom i ) : SV_Target{ return FinalComposition ( i, 4, true, true, true, false ); } ENDCG }
		Pass{ CGPROGRAM half4 frag ( v2f_img_custom i ) : SV_Target{ return FinalComposition ( i, 4, true, true, true, true ); } ENDCG }
		// count 5
		Pass{ CGPROGRAM half4 frag ( v2f_img_custom i ) : SV_Target{ return FinalComposition ( i, 5, false, false, false, false ); } ENDCG }
		Pass{ CGPROGRAM half4 frag ( v2f_img_custom i ) : SV_Target{ return FinalComposition ( i, 5, false, false, false, true ); } ENDCG }
		Pass{ CGPROGRAM half4 frag ( v2f_img_custom i ) : SV_Target{ return FinalComposition ( i, 5, false, false, true, false ); } ENDCG }
		Pass{ CGPROGRAM half4 frag ( v2f_img_custom i ) : SV_Target{ return FinalComposition ( i, 5, false, false, true, true ); } ENDCG }
		Pass{ CGPROGRAM half4 frag ( v2f_img_custom i ) : SV_Target{ return FinalComposition ( i, 5, false, true, false, false ); } ENDCG }
		Pass{ CGPROGRAM half4 frag ( v2f_img_custom i ) : SV_Target{ return FinalComposition ( i, 5, false, true, false, true ); } ENDCG }
		Pass{ CGPROGRAM half4 frag ( v2f_img_custom i ) : SV_Target{ return FinalComposition ( i, 5, false, true, true, false ); } ENDCG }
		Pass{ CGPROGRAM half4 frag ( v2f_img_custom i ) : SV_Target{ return FinalComposition ( i, 5, false, true, true, true ); } ENDCG }
		Pass{ CGPROGRAM half4 frag ( v2f_img_custom i ) : SV_Target{ return FinalComposition ( i, 5, true, false, false, false ); } ENDCG }
		Pass{ CGPROGRAM half4 frag ( v2f_img_custom i ) : SV_Target{ return FinalComposition ( i, 5, true, false, false, true ); } ENDCG }
		Pass{ CGPROGRAM half4 frag ( v2f_img_custom i ) : SV_Target{ return FinalComposition ( i, 5, true, false, true, false ); } ENDCG }
		Pass{ CGPROGRAM half4 frag ( v2f_img_custom i ) : SV_Target{ return FinalComposition ( i, 5, true, false, true, true ); } ENDCG }
		Pass{ CGPROGRAM half4 frag ( v2f_img_custom i ) : SV_Target{ return FinalComposition ( i, 5, true, true, false, false ); } ENDCG }
		Pass{ CGPROGRAM half4 frag ( v2f_img_custom i ) : SV_Target{ return FinalComposition ( i, 5, true, true, false, true ); } ENDCG }
		Pass{ CGPROGRAM half4 frag ( v2f_img_custom i ) : SV_Target{ return FinalComposition ( i, 5, true, true, true, false ); } ENDCG }
		Pass{ CGPROGRAM half4 frag ( v2f_img_custom i ) : SV_Target{ return FinalComposition ( i, 5, true, true, true, true ); } ENDCG }
		// count 6
		Pass{ CGPROGRAM half4 frag ( v2f_img_custom i ) : SV_Target{ return FinalComposition ( i, 6, false, false, false, false ); } ENDCG }
		Pass{ CGPROGRAM half4 frag ( v2f_img_custom i ) : SV_Target{ return FinalComposition ( i, 6, false, false, false, true ); } ENDCG }
		Pass{ CGPROGRAM half4 frag ( v2f_img_custom i ) : SV_Target{ return FinalComposition ( i, 6, false, false, true, false ); } ENDCG }
		Pass{ CGPROGRAM half4 frag ( v2f_img_custom i ) : SV_Target{ return FinalComposition ( i, 6, false, false, true, true ); } ENDCG }
		Pass{ CGPROGRAM half4 frag ( v2f_img_custom i ) : SV_Target{ return FinalComposition ( i, 6, false, true, false, false ); } ENDCG }
		Pass{ CGPROGRAM half4 frag ( v2f_img_custom i ) : SV_Target{ return FinalComposition ( i, 6, false, true, false, true ); } ENDCG }
		Pass{ CGPROGRAM half4 frag ( v2f_img_custom i ) : SV_Target{ return FinalComposition ( i, 6, false, true, true, false ); } ENDCG }
		Pass{ CGPROGRAM half4 frag ( v2f_img_custom i ) : SV_Target{ return FinalComposition ( i, 6, false, true, true, true ); } ENDCG }
		Pass{ CGPROGRAM half4 frag ( v2f_img_custom i ) : SV_Target{ return FinalComposition ( i, 6, true, false, false, false ); } ENDCG }
		Pass{ CGPROGRAM half4 frag ( v2f_img_custom i ) : SV_Target{ return FinalComposition ( i, 6, true, false, false, true ); } ENDCG }
		Pass{ CGPROGRAM half4 frag ( v2f_img_custom i ) : SV_Target{ return FinalComposition ( i, 6, true, false, true, false ); } ENDCG }
		Pass{ CGPROGRAM half4 frag ( v2f_img_custom i ) : SV_Target{ return FinalComposition ( i, 6, true, false, true, true ); } ENDCG }
		Pass{ CGPROGRAM half4 frag ( v2f_img_custom i ) : SV_Target{ return FinalComposition ( i, 6, true, true, false, false ); } ENDCG }
		Pass{ CGPROGRAM half4 frag ( v2f_img_custom i ) : SV_Target{ return FinalComposition ( i, 6, true, true, false, true ); } ENDCG }
		Pass{ CGPROGRAM half4 frag ( v2f_img_custom i ) : SV_Target{ return FinalComposition ( i, 6, true, true, true, false ); } ENDCG }
		Pass{ CGPROGRAM half4 frag ( v2f_img_custom i ) : SV_Target{ return FinalComposition ( i, 6, true, true, true, true ); } ENDCG }
	}
	// LOW END DEVICES WITH ONLY 8 TEXTURE LOOKUP CAPABILITIES
	SubShader
	{
		ZTest Always Cull Off ZWrite Off
		Tags{ "Mode" = "Half" }
		// count 1
		Pass{ CGPROGRAM half4 frag ( v2f_img_custom i ) : SV_Target{ return FinalComposition ( i, 1, false, false, false, false ); } ENDCG }
		Pass{ CGPROGRAM half4 frag ( v2f_img_custom i ) : SV_Target{ return FinalComposition ( i, 1, false, false, false, true ); } ENDCG }
		Pass{ CGPROGRAM half4 frag ( v2f_img_custom i ) : SV_Target{ return FinalComposition ( i, 1, false, false, true, false ); } ENDCG }
		Pass{ CGPROGRAM half4 frag ( v2f_img_custom i ) : SV_Target{ return FinalComposition ( i, 1, false, false, true, true ); } ENDCG }
		Pass{ CGPROGRAM half4 frag ( v2f_img_custom i ) : SV_Target{ return FinalComposition ( i, 1, false, true, false, false ); } ENDCG }
		Pass{ CGPROGRAM half4 frag ( v2f_img_custom i ) : SV_Target{ return FinalComposition ( i, 1, false, true, false, true ); } ENDCG }
		Pass{ CGPROGRAM half4 frag ( v2f_img_custom i ) : SV_Target{ return FinalComposition ( i, 1, false, true, true, false ); } ENDCG }
		Pass{ CGPROGRAM half4 frag ( v2f_img_custom i ) : SV_Target{ return FinalComposition ( i, 1, false, true, true, true ); } ENDCG }
		Pass{ CGPROGRAM half4 frag ( v2f_img_custom i ) : SV_Target{ return FinalComposition ( i, 1, true, false, false, false ); } ENDCG }
		Pass{ CGPROGRAM half4 frag ( v2f_img_custom i ) : SV_Target{ return FinalComposition ( i, 1, true, false, false, true ); } ENDCG }
		Pass{ CGPROGRAM half4 frag ( v2f_img_custom i ) : SV_Target{ return FinalComposition ( i, 1, true, false, true, false ); } ENDCG }
		Pass{ CGPROGRAM half4 frag ( v2f_img_custom i ) : SV_Target{ return FinalComposition ( i, 1, true, false, true, true ); } ENDCG }
		Pass{ CGPROGRAM half4 frag ( v2f_img_custom i ) : SV_Target{ return FinalComposition ( i, 1, true, true, false, false ); } ENDCG }
		Pass{ CGPROGRAM half4 frag ( v2f_img_custom i ) : SV_Target{ return FinalComposition ( i, 1, true, true, false, true ); } ENDCG }
		Pass{ CGPROGRAM half4 frag ( v2f_img_custom i ) : SV_Target{ return FinalComposition ( i, 1, true, true, true, false ); } ENDCG }
		Pass{ CGPROGRAM half4 frag ( v2f_img_custom i ) : SV_Target{ return FinalComposition ( i, 1, true, true, true, true ); } ENDCG }
			// count 2
		Pass{ CGPROGRAM half4 frag ( v2f_img_custom i ) : SV_Target{ return FinalComposition ( i, 2, false, false, false, false ); } ENDCG }
		Pass{ CGPROGRAM half4 frag ( v2f_img_custom i ) : SV_Target{ return FinalComposition ( i, 2, false, false, false, true ); } ENDCG }
		Pass{ CGPROGRAM half4 frag ( v2f_img_custom i ) : SV_Target{ return FinalComposition ( i, 2, false, false, true, false ); } ENDCG }
		Pass{ CGPROGRAM half4 frag ( v2f_img_custom i ) : SV_Target{ return FinalComposition ( i, 2, false, false, true, true ); } ENDCG }
		Pass{ CGPROGRAM half4 frag ( v2f_img_custom i ) : SV_Target{ return FinalComposition ( i, 2, false, true, false, false ); } ENDCG }
		Pass{ CGPROGRAM half4 frag ( v2f_img_custom i ) : SV_Target{ return FinalComposition ( i, 2, false, true, false, true ); } ENDCG }
		Pass{ CGPROGRAM half4 frag ( v2f_img_custom i ) : SV_Target{ return FinalComposition ( i, 2, false, true, true, false ); } ENDCG }
		Pass{ CGPROGRAM half4 frag ( v2f_img_custom i ) : SV_Target{ return FinalComposition ( i, 2, false, true, true, true ); } ENDCG }
		Pass{ CGPROGRAM half4 frag ( v2f_img_custom i ) : SV_Target{ return FinalComposition ( i, 2, true, false, false, false ); } ENDCG }
		Pass{ CGPROGRAM half4 frag ( v2f_img_custom i ) : SV_Target{ return FinalComposition ( i, 2, true, false, false, true ); } ENDCG }
		Pass{ CGPROGRAM half4 frag ( v2f_img_custom i ) : SV_Target{ return FinalComposition ( i, 2, true, false, true, false ); } ENDCG }
		Pass{ CGPROGRAM half4 frag ( v2f_img_custom i ) : SV_Target{ return FinalComposition ( i, 2, true, false, true, true ); } ENDCG }
		Pass{ CGPROGRAM half4 frag ( v2f_img_custom i ) : SV_Target{ return FinalComposition ( i, 2, true, true, false, false ); } ENDCG }
		Pass{ CGPROGRAM half4 frag ( v2f_img_custom i ) : SV_Target{ return FinalComposition ( i, 2, true, true, false, true ); } ENDCG }
		Pass{ CGPROGRAM half4 frag ( v2f_img_custom i ) : SV_Target{ return FinalComposition ( i, 2, true, true, true, false ); } ENDCG }
		Pass{ CGPROGRAM half4 frag ( v2f_img_custom i ) : SV_Target{ return FinalComposition ( i, 2, true, true, true, true ); } ENDCG }
			// count 3
		Pass{ CGPROGRAM half4 frag ( v2f_img_custom i ) : SV_Target{ return FinalComposition ( i, 3, false, false, false, false ); } ENDCG }
		Pass{ CGPROGRAM half4 frag ( v2f_img_custom i ) : SV_Target{ return FinalComposition ( i, 3, false, false, false, true ); } ENDCG }
		Pass{ CGPROGRAM half4 frag ( v2f_img_custom i ) : SV_Target{ return FinalComposition ( i, 3, false, false, true, false ); } ENDCG }
		Pass{ CGPROGRAM half4 frag ( v2f_img_custom i ) : SV_Target{ return FinalComposition ( i, 3, false, false, true, true ); } ENDCG }
		Pass{ CGPROGRAM half4 frag ( v2f_img_custom i ) : SV_Target{ return FinalComposition ( i, 3, false, true, false, false ); } ENDCG }
		Pass{ CGPROGRAM half4 frag ( v2f_img_custom i ) : SV_Target{ return FinalComposition ( i, 3, false, true, false, true ); } ENDCG }
		Pass{ CGPROGRAM half4 frag ( v2f_img_custom i ) : SV_Target{ return FinalComposition ( i, 3, false, true, true, false ); } ENDCG }
		Pass{ CGPROGRAM half4 frag ( v2f_img_custom i ) : SV_Target{ return FinalComposition ( i, 3, false, true, true, true ); } ENDCG }
		Pass{ CGPROGRAM half4 frag ( v2f_img_custom i ) : SV_Target{ return FinalComposition ( i, 3, true, false, false, false ); } ENDCG }
		Pass{ CGPROGRAM half4 frag ( v2f_img_custom i ) : SV_Target{ return FinalComposition ( i, 3, true, false, false, true ); } ENDCG }
		Pass{ CGPROGRAM half4 frag ( v2f_img_custom i ) : SV_Target{ return FinalComposition ( i, 3, true, false, true, false ); } ENDCG }
		Pass{ CGPROGRAM half4 frag ( v2f_img_custom i ) : SV_Target{ return FinalComposition ( i, 3, true, false, true, true ); } ENDCG }
		Pass{ CGPROGRAM half4 frag ( v2f_img_custom i ) : SV_Target{ return FinalComposition ( i, 3, true, true, false, false ); } ENDCG }
		Pass{ CGPROGRAM half4 frag ( v2f_img_custom i ) : SV_Target{ return FinalComposition ( i, 3, true, true, false, true ); } ENDCG }
		Pass{ CGPROGRAM half4 frag ( v2f_img_custom i ) : SV_Target{ return FinalComposition ( i, 3, true, true, true, false ); } ENDCG }
		Pass{ CGPROGRAM half4 frag ( v2f_img_custom i ) : SV_Target{ return FinalComposition ( i, 3, true, true, true, true ); } ENDCG }
			// count 4
		Pass{ CGPROGRAM half4 frag ( v2f_img_custom i ) : SV_Target{ return FinalComposition ( i, 4, false, false, false, false ); } ENDCG }
		Pass{ CGPROGRAM half4 frag ( v2f_img_custom i ) : SV_Target{ return FinalComposition ( i, 4, false, false, false, true ); } ENDCG }
		Pass{ CGPROGRAM half4 frag ( v2f_img_custom i ) : SV_Target{ return FinalComposition ( i, 4, false, false, true, false ); } ENDCG }
		Pass{ CGPROGRAM half4 frag ( v2f_img_custom i ) : SV_Target{ return FinalComposition ( i, 4, false, false, true, true ); } ENDCG }
		Pass{ CGPROGRAM half4 frag ( v2f_img_custom i ) : SV_Target{ return FinalComposition ( i, 4, false, true, false, false ); } ENDCG }
		Pass{ CGPROGRAM half4 frag ( v2f_img_custom i ) : SV_Target{ return FinalComposition ( i, 4, false, true, false, true ); } ENDCG }
		Pass{ CGPROGRAM half4 frag ( v2f_img_custom i ) : SV_Target{ return FinalComposition ( i, 4, false, true, true, false ); } ENDCG }
		Pass{ CGPROGRAM half4 frag ( v2f_img_custom i ) : SV_Target{ return FinalComposition ( i, 4, false, true, true, true ); } ENDCG }
		Pass{ CGPROGRAM half4 frag ( v2f_img_custom i ) : SV_Target{ return FinalComposition ( i, 4, true, false, false, false ); } ENDCG }
		Pass{ CGPROGRAM half4 frag ( v2f_img_custom i ) : SV_Target{ return FinalComposition ( i, 4, true, false, false, true ); } ENDCG }
		Pass{ CGPROGRAM half4 frag ( v2f_img_custom i ) : SV_Target{ return FinalComposition ( i, 4, true, false, true, false ); } ENDCG }
		Pass{ CGPROGRAM half4 frag ( v2f_img_custom i ) : SV_Target{ return FinalComposition ( i, 4, true, false, true, true ); } ENDCG }
		Pass{ CGPROGRAM half4 frag ( v2f_img_custom i ) : SV_Target{ return FinalComposition ( i, 4, true, true, false, false ); } ENDCG }
		Pass{ CGPROGRAM half4 frag ( v2f_img_custom i ) : SV_Target{ return FinalComposition ( i, 4, true, true, false, true ); } ENDCG }
		Pass{ CGPROGRAM half4 frag ( v2f_img_custom i ) : SV_Target{ return FinalComposition ( i, 4, true, true, true, false ); } ENDCG }
		Pass{ CGPROGRAM half4 frag ( v2f_img_custom i ) : SV_Target{ return FinalComposition ( i, 3, true, true, true, true ); } ENDCG }
			// count 5
		Pass{ CGPROGRAM half4 frag ( v2f_img_custom i ) : SV_Target{ return FinalComposition ( i, 5, false, false, false, false ); } ENDCG }
		Pass{ CGPROGRAM half4 frag ( v2f_img_custom i ) : SV_Target{ return FinalComposition ( i, 5, false, false, false, true ); } ENDCG }
		Pass{ CGPROGRAM half4 frag ( v2f_img_custom i ) : SV_Target{ return FinalComposition ( i, 5, false, false, true, false ); } ENDCG }
		Pass{ CGPROGRAM half4 frag ( v2f_img_custom i ) : SV_Target{ return FinalComposition ( i, 5, false, false, true, true ); } ENDCG }
		Pass{ CGPROGRAM half4 frag ( v2f_img_custom i ) : SV_Target{ return FinalComposition ( i, 5, false, true, false, false ); } ENDCG }
		Pass{ CGPROGRAM half4 frag ( v2f_img_custom i ) : SV_Target{ return FinalComposition ( i, 5, false, true, false, true ); } ENDCG }
		Pass{ CGPROGRAM half4 frag ( v2f_img_custom i ) : SV_Target{ return FinalComposition ( i, 5, false, true, true, false ); } ENDCG }
		Pass{ CGPROGRAM half4 frag ( v2f_img_custom i ) : SV_Target{ return FinalComposition ( i, 4, false, true, true, true ); } ENDCG }
		Pass{ CGPROGRAM half4 frag ( v2f_img_custom i ) : SV_Target{ return FinalComposition ( i, 5, true, false, false, false ); } ENDCG }
		Pass{ CGPROGRAM half4 frag ( v2f_img_custom i ) : SV_Target{ return FinalComposition ( i, 5, true, false, false, true ); } ENDCG }
		Pass{ CGPROGRAM half4 frag ( v2f_img_custom i ) : SV_Target{ return FinalComposition ( i, 5, true, false, true, false ); } ENDCG }
		Pass{ CGPROGRAM half4 frag ( v2f_img_custom i ) : SV_Target{ return FinalComposition ( i, 4, true, false, true, true ); } ENDCG }
		Pass{ CGPROGRAM half4 frag ( v2f_img_custom i ) : SV_Target{ return FinalComposition ( i, 5, true, true, false, false ); } ENDCG }
		Pass{ CGPROGRAM half4 frag ( v2f_img_custom i ) : SV_Target{ return FinalComposition ( i, 4, true, true, false, true ); } ENDCG }
		Pass{ CGPROGRAM half4 frag ( v2f_img_custom i ) : SV_Target{ return FinalComposition ( i, 4, true, true, true, false ); } ENDCG }
		Pass{ CGPROGRAM half4 frag ( v2f_img_custom i ) : SV_Target{ return FinalComposition ( i, 3, true, true, true, true ); } ENDCG }
			// count 6
		Pass{ CGPROGRAM half4 frag ( v2f_img_custom i ) : SV_Target{ return FinalComposition ( i, 6, false, false, false, false ); } ENDCG }
		Pass{ CGPROGRAM half4 frag ( v2f_img_custom i ) : SV_Target{ return FinalComposition ( i, 6, false, false, false, true ); } ENDCG }
		Pass{ CGPROGRAM half4 frag ( v2f_img_custom i ) : SV_Target{ return FinalComposition ( i, 6, false, false, true, false ); } ENDCG }
		Pass{ CGPROGRAM half4 frag ( v2f_img_custom i ) : SV_Target{ return FinalComposition ( i, 5, false, false, true, true ); } ENDCG }
		Pass{ CGPROGRAM half4 frag ( v2f_img_custom i ) : SV_Target{ return FinalComposition ( i, 6, false, true, false, false ); } ENDCG }
		Pass{ CGPROGRAM half4 frag ( v2f_img_custom i ) : SV_Target{ return FinalComposition ( i, 5, false, true, false, true ); } ENDCG }
		Pass{ CGPROGRAM half4 frag ( v2f_img_custom i ) : SV_Target{ return FinalComposition ( i, 5, false, true, true, false ); } ENDCG }
		Pass{ CGPROGRAM half4 frag ( v2f_img_custom i ) : SV_Target{ return FinalComposition ( i, 4, false, true, true, true ); } ENDCG }
		Pass{ CGPROGRAM half4 frag ( v2f_img_custom i ) : SV_Target{ return FinalComposition ( i, 6, true, false, false, false ); } ENDCG }
		Pass{ CGPROGRAM half4 frag ( v2f_img_custom i ) : SV_Target{ return FinalComposition ( i, 5, true, false, false, true ); } ENDCG }
		Pass{ CGPROGRAM half4 frag ( v2f_img_custom i ) : SV_Target{ return FinalComposition ( i, 5, true, false, true, false ); } ENDCG }
		Pass{ CGPROGRAM half4 frag ( v2f_img_custom i ) : SV_Target{ return FinalComposition ( i, 4, true, false, true, true ); } ENDCG }
		Pass{ CGPROGRAM half4 frag ( v2f_img_custom i ) : SV_Target{ return FinalComposition ( i, 5, true, true, false, false ); } ENDCG }
		Pass{ CGPROGRAM half4 frag ( v2f_img_custom i ) : SV_Target{ return FinalComposition ( i, 4, true, true, false, true ); } ENDCG }
		Pass{ CGPROGRAM half4 frag ( v2f_img_custom i ) : SV_Target{ return FinalComposition ( i, 4, true, true, true, false ); } ENDCG }
		Pass{ CGPROGRAM half4 frag ( v2f_img_custom i ) : SV_Target{ return FinalComposition ( i, 3, true, true, true, true ); } ENDCG }
	}
}
