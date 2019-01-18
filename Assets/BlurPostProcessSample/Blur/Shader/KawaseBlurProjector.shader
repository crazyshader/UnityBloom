Shader "Blur/KawaseBlurProjector"
{
	SubShader
	{
		Tags { "Queue"="Transparent" "RenderType"="Opaque" }

		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			
			#include "UnityCG.cginc"
			struct v2f
			{
				float4 uv : TEXCOORD0;
				float4 vertex : SV_POSITION;
			};

			sampler2D _GrabBlurTexture;
			fixed4 _GrabBlurTexture_ST;
			
			v2f vert (appdata_base v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = ComputeGrabScreenPos(o.vertex);
				return o;
			}

			fixed4 frag (v2f i) : SV_Target
			{
				fixed4 col = tex2Dproj (_GrabBlurTexture, UNITY_PROJ_COORD(i.uv));
				return col;
			}
			ENDCG
		}
	}
}
