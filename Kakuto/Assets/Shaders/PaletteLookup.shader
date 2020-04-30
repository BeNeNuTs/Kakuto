Shader "Sprites/PaletteLookup"
{
	Properties
	{
		_Color("Tint", Color) = (0, 0, 0, 1)
		_MainTex("Texture", 2D) = "white" {}
		_PaletteTex("Texture", 2D) = "white" {}
	}

	SubShader
	{
		Tags
		{
			"RenderType" = "Transparent"
			"Queue" = "Transparent"
		}

		Blend SrcAlpha OneMinusSrcAlpha

		ZWrite off
		Cull off

		Pass
		{
			CGPROGRAM

			#include "UnityCG.cginc"

			#pragma vertex vert
			#pragma fragment frag

			sampler2D _MainTex;
			float4 _MainTex_ST;

			fixed4 _Color;

			sampler2D _PaletteTex;

			struct appdata 
			{
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
				fixed4 color : COLOR;
			};

			struct v2f 
			{
				float4 position : SV_POSITION;
				float2 uv : TEXCOORD0;
				fixed4 color : COLOR;
			};

			v2f vert(appdata v) 
			{
				v2f o;
				o.position = UnityObjectToClipPos(v.vertex);
				o.uv = TRANSFORM_TEX(v.uv, _MainTex);
				o.color = v.color;
				return o;
			}

			fixed4 frag(v2f i) : SV_TARGET
			{
				float4 col;
				float x = tex2D(_MainTex, i.uv).r;
				col.rgb = tex2D(_PaletteTex, float2(x, 0)).rgb;
				col.a = tex2D(_MainTex, i.uv).a;

				col *= _Color;
				col *= i.color;
				return col;
			}

			ENDCG
		}
	}
}