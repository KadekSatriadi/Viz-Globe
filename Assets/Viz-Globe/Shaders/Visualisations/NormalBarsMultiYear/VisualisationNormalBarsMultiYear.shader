Shader "Unlit/VisualisationNormalBarsMultiYear"
{
	Properties
	{
		_MaxColor("MaxColor", Color) = (1,1,1,1)
		_MinColor("MinColor", Color) = (1,1,1,1)
		_Radius("Radius", float) = 1 //globe radius
		_Size("Size", float) = 1
		_MaxRange("MaxRange", float) = 1
		_MainTex("Texture", 2D) = "white" {}
	    _ActiveYear("ActiveYear", Range(1.0,5.0)) = 1
	}
		SubShader
	{
		Tags { "RenderType" = "Opaque" }
		LOD 100

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
			#include "PropertiesBarMulti.cginc"
			#include "../ColorHelper.cginc"
			#include "GlobeBarMulti.cginc"
		ENDCG
		}

		//-----------shadow caster pass
	/*	Pass {
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
			#include "PropertiesBarMulti.cginc"
			#include "../ColorHelper.cginc"
			#include "GlobeBarMulti.cginc"

			float4 MyShadowVertexProgram(g2f v) : SV_POSITION {
				return UnityObjectToClipPos(v.vertex);
			}

			half4 MyShadowFragmentProgram() : SV_TARGET {
				return 0;
			}

		ENDCG
		}*/
	}
		FallBack "Diffuse"
}
