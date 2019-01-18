Shader "Hide/KawaseBlurPostProcess"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
	}
	SubShader
	{
		ZTest Always Cull Off ZWrite Off
		Fog { Mode off }
		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			
			#include "UnityCG.cginc"

			struct v2f
			{
				float4 vertex : SV_POSITION;
				fixed2 convolution[4] : TEXCOORD0;
			};

			sampler2D _MainTex;
			float4 _MainTex_ST;
			fixed2 _MainTex_TexelSize;
			fixed _Offset;

			v2f vert (appdata_base v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				fixed2 uv = TRANSFORM_TEX(v.texcoord, _MainTex);//center
				//_MainTex_TexelSize *= 0.5;
				o.convolution[0] = uv + _MainTex_TexelSize * fixed2(_Offset+0.5,_Offset+0.5) ;//top right
				o.convolution[1] = uv + _MainTex_TexelSize * fixed2(-_Offset-0.5,-_Offset-0.5) ;//bottom left
				o.convolution[2] = uv + _MainTex_TexelSize * fixed2(-_Offset-0.5,_Offset+0.5) ;//top left
				o.convolution[3] = uv + _MainTex_TexelSize * fixed2(_Offset+0.5,-_Offset-0.5) ;//bottom right

				return o;
			}

			fixed4 frag (v2f i) : SV_Target{
				fixed4 sum	= 0;
				sum += tex2D(_MainTex, i.convolution[0]);
				sum += tex2D(_MainTex, i.convolution[1]);
				sum += tex2D(_MainTex, i.convolution[2]);
				sum += tex2D(_MainTex, i.convolution[3]);

				return sum * 0.25;
			}
			ENDCG
		}
	}
}
