// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'

// Amplify Bloom - Advanced Bloom Post-Effect for Unity
// Copyright (c) Amplify Creations, Lda <info@amplify.pt>

#ifndef AMPLIFY_BLOOMLIB_INCLUDED
#define AMPLIFY_BLOOMLIB_INCLUDED

#include "UnityCG.cginc"


uniform half4		_AnamorphicGlareWeights0;
uniform half4		_AnamorphicGlareWeights1;
uniform half4		_AnamorphicGlareWeights2;
uniform half4		_AnamorphicGlareWeights3;
uniform half4		_AnamorphicGlareWeights4;
uniform half4		_AnamorphicGlareWeights5;
uniform half4		_AnamorphicGlareWeights6;
uniform half4		_AnamorphicGlareWeights7;


uniform half4x4		_AnamorphicGlareOffsetsMat0;
uniform half4x4		_AnamorphicGlareOffsetsMat1;
uniform half4x4		_AnamorphicGlareOffsetsMat2;
uniform half4x4		_AnamorphicGlareOffsetsMat3;

uniform half4x4		_AnamorphicGlareWeightsMat0;
uniform half4x4		_AnamorphicGlareWeightsMat1;
uniform half4x4		_AnamorphicGlareWeightsMat2;
uniform half4x4		_AnamorphicGlareWeightsMat3;

uniform sampler2D	_AnamorphicRTS0;
uniform sampler2D	_AnamorphicRTS1;
uniform sampler2D	_AnamorphicRTS2;
uniform sampler2D	_AnamorphicRTS3;
uniform sampler2D	_AnamorphicRTS4;
uniform sampler2D	_AnamorphicRTS5;
uniform sampler2D	_AnamorphicRTS6;
uniform sampler2D	_AnamorphicRTS7;


uniform float4		_LensFlareGhostsParams;
uniform float4		_LensFlareHaloParams;
uniform float		_LensFlareGhostChrDistortion;
uniform float		_LensFlareHaloChrDistortion;

uniform sampler2D	_LensFlareLUT;

half4 _MainTex_ST;

inline float4 CustomObjectToClipPos( in float3 pos )
{
#if UNITY_VERSION >= 540
	return UnityObjectToClipPos( pos );
#else
	return mul( UNITY_MATRIX_VP, mul( unity_ObjectToWorld, float4( pos, 1.0 ) ) );
#endif
}

// Enabling Stereo adjustment in versions prior to 4.5
#ifndef UnityStereoScreenSpaceUVAdjust
	#ifdef UNITY_SINGLE_PASS_STEREO
		inline float2 UnityStereoScreenSpaceUVAdjustInternal ( float2 uv, float4 scaleAndOffset )
		{
			return saturate ( uv.xy ) * scaleAndOffset.xy + scaleAndOffset.zw;
		}

		inline float4 UnityStereoScreenSpaceUVAdjustInternal ( float4 uv, float4 scaleAndOffset )
		{
			return float4( UnityStereoScreenSpaceUVAdjustInternal ( uv.xy, scaleAndOffset ), UnityStereoScreenSpaceUVAdjustInternal ( uv.zw, scaleAndOffset ) );
		}
		#define UnityStereoScreenSpaceUVAdjust(x, y) UnityStereoScreenSpaceUVAdjustInternal(x, y)
	#else
		#define UnityStereoScreenSpaceUVAdjust(x, y) x
	#endif
#endif

// TONEMAP
inline half TonemapRCP ( half x ) { return 1.0 / x; }
inline half3 TonemapForward ( half3 c ) { return c * TonemapRCP ( 1.0 + Luminance ( c ) ); }
inline half3 TonemapInverse ( half3 c ) { return c * TonemapRCP ( 1.0 - Luminance ( saturate ( c ) ) ); }

// ENCODE / DECODE
uniform half4 _BloomRange; // x - bloom range y - 1 / bloom range
inline half4 EncodeColor ( half3 color )
{
#ifdef AB_HIGH_PRECISION
	return half4( color, 0 );
#else
	half4 enc = half4( 0, 0, 0, 0 );
	color *= _BloomRange.y;
	enc.a = saturate ( max ( max ( color.r, color.g ), max ( color.b, 1e-6 ) ) );
	enc.a = ceil ( enc.a * 255.0 ) / 255.0;
	enc.rgb = color / enc.a;
	return enc;
#endif
}

inline half3 DecodeColor ( half4 enc )
{
#ifdef AB_HIGH_PRECISION
	return enc.rgb;
#else
	return _BloomRange.x * enc.rgb * enc.a;
#endif
}

// THRESHOLD
inline half4 CalcThreshold ( half threshold, float2 uv, sampler2D diffuseMap )
{
	half4 color = tex2D ( diffuseMap, UnityStereoScreenSpaceUVAdjust ( uv, _MainTex_ST ) );

#ifdef AB_HIGH_PRECISION
	return  max ( color - threshold, 0 );
#else
	return EncodeColor ( clamp ( color.rgb - threshold.xxx, ( 0 ).xxx, _BloomRange.xxx ) );
#endif
}


inline half4 CalcThresholdWithMask ( half threshold, float2 uv, sampler2D diffuseMap, sampler2D maskMap )
{
	half4 color = tex2D ( diffuseMap, UnityStereoScreenSpaceUVAdjust ( uv, _MainTex_ST ) )*tex2D ( maskMap, uv );

#ifdef AB_HIGH_PRECISION
	return  max ( color - threshold, 0 );
#else
	return EncodeColor ( clamp ( color.rgb - threshold.xxx, ( 0 ).xxx, _BloomRange.xxx ) );
#endif
}

// BOKEH FILTER
inline half CalculateBokehWeight ( half depth, half aperture, half focalLength, half focalDistance, half FarPlane, half MaxCoCDiameter )
{
	half S2 = depth * FarPlane;
	half c = aperture *( abs ( S2 - focalDistance ) / S2 )*( focalLength / ( focalDistance - focalLength ) );
	half invSensorHeight = 41.667;
	half percentOfSensor = c * invSensorHeight;
	return clamp ( percentOfSensor, 0.0, MaxCoCDiameter );
}

// CHROMATIC ABERRATION
inline half3 CalcChromaticAberration ( sampler2D texMap, float2 uv, float2 dir, float3 distortion )
{
	return half3(	DecodeColor ( tex2D ( texMap,  UnityStereoScreenSpaceUVAdjust ( uv + dir * distortion.r, _MainTex_ST ) ) ).r,
		DecodeColor ( tex2D ( texMap, UnityStereoScreenSpaceUVAdjust ( uv + dir * distortion.g, _MainTex_ST ) ) ).g,
		DecodeColor ( tex2D ( texMap, UnityStereoScreenSpaceUVAdjust ( uv + dir * distortion.b, _MainTex_ST ) ) ).b );
}

// PSEUDO - LENS FLARE
// Halo Params - x - strength y - width  z - factor w - falloff
// Ghost Params - x - strength y - dispersal  z - factor w - falloff

inline half4 CalcLensFlare ( const int ghostsAmount, float2 texelSize, float2 uv, sampler2D thresholdMap )
{
	half3 result = half3( 0, 0, 0 );
	float2 flippedUV = float2( 1, 1 ) - uv;

	//GHOST VECTOR TO IMAGE CENTER
	float2 imageCenter = float2( 0.5, 0.5 );

	const float imageCenterLength = 0.7071;
	const float invImageCenterLength = 1.4142;

	float2  ghostVec = ( imageCenter - uv )*_LensFlareGhostsParams.y;

	float uvLen = length ( imageCenter - uv ) * invImageCenterLength;
	float2 lutUV = float2( frac ( uvLen ), 0 );
	half3 lutColor = tex2D ( _LensFlareLUT, lutUV );

	float2 chromaticDir = normalize ( ghostVec );
	float3 chromaticDistVec = float3( -texelSize.x*_LensFlareGhostChrDistortion, 0.0, texelSize.x*_LensFlareGhostChrDistortion );

	// GHOSTS
	UNITY_UNROLL
		for ( int i = 0; i < ghostsAmount; i++ )
		{
			float2 ghostUV = frac ( uv + ghostVec*( float ) i );
			float weight = length ( imageCenter - ghostUV ) *invImageCenterLength;
			weight = pow ( ( 1 - weight )*_LensFlareGhostsParams.z, _LensFlareGhostsParams.w );
			weight = weight*weight;
			result += CalcChromaticAberration ( thresholdMap, ghostUV, chromaticDir, chromaticDistVec ).rgb*weight*lutColor*_LensFlareGhostsParams.x;
		}

	chromaticDistVec = float3( -texelSize.x*_LensFlareHaloChrDistortion, 0.0, texelSize.x*_LensFlareHaloChrDistortion );

	// HALO
	float2 haloVec = chromaticDir * _LensFlareHaloParams.y;
	float haloWeight = length ( imageCenter - frac ( uv + haloVec ) )*invImageCenterLength;
	haloWeight = pow ( ( 1 - haloWeight )*_LensFlareHaloParams.z, _LensFlareHaloParams.w );
	result += CalcChromaticAberration ( thresholdMap, frac ( uv + haloVec ), chromaticDir, chromaticDistVec ).rgb*haloWeight*_LensFlareHaloParams.x*lutColor;

	return EncodeColor ( result );
}

// ANAMORPHIC GLARE
//inline half4 AnamorphicGlareMat ( float2 uv, sampler2D diffuseMap )
//{
//	half3 finalColor = half3( 0, 0, 0 );
//	UNITY_UNROLL
//	for ( uint matIdx = 0; matIdx < 4; matIdx++ )
//	{
//		UNITY_UNROLL
//		for ( int vecIdx = 0; vecIdx < 4; vecIdx++ )
//		{
//			float2 uvOffset = uv + _AnamorphicGlareOffsetsMat[ matIdx ][ vecIdx ].xy;
//			half3 mainColor = DecodeColor ( tex2D ( diffuseMap, uvOffset ) );
//			finalColor.rgb += _AnamorphicGlareWeightsMat[ matIdx ][ vecIdx ].rgb*mainColor.rgb;
//		}
//	}
//	return EncodeColor ( finalColor );
//}

inline half4 AnamorphicGlareMat ( float2 uv, sampler2D diffuseMap )
{
	half3 finalColor = half3( 0, 0, 0 );
	float2 uvOffset = float2( 0, 0 );
	half3 mainColor = half3( 0, 0, 0 );

	//MatIdx 0
	{
		//vecIdx 0
		uvOffset = uv + _AnamorphicGlareOffsetsMat0[ 0 ].xy;
		mainColor = DecodeColor ( tex2D ( diffuseMap, UnityStereoScreenSpaceUVAdjust ( uvOffset, _MainTex_ST ) ) );
		finalColor.rgb += _AnamorphicGlareWeightsMat0[ 0 ].rgb*mainColor.rgb;

		//vecIdx 1
		uvOffset = uv + _AnamorphicGlareOffsetsMat0[ 1 ].xy;
		mainColor = DecodeColor ( tex2D ( diffuseMap, UnityStereoScreenSpaceUVAdjust ( uvOffset, _MainTex_ST ) ) );
		finalColor.rgb += _AnamorphicGlareWeightsMat0[ 1 ].rgb*mainColor.rgb;

		//vecIdx 2
		uvOffset = uv + _AnamorphicGlareOffsetsMat0[ 2 ].xy;
		mainColor = DecodeColor ( tex2D ( diffuseMap, UnityStereoScreenSpaceUVAdjust ( uvOffset, _MainTex_ST ) ) );
		finalColor.rgb += _AnamorphicGlareWeightsMat0[ 2 ].rgb*mainColor.rgb;

		//vecIdx 3
		uvOffset = uv + _AnamorphicGlareOffsetsMat0[ 3 ].xy;
		mainColor = DecodeColor ( tex2D ( diffuseMap, UnityStereoScreenSpaceUVAdjust ( uvOffset, _MainTex_ST ) ) );
		finalColor.rgb += _AnamorphicGlareWeightsMat0[ 3 ].rgb*mainColor.rgb;

	}

	//MatIdx 1
	{
		//vecIdx 0
		uvOffset = uv + _AnamorphicGlareOffsetsMat1[ 0 ].xy;
		mainColor = DecodeColor ( tex2D ( diffuseMap, UnityStereoScreenSpaceUVAdjust ( uvOffset, _MainTex_ST ) ) );
		finalColor.rgb += _AnamorphicGlareWeightsMat1[ 0 ].rgb*mainColor.rgb;

		//vecIdx 1
		uvOffset = uv + _AnamorphicGlareOffsetsMat1[ 1 ].xy;
		mainColor = DecodeColor ( tex2D ( diffuseMap, UnityStereoScreenSpaceUVAdjust ( uvOffset, _MainTex_ST ) ) );
		finalColor.rgb += _AnamorphicGlareWeightsMat1[ 1 ].rgb*mainColor.rgb;

		//vecIdx 2
		uvOffset = uv + _AnamorphicGlareOffsetsMat1[ 2 ].xy;
		mainColor = DecodeColor ( tex2D ( diffuseMap, UnityStereoScreenSpaceUVAdjust ( uvOffset, _MainTex_ST ) ) );
		finalColor.rgb += _AnamorphicGlareWeightsMat1[ 2 ].rgb*mainColor.rgb;

		//vecIdx 3
		uvOffset = uv + _AnamorphicGlareOffsetsMat1[ 3 ].xy;
		mainColor = DecodeColor ( tex2D ( diffuseMap, UnityStereoScreenSpaceUVAdjust ( uvOffset, _MainTex_ST ) ) );
		finalColor.rgb += _AnamorphicGlareWeightsMat1[ 3 ].rgb*mainColor.rgb;
	}

	//MatIdx 2
	{
		//vecIdx 0
		uvOffset = uv + _AnamorphicGlareOffsetsMat2[ 0 ].xy;
		mainColor = DecodeColor ( tex2D ( diffuseMap, UnityStereoScreenSpaceUVAdjust ( uvOffset, _MainTex_ST ) ) );
		finalColor.rgb += _AnamorphicGlareWeightsMat2[ 0 ].rgb*mainColor.rgb;

		//vecIdx 1
		uvOffset = uv + _AnamorphicGlareOffsetsMat2[ 1 ].xy;
		mainColor = DecodeColor ( tex2D ( diffuseMap, UnityStereoScreenSpaceUVAdjust ( uvOffset, _MainTex_ST ) ) );
		finalColor.rgb += _AnamorphicGlareWeightsMat2[ 1 ].rgb*mainColor.rgb;

		//vecIdx 2
		uvOffset = uv + _AnamorphicGlareOffsetsMat2[ 2 ].xy;
		mainColor = DecodeColor ( tex2D ( diffuseMap, UnityStereoScreenSpaceUVAdjust ( uvOffset, _MainTex_ST ) ) );
		finalColor.rgb += _AnamorphicGlareWeightsMat2[ 2 ].rgb*mainColor.rgb;

		//vecIdx 3
		uvOffset = uv + _AnamorphicGlareOffsetsMat2[ 3 ].xy;
		mainColor = DecodeColor ( tex2D ( diffuseMap, UnityStereoScreenSpaceUVAdjust ( uvOffset, _MainTex_ST ) ) );
		finalColor.rgb += _AnamorphicGlareWeightsMat2[ 3 ].rgb*mainColor.rgb;
	}

	//MatIdx 3
	{
		//vecIdx 0
		uvOffset = uv + _AnamorphicGlareOffsetsMat3[ 0 ].xy;
		mainColor = DecodeColor ( tex2D ( diffuseMap, UnityStereoScreenSpaceUVAdjust ( uvOffset, _MainTex_ST ) ) );
		finalColor.rgb += _AnamorphicGlareWeightsMat3[ 0 ].rgb*mainColor.rgb;

		//vecIdx 1
		uvOffset = uv + _AnamorphicGlareOffsetsMat3[ 1 ].xy;
		mainColor = DecodeColor ( tex2D ( diffuseMap, UnityStereoScreenSpaceUVAdjust ( uvOffset, _MainTex_ST ) ) );
		finalColor.rgb += _AnamorphicGlareWeightsMat3[ 1 ].rgb*mainColor.rgb;

		//vecIdx 2
		uvOffset = uv + _AnamorphicGlareOffsetsMat3[ 2 ].xy;
		mainColor = DecodeColor ( tex2D ( diffuseMap, UnityStereoScreenSpaceUVAdjust ( uvOffset, _MainTex_ST ) ) );
		finalColor.rgb += _AnamorphicGlareWeightsMat3[ 2 ].rgb*mainColor.rgb;

		//vecIdx 3
		uvOffset = uv + _AnamorphicGlareOffsetsMat3[ 3 ].xy;
		mainColor = DecodeColor ( tex2D ( diffuseMap, UnityStereoScreenSpaceUVAdjust ( uvOffset, _MainTex_ST ) ) );
		finalColor.rgb += _AnamorphicGlareWeightsMat3[ 3 ].rgb*mainColor.rgb;
	}

	return EncodeColor ( finalColor );
}


// GAUSSIAN FUNCTION
inline half4 NineTapGaussian ( float2 uv, sampler2D diffuseMap, float2 stride )
{
	half4 color = half4( 0, 0, 0, 0 );
	color.rgb += DecodeColor ( tex2D ( diffuseMap, UnityStereoScreenSpaceUVAdjust ( uv, _MainTex_ST ) ) ) * 0.227027027;

	float2 d1 = stride * 1.3846153846;
	color.rgb += DecodeColor ( tex2D ( diffuseMap, UnityStereoScreenSpaceUVAdjust ( uv + d1, _MainTex_ST )  ) ) * 0.3162162162;
	color.rgb += DecodeColor ( tex2D ( diffuseMap, UnityStereoScreenSpaceUVAdjust ( uv - d1, _MainTex_ST )  ) ) * 0.3162162162;

	float2 d2 = stride * 3.2307692308;
	color.rgb += DecodeColor ( tex2D ( diffuseMap, UnityStereoScreenSpaceUVAdjust ( uv + d2, _MainTex_ST )  ) ) * 0.0702702703;
	color.rgb += DecodeColor ( tex2D ( diffuseMap, UnityStereoScreenSpaceUVAdjust ( uv - d2, _MainTex_ST )  ) ) * 0.0702702703;

	return EncodeColor ( color );
}

// UPSCALE FUNCTIONS
inline half4 FirstPassUpscaleBlurTent ( sampler2D currentMipRT, float2 uvCoords, float2 oneOverTexSize, float BlurRadius )
{
	float2 TexelOffsets[ 9 ] =
	{
		float2( -1, -1 ),
		float2( 0, -1 ),
		float2( 1, -1 ),
		float2( -1, 0 ),
		float2( 0, 0 ),
		float2( 1, 0 ),
		float2( -1, 1 ),
		float2( 0, 1 ),
		float2( 1, 1 )
	};

	half Weights[ 9 ] =
	{
		0.0625,
		0.125,
		0.0625,
		0.125,
		0.25,
		0.125,
		0.0625,
		0.125,
		0.0625
	};

	half4 color = half4( 0.0, 0.0, 0.0, 0.0 );

	UNITY_UNROLL
		for ( int i = 0; i < 9; i++ )
		{
			float2 uv = uvCoords + TexelOffsets[ i ] * oneOverTexSize * BlurRadius;
			color.rgb += DecodeColor ( tex2D ( currentMipRT, UnityStereoScreenSpaceUVAdjust ( uv, _MainTex_ST ) ) ) * Weights[ i ];
		}

	return EncodeColor ( color );
}

inline half4 UpscaleBlurTent ( sampler2D currentMipRT, sampler2D previousUpscale, float2 uvCoords, float2 oneOverTexSize, float BlurRadius )
{
	float2 TexelOffsets[ 9 ] =
	{
		float2( -1, -1 ),
		float2( 0, -1 ),
		float2( 1, -1 ),
		float2( -1, 0 ),
		float2( 0, 0 ),
		float2( 1, 0 ),
		float2( -1, 1 ),
		float2( 0, 1 ),
		float2( 1, 1 )
	};

	half Weights[ 9 ] =
	{
		0.0625,
		0.125,
		0.0625,
		0.125,
		0.25,
		0.125,
		0.0625,
		0.125,
		0.0625
	};

	half4 color = half4( 0.0, 0.0, 0.0, 0.0 );

	UNITY_UNROLL
		for ( int i = 0; i < 9; i++ )
		{
			float2 uv = uvCoords + TexelOffsets[ i ] * oneOverTexSize * BlurRadius;
			color.rgb += DecodeColor ( tex2D ( currentMipRT, UnityStereoScreenSpaceUVAdjust ( uv, _MainTex_ST ) ) ) * Weights[ i ];
		}
	color.rgb += DecodeColor ( tex2D ( previousUpscale, UnityStereoScreenSpaceUVAdjust ( uvCoords, _MainTex_ST ) ) );

	return EncodeColor ( color );
}

// DOWNSAMPLING FUNCTIONS
inline half4 DownsampleWithKaris ( float2 texcoord, float2 oneOverTextureSize, sampler2D DiffuseMap )
{
	const int NUM_SAMPLES = 13;
	float2 TexelOffsets[ NUM_SAMPLES ] =
	{
		float2( -1.0,-1.0 ),
		float2( 1.0,-1.0 ),
		float2( 1.0,1.0 ),
		float2( -1.0,1.0 ),
		float2( -2.0,-2.0 ),
		float2( 0.0,-2.0 ),
		float2( 2.0,-2.0 ),
		float2( -2.0,0.0 ),
		float2( 0.0,0.0 ),
		float2( 2.0,0.0 ),
		float2( -2.0,2.0 ),
		float2( 0.0,2.0 ),
		float2( 2.0,2.0 )
	};


	half4 redSum = half4( 1.0, 1.0, 1.0, 1.0 );
	half4 yellowSum = half4( 1.0, 1.0, 1.0, 1.0 );
	half4 greenSum = half4( 1.0, 1.0, 1.0, 1.0 );
	half4 blueSum = half4( 1.0, 1.0, 1.0, 1.0 );
	half4 purpleSum = half4( 1.0, 1.0, 1.0, 1.0 );
	half4 texels[ NUM_SAMPLES ];


	half4 sum = half4( 1.0, 1.0, 1.0, 1.0 );
	UNITY_UNROLL
		for ( int i = 0; i < NUM_SAMPLES; ++i )
		{
			float2 uv = texcoord + ( TexelOffsets[ i ] * oneOverTextureSize );
			texels[ i ].rgb = DecodeColor ( tex2D ( DiffuseMap, UnityStereoScreenSpaceUVAdjust ( uv, _MainTex_ST ) ) );
		}

	redSum = ( texels[ 0 ] + texels[ 1 ] + texels[ 2 ] + texels[ 3 ] ) * 0.25;
	yellowSum = ( texels[ 7 ] + texels[ 8 ] + texels[ 10 ] + texels[ 11 ] ) * 0.25;
	greenSum = ( texels[ 8 ] + texels[ 9 ] + texels[ 11 ] + texels[ 12 ] ) * 0.25;
	blueSum = ( texels[ 5 ] + texels[ 6 ] + texels[ 8 ] + texels[ 9 ] ) * 0.25;
	purpleSum = ( texels[ 4 ] + texels[ 5 ] + texels[ 7 ] + texels[ 8 ] ) * 0.25;

	redSum.rgb = TonemapForward ( redSum.rgb );
	yellowSum.rgb = TonemapForward ( yellowSum.rgb );
	greenSum.rgb = TonemapForward ( greenSum.rgb );
	blueSum.rgb = TonemapForward ( blueSum.rgb );
	purpleSum.rgb = TonemapForward ( purpleSum.rgb );

	sum = ( redSum * 0.5 ) + ( ( yellowSum + greenSum + blueSum + purpleSum ) * 0.125 );

	return EncodeColor ( sum.rgb );
}


inline half4 DownsampleWithoutKaris ( float2 texcoord, float2 oneOverTextureSize, sampler2D DiffuseMap )
{
	const int NUM_SAMPLES = 13;
	float2 TexelOffsets[ NUM_SAMPLES ] =
	{
		float2( -1.0,-1.0 ),
		float2( 1.0,-1.0 ),
		float2( 1.0,1.0 ),
		float2( -1.0,1.0 ),
		float2( -2.0,-2.0 ),
		float2( 0.0,-2.0 ),
		float2( 2.0,-2.0 ),
		float2( -2.0,0.0 ),
		float2( 0.0,0.0 ),
		float2( 2.0,0.0 ),
		float2( -2.0,2.0 ),
		float2( 0.0,2.0 ),
		float2( 2.0,2.0 )
	};


	half4 redSum = half4( 1.0, 1.0, 1.0, 1.0 );
	half4 yellowSum = half4( 1.0, 1.0, 1.0, 1.0 );
	half4 greenSum = half4( 1.0, 1.0, 1.0, 1.0 );
	half4 blueSum = half4( 1.0, 1.0, 1.0, 1.0 );
	half4 purpleSum = half4( 1.0, 1.0, 1.0, 1.0 );
	half4 texels[ NUM_SAMPLES ];


	half4 sum = half4( 1.0, 1.0, 1.0, 1.0 );
	UNITY_UNROLL
		for ( int i = 0; i < NUM_SAMPLES; ++i )
		{
			float2 uv = texcoord + ( TexelOffsets[ i ] * oneOverTextureSize );
			texels[ i ].rgb = DecodeColor ( tex2D ( DiffuseMap, UnityStereoScreenSpaceUVAdjust ( uv, _MainTex_ST ) ) );
		}

	redSum = ( texels[ 0 ] + texels[ 1 ] + texels[ 2 ] + texels[ 3 ] ) * 0.25;
	yellowSum = ( texels[ 7 ] + texels[ 8 ] + texels[ 10 ] + texels[ 11 ] ) * 0.25;
	greenSum = ( texels[ 8 ] + texels[ 9 ] + texels[ 11 ] + texels[ 12 ] ) * 0.25;
	blueSum = ( texels[ 5 ] + texels[ 6 ] + texels[ 8 ] + texels[ 9 ] ) * 0.25;
	purpleSum = ( texels[ 4 ] + texels[ 5 ] + texels[ 7 ] + texels[ 8 ] ) * 0.25;
	sum = ( redSum * 0.5 ) + ( ( yellowSum + greenSum + blueSum + purpleSum ) * 0.125 );
	return EncodeColor ( sum.rgb );
}


inline half4 DownsampleNoWeightedAvg ( float2 texcoord, float2 oneOverTextureSize, sampler2D DiffuseMap )
{
	const int NUM_SAMPLES = 13;
	float2 TexelOffsets[ NUM_SAMPLES ] =
	{
		float2( -1.0, -1.0 ),
		float2( 1.0, -1.0 ),
		float2( 1.0, 1.0 ),
		float2( -1.0, 1.0 ),
		float2( -2.0, -2.0 ),
		float2( 0.0, -2.0 ),
		float2( 2.0, -2.0 ),
		float2( -2.0, 0.0 ),
		float2( 0.0, 0.0 ),
		float2( 2.0, 0.0 ),
		float2( -2.0, 2.0 ),
		float2( 0.0, 2.0 ),
		float2( 2.0, 2.0 )
	};


	half4 redSum = half4( 1.0, 1.0, 1.0, 1.0 );
	half4 yellowSum = half4( 1.0, 1.0, 1.0, 1.0 );
	half4 greenSum = half4( 1.0, 1.0, 1.0, 1.0 );
	half4 blueSum = half4( 1.0, 1.0, 1.0, 1.0 );
	half4 purpleSum = half4( 1.0, 1.0, 1.0, 1.0 );
	half4 texels[ NUM_SAMPLES ];


	half4 sum = half4( 1.0, 1.0, 1.0, 1.0 );
	UNITY_UNROLL
		for ( int i = 0; i < NUM_SAMPLES; ++i )
		{
			float2 uv = texcoord + ( TexelOffsets[ i ] * oneOverTextureSize );
			texels[ i ].rgb = DecodeColor ( tex2D ( DiffuseMap, UnityStereoScreenSpaceUVAdjust ( uv, _MainTex_ST ) ) );
		}

	redSum = ( texels[ 0 ] + texels[ 1 ] + texels[ 2 ] + texels[ 3 ] ) * 0.25;
	yellowSum = ( texels[ 7 ] + texels[ 8 ] + texels[ 10 ] + texels[ 11 ] ) * 0.25;
	greenSum = ( texels[ 8 ] + texels[ 9 ] + texels[ 11 ] + texels[ 12 ] ) * 0.25;
	blueSum = ( texels[ 5 ] + texels[ 6 ] + texels[ 8 ] + texels[ 9 ] ) * 0.25;
	purpleSum = ( texels[ 4 ] + texels[ 5 ] + texels[ 7 ] + texels[ 8 ] ) * 0.25;

	sum = ( redSum * 0.5 ) + ( ( yellowSum + greenSum + blueSum + purpleSum ) * 0.125 );

	return EncodeColor ( sum.rgb );
}


#endif
