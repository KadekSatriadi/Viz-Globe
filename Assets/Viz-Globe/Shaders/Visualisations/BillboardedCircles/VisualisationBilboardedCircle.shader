// Circles on a globe visualisation
// Kadek Satriadi (kadeksatriadi.com)
//
Shader "Unlit/VisualisationBilboardedCircle"
{
	Properties
	{
		_Radius("Radius", float) = 1 //globeradius
		_CircleMaxRadius("CircleMaxRadius", float) = 1
		_CircleMaxHeight("CircleMaxHeight", float) = 1
		_MainTex("Texture", 2D) = "white" {}
		_HighlightColor("Highlight Color", Color) = (0,0,0,1)
	}
		SubShader
	{
		Lighting Off
		ZTest On
		ZWrite On
		Cull Off
		Blend SrcAlpha OneMinusSrcAlpha
		Tags { "Queue" = "Transparent" "RenderType" = "Transparent" "IgnoreProjector" = "True" }


		//----------base pass
		Pass
		{
			AlphaToMask On

		CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma geometry geom
			#pragma multi_compile_fog

			#include "UnityCG.cginc"
			#include "PropertiesCircle.cginc"
			#include "../ColorHelper.cginc"
			#include "GlobeCircle.cginc"						
		ENDCG
		}
	
	}
		FallBack "Diffuse"
}
