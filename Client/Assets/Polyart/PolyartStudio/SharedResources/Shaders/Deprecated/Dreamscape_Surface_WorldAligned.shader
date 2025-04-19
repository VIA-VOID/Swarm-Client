// Made with Amplify Shader Editor
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "Polyart/Dreamscape Surface World Aligned"
{
	Properties
	{
		_AlbedoTexture("Albedo Texture", 2D) = "white" {}
		[Normal]_NormalTexture("Normal Texture", 2D) = "bump" {}
		_RoughnessTexture("Roughness Texture", 2D) = "white" {}
		_Tiling("Tiling", Float) = 1
		_NormalIntensity("Normal Intensity", Range( 0 , 1)) = 1
		_SmoothnessIntensity("Smoothness Intensity", Range( -1 , 1)) = 1
		[HideInInspector] __dirty( "", Int ) = 1
	}

	SubShader
	{
		Tags{ "RenderType" = "Opaque"  "Queue" = "Geometry+0" "NatureRendererInstancing"="True" }
		Cull Back
		CGPROGRAM
		#include "UnityStandardUtils.cginc"
		#pragma target 3.0
		#pragma multi_compile_instancing
		//#include "Assets/Visual Design Cafe/Nature Shaders/Common/Nodes/Integrations/Nature Renderer.cginc"
		#pragma instancing_options procedural:SetupNatureRenderer
		#pragma surface surf Standard keepalpha addshadow fullforwardshadows vertex:vertexDataFunc 
		struct Input
		{
			float3 worldPos;
			float vertexToFrag29_g16;
			float vertexToFrag31_g16;
			float vertexToFrag29_g17;
			float vertexToFrag31_g17;
			float vertexToFrag29_g15;
			float vertexToFrag31_g15;
		};

		uniform sampler2D _NormalTexture;
		uniform float _Tiling;
		uniform float _NormalIntensity;
		uniform sampler2D _AlbedoTexture;
		uniform sampler2D _RoughnessTexture;
		uniform float _SmoothnessIntensity;

		void vertexDataFunc( inout appdata_full v, out Input o )
		{
			UNITY_INITIALIZE_OUTPUT( Input, o );
			float3 ase_worldNormal = UnityObjectToWorldNormal( v.normal );
			o.vertexToFrag29_g16 = ase_worldNormal.x;
			o.vertexToFrag31_g16 = ase_worldNormal.z;
			o.vertexToFrag29_g17 = ase_worldNormal.x;
			o.vertexToFrag31_g17 = ase_worldNormal.z;
			o.vertexToFrag29_g15 = ase_worldNormal.x;
			o.vertexToFrag31_g15 = ase_worldNormal.z;
		}

		void surf( Input i , inout SurfaceOutputStandard o )
		{
			float3 ase_worldPos = i.worldPos;
			float3 break9_g16 = ( ase_worldPos * ( abs( _Tiling ) * -1.0 ) );
			float2 appendResult12_g16 = (float2(break9_g16.x , break9_g16.z));
			float2 XZ_Coord13_g16 = appendResult12_g16;
			float4 XZ_Texture21_g16 = tex2D( _NormalTexture, XZ_Coord13_g16 );
			float2 appendResult14_g16 = (float2(break9_g16.y , break9_g16.z));
			float2 YZ_Coord15_g16 = appendResult14_g16;
			float4 YZ_Texture23_g16 = tex2D( _NormalTexture, YZ_Coord15_g16 );
			float4 lerpResult33_g16 = lerp( XZ_Texture21_g16 , YZ_Texture23_g16 , abs( i.vertexToFrag29_g16 ));
			float2 appendResult10_g16 = (float2(break9_g16.x , break9_g16.y));
			float2 XY_Coord11_g16 = appendResult10_g16;
			float4 XY_Texture19_g16 = tex2D( _NormalTexture, XY_Coord11_g16 );
			float4 lerpResult36_g16 = lerp( lerpResult33_g16 , XY_Texture19_g16 , abs( i.vertexToFrag31_g16 ));
			o.Normal = UnpackScaleNormal( lerpResult36_g16, _NormalIntensity );
			float3 break9_g17 = ( ase_worldPos * ( abs( _Tiling ) * -1.0 ) );
			float2 appendResult12_g17 = (float2(break9_g17.x , break9_g17.z));
			float2 XZ_Coord13_g17 = appendResult12_g17;
			float4 XZ_Texture21_g17 = tex2D( _AlbedoTexture, XZ_Coord13_g17 );
			float2 appendResult14_g17 = (float2(break9_g17.y , break9_g17.z));
			float2 YZ_Coord15_g17 = appendResult14_g17;
			float4 YZ_Texture23_g17 = tex2D( _AlbedoTexture, YZ_Coord15_g17 );
			float4 lerpResult33_g17 = lerp( XZ_Texture21_g17 , YZ_Texture23_g17 , abs( i.vertexToFrag29_g17 ));
			float2 appendResult10_g17 = (float2(break9_g17.x , break9_g17.y));
			float2 XY_Coord11_g17 = appendResult10_g17;
			float4 XY_Texture19_g17 = tex2D( _AlbedoTexture, XY_Coord11_g17 );
			float4 lerpResult36_g17 = lerp( lerpResult33_g17 , XY_Texture19_g17 , abs( i.vertexToFrag31_g17 ));
			o.Albedo = lerpResult36_g17.rgb;
			float3 break9_g15 = ( ase_worldPos * ( abs( _Tiling ) * -1.0 ) );
			float2 appendResult12_g15 = (float2(break9_g15.x , break9_g15.z));
			float2 XZ_Coord13_g15 = appendResult12_g15;
			float4 XZ_Texture21_g15 = tex2D( _RoughnessTexture, XZ_Coord13_g15 );
			float2 appendResult14_g15 = (float2(break9_g15.y , break9_g15.z));
			float2 YZ_Coord15_g15 = appendResult14_g15;
			float4 YZ_Texture23_g15 = tex2D( _RoughnessTexture, YZ_Coord15_g15 );
			float4 lerpResult33_g15 = lerp( XZ_Texture21_g15 , YZ_Texture23_g15 , abs( i.vertexToFrag29_g15 ));
			float2 appendResult10_g15 = (float2(break9_g15.x , break9_g15.y));
			float2 XY_Coord11_g15 = appendResult10_g15;
			float4 XY_Texture19_g15 = tex2D( _RoughnessTexture, XY_Coord11_g15 );
			float4 lerpResult36_g15 = lerp( lerpResult33_g15 , XY_Texture19_g15 , abs( i.vertexToFrag31_g15 ));
			o.Smoothness = ( ( 1.0 - lerpResult36_g15 ) * _SmoothnessIntensity ).r;
			o.Alpha = 1;
		}

		ENDCG
	}
	Fallback "Diffuse"
}