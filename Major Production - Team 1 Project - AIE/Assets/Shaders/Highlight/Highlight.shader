////////////////////////////////////////////////////////////
// Author: <Jak Revai>                                     
// Date Created: <16/02/18>                               
// Brief: <Replacement Shader that replaces "Highlight-Replacable" on the CIVS>  
// Tutorial link i followed: https://www.youtube.com/watch?v=OJkGGuudm38
////////////////////////////////////////////////////////////

Shader "Jak's Shaders/Highlight"
{
	Properties
	{
		_HighlightColor("Highlight Color", Color) = (1,1,1,1)
	}

	SubShader
	{
		//Using the stencil buffer to render the outline through walls - disabled for now
		//Stencil
		//{
		//	Ref 0
		//	Comp NotEqual
		//}

		Tags
		{
			"Queue" = "Transparent"
			"RenderType" = "Transparent"
			"EdgeHighlight" = "Highlight"
		}

		ZWrite Off
		ZTest Always
		Blend One One

		Pass
		{

			CGPROGRAM

			#pragma vertex vert
			#pragma fragment frag
			
			#include "UnityCG.cginc"

			struct appdata
			{
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
				float3 normal : NORMAL;
			};

			struct v2f
			{
				float2 uv : TEXCOORD0;
				float4 vertex : SV_POSITION;
				float3 normal : NORMAL;
				float3 viewDir : TEXCOORD1;
			};

			v2f vert (appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = v.uv;
				o.normal = UnityObjectToWorldNormal(v.normal);
				o.viewDir = normalize(_WorldSpaceCameraPos.xyz - mul(unity_ObjectToWorld, v.vertex).xyz);
				return o;
			}

			float4 _EdgeColor;

			fixed4 frag (v2f i) : SV_Target
			{
				float NdotV = 1 - dot(i.normal, i.viewDir) * 1.0;
				return _EdgeColor * NdotV;
			}

			ENDCG
		}
	}
}
