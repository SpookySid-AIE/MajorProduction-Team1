// Made with Amplify Shader Editor
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "Custom/sdr Ghost Rim"
{
	Properties
	{
		_RimPulseFrequency("Rim Pulse Frequency", Range( 0 , 100)) = 1.1
		_Blending("Blending", Range( 0 , 1)) = 0
		_RimPower("RimPower", Range( 0 , 10)) = 0
		_TextureSample1("Texture Sample 1", 2D) = "white" {}
		_RimColor2("Rim Color2", Color) = (0,0.9172413,1,0)
		_RimColor("Rim Color", Color) = (0,0.9172413,1,0)
		_Emission("Emission", Range( 0 , 10)) = 3.94
		[Toggle]_ToggleSwitch0("Toggle Switch0", Float) = 1
		[HideInInspector] _texcoord( "", 2D ) = "white" {}
		[HideInInspector] __dirty( "", Int ) = 1
	}

	SubShader
	{
		Tags{ "RenderType" = "Opaque"  "Queue" = "AlphaTest+0" "IsEmissive" = "true"  }
		Cull Off
		GrabPass{ }
		CGPROGRAM
		#include "UnityShaderVariables.cginc"
		#pragma target 3.0
		#pragma surface surf Standard keepalpha noshadow 
		struct Input
		{
			float3 worldPos;
			float3 worldNormal;
			float4 screenPos;
			float2 uv_texcoord;
		};

		uniform float _ToggleSwitch0;
		uniform float4 _RimColor;
		uniform float4 _RimColor2;
		uniform float _RimPulseFrequency;
		uniform float _RimPower;
		uniform sampler2D _GrabTexture;
		uniform sampler2D _TextureSample1;
		uniform float4 _TextureSample1_ST;
		uniform float _Blending;
		uniform float _Emission;

		void surf( Input i , inout SurfaceOutputStandard o )
		{
			float4 lerpResult55 = lerp( _RimColor2 , _RimColor , sin( ( _Time.y * _RimPulseFrequency ) ));
			float3 ase_worldPos = i.worldPos;
			float3 ase_worldViewDir = normalize( UnityWorldSpaceViewDir( ase_worldPos ) );
			float3 ase_worldNormal = i.worldNormal;
			float fresnelNDotV34 = dot( ase_worldNormal, ase_worldViewDir );
			float fresnelNode34 = ( 0.0 + 1.0 * pow( 1.0 - fresnelNDotV34, (10.0 + (_RimPower - 0.0) * (0.0 - 10.0) / (10.0 - 0.0)) ) );
			float4 ase_screenPos = float4( i.screenPos.xyz , i.screenPos.w + 0.00000000001 );
			float4 ase_screenPos4 = ase_screenPos;
			#if UNITY_UV_STARTS_AT_TOP
			float scale4 = -1.0;
			#else
			float scale4 = 1.0;
			#endif
			float halfPosW4 = ase_screenPos4.w * 0.5;
			ase_screenPos4.y = ( ase_screenPos4.y - halfPosW4 ) * _ProjectionParams.x* scale4 + halfPosW4;
			ase_screenPos4.w += 0.00000000001;
			ase_screenPos4.xyzw /= ase_screenPos4.w;
			float4 screenColor4 = tex2Dproj( _GrabTexture, UNITY_PROJ_COORD( ase_screenPos4 ) );
			float2 uv_TextureSample1 = i.uv_texcoord * _TextureSample1_ST.xy + _TextureSample1_ST.zw;
			float4 lerpResult7 = lerp( screenColor4 , tex2D( _TextureSample1, uv_TextureSample1 ) , _Blending);
			o.Emission = ( ( ( ( lerp(_RimColor,lerpResult55,_ToggleSwitch0) * fresnelNode34 ) * 5.0 ) + lerpResult7 ) * _Emission ).rgb;
			o.Alpha = 1;
		}

		ENDCG
	}
	CustomEditor "ASEMaterialInspector"
}
/*ASEBEGIN
Version=14001
4;366;1586;695;1055.202;1236.846;1;True;True
Node;AmplifyShaderEditor.RangedFloatNode;61;-879.985,-383.3856;Float;False;Property;_RimPulseFrequency;Rim Pulse Frequency;-1;0;1.1;0;100;0;1;FLOAT;0
Node;AmplifyShaderEditor.TimeNode;62;-865.703,-606.5231;Float;False;0;5;FLOAT4;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;63;-592.5912,-556.6027;Float;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.ColorNode;44;-518.6993,-935.4641;Float;False;Property;_RimColor;Rim Color;7;0;0,0.9172413,1,0;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.ColorNode;54;-508.4562,-1136.997;Float;False;Property;_RimColor2;Rim Color2;7;0;0,0.9172413,1,0;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RangedFloatNode;36;-485.2201,-363.6124;Float;False;Property;_RimPower;RimPower;3;0;0;0;10;0;1;FLOAT;0
Node;AmplifyShaderEditor.SinOpNode;64;-427.5551,-617.2833;Float;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.LerpOp;55;-156.3583,-682.0569;Float;True;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.TFHCRemapNode;35;-148.1911,-399.4317;Float;False;5;0;FLOAT;0.0;False;1;FLOAT;0.0;False;2;FLOAT;10.0;False;3;FLOAT;10.0;False;4;FLOAT;0.0;False;1;FLOAT;0
Node;AmplifyShaderEditor.ToggleSwitchNode;58;161.871,-864.3161;Float;True;Property;_ToggleSwitch0;Toggle Switch0;11;0;1;2;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.FresnelNode;34;63.40817,-397.732;Float;False;World;4;0;FLOAT3;0,0,0;False;1;FLOAT;0.0;False;2;FLOAT;1.0;False;3;FLOAT;5.0;False;1;FLOAT;0
Node;AmplifyShaderEditor.ScreenColorNode;4;-59.01227,-128.778;Float;False;Global;_GrabScreen0;Grab Screen 0;0;0;Object;-1;False;1;0;FLOAT4;0,0,0,0;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SamplerNode;51;-490.3571,-189.8683;Float;True;Property;_TextureSample1;Texture Sample 1;6;0;None;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;0,0;False;1;FLOAT2;1,0;False;2;FLOAT;1.0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1.0;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RangedFloatNode;41;438.6674,-196.9133;Float;False;Constant;_Float0;Float 0;6;0;5;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;46;491.579,-527.6887;Float;True;2;2;0;COLOR;0,0,0,0;False;1;FLOAT;0.0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.RangedFloatNode;9;-57.4889,210.0958;Float;False;Property;_Blending;Blending;0;0;0;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.LerpOp;7;660.1767,-56.83733;Float;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;40;725.6804,-297.8139;Float;False;2;2;0;COLOR;0,0,0,0;False;1;FLOAT;0.0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleAddOpNode;43;925.8301,-162.1226;Float;True;2;2;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.RangedFloatNode;53;1047.342,-398.381;Float;False;Property;_Emission;Emission;8;0;3.94;0;10;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;20;-2177.635,16.24591;Float;False;2;2;0;FLOAT;0.0;False;1;FLOAT4;0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.GrabScreenPosition;11;-2312.8,232.2327;Float;False;0;0;5;FLOAT4;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SamplerNode;10;-1477.288,-82.60828;Float;True;Property;_DistortionMap;DistortionMap;2;0;Assets/AmplifyShaderEditor/Examples/Assets/Textures/Misc/SmallWaves.png;True;0;True;bump;Auto;True;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0.0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1.0;False;5;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;18;-1778.061,-101.4279;Float;False;2;2;0;FLOAT;0.0;False;1;FLOAT2;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SimpleAddOpNode;12;-797.2832,32.80552;Float;False;2;2;0;FLOAT3;0,0,0,0;False;1;FLOAT4;0,0,0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;21;-2402.338,-42.50298;Float;False;2;2;0;FLOAT;0.0;False;1;FLOAT;0.0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;14;-1156.068,30.47018;Float;False;Property;_DistortionScale;DistortionScale;1;0;0.2;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;19;-2090.996,-222.981;Float;False;Property;_RippleScale;RippleScale;4;0;0;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;22;-2635.089,123.1434;Float;False;Property;_RippleSpeed;RippleSpeed;5;0;0;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.SwizzleNode;17;-2003.062,-96.25537;Float;False;FLOAT2;0;0;2;3;1;0;FLOAT4;0,0,0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;52;1191.701,-165.3461;Float;True;2;2;0;COLOR;0,0,0,0;False;1;FLOAT;0.0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;13;-987.4895,-150.7194;Float;False;2;2;0;FLOAT3;0,0,0;False;1;FLOAT;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.StandardSurfaceOutputNode;0;1760.635,-291.4811;Float;False;True;2;Float;ASEMaterialInspector;0;0;Standard;Custom/sdr Ghost Rim;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;Off;0;0;False;0;0;Custom;0.5;True;False;0;True;Opaque;AlphaTest;All;True;True;True;True;True;True;True;True;True;True;True;True;True;True;True;True;True;False;0;255;255;0;0;0;0;0;0;0;0;False;2;15;10;25;False;0.5;False;0;Zero;Zero;0;One;One;OFF;OFF;0;False;-0.03;0,0,0,0;VertexScale;True;False;Cylindrical;False;Relative;0;;-1;-1;-1;-1;0;0;0;False;0;16;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;3;FLOAT;0.0;False;4;FLOAT;0.0;False;5;FLOAT;0.0;False;6;FLOAT3;0,0,0;False;7;FLOAT3;0,0,0;False;8;FLOAT;0.0;False;9;FLOAT;0.0;False;10;FLOAT;0.0;False;13;FLOAT3;0,0,0;False;11;FLOAT3;0,0,0;False;12;FLOAT3;0,0,0;False;14;FLOAT4;0,0,0,0;False;15;FLOAT3;0,0,0;False;0
WireConnection;63;0;62;2
WireConnection;63;1;61;0
WireConnection;64;0;63;0
WireConnection;55;0;54;0
WireConnection;55;1;44;0
WireConnection;55;2;64;0
WireConnection;35;0;36;0
WireConnection;58;0;44;0
WireConnection;58;1;55;0
WireConnection;34;3;35;0
WireConnection;46;0;58;0
WireConnection;46;1;34;0
WireConnection;7;0;4;0
WireConnection;7;1;51;0
WireConnection;7;2;9;0
WireConnection;40;0;46;0
WireConnection;40;1;41;0
WireConnection;43;0;40;0
WireConnection;43;1;7;0
WireConnection;20;0;21;0
WireConnection;20;1;11;0
WireConnection;10;1;18;0
WireConnection;18;0;19;0
WireConnection;18;1;17;0
WireConnection;12;0;13;0
WireConnection;12;1;11;0
WireConnection;21;1;22;0
WireConnection;17;0;20;0
WireConnection;52;0;43;0
WireConnection;52;1;53;0
WireConnection;13;0;10;0
WireConnection;13;1;14;0
WireConnection;0;2;52;0
ASEEND*/
//CHKSM=DD4294710BA97D739A7CC3289E77C564A8B29BD8