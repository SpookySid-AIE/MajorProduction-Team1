// Made with Amplify Shader Editor
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "Custom/sdr Ghost Rim"
{
	Properties
	{
		_Blending("Blending", Range( 0 , 1)) = 0
		_Cutoff( "Mask Clip Value", Float ) = -2.18
		_RimPower("RimPower", Range( 0 , 10)) = 0
		_CharacterTexture("Character Texture", 2D) = "white" {}
		_RimColor("Rim Color", Color) = (0,0.9172413,1,0)
		_Emission("Emission", Range( 0 , 10)) = 3.94
		_MaskingFalloff("Masking Falloff", Range( 0.01 , 5)) = 0
		_ParticleMaskingPosition("Particle Masking Position", Range( -2 , 3)) = 0
		_ParticleSpeed("Particle Speed", Vector) = (0,0,0,0)
		_DissolvePattern("Dissolve Pattern", 2D) = "white" {}
		_DissolvePattern2("Dissolve Pattern 2", 2D) = "white" {}
		[HideInInspector] _texcoord( "", 2D ) = "white" {}
		[HideInInspector] __dirty( "", Int ) = 1
	}

	SubShader
	{
		Tags{ "RenderType" = "TransparentCutout"  "Queue" = "AlphaTest+0" "IgnoreProjector" = "True" "IsEmissive" = "true"  }
		Cull Off
		GrabPass{ }
		CGPROGRAM
		#include "UnityShaderVariables.cginc"
		#pragma target 3.0
		#pragma surface surf Standard keepalpha noshadow exclude_path:deferred 
		struct Input
		{
			float3 worldPos;
			float3 worldNormal;
			float4 screenPos;
			float2 uv_texcoord;
		};

		uniform float4 _RimColor;
		uniform float _RimPower;
		uniform sampler2D _GrabTexture;
		uniform sampler2D _CharacterTexture;
		uniform float4 _CharacterTexture_ST;
		uniform float _Blending;
		uniform float _Emission;
		uniform float _ParticleMaskingPosition;
		uniform float _MaskingFalloff;
		uniform sampler2D _DissolvePattern;
		uniform sampler2D _DissolvePattern2;
		uniform float2 _ParticleSpeed;
		uniform float _Cutoff = -2.18;


		inline float4 ASE_ComputeGrabScreenPos( float4 pos )
		{
			#if UNITY_UV_STARTS_AT_TOP
			float scale = -1.0;
			#else
			float scale = 1.0;
			#endif
			float4 o = pos;
			o.y = pos.w * 0.5f;
			o.y = ( pos.y - o.y ) * _ProjectionParams.x * scale + o.y;
			return o;
		}


		void surf( Input i , inout SurfaceOutputStandard o )
		{
			float3 ase_worldPos = i.worldPos;
			float3 ase_worldViewDir = normalize( UnityWorldSpaceViewDir( ase_worldPos ) );
			float3 ase_worldNormal = i.worldNormal;
			float fresnelNDotV34 = dot( ase_worldNormal, ase_worldViewDir );
			float fresnelNode34 = ( 0.0 + 1.0 * pow( 1.0 - fresnelNDotV34, (10.0 + (_RimPower - 0.0) * (0.0 - 10.0) / (10.0 - 0.0)) ) );
			float4 ase_screenPos = float4( i.screenPos.xyz , i.screenPos.w + 0.00000000001 );
			float4 ase_grabScreenPos = ASE_ComputeGrabScreenPos( ase_screenPos );
			float4 screenColor4 = tex2Dproj( _GrabTexture, UNITY_PROJ_COORD( ase_grabScreenPos ) );
			float2 uv_CharacterTexture = i.uv_texcoord * _CharacterTexture_ST.xy + _CharacterTexture_ST.zw;
			float4 lerpResult7 = lerp( screenColor4 , tex2D( _CharacterTexture, uv_CharacterTexture ) , _Blending);
			float4 lerpResult63 = lerp( ( ( ( _RimColor * fresnelNode34 ) * 5.0 ) + lerpResult7 ) , screenColor4 , 0);
			o.Emission = ( lerpResult63 * _Emission ).rgb;
			o.Alpha = 1;
			float3 ase_vertex3Pos = mul( unity_WorldToObject, float4( i.worldPos , 1 ) );
			float temp_output_78_0 = ( ase_vertex3Pos.x + _ParticleMaskingPosition );
			float temp_output_106_0 = ( temp_output_78_0 * _MaskingFalloff );
			float lerpResult84 = lerp( temp_output_106_0 , temp_output_78_0 , 0.0);
			float2 uv_TexCoord101 = i.uv_texcoord * float2( 1,1 ) + float2( 0,0 );
			float2 panner109 = ( uv_TexCoord101 + _Time.x * float2( 0,-0.5 ));
			float2 panner103 = ( uv_TexCoord101 + _Time.x * _ParticleSpeed);
			float normalizeResult91 = normalize( temp_output_106_0 );
			float4 lerpResult71 = lerp( float4(0,0,0,0) , ( tex2D( _DissolvePattern, panner109 ) + tex2D( _DissolvePattern2, panner103 ) ) , ( normalizeResult91 + normalizeResult91 ));
			clip( ( lerpResult84 + lerpResult71 ).r - _Cutoff );
		}

		ENDCG
	}
	CustomEditor "ASEMaterialInspector"
}
/*ASEBEGIN
Version=14201
-497;128;1906;1004;-764.5959;1281.297;1.975807;True;True
Node;AmplifyShaderEditor.RangedFloatNode;36;1949.939,-314.6152;Float;False;Property;_RimPower;RimPower;2;0;Create;0;4.52;0;10;0;1;FLOAT;0
Node;AmplifyShaderEditor.PosVertexDataNode;72;2338.624,981.4601;Float;False;0;0;5;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.TFHCRemapNode;35;2286.967,-350.4345;Float;False;5;0;FLOAT;0.0;False;1;FLOAT;0.0;False;2;FLOAT;10.0;False;3;FLOAT;10.0;False;4;FLOAT;0.0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;73;2201.016,1158.637;Float;False;Property;_ParticleMaskingPosition;Particle Masking Position;7;0;Create;0;3;-2;3;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;89;2511.67,843.7455;Float;False;Property;_MaskingFalloff;Masking Falloff;6;0;Create;0;1.74;0.01;5;0;1;FLOAT;0
Node;AmplifyShaderEditor.FresnelNode;34;2498.566,-348.7348;Float;False;World;4;0;FLOAT3;0,0,0;False;1;FLOAT;0.0;False;2;FLOAT;1.0;False;3;FLOAT;5.0;False;1;FLOAT;0
Node;AmplifyShaderEditor.Vector2Node;102;2513.416,524.9904;Float;False;Property;_ParticleSpeed;Particle Speed;8;0;Create;0,0;0,6.33;0;3;FLOAT2;0;FLOAT;1;FLOAT;2
Node;AmplifyShaderEditor.ColorNode;44;2636.008,-569.3091;Float;False;Property;_RimColor;Rim Color;4;0;Create;0,0.9172413,1,0;0,1,0.9172416,0;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.TextureCoordinatesNode;101;2537.877,341.8011;Float;False;0;-1;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.TimeNode;104;2544.947,678.9842;Float;False;0;5;FLOAT4;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleAddOpNode;78;2587.119,1110.303;Float;True;2;2;0;FLOAT;0,0,0;False;1;FLOAT;0.0;False;1;FLOAT;0
Node;AmplifyShaderEditor.Vector2Node;112;2837.167,231.0234;Float;False;Constant;_Vector0;Vector 0;11;0;Create;0,-0.5;0,0;0;3;FLOAT2;0;FLOAT;1;FLOAT;2
Node;AmplifyShaderEditor.RangedFloatNode;9;2377.669,259.093;Float;False;Property;_Blending;Blending;0;0;Create;0;0.375;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.ScreenColorNode;4;2376.146,-79.78082;Float;False;Global;_GrabScreen0;Grab Screen 0;0;0;Create;Object;-1;False;True;1;0;FLOAT4;0,0,0,0;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.PannerNode;109;3049.945,324.8134;Float;False;3;0;FLOAT2;0,0;False;2;FLOAT2;-1,0;False;1;FLOAT;1.0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.PannerNode;103;2931.674,571.0844;Float;False;3;0;FLOAT2;0,0;False;2;FLOAT2;-1,0;False;1;FLOAT;1.0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;106;2958.926,897.5379;Float;True;2;2;0;FLOAT;0.0;False;1;FLOAT;0.0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;46;2926.737,-478.6915;Float;True;2;2;0;COLOR;0,0,0,0;False;1;FLOAT;0.0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.RangedFloatNode;41;2873.825,-147.9161;Float;False;Constant;_Float0;Float 0;6;0;Create;5;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SamplerNode;51;2615.269,-850.5494;Float;True;Property;_CharacterTexture;Character Texture;3;0;Create;None;40aecc6e963014a44b09bb9fe7eef9e2;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;0,0;False;1;FLOAT2;1,0;False;2;FLOAT;1.0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1.0;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SamplerNode;107;3241.024,363.0508;Float;True;Property;_DissolvePattern;Dissolve Pattern;9;0;Create;None;31e417e2506e4bf4cbc2b47d5fc113ea;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0.0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1.0;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.NormalizeNode;91;3491.593,898.5901;Float;True;1;0;FLOAT;0,0,0,0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;40;3160.839,-248.8167;Float;False;2;2;0;COLOR;0,0,0,0;False;1;FLOAT;0.0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.LerpOp;7;3095.335,-7.840147;Float;True;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SamplerNode;94;3137.395,679.7789;Float;True;Property;_DissolvePattern2;Dissolve Pattern 2;10;0;Create;None;31e417e2506e4bf4cbc2b47d5fc113ea;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0.0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1.0;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleAddOpNode;93;3751.245,900.1974;Float;True;2;2;0;FLOAT;0,0,0;False;1;FLOAT;0,0,0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;108;3907.265,420.7428;Float;True;2;2;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.ColorNode;68;4189.159,523.7913;Float;False;Constant;_Color0;Color 0;9;0;Create;0,0,0,0;0,0,0,0;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleAddOpNode;43;3450.796,-238.2563;Float;True;2;2;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.LerpOp;63;4179.069,-74.4278;Float;True;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.LerpOp;71;4426.759,846.0987;Float;True;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.RangedFloatNode;53;3731.564,-477.2212;Float;False;Property;_Emission;Emission;5;0;Create;3.94;0.76;0;10;0;1;FLOAT;0
Node;AmplifyShaderEditor.LerpOp;84;3369.667,1121.025;Float;True;3;0;FLOAT;0,0,0;False;1;FLOAT;0,0,0;False;2;FLOAT;0.0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;52;4545.452,-305.7149;Float;True;2;2;0;COLOR;0,0,0,0;False;1;FLOAT;0.0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.NegateNode;110;2834.366,399.0062;Float;False;1;0;FLOAT;0.0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;96;4427.042,295.775;Float;True;2;2;0;FLOAT;0,0,0;False;1;COLOR;0;False;1;COLOR;0
Node;AmplifyShaderEditor.StandardSurfaceOutputNode;0;4978.032,-52.40177;Float;False;True;2;Float;ASEMaterialInspector;0;0;Standard;Custom/sdr Ghost Rim;False;False;False;False;False;False;False;False;False;False;False;False;False;False;True;False;False;Off;0;0;False;0;0;Masked;-2.18;True;False;0;False;TransparentCutout;AlphaTest;ForwardOnly;True;True;True;True;True;True;True;True;True;True;True;True;True;True;True;True;True;False;0;255;255;0;0;0;0;0;0;0;0;False;2;15;10;25;False;0.5;False;0;OneMinusDstColor;One;0;One;One;OFF;OFF;0;False;-0.03;0,0,0,0;VertexScale;True;False;Cylindrical;False;Relative;0;;1;-1;-1;-1;0;0;0;False;0;0;16;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;3;FLOAT;0.0;False;4;FLOAT;0.0;False;5;FLOAT;0.0;False;6;FLOAT3;0,0,0;False;7;FLOAT3;0,0,0;False;8;FLOAT;0.0;False;9;FLOAT;0.0;False;10;FLOAT;0.0;False;13;FLOAT3;0,0,0;False;11;FLOAT3;0,0,0;False;12;FLOAT3;0,0,0;False;14;FLOAT4;0,0,0,0;False;15;FLOAT3;0,0,0;False;0
WireConnection;35;0;36;0
WireConnection;34;3;35;0
WireConnection;78;0;72;1
WireConnection;78;1;73;0
WireConnection;109;0;101;0
WireConnection;109;2;112;0
WireConnection;109;1;104;1
WireConnection;103;0;101;0
WireConnection;103;2;102;0
WireConnection;103;1;104;1
WireConnection;106;0;78;0
WireConnection;106;1;89;0
WireConnection;46;0;44;0
WireConnection;46;1;34;0
WireConnection;107;1;109;0
WireConnection;91;0;106;0
WireConnection;40;0;46;0
WireConnection;40;1;41;0
WireConnection;7;0;4;0
WireConnection;7;1;51;0
WireConnection;7;2;9;0
WireConnection;94;1;103;0
WireConnection;93;0;91;0
WireConnection;93;1;91;0
WireConnection;108;0;107;0
WireConnection;108;1;94;0
WireConnection;43;0;40;0
WireConnection;43;1;7;0
WireConnection;63;0;43;0
WireConnection;63;1;4;0
WireConnection;71;0;68;0
WireConnection;71;1;108;0
WireConnection;71;2;93;0
WireConnection;84;0;106;0
WireConnection;84;1;78;0
WireConnection;52;0;63;0
WireConnection;52;1;53;0
WireConnection;110;0;102;2
WireConnection;96;0;84;0
WireConnection;96;1;71;0
WireConnection;0;2;52;0
WireConnection;0;10;96;0
ASEEND*/
//CHKSM=68ABECFD3AE625BCB5417E9260D310A0B68F0BE2