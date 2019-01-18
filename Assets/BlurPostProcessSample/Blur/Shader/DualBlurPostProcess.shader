Shader "Hide/DualBlurPostProcess"
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
			Name "DownSample"
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
			fixed2 _MainTex_TexelSize;
			fixed _Offset;

			v2f vert (appdata_base v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				fixed2 uv = TRANSFORM_TEX(v.texcoord, _MainTex);//center
				_MainTex_TexelSize *= 0.5;
				o.convolution[0] = uv;
				o.convolution[1] = uv - _MainTex_TexelSize * fixed2(1+_Offset,1+_Offset);//top right
				o.convolution[2] = uv + _MainTex_TexelSize * fixed2(1+_Offset,1+_Offset);//bottom left
				o.convolution[3] = uv - fixed2(_MainTex_TexelSize.x,-_MainTex_TexelSize.y) * fixed2(1+_Offset,1+_Offset);//top left
				o.convolution[4] = uv + fixed2(_MainTex_TexelSize.x,-_MainTex_TexelSize.y) * fixed2(1+_Offset,1+_Offset);//bottom right

				return o;
			}

			fixed4 frag (v2f i) : SV_Target{
				fixed4 sum = tex2D(_MainTex, i.convolution[0]) * 4;
				sum += tex2D(_MainTex, i.convolution[1]);
				sum += tex2D(_MainTex, i.convolution[2]);
				sum += tex2D(_MainTex, i.convolution[3]);
				sum += tex2D(_MainTex, i.convolution[4]);

				return sum * 0.125;
			}
			ENDCG
		}

		Pass
		{
			Name "UpSample"
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			
			#include "UnityCG.cginc"

			struct v2f
			{
				float4 vertex : SV_POSITION;
				fixed2 convolution[8] : TEXCOORD0;
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
				_MainTex_TexelSize *= 0.5;
				_Offset = fixed2(1+_Offset,1+_Offset);

				o.convolution[0] = uv + fixed2(-_MainTex_TexelSize.x * 2 ,0) * _Offset;
				o.convolution[1] = uv + fixed2(-_MainTex_TexelSize.x,_MainTex_TexelSize.y) * _Offset ;
				o.convolution[2] = uv + fixed2(0,_MainTex_TexelSize.y * 2) * _Offset;
				o.convolution[3] = uv + _MainTex_TexelSize * _Offset;
				o.convolution[4] = uv + fixed2(_MainTex_TexelSize.x * 2,0) * _Offset;
				o.convolution[5] = uv + fixed2(_MainTex_TexelSize.x,-_MainTex_TexelSize.y) * _Offset;
				o.convolution[6] = uv + fixed2(0,-_MainTex_TexelSize.y * 2) * _Offset;
				o.convolution[7] = uv -	_MainTex_TexelSize * _Offset;

				return o;
			}

			fixed4 frag (v2f i) : SV_Target{
				fixed4 sum = 0;
				sum += tex2D(_MainTex, i.convolution[0]);
				sum += tex2D(_MainTex, i.convolution[1]) * 2;
				sum += tex2D(_MainTex, i.convolution[2]);
				sum += tex2D(_MainTex, i.convolution[3]) * 2;
				sum += tex2D(_MainTex, i.convolution[4]);
				sum += tex2D(_MainTex, i.convolution[5]) * 2;
				sum += tex2D(_MainTex, i.convolution[6]);
				sum += tex2D(_MainTex, i.convolution[7]) * 2;

				return sum * 0.0833;
			}
			ENDCG
		}

	}
}
