// Amplify Bloom - Advanced Bloom Post-Effect for Unity
// Copyright (c) Amplify Creations, Lda <info@amplify.pt>

#ifndef AMPLIFY_BLOOMFRAG_INCLUDED
#define AMPLIFY_BLOOMFRAG_INCLUDED

#include "UnityCG.cginc"

#include "UnityCG.cginc"
#ifndef UNITY_DECLARE_DEPTH_TEXTURE
#define UNITY_DECLARE_DEPTH_TEXTURE(tex) sampler2D_float tex
#endif

UNITY_DECLARE_DEPTH_TEXTURE( _CameraDepthTexture );

uniform sampler2D	_MainTex;
uniform sampler2D	_MaskTex;

uniform float		_BlurRadius;
uniform half4		_BloomParams;// x - overallIntensity y - threshold, z - blur radius w - bloom scale
uniform half		_TempFilterValue;

uniform half4x4		_LensFlareStarMatrix;
uniform half		_LensFlareStarburstStrength;

uniform half4 _BokehParams;



half4 frag_decode ( v2f_img input ) : SV_Target
{
	return half4( DecodeColor ( tex2D ( _MainTex, input.uv ) ),1 );
}

half4 frag_threshold ( v2f_img input ) : SV_Target
{
	return CalcThreshold ( _BloomParams.y,input.uv, _MainTex );
}

half4 frag_thresholdMask ( v2f_img input ) : SV_Target
{
	return CalcThresholdWithMask ( _BloomParams.y,input.uv, _MainTex ,_MaskTex );
}

//
//half4 frag_BokehFiltering ( v2f_img input ) : SV_Target
//{
//	const half bleedingBias = 0.02;
//const half bleedingMult = 30;
//
//half4 centerPixel = tex2D ( _MainTex, input.uv );
//half centerDepth = SAMPLE_DEPTH_TEXTURE ( _CameraDepthTexture, input.uv );
//
//half centerPixelWeight = CalculateBokehWeight ( centerDepth, _BokehParams.x, _BokehParams.y, _BokehParams.z, _ProjectionParams.z, _BokehParams.w );
//
//half4 color = half4( 0,0,0,0 );
//half totalWeight = 0;
//
//UNITY_UNROLL
//for ( int t = 0; t < 8; t++ )
//{
//	half2 sampleCoords = input.uv + _AnamorphicGlareWeights[ t ].xy*centerPixelWeight;
//	half4 samplePixel = tex2D ( _MainTex, sampleCoords );
//	half4 sampleDepth = SAMPLE_DEPTH_TEXTURE ( _CameraDepthTexture, sampleCoords );
//	half weight = ( sampleDepth < centerDepth ) ? centerPixelWeight*bleedingMult : 1;
//	weight = ( centerPixelWeight > ( samplePixel.a + bleedingBias ) ) ? weight : 1;
//	weight = saturate ( weight );
//	color += samplePixel * weight;
//	totalWeight += weight;
//}
//return  color / totalWeight;
//}


half4 frag_BokehFiltering ( v2f_img input ) : SV_Target
{
	const half bleedingBias = 0.02;
	const half bleedingMult = 30;

	half4 centerPixel = tex2D ( _MainTex, UnityStereoScreenSpaceUVAdjust ( input.uv, _MainTex_ST ));
	half centerDepth = SAMPLE_DEPTH_TEXTURE ( _CameraDepthTexture, UnityStereoScreenSpaceUVAdjust ( input.uv, _MainTex_ST ) );

	half centerPixelWeight = CalculateBokehWeight ( centerDepth, _BokehParams.x, _BokehParams.y, _BokehParams.z, _ProjectionParams.z, _BokehParams.w );

	half4 color = half4( 0,0,0,0 );
	half totalWeight = 0;

	half2 sampleCoords = half2( 0, 0 );
	half4 samplePixel = half4( 0,0,0,0 );
	half4 sampleDepth = half4( 0,0,0,0 );
	half weight = 0;

	//0
	sampleCoords = input.uv + _AnamorphicGlareWeights0.xy*centerPixelWeight;
	samplePixel = tex2D ( _MainTex, UnityStereoScreenSpaceUVAdjust ( sampleCoords, _MainTex_ST ));
	sampleDepth = SAMPLE_DEPTH_TEXTURE ( _CameraDepthTexture, sampleCoords );
	weight = ( sampleDepth < centerDepth ) ? centerPixelWeight*bleedingMult : 1;
	weight = ( centerPixelWeight > ( samplePixel.a + bleedingBias ) ) ? weight : 1;
	weight = saturate ( weight );
	color += samplePixel * weight;
	totalWeight += weight;
	//1
	sampleCoords = input.uv + _AnamorphicGlareWeights1.xy*centerPixelWeight;
	samplePixel = tex2D ( _MainTex, UnityStereoScreenSpaceUVAdjust ( sampleCoords, _MainTex_ST ) );
	sampleDepth = SAMPLE_DEPTH_TEXTURE ( _CameraDepthTexture, sampleCoords );
	weight = ( sampleDepth < centerDepth ) ? centerPixelWeight*bleedingMult : 1;
	weight = ( centerPixelWeight > ( samplePixel.a + bleedingBias ) ) ? weight : 1;
	weight = saturate ( weight );
	color += samplePixel * weight;
	totalWeight += weight;
	//2
	sampleCoords = input.uv + _AnamorphicGlareWeights2.xy*centerPixelWeight;
	samplePixel = tex2D ( _MainTex, UnityStereoScreenSpaceUVAdjust ( sampleCoords, _MainTex_ST ) );
	sampleDepth = SAMPLE_DEPTH_TEXTURE ( _CameraDepthTexture, sampleCoords );
	weight = ( sampleDepth < centerDepth ) ? centerPixelWeight*bleedingMult : 1;
	weight = ( centerPixelWeight > ( samplePixel.a + bleedingBias ) ) ? weight : 1;
	weight = saturate ( weight );
	color += samplePixel * weight;
	totalWeight += weight;
	//3
	sampleCoords = input.uv + _AnamorphicGlareWeights3.xy*centerPixelWeight;
	samplePixel = tex2D ( _MainTex, UnityStereoScreenSpaceUVAdjust ( sampleCoords, _MainTex_ST ) );
	sampleDepth = SAMPLE_DEPTH_TEXTURE ( _CameraDepthTexture, sampleCoords );
	weight = ( sampleDepth < centerDepth ) ? centerPixelWeight*bleedingMult : 1;
	weight = ( centerPixelWeight > ( samplePixel.a + bleedingBias ) ) ? weight : 1;
	weight = saturate ( weight );
	color += samplePixel * weight;
	totalWeight += weight;
	//4
	sampleCoords = input.uv + _AnamorphicGlareWeights4.xy*centerPixelWeight;
	samplePixel = tex2D ( _MainTex, UnityStereoScreenSpaceUVAdjust ( sampleCoords, _MainTex_ST ) );
	sampleDepth = SAMPLE_DEPTH_TEXTURE ( _CameraDepthTexture, sampleCoords );
	weight = ( sampleDepth < centerDepth ) ? centerPixelWeight*bleedingMult : 1;
	weight = ( centerPixelWeight > ( samplePixel.a + bleedingBias ) ) ? weight : 1;
	weight = saturate ( weight );
	color += samplePixel * weight;
	totalWeight += weight;
	//5
	sampleCoords = input.uv + _AnamorphicGlareWeights5.xy*centerPixelWeight;
	samplePixel = tex2D ( _MainTex, UnityStereoScreenSpaceUVAdjust ( sampleCoords, _MainTex_ST ) );
	sampleDepth = SAMPLE_DEPTH_TEXTURE ( _CameraDepthTexture, sampleCoords );
	weight = ( sampleDepth < centerDepth ) ? centerPixelWeight*bleedingMult : 1;
	weight = ( centerPixelWeight > ( samplePixel.a + bleedingBias ) ) ? weight : 1;
	weight = saturate ( weight );
	color += samplePixel * weight;
	totalWeight += weight;
	//6
	sampleCoords = input.uv + _AnamorphicGlareWeights6.xy*centerPixelWeight;
	samplePixel = tex2D ( _MainTex, UnityStereoScreenSpaceUVAdjust ( sampleCoords, _MainTex_ST ) );
	sampleDepth = SAMPLE_DEPTH_TEXTURE ( _CameraDepthTexture, sampleCoords );
	weight = ( sampleDepth < centerDepth ) ? centerPixelWeight*bleedingMult : 1;
	weight = ( centerPixelWeight > ( samplePixel.a + bleedingBias ) ) ? weight : 1;
	weight = saturate ( weight );
	color += samplePixel * weight;
	totalWeight += weight;
	//7
	sampleCoords = input.uv + _AnamorphicGlareWeights7.xy*centerPixelWeight;
	samplePixel = tex2D ( _MainTex, UnityStereoScreenSpaceUVAdjust ( sampleCoords, _MainTex_ST ) );
	sampleDepth = SAMPLE_DEPTH_TEXTURE ( _CameraDepthTexture, sampleCoords );
	weight = ( sampleDepth < centerDepth ) ? centerPixelWeight*bleedingMult : 1;
	weight = ( centerPixelWeight > ( samplePixel.a + bleedingBias ) ) ? weight : 1;
	weight = saturate ( weight );
	color += samplePixel * weight;


	totalWeight += weight;
	return  color / totalWeight;
}

half4 frag_BokehComposition2S ( v2f_img input ) : SV_Target
{
	return min ( tex2D ( _MainTex,UnityStereoScreenSpaceUVAdjust ( input.uv, _MainTex_ST ) ) , tex2D ( _AnamorphicRTS0, UnityStereoScreenSpaceUVAdjust ( input.uv, _MainTex_ST ) ) );
}

half4 frag_BokehComposition3S ( v2f_img input ) : SV_Target
{
	half4 colorMinA = min ( tex2D ( _MainTex,UnityStereoScreenSpaceUVAdjust ( input.uv, _MainTex_ST ) ) , tex2D ( _AnamorphicRTS0, UnityStereoScreenSpaceUVAdjust ( input.uv, _MainTex_ST ) ) );
	return min ( colorMinA,tex2D ( _AnamorphicRTS1, UnityStereoScreenSpaceUVAdjust ( input.uv, _MainTex_ST ) ) );
}

half4 frag_BokehComposition4S ( v2f_img input ) : SV_Target
{
	half4 colorMin = min ( tex2D ( _MainTex,UnityStereoScreenSpaceUVAdjust ( input.uv, _MainTex_ST ) ) , tex2D ( _AnamorphicRTS0, UnityStereoScreenSpaceUVAdjust ( input.uv, _MainTex_ST ) ) );
	colorMin = min ( colorMin, tex2D ( _AnamorphicRTS1, UnityStereoScreenSpaceUVAdjust ( input.uv, _MainTex_ST ) ) );
	colorMin = min ( colorMin, tex2D ( _AnamorphicRTS2, UnityStereoScreenSpaceUVAdjust ( input.uv, _MainTex_ST ) ) );
	return colorMin;
}

half4 frag_BokehComposition5S ( v2f_img input ) : SV_Target
{
	half4 colorMin = min ( tex2D ( _MainTex,UnityStereoScreenSpaceUVAdjust ( input.uv, _MainTex_ST ) ) , tex2D ( _AnamorphicRTS0, UnityStereoScreenSpaceUVAdjust ( input.uv, _MainTex_ST ) ) );
	colorMin = min ( colorMin, tex2D ( _AnamorphicRTS1, UnityStereoScreenSpaceUVAdjust ( input.uv, _MainTex_ST ) ) );
	colorMin = min ( colorMin, tex2D ( _AnamorphicRTS2, UnityStereoScreenSpaceUVAdjust ( input.uv, _MainTex_ST ) ) );
	colorMin = min ( colorMin, tex2D ( _AnamorphicRTS3, UnityStereoScreenSpaceUVAdjust ( input.uv, _MainTex_ST ) ) );
	return colorMin;
}

half4 frag_BokehComposition6S ( v2f_img input ) : SV_Target
{
	half4 colorMin = min ( tex2D ( _MainTex,UnityStereoScreenSpaceUVAdjust ( input.uv, _MainTex_ST ) ) , tex2D ( _AnamorphicRTS0, UnityStereoScreenSpaceUVAdjust ( input.uv, _MainTex_ST ) ) );
	colorMin = min ( colorMin, tex2D ( _AnamorphicRTS1, UnityStereoScreenSpaceUVAdjust ( input.uv, _MainTex_ST ) ) );
	colorMin = min ( colorMin, tex2D ( _AnamorphicRTS2, UnityStereoScreenSpaceUVAdjust ( input.uv, _MainTex_ST ) ) );
	colorMin = min ( colorMin, tex2D ( _AnamorphicRTS3, UnityStereoScreenSpaceUVAdjust ( input.uv, _MainTex_ST ) ) );
	colorMin = min ( colorMin, tex2D ( _AnamorphicRTS4, UnityStereoScreenSpaceUVAdjust ( input.uv, _MainTex_ST ) ) );
	return colorMin;
}

half4 frag_weightedAddPS1 ( v2f_img input ) : SV_Target
{
	half3 vColor = half3( 0,0,0 );
	vColor += _AnamorphicGlareWeights0 * DecodeColor ( tex2D ( _AnamorphicRTS0, UnityStereoScreenSpaceUVAdjust ( input.uv, _MainTex_ST ) ) );
	return EncodeColor ( vColor );
}

half4 frag_weightedAddPS2 ( v2f_img input ) : SV_Target
{
	half3 vColor = half3( 0,0,0 );
	vColor += _AnamorphicGlareWeights0 * DecodeColor ( tex2D ( _AnamorphicRTS0,  UnityStereoScreenSpaceUVAdjust ( input.uv, _MainTex_ST ) ) );
	vColor += _AnamorphicGlareWeights1 * DecodeColor ( tex2D ( _AnamorphicRTS1,  UnityStereoScreenSpaceUVAdjust ( input.uv, _MainTex_ST ) ) );
	return EncodeColor ( vColor );
}

half4 frag_weightedAddPS3 ( v2f_img input ) : SV_Target
{
	half3 vColor = half3( 0,0,0 );
	vColor += _AnamorphicGlareWeights0 * DecodeColor ( tex2D ( _AnamorphicRTS0,  UnityStereoScreenSpaceUVAdjust ( input.uv, _MainTex_ST ) ) );
	vColor += _AnamorphicGlareWeights1 * DecodeColor ( tex2D ( _AnamorphicRTS1,  UnityStereoScreenSpaceUVAdjust ( input.uv, _MainTex_ST ) ) );
	vColor += _AnamorphicGlareWeights2 * DecodeColor ( tex2D ( _AnamorphicRTS2,  UnityStereoScreenSpaceUVAdjust ( input.uv, _MainTex_ST ) ) );
	return EncodeColor ( vColor );
}

half4 frag_weightedAddPS4 ( v2f_img input ) : SV_Target
{
	half3 vColor = half3( 0,0,0 );
	vColor += _AnamorphicGlareWeights0 * DecodeColor ( tex2D ( _AnamorphicRTS0,  UnityStereoScreenSpaceUVAdjust ( input.uv, _MainTex_ST ) ) );
	vColor += _AnamorphicGlareWeights1 * DecodeColor ( tex2D ( _AnamorphicRTS1,  UnityStereoScreenSpaceUVAdjust ( input.uv, _MainTex_ST ) ) );
	vColor += _AnamorphicGlareWeights2 * DecodeColor ( tex2D ( _AnamorphicRTS2,  UnityStereoScreenSpaceUVAdjust ( input.uv, _MainTex_ST ) ) );
	vColor += _AnamorphicGlareWeights3 * DecodeColor ( tex2D ( _AnamorphicRTS3,  UnityStereoScreenSpaceUVAdjust ( input.uv, _MainTex_ST ) ) );
	return EncodeColor ( vColor );
}

half4 frag_weightedAddPS5 ( v2f_img input ) : SV_Target
{
	half3 vColor = half3( 0,0,0 );
	vColor += _AnamorphicGlareWeights0 * DecodeColor ( tex2D ( _AnamorphicRTS0,  UnityStereoScreenSpaceUVAdjust ( input.uv, _MainTex_ST ) ) );
	vColor += _AnamorphicGlareWeights1 * DecodeColor ( tex2D ( _AnamorphicRTS1,  UnityStereoScreenSpaceUVAdjust ( input.uv, _MainTex_ST ) ) );
	vColor += _AnamorphicGlareWeights2 * DecodeColor ( tex2D ( _AnamorphicRTS2,  UnityStereoScreenSpaceUVAdjust ( input.uv, _MainTex_ST ) ) );
	vColor += _AnamorphicGlareWeights3 * DecodeColor ( tex2D ( _AnamorphicRTS3,  UnityStereoScreenSpaceUVAdjust ( input.uv, _MainTex_ST ) ) );
	vColor += _AnamorphicGlareWeights4 * DecodeColor ( tex2D ( _AnamorphicRTS4,  UnityStereoScreenSpaceUVAdjust ( input.uv, _MainTex_ST ) ) );
	return EncodeColor ( vColor );
}

half4 frag_weightedAddPS6 ( v2f_img input ) : SV_Target
{

	half3 vColor = half3( 0,0,0 );
	vColor += _AnamorphicGlareWeights0 * DecodeColor ( tex2D ( _AnamorphicRTS0,  UnityStereoScreenSpaceUVAdjust ( input.uv, _MainTex_ST ) ) );
	vColor += _AnamorphicGlareWeights1 * DecodeColor ( tex2D ( _AnamorphicRTS1,  UnityStereoScreenSpaceUVAdjust ( input.uv, _MainTex_ST ) ) );
	vColor += _AnamorphicGlareWeights2 * DecodeColor ( tex2D ( _AnamorphicRTS2,  UnityStereoScreenSpaceUVAdjust ( input.uv, _MainTex_ST ) ) );
	vColor += _AnamorphicGlareWeights3 * DecodeColor ( tex2D ( _AnamorphicRTS3,  UnityStereoScreenSpaceUVAdjust ( input.uv, _MainTex_ST ) ) );
	vColor += _AnamorphicGlareWeights4 * DecodeColor ( tex2D ( _AnamorphicRTS4,  UnityStereoScreenSpaceUVAdjust ( input.uv, _MainTex_ST ) ) );
	vColor += _AnamorphicGlareWeights5 * DecodeColor ( tex2D ( _AnamorphicRTS5,  UnityStereoScreenSpaceUVAdjust ( input.uv, _MainTex_ST ) ) );
	return EncodeColor ( vColor );
}

half4 frag_weightedAddPS7 ( v2f_img input ) : SV_Target
{
	half3 vColor = half3( 0,0,0 );
	vColor += _AnamorphicGlareWeights0 * DecodeColor ( tex2D ( _AnamorphicRTS0,  UnityStereoScreenSpaceUVAdjust ( input.uv, _MainTex_ST ) ) );
	vColor += _AnamorphicGlareWeights1 * DecodeColor ( tex2D ( _AnamorphicRTS1,  UnityStereoScreenSpaceUVAdjust ( input.uv, _MainTex_ST ) ) );
	vColor += _AnamorphicGlareWeights2 * DecodeColor ( tex2D ( _AnamorphicRTS2,  UnityStereoScreenSpaceUVAdjust ( input.uv, _MainTex_ST ) ) );
	vColor += _AnamorphicGlareWeights3 * DecodeColor ( tex2D ( _AnamorphicRTS3,  UnityStereoScreenSpaceUVAdjust ( input.uv, _MainTex_ST ) ) );
	vColor += _AnamorphicGlareWeights4 * DecodeColor ( tex2D ( _AnamorphicRTS4,  UnityStereoScreenSpaceUVAdjust ( input.uv, _MainTex_ST ) ) );
	vColor += _AnamorphicGlareWeights5 * DecodeColor ( tex2D ( _AnamorphicRTS5,  UnityStereoScreenSpaceUVAdjust ( input.uv, _MainTex_ST ) ) );
	vColor += _AnamorphicGlareWeights6 * DecodeColor ( tex2D ( _AnamorphicRTS6,  UnityStereoScreenSpaceUVAdjust ( input.uv, _MainTex_ST ) ) );
	return EncodeColor ( vColor );
}

half4 frag_weightedAddPS8 ( v2f_img input ) : SV_Target
{
	half3 vColor = half3( 0,0,0 );
	vColor += _AnamorphicGlareWeights0 * DecodeColor ( tex2D ( _AnamorphicRTS0,  UnityStereoScreenSpaceUVAdjust ( input.uv, _MainTex_ST ) ) );
	vColor += _AnamorphicGlareWeights1 * DecodeColor ( tex2D ( _AnamorphicRTS1,  UnityStereoScreenSpaceUVAdjust ( input.uv, _MainTex_ST ) ) );
	vColor += _AnamorphicGlareWeights2 * DecodeColor ( tex2D ( _AnamorphicRTS2,  UnityStereoScreenSpaceUVAdjust ( input.uv, _MainTex_ST ) ) );
	vColor += _AnamorphicGlareWeights3 * DecodeColor ( tex2D ( _AnamorphicRTS3,  UnityStereoScreenSpaceUVAdjust ( input.uv, _MainTex_ST ) ) );
	vColor += _AnamorphicGlareWeights4 * DecodeColor ( tex2D ( _AnamorphicRTS4,  UnityStereoScreenSpaceUVAdjust ( input.uv, _MainTex_ST ) ) );
	vColor += _AnamorphicGlareWeights5 * DecodeColor ( tex2D ( _AnamorphicRTS5,  UnityStereoScreenSpaceUVAdjust ( input.uv, _MainTex_ST ) ) );
	vColor += _AnamorphicGlareWeights6 * DecodeColor ( tex2D ( _AnamorphicRTS6,  UnityStereoScreenSpaceUVAdjust ( input.uv, _MainTex_ST ) ) );
	vColor += _AnamorphicGlareWeights7 * DecodeColor ( tex2D ( _AnamorphicRTS7,  UnityStereoScreenSpaceUVAdjust ( input.uv, _MainTex_ST ) ) );
	return EncodeColor ( vColor );
}

half4 frag_anamorphicGlare ( v2f_img input ) : SV_Target
{
	return AnamorphicGlareMat ( input.uv,_MainTex );
}

half4 frag_lensFlare0 ( v2f_img input ) : SV_Target
{
	return CalcLensFlare ( 0,_MainTex_TexelSize.xy, input.uv , _MainTex );
}

half4 frag_lensFlare1 ( v2f_img input ) : SV_Target
{
	return CalcLensFlare ( 1,_MainTex_TexelSize.xy, input.uv , _MainTex );
}

half4 frag_lensFlare2 ( v2f_img input ) : SV_Target
{
	return CalcLensFlare ( 2,_MainTex_TexelSize.xy, input.uv , _MainTex );
}

half4 frag_lensFlare3 ( v2f_img input ) : SV_Target
{
	return CalcLensFlare ( 3,_MainTex_TexelSize.xy, input.uv , _MainTex );
}

half4 frag_lensFlare4 ( v2f_img input ) : SV_Target
{
	return CalcLensFlare ( 4,_MainTex_TexelSize.xy, input.uv , _MainTex );
}

half4 frag_lensFlare5 ( v2f_img input ) : SV_Target
{
	return CalcLensFlare ( 5,_MainTex_TexelSize.xy, input.uv , _MainTex );
}

half4 frag_downsampler_with_karis ( v2f_img input ) : SV_Target
{
	return DownsampleWithKaris ( input.uv, _MainTex_TexelSize.xy, _MainTex );
}

half4 frag_downsampler_without_karis ( v2f_img input ) : SV_Target
{
	return DownsampleWithoutKaris ( input.uv, _MainTex_TexelSize.xy, _MainTex );
}

half4 frag_downsampler_temp_filter_with_karis ( v2f_img input ) : SV_Target
{
	half4 currentColor = DownsampleWithKaris ( input.uv, _MainTex_TexelSize.xy, _MainTex );
	half4 prevColor = tex2D ( _AnamorphicRTS0, UnityStereoScreenSpaceUVAdjust ( input.uv, _MainTex_ST ) );
	return lerp ( currentColor, prevColor, _TempFilterValue );
}

half4 frag_downsampler_temp_filter_without_karis ( v2f_img input ) : SV_Target
{
	half4 currentColor = DownsampleWithoutKaris ( input.uv, _MainTex_TexelSize.xy, _MainTex );
	half4 prevColor = tex2D ( _AnamorphicRTS0, UnityStereoScreenSpaceUVAdjust ( input.uv, _MainTex_ST ) );
	return lerp ( currentColor, prevColor, _TempFilterValue );
}

half4 frag_downsamplerNoWeightedAvg ( v2f_img input ) : SV_Target
{
	return DownsampleNoWeightedAvg ( input.uv, _MainTex_TexelSize.xy, _MainTex );
}

half4 frag_horizontal_gaussian_blur ( v2f_img input ) : SV_Target
{
	return NineTapGaussian ( input.uv, _MainTex,float2( _BlurRadius*_MainTex_TexelSize.x, 0 ) );
}

half4 frag_vertical_gaussian_blur ( v2f_img input ) : SV_Target
{
	return NineTapGaussian ( input.uv, _MainTex,float2( 0, _BlurRadius*_MainTex_TexelSize.y ) );
}

half4 frag_vertical_gaussian_blur_temp_filter ( v2f_img input ) : SV_Target
{
	half4 currentColor = NineTapGaussian ( input.uv, _MainTex, float2( 0, _BlurRadius*_MainTex_TexelSize.y ) );
	half4 prevColor = tex2D ( _AnamorphicRTS0, UnityStereoScreenSpaceUVAdjust ( input.uv, _MainTex_ST ) );
	return lerp ( currentColor, prevColor, _TempFilterValue );
}

half4 frag_upscaleTentFirstPass ( v2f_img input ) : SV_Target
{
	return FirstPassUpscaleBlurTent ( _MainTex, input.uv, _MainTex_TexelSize.xy, _BloomParams.z );
}

half4 frag_upscaleTent ( v2f_img input ) : SV_Target
{
	return UpscaleBlurTent ( _MainTex, _AnamorphicRTS0,input.uv, _MainTex_TexelSize.xy, _BloomParams.z );
}

half4 frag_add ( v2f_img input ) : SV_Target
{
	return ( tex2D ( _MainTex, UnityStereoScreenSpaceUVAdjust ( input.uv, _MainTex_ST ) ) + tex2D ( _AnamorphicRTS0, UnityStereoScreenSpaceUVAdjust ( input.uv, _MainTex_ST ) ) );
}

#endif
