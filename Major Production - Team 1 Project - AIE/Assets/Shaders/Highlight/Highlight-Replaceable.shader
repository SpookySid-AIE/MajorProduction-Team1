////////////////////////////////////////////////////////////
// Author: <Jak Revai>                                     
// Date Created: <16/02/18>                               
// Brief: <This is just the standard shader with the highlight tags, so we can replace it on the CIVS>
// This is the one that will be placed on the Civs, when they enter Lure, then is replaced with "Highlight"
// Tutorial link i followed: https://www.youtube.com/watch?v=OJkGGuudm38
////////////////////////////////////////////////////////////

Shader "Jak's Shaders/Highlight-Replaceable"
{
	Properties
	{
		_Color("Main Color", Color) = (1,1,1,1)
		_HighlightColour("Highlight Colour", Color) = (0,0,0,0)
		_MainTex("Base (RGB)", 2D) = "white" {}
	}

	SubShader
	{
		Tags
		{
			"Queue" = "Geometry-1" //Note: Forcing the highlight to render before other geometry so the depth info is already stored
			"RenderType" = "Opaque"
			"EdgeHighlight" = "Highlight"
		}
		LOD 200

		CGPROGRAM
		#pragma surface surf Lambert

		sampler2D _MainTex;
		fixed4 _Color;

		struct Input {
			float2 uv_MainTex;
		};

		void surf(Input IN, inout SurfaceOutput o)
		{
			fixed4 c = tex2D(_MainTex, IN.uv_MainTex) * _Color;
			o.Albedo = c.rgb;
			o.Alpha = c.a;
		}
		ENDCG
	}
	
	Fallback "Legacy Shaders/VertexLit"
}
