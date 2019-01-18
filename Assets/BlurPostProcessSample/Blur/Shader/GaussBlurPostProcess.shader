Shader "Hide/GaussBlurPostProcess"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
	}
	SubShader
	{
		Tags { "RenderType"="Opaque" }

		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			
			#include "UnityCG.cginc"

			struct v2f
			{
				float4 vertex : SV_POSITION;
				fixed2 convolution[5] : TEXCOORD0;
			};

			sampler2D _MainTex;
			float4 _MainTex_ST;
			fixed2 _MainTex_TexelSize,_Offset;
			fixed _Weights[3],_Totalweight;
			
			//Guass math
			//fixed CaculateWeight(fixed r){
			//	return 0.39894 * sigma * sigma * pow(2.718,-r * r / (2 * sigma * sigma) );
			//}

			v2f vert (appdata_base v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.convolution[0] = TRANSFORM_TEX(v.texcoord, _MainTex);
				o.convolution[1] = o.convolution[0] + _MainTex_TexelSize * _Offset;
				o.convolution[2] = o.convolution[0] - _MainTex_TexelSize * _Offset;
				o.convolution[3] = o.convolution[0] + _MainTex_TexelSize * fixed2(2,2) * _Offset;
				o.convolution[4] = o.convolution[0] - _MainTex_TexelSize * fixed2(2,2) * _Offset;

				return o;
			}

			fixed4 frag (v2f i) : SV_Target{
				fixed4 sum = 0;
				sum += _Weights[0] * tex2D(_MainTex, i.convolution[0]) / _Totalweight;
				sum += _Weights[1] * tex2D(_MainTex, i.convolution[1]) / _Totalweight;
				sum += _Weights[1] * tex2D(_MainTex, i.convolution[2]) / _Totalweight;
				sum += _Weights[2] * tex2D(_MainTex, i.convolution[3]) / _Totalweight;
				sum += _Weights[2] * tex2D(_MainTex, i.convolution[4]) / _Totalweight;

				return sum;
			}
			ENDCG
		}
	}
}
