// Made with Amplify Shader Editor
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "Custom/Civilian1 Toon"
{
	Properties
	{
		_ASEOutlineColor( "Outline Color", Color ) = (0,0,0,0)
		_ASEOutlineWidth( "Outline Width", Float ) = 0.0025
		_MainColour("Main Colour", 2D) = "white" {}
		_ColourMasks("Colour Masks", 2D) = "white" {}
		_Top2Colour("Top2 Colour", Color) = (0,0,0,0)
		_Top1Colour("Top1 Colour", Color) = (0,0,0,0)
		_PantsColour("Pants Colour", Color) = (0,0,0,0)
		[HideInInspector] _texcoord( "", 2D ) = "white" {}
		[HideInInspector] __dirty( "", Int ) = 1
	}

	SubShader
	{
		Tags{ }
		Cull Front
		CGPROGRAM
		#pragma target 3.0
		#pragma surface outlineSurf Outline nofog keepalpha noshadow noambient novertexlights nolightmap nodynlightmap nodirlightmap nometa noforwardadd vertex:outlineVertexDataFunc 
		struct Input {
			fixed filler;
		};
		uniform fixed4 _ASEOutlineColor;
		uniform fixed _ASEOutlineWidth;
		void outlineVertexDataFunc( inout appdata_full v, out Input o )
		{
			UNITY_INITIALIZE_OUTPUT( Input, o );
			v.vertex.xyz += ( v.normal * _ASEOutlineWidth );
		}
		inline fixed4 LightingOutline( SurfaceOutput s, half3 lightDir, half atten ) { return fixed4 ( 0,0,0, s.Alpha); }
		void outlineSurf( Input i, inout SurfaceOutput o )
		{
			o.Emission = _ASEOutlineColor.rgb;
			o.Alpha = 1;
		}
		ENDCG
		

		Tags{ "RenderType" = "Opaque"  "Queue" = "Geometry+0" "IsEmissive" = "true"  }
		Cull Back
		CGPROGRAM
		#pragma target 3.0
		#pragma surface surf Unlit keepalpha addshadow fullforwardshadows 
		struct Input
		{
			float2 uv_texcoord;
		};

		uniform sampler2D _MainColour;
		uniform float4 _MainColour_ST;
		uniform float4 _PantsColour;
		uniform sampler2D _ColourMasks;
		uniform float4 _ColourMasks_ST;
		uniform float4 _Top1Colour;
		uniform float4 _Top2Colour;

		inline fixed4 LightingUnlit( SurfaceOutput s, half3 lightDir, half atten )
		{
			return fixed4 ( 0, 0, 0, s.Alpha );
		}

		void surf( Input i , inout SurfaceOutput o )
		{
			float2 uv_MainColour = i.uv_texcoord * _MainColour_ST.xy + _MainColour_ST.zw;
			float4 tex2DNode1 = tex2D( _MainColour, uv_MainColour );
			float4 blendOpSrc17 = tex2DNode1;
			float4 blendOpDest17 = _PantsColour;
			float2 uv_ColourMasks = i.uv_texcoord * _ColourMasks_ST.xy + _ColourMasks_ST.zw;
			float4 tex2DNode2 = tex2D( _ColourMasks, uv_ColourMasks );
			float4 lerpResult19 = lerp( tex2DNode1 , ( saturate( (( blendOpSrc17 > 0.5 ) ? ( blendOpDest17 / ( ( 1.0 - blendOpSrc17 ) * 2.0 ) ) : ( 1.0 - ( ( ( 1.0 - blendOpDest17 ) * 0.5 ) / blendOpSrc17 ) ) ) )) , tex2DNode2.r);
			float4 blendOpSrc20 = tex2DNode1;
			float4 blendOpDest20 = _Top1Colour;
			float4 lerpResult22 = lerp( lerpResult19 , ( saturate( (( blendOpSrc20 > 0.5 ) ? ( blendOpDest20 / ( ( 1.0 - blendOpSrc20 ) * 2.0 ) ) : ( 1.0 - ( ( ( 1.0 - blendOpDest20 ) * 0.5 ) / blendOpSrc20 ) ) ) )) , tex2DNode2.g);
			float4 blendOpSrc24 = tex2DNode1;
			float4 blendOpDest24 = _Top2Colour;
			float4 lerpResult23 = lerp( lerpResult22 , ( saturate( (( blendOpSrc24 > 0.5 ) ? ( blendOpDest24 / ( ( 1.0 - blendOpSrc24 ) * 2.0 ) ) : ( 1.0 - ( ( ( 1.0 - blendOpDest24 ) * 0.5 ) / blendOpSrc24 ) ) ) )) , tex2DNode2.b);
			o.Emission = lerpResult23.rgb;
			o.Alpha = 1;
		}

		ENDCG
	}
	Fallback "Diffuse"
	CustomEditor "ASEMaterialInspector"
}
/*ASEBEGIN
Version=14201
245;238;1485;967;1440.974;811.7562;1.708703;True;True
Node;AmplifyShaderEditor.SamplerNode;1;-727.7571,-245.3947;Float;True;Property;_MainColour;Main Colour;0;0;Create;None;ad810fac74d0185479af096d5d3ab3a7;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0.0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1.0;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.ColorNode;4;-762.3035,-625.7952;Float;False;Property;_PantsColour;Pants Colour;4;0;Create;0,0,0,0;0.6911765,0.5692042,0.622199,0;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.BlendOpsNode;17;-305.5965,-427.6575;Float;True;VividLight;True;2;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SamplerNode;2;-804.717,20.02719;Float;True;Property;_ColourMasks;Colour Masks;1;0;Create;None;453fd8b8a497b3a47876f96bead21d67;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0.0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1.0;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.ColorNode;21;-647.3002,302.2003;Float;False;Property;_Top1Colour;Top1 Colour;3;0;Create;0,0,0,0;0.5681288,0.7132353,0.403817,0;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.LerpOp;19;-19.03541,-219.1779;Float;True;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0.0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.ColorNode;25;-756.8785,563.6813;Float;False;Property;_Top2Colour;Top2 Colour;2;0;Create;0,0,0,0;0,0.7205882,0.1242394,0;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.BlendOpsNode;20;-304.2909,174.715;Float;True;VividLight;True;2;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.LerpOp;22;274.6083,-63.09862;Float;True;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.BlendOpsNode;24;-345.5751,739.5266;Float;True;VividLight;True;2;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.LerpOp;23;575.2332,249.2603;Float;True;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.StandardSurfaceOutputNode;0;953.9459,-79.28225;Float;False;True;2;Float;ASEMaterialInspector;0;0;Unlit;Custom/Civilian1 Toon;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;Back;0;0;False;0;0;Opaque;0.5;True;True;0;False;Opaque;Geometry;All;True;True;True;True;True;True;True;True;True;True;True;True;True;True;True;True;True;False;0;255;255;0;0;0;0;0;0;0;0;False;2;15;10;25;False;0.5;True;0;Zero;Zero;0;Zero;Zero;OFF;OFF;0;True;0.0025;0,0,0,0;VertexOffset;True;False;Cylindrical;False;Relative;0;;-1;-1;-1;-1;0;0;0;False;0;0;15;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;3;FLOAT;0.0;False;4;FLOAT;0.0;False;6;FLOAT3;0,0,0;False;7;FLOAT3;0,0,0;False;8;FLOAT;0.0;False;9;FLOAT;0.0;False;10;FLOAT;0.0;False;13;FLOAT3;0,0,0;False;11;FLOAT3;0,0,0;False;12;FLOAT3;0,0,0;False;14;FLOAT4;0,0,0,0;False;15;FLOAT3;0,0,0;False;0
WireConnection;17;0;1;0
WireConnection;17;1;4;0
WireConnection;19;0;1;0
WireConnection;19;1;17;0
WireConnection;19;2;2;1
WireConnection;20;0;1;0
WireConnection;20;1;21;0
WireConnection;22;0;19;0
WireConnection;22;1;20;0
WireConnection;22;2;2;2
WireConnection;24;0;1;0
WireConnection;24;1;25;0
WireConnection;23;0;22;0
WireConnection;23;1;24;0
WireConnection;23;2;2;3
WireConnection;0;2;23;0
ASEEND*/
//CHKSM=75DE14CCDB67A768719EF22F259CCA5A7BB688F9