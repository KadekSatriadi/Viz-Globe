Shader "Custom/VisualisationTextureBars"
{
	Properties
	{
		_MainTex("Texture", 2D) = "white" {}

		_MaxColor("MaxColor", Color) = (1,1,1,1)
		_MinColor("MinColor", Color) = (1,1,1,1)
		_MaxValue("Max Value", float) = 1
		_Size("Size", float) = 1
		_MaxRange("MaxRange", float) = 1
	}
		SubShader
	{
		Tags { "RenderType" = "Opaque" }
		LOD 100
		Cull Off

		//----------base pass
		Pass
		{
			Tags { "LightMode" = "ForwardBase"}
		CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma geometry geom
			#include "UnityCG.cginc"
			#include "Lighting.cginc"
			#include "Bar.cginc"
		ENDCG
	}
		//----------end base pass
		
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

				#include "Bar.cginc"

				float4 MyShadowVertexProgram(g2f v) : SV_POSITION {
					return UnityObjectToClipPos(v.vertex);
				}

				half4 MyShadowFragmentProgram() : SV_TARGET {
					return 0;
				}

		ENDCG
	}
}
}
