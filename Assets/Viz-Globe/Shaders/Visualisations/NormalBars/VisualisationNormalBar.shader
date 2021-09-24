// Upgrade NOTE: replaced 'UNITY_INSTANCE_ID' with 'UNITY_VERTEX_INPUT_INSTANCE_ID'
// Bars on a globe visualisation
// Kadek Satriadi (kadeksatriadi.com)
//
Shader "VizGlobe/VisualisationNormalBar"
{
    Properties
    {
        _MaxColor ("MaxColor", Color) = (1,1,1,1)
        _MinColor ("MinColor", Color) = (1,1,1,1)
		_Radius("Radius", float) = 1 //globe radius
		_Size("Size", float) = 1
		_MaxRange("MaxRange", float) = 1
		_MainTex("Texture", 2D) = "white" {}
	}
		SubShader
	{
		Tags { "RenderType" = "Opaque" }
		LOD 100
		//Cull On

		//----------base pass
		Pass
		{
		CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma geometry geom
			#pragma multi_compile_fog

			#include "UnityCG.cginc"
			#include "Lighting.cginc"
			#include "PropertiesBar.cginc"
			#include "../ColorHelper.cginc"
			#include "GlobeBar.cginc"						
		ENDCG
		}
		//-----------shadow caster pass
		Pass {
			Tags {
				"LightMode" = "ShadowCaster"
			}

		CGPROGRAM
			#pragma target 3.0
			#pragma vertex vert
			#pragma geometry geom
			#pragma fragment frag
			#pragma multi_compile_shadowcaster
			#include "UnityCG.cginc"
			#include "Lighting.cginc"
			#include "PropertiesBar.cginc"
			#include "../ColorHelper.cginc"
			#include "GlobeBar.cginc"

			float4 MyShadowVertexProgram(g2f v) : SV_POSITION {
				return UnityObjectToClipPos(v.vertex);
			}

			half4 MyShadowFragmentProgram() : SV_TARGET {
				return 0;
			}

		ENDCG
		}
		
    }
    FallBack "Diffuse"
}
