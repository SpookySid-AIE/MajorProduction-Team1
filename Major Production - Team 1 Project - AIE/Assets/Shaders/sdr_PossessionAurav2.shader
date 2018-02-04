// Made with Amplify Shader Editor
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "Custom/Possession Aura2"
{
	Properties
	{
		_ASEOutlineColor( "Outline Color", Color ) = (0,0,0,0)
		_ASEOutlineWidth( "Outline Width", Float ) = 0.0001
		_MainColor("Main Color", Color) = (1,1,1,0)
		_ObjectTexture("Object Texture", 2D) = "white" {}
		_AuraTexture("Aura Texture", 2D) = "white" {}
		_AuraTexture2("Aura Texture 2", 2D) = "white" {}
		_AuraColour("Aura Colour", Color) = (0,0.8758621,1,0)
		_AuraBrightness("Aura Brightness", Range( 0 , 10)) = 4
		_AuraFalloff1("Aura Falloff 1", Range( 0 , 5)) = 1.87
		_AuraFalloff2("Aura Falloff 2", Range( 0 , 5)) = 0.68
		_RimPower("Rim Power", Range( 0 , 10)) = 3
		_RimPulse("Rim Pulse", Range( 0 , 20)) = 1.1
		[Toggle]_InvertAura("Invert Aura", Float) = 1
		[Toggle]_AuraOnOff("Aura OnOff", Float) = 1
		_AuraTextureSize("Aura Texture Size", Float) = 1
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
		CGINCLUDE
		#include "UnityShaderVariables.cginc"
		#include "UnityPBSLighting.cginc"
		#include "Lighting.cginc"
		#pragma target 3.0
		struct Input
		{
			float2 uv_texcoord;
			float3 worldPos;
			float3 worldNormal;
		};

		uniform float _AuraOnOff;
		uniform sampler2D _ObjectTexture;
		uniform float4 _ObjectTexture_ST;
		uniform float4 _MainColor;
		uniform float4 _AuraColour;
		uniform float _RimPower;
		uniform float _RimPulse;
		uniform float _AuraFalloff2;
		uniform float _AuraFalloff1;
		uniform float _InvertAura;
		uniform sampler2D _AuraTexture;
		uniform float _AuraTextureSize;
		uniform sampler2D _AuraTexture2;
		uniform float _AuraBrightness;

		void surf( Input i , inout SurfaceOutputStandard o )
		{
			float2 uv_ObjectTexture = i.uv_texcoord * _ObjectTexture_ST.xy + _ObjectTexture_ST.zw;
			float4 blendOpSrc141 = tex2D( _ObjectTexture, uv_ObjectTexture );
			float4 blendOpDest141 = _MainColor;
			float4 temp_output_141_0 = ( saturate( ( blendOpSrc141 * blendOpDest141 ) ));
			float3 ase_worldPos = i.worldPos;
			float3 ase_worldViewDir = normalize( UnityWorldSpaceViewDir( ase_worldPos ) );
			float3 ase_worldNormal = i.worldNormal;
			float fresnelNDotV124 = dot( ase_worldNormal, ase_worldViewDir );
			float fresnelNode124 = ( 0.0 + 1.0 * pow( 1.0 - fresnelNDotV124, (10.0 + (( _RimPower * (0.6 + (sin( ( _Time.y * _RimPulse ) ) - 0.0) * (2.0 - 0.6) / (3.0 - 0.0)) ) - 0.0) * (0.0 - 10.0) / (10.0 - 0.0)) ) );
			float3 ase_vertex3Pos = mul( unity_WorldToObject, float4( i.worldPos , 1 ) );
			float temp_output_100_0 = ( -ase_vertex3Pos.y - 1.0 );
			float temp_output_98_0 = ( -ase_vertex3Pos.y + 1.0 );
			float lerpResult103 = lerp( temp_output_100_0 , temp_output_98_0 , ( _AuraFalloff2 / _AuraFalloff1 ));
			float clampResult105 = clamp( lerpResult103 , 0.0 , 5.0 );
			float lerpResult99 = lerp( temp_output_100_0 , temp_output_98_0 , _AuraFalloff2);
			float clampResult96 = clamp( lerpResult99 , 0.0 , 5.0 );
			float2 uv_TexCoord153 = i.uv_texcoord * float2( 1,1 ) + float2( 0,0 );
			float2 panner107 = ( uv_TexCoord153 + _Time.x * float2( 0.61,1.5 ));
			float2 temp_output_151_0 = (panner107*_AuraTextureSize + 0.0);
			float2 panner8 = ( temp_output_151_0 + _Time.x * float2( -0.64,-6.54 ));
			float blendOpSrc119 = tex2D( _AuraTexture, (panner8*_AuraTextureSize + 0.0) ).r;
			float blendOpDest119 = tex2D( _AuraTexture2, temp_output_151_0 ).r;
			float temp_output_128_0 = (0.0 + (( saturate(  (( blendOpSrc119 > 0.5 ) ? ( 1.0 - ( 1.0 - 2.0 * ( blendOpSrc119 - 0.5 ) ) * ( 1.0 - blendOpDest119 ) ) : ( 2.0 * blendOpSrc119 * blendOpDest119 ) ) )) - 0.2) * (1.0 - 0.0) / (0.8 - 0.2));
			float clampResult130 = clamp( temp_output_128_0 , 0.0 , 1.0 );
			float4 lerpResult21 = lerp( temp_output_141_0 , _AuraColour , ( ( fresnelNode124 + ( clampResult105 + ( clampResult96 * lerp(clampResult130,(1.0 + (temp_output_128_0 - 0.0) * (0.0 - 1.0) / (1.0 - 0.0)),_InvertAura) ) ) ) * _AuraBrightness ));
			o.Emission = lerp(temp_output_141_0,lerpResult21,_AuraOnOff).rgb;
			o.Alpha = 1;
		}

		ENDCG
		CGPROGRAM
		#pragma surface surf Standard keepalpha fullforwardshadows 

		ENDCG
		Pass
		{
			Name "ShadowCaster"
			Tags{ "LightMode" = "ShadowCaster" }
			ZWrite On
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma target 3.0
			#pragma multi_compile_shadowcaster
			#pragma multi_compile UNITY_PASS_SHADOWCASTER
			#pragma skip_variants FOG_LINEAR FOG_EXP FOG_EXP2
			#include "HLSLSupport.cginc"
			#if ( SHADER_API_D3D11 || SHADER_API_GLCORE || SHADER_API_GLES3 || SHADER_API_METAL || SHADER_API_VULKAN )
				#define CAN_SKIP_VPOS
			#endif
			#include "UnityCG.cginc"
			#include "Lighting.cginc"
			#include "UnityPBSLighting.cginc"
			struct v2f
			{
				V2F_SHADOW_CASTER;
				float2 customPack1 : TEXCOORD1;
				float3 worldPos : TEXCOORD2;
				float3 worldNormal : TEXCOORD3;
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};
			v2f vert( appdata_full v )
			{
				v2f o;
				UNITY_SETUP_INSTANCE_ID( v );
				UNITY_INITIALIZE_OUTPUT( v2f, o );
				UNITY_TRANSFER_INSTANCE_ID( v, o );
				Input customInputData;
				float3 worldPos = mul( unity_ObjectToWorld, v.vertex ).xyz;
				fixed3 worldNormal = UnityObjectToWorldNormal( v.normal );
				o.worldNormal = worldNormal;
				o.customPack1.xy = customInputData.uv_texcoord;
				o.customPack1.xy = v.texcoord;
				o.worldPos = worldPos;
				TRANSFER_SHADOW_CASTER_NORMALOFFSET( o )
				return o;
			}
			fixed4 frag( v2f IN
			#if !defined( CAN_SKIP_VPOS )
			, UNITY_VPOS_TYPE vpos : VPOS
			#endif
			) : SV_Target
			{
				UNITY_SETUP_INSTANCE_ID( IN );
				Input surfIN;
				UNITY_INITIALIZE_OUTPUT( Input, surfIN );
				surfIN.uv_texcoord = IN.customPack1.xy;
				float3 worldPos = IN.worldPos;
				fixed3 worldViewDir = normalize( UnityWorldSpaceViewDir( worldPos ) );
				surfIN.worldPos = worldPos;
				surfIN.worldNormal = IN.worldNormal;
				SurfaceOutputStandard o;
				UNITY_INITIALIZE_OUTPUT( SurfaceOutputStandard, o )
				surf( surfIN, o );
				#if defined( CAN_SKIP_VPOS )
				float2 vpos = IN.pos;
				#endif
				SHADOW_CASTER_FRAGMENT( IN )
			}
			ENDCG
		}
	}
	Fallback "Diffuse"
	CustomEditor "ASEMaterialInspector"
}
/*ASEBEGIN
Version=14201
-55;416;1277;634;3028.723;244.3829;2.104991;False;True
Node;AmplifyShaderEditor.Vector2Node;113;-3763.322,1820.902;Float;False;Constant;_AuraTextureMoveSpeed2;Aura Texture Move Speed 2;3;0;Create;0.61,1.5;0.61,1.5;0;3;FLOAT2;0;FLOAT;1;FLOAT;2
Node;AmplifyShaderEditor.TimeNode;62;-3744.596,1397.389;Float;False;0;5;FLOAT4;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.TextureCoordinatesNode;153;-3786.272,1633.773;Float;False;0;-1;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RangedFloatNode;149;-3390.157,1375.58;Float;False;Property;_AuraTextureSize;Aura Texture Size;12;0;Create;1;1.4;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.PannerNode;107;-3370.407,1568.808;Float;False;3;0;FLOAT2;0,0;False;2;FLOAT2;-1,0;False;1;FLOAT;1.0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.Vector2Node;10;-3747.869,1017.804;Float;False;Constant;_AuraTextureMoveSpeed;Aura Texture Move Speed;4;0;Create;-0.64,-6.54;-0.72,-6.33;0;3;FLOAT2;0;FLOAT;1;FLOAT;2
Node;AmplifyShaderEditor.ScaleAndOffsetNode;151;-3055.759,1530.177;Float;True;3;0;FLOAT2;0,0,0;False;1;FLOAT;1.0;False;2;FLOAT;0.0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.PannerNode;8;-3368.177,1143.825;Float;False;3;0;FLOAT2;0,0;False;2;FLOAT2;-1,0;False;1;FLOAT;1.0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.ScaleAndOffsetNode;157;-2973.425,1062.161;Float;True;3;0;FLOAT2;0,0,0;False;1;FLOAT;1.0;False;2;FLOAT;0.0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SamplerNode;2;-2633.891,1039.046;Float;True;Property;_AuraTexture;Aura Texture;2;0;Create;None;d929031db171f3644a388219616645df;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0.0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1.0;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RangedFloatNode;137;-1243.51,1713.807;Float;False;Property;_RimPulse;Rim Pulse;9;0;Create;1.1;13.2;0;20;0;1;FLOAT;0
Node;AmplifyShaderEditor.SamplerNode;108;-2618.233,1564.643;Float;True;Property;_AuraTexture2;Aura Texture 2;3;0;Create;None;d929031db171f3644a388219616645df;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0.0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1.0;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.TimeNode;136;-1229.228,1490.67;Float;False;0;5;FLOAT4;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.PosVertexDataNode;91;-2006.659,312.5904;Float;False;0;0;5;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;135;-1008.116,1553.59;Float;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.BlendOpsNode;119;-1536.527,1340.943;Float;True;HardLight;True;2;0;FLOAT;0,0,0,0;False;1;FLOAT;0,0,0,0;False;1;FLOAT;0
Node;AmplifyShaderEditor.NegateNode;94;-1770.956,354.3501;Float;True;1;0;FLOAT;0.0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;95;-1711.908,677.8863;Float;False;Property;_AuraFalloff2;Aura Falloff 2;7;0;Create;0.68;1.42;0;5;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;98;-1528.376,443.7023;Float;True;2;2;0;FLOAT;0.0;False;1;FLOAT;1.0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;106;-1680.606,823.0789;Float;False;Property;_AuraFalloff1;Aura Falloff 1;6;0;Create;1.87;1.44;0;5;0;1;FLOAT;0
Node;AmplifyShaderEditor.TFHCRemapNode;128;-1298.209,1055.193;Float;True;5;0;FLOAT;0.0;False;1;FLOAT;0.2;False;2;FLOAT;0.8;False;3;FLOAT;0.0;False;4;FLOAT;1.0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SinOpNode;134;-851.0266,1532.37;Float;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleSubtractOpNode;100;-1441.946,165.5582;Float;True;2;0;FLOAT;0.0;False;1;FLOAT;1.0;False;1;FLOAT;0
Node;AmplifyShaderEditor.ClampOpNode;130;-962.3532,948.9852;Float;False;3;0;FLOAT;0.0;False;1;FLOAT;0.0;False;2;FLOAT;1.0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleDivideOpNode;104;-1347.676,735.2687;Float;False;2;0;FLOAT;0.0;False;1;FLOAT;2.0;False;1;FLOAT;0
Node;AmplifyShaderEditor.TFHCRemapNode;129;-943.5275,1149.697;Float;False;5;0;FLOAT;0.0;False;1;FLOAT;0.0;False;2;FLOAT;1.0;False;3;FLOAT;1.0;False;4;FLOAT;0.0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;122;-747.7485,1076.147;Float;False;Property;_RimPower;Rim Power;8;0;Create;3;5.55;0;10;0;1;FLOAT;0
Node;AmplifyShaderEditor.LerpOp;99;-1203.956,409.8615;Float;True;3;0;FLOAT;0.0;False;1;FLOAT;0.0;False;2;FLOAT;0.31;False;1;FLOAT;0
Node;AmplifyShaderEditor.TFHCRemapNode;138;-622.8919,1289.892;Float;False;5;0;FLOAT;0.0;False;1;FLOAT;0.0;False;2;FLOAT;3.0;False;3;FLOAT;0.6;False;4;FLOAT;2.0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;132;-344.3571,1206.597;Float;True;2;2;0;FLOAT;0.0;False;1;FLOAT;0.0;False;1;FLOAT;0
Node;AmplifyShaderEditor.ClampOpNode;96;-946.747,413.495;Float;True;3;0;FLOAT;0.0;False;1;FLOAT;0.0;False;2;FLOAT;5.0;False;1;FLOAT;0
Node;AmplifyShaderEditor.ToggleSwitchNode;26;-750.105,878.7986;Float;False;Property;_InvertAura;Invert Aura;10;0;Create;1;2;0;FLOAT;0,0,0,0;False;1;FLOAT;0,0,0,0;False;1;FLOAT;0
Node;AmplifyShaderEditor.LerpOp;103;-1187.877,670.5073;Float;True;3;0;FLOAT;0.0;False;1;FLOAT;0.0;False;2;FLOAT;0.31;False;1;FLOAT;0
Node;AmplifyShaderEditor.ClampOpNode;105;-913.7874,660.8365;Float;True;3;0;FLOAT;0.0;False;1;FLOAT;0.0;False;2;FLOAT;5.0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;101;-547.9523,619.9699;Float;True;2;2;0;FLOAT;0,0,0,0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.TFHCRemapNode;123;-100.7524,1078.758;Float;False;5;0;FLOAT;0.0;False;1;FLOAT;0.0;False;2;FLOAT;10.0;False;3;FLOAT;10.0;False;4;FLOAT;0.0;False;1;FLOAT;0
Node;AmplifyShaderEditor.FresnelNode;124;103.0762,1003.582;Float;False;World;4;0;FLOAT3;0,0,0;False;1;FLOAT;0.0;False;2;FLOAT;1.0;False;3;FLOAT;5.0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;102;-257.8118,705.8286;Float;True;2;2;0;FLOAT;0.0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.ColorNode;139;226.7291,-255.782;Float;False;Property;_MainColor;Main Color;0;0;Create;1,1,1,0;1,1,1,0;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleAddOpNode;126;45.86934,701.985;Float;True;2;2;0;FLOAT;0.0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SamplerNode;20;146.9303,-97.7473;Float;True;Property;_ObjectTexture;Object Texture;1;0;Create;None;8123f5d2b691949429176b65f165b36a;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0.0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1.0;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RangedFloatNode;23;286.7227,1207.983;Float;False;Property;_AuraBrightness;Aura Brightness;5;0;Create;4;0.61;0;10;0;1;FLOAT;0
Node;AmplifyShaderEditor.BlendOpsNode;141;480.1784,-127.0134;Float;True;Multiply;True;2;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.ColorNode;19;431.2851,184.8021;Float;False;Property;_AuraColour;Aura Colour;4;0;Create;0,0.8758621,1,0;0.2205882,0.6129819,1,0;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;22;357.5143,732.3334;Float;True;2;2;0;FLOAT;0,0,0,0;False;1;FLOAT;0,0,0,0;False;1;FLOAT;0
Node;AmplifyShaderEditor.LerpOp;21;874.3419,204.7545;Float;True;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.ToggleSwitchNode;27;1217.881,128.9308;Float;False;Property;_AuraOnOff;Aura OnOff;11;0;Create;1;2;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.StandardSurfaceOutputNode;0;1478.353,235.277;Float;False;True;2;Float;ASEMaterialInspector;0;0;Standard;Custom/Possession Aura2;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;Back;0;0;False;0;0;Opaque;0.5;True;True;0;False;Opaque;Geometry;All;True;True;True;True;True;True;True;True;True;True;True;True;True;True;True;True;True;False;0;255;255;0;0;0;0;0;0;0;0;False;2;15;10;25;False;0.5;True;0;Zero;Zero;0;Zero;Zero;OFF;OFF;0;True;0.0001;0,0,0,0;VertexOffset;True;False;Cylindrical;False;Relative;0;;-1;-1;-1;-1;0;0;0;False;0;0;16;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;3;FLOAT;0.0;False;4;FLOAT;0.0;False;5;FLOAT;0.0;False;6;FLOAT3;0,0,0;False;7;FLOAT3;0,0,0;False;8;FLOAT;0.0;False;9;FLOAT;0.0;False;10;FLOAT;0.0;False;13;FLOAT3;0,0,0;False;11;FLOAT3;0,0,0;False;12;FLOAT3;0,0,0;False;14;FLOAT4;0,0,0,0;False;15;FLOAT3;0,0,0;False;0
WireConnection;107;0;153;0
WireConnection;107;2;113;0
WireConnection;107;1;62;1
WireConnection;151;0;107;0
WireConnection;151;1;149;0
WireConnection;8;0;151;0
WireConnection;8;2;10;0
WireConnection;8;1;62;1
WireConnection;157;0;8;0
WireConnection;157;1;149;0
WireConnection;2;1;157;0
WireConnection;108;1;151;0
WireConnection;135;0;136;2
WireConnection;135;1;137;0
WireConnection;119;0;2;1
WireConnection;119;1;108;1
WireConnection;94;0;91;2
WireConnection;98;0;94;0
WireConnection;128;0;119;0
WireConnection;134;0;135;0
WireConnection;100;0;94;0
WireConnection;130;0;128;0
WireConnection;104;0;95;0
WireConnection;104;1;106;0
WireConnection;129;0;128;0
WireConnection;99;0;100;0
WireConnection;99;1;98;0
WireConnection;99;2;95;0
WireConnection;138;0;134;0
WireConnection;132;0;122;0
WireConnection;132;1;138;0
WireConnection;96;0;99;0
WireConnection;26;0;130;0
WireConnection;26;1;129;0
WireConnection;103;0;100;0
WireConnection;103;1;98;0
WireConnection;103;2;104;0
WireConnection;105;0;103;0
WireConnection;101;0;96;0
WireConnection;101;1;26;0
WireConnection;123;0;132;0
WireConnection;124;3;123;0
WireConnection;102;0;105;0
WireConnection;102;1;101;0
WireConnection;126;0;124;0
WireConnection;126;1;102;0
WireConnection;141;0;20;0
WireConnection;141;1;139;0
WireConnection;22;0;126;0
WireConnection;22;1;23;0
WireConnection;21;0;141;0
WireConnection;21;1;19;0
WireConnection;21;2;22;0
WireConnection;27;0;141;0
WireConnection;27;1;21;0
WireConnection;0;2;27;0
ASEEND*/
//CHKSM=5A690E81719385D53B21B75626FC1A3A86675CA9