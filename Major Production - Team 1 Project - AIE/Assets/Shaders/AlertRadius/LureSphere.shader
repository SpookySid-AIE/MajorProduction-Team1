////////////////////////////////////////////////////////////
// Author: <Jak Revai>                                     
// Date Created: <17/02/18>                               
// Brief: <Shader to attach to an empty uv sphere that generates a transparent sphere of the mesh with glowy intersects when the sphere collides>
// Tutorial i followed: https://www.youtube.com/watch?v=C6lGEgcHbWc
////////////////////////////////////////////////////////////

Shader "Jak's Shaders/LureSphere"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
		_Color ("Color", Color) = (0,0,0,0)
		_ScrollSpeedU("Scroll U Speed",float) = 2
		_ScrollSpeedV("Scroll V Speed",float) = 0
	}

	SubShader
	{
		Blend One One
		ZWrite Off
		Cull Off

		Tags
		{
			"RenderType"="Transparent"
			"Queue"="Transparent"
		}

		Pass
		{
			CGPROGRAM
			#pragma target 3.0
			#pragma vertex vert
			#pragma fragment frag

			#include "UnityCG.cginc"

			struct appdata
			{
				float4 vertex : POSITION;
				float3 normal : NORMAL;
				fixed3 uv : TEXCOORD0;
			};

			struct v2f
			{
				fixed2 uv : TEXCOORD0;
				float2 screenuv : TEXCOORD1;
				float3 viewDir : TEXCOORD2;
				float3 objectPos : TEXCOORD3;
				float4 vertex : SV_POSITION;
				float depth : DEPTH;
				float3 normal : NORMAL;
			};

			sampler2D _MainTex;
			float4 _MainTex_ST;
			fixed _ScrollSpeedU, _ScrollSpeedV;

			v2f vert (appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = TRANSFORM_TEX(v.uv, _MainTex);

				//scroll uv
				o.uv.x += _Time * _ScrollSpeedU;
				o.uv.y += _Time * _ScrollSpeedV;

				o.screenuv = ((o.vertex.xy / o.vertex.w) + 1)/2;
				o.screenuv.y = 1 - o.screenuv.y;

				//Accessing the farplane of the camera to use in the intersect glow
				o.depth = -UnityObjectToViewPos(v.vertex).z * _ProjectionParams.w;

				o.objectPos = v.vertex.xyz;		
				o.normal = UnityObjectToWorldNormal(v.normal);
				o.viewDir = normalize(UnityWorldSpaceViewDir(mul(unity_ObjectToWorld, v.vertex)));

				return o;
			}
			
			sampler2D _CameraDepthNormalsTexture;
			fixed4 _Color;

			//Move the texture around the sphere
			fixed4 texColor(v2f i)
			{
				fixed4 mainTex = tex2D(_MainTex, i.uv);			
				mainTex.g *= (sin(_Time.z + mainTex.b) + 1);
				return mainTex.r * _Color * 0.05 + mainTex.g * _Color * 0.5;
											//Magic numbers that affect how bright texture glows to
			}

			fixed4 frag (v2f i) : SV_Target
			{
				//Main depth buffer code to create the intersect for the glow
				float screenDepth = DecodeFloatRG(tex2D(_CameraDepthNormalsTexture, i.screenuv).zw);
				float diff = screenDepth - i.depth;
				float intersect = 0;
				
				fixed3 main = tex2D(_MainTex, i.uv);

				if (diff > 0)
					intersect = 1 - smoothstep(0, _ProjectionParams.w * 0.5, diff);

				float rim = 1 - abs(dot(i.normal, normalize(i.viewDir))) * 2;
											
											//Controls the transparency of the rim glow
				float glow = max(intersect, 0.05); //rim

				fixed4 glowColor = fixed4(lerp(_Color.rgb, fixed3(1, 1, 1), pow(glow, 4)), 1);

				fixed4 tex = texColor(i);

				fixed4 col = _Color * _Color.a + glowColor * glow + tex;
				//main *= col;
				return col;
			}
			ENDCG
		}
	}
}
