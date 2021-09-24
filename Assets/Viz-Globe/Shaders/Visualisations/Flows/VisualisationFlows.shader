Shader "Unlit/VisualisationFlows"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100

        Pass
        {
            CGPROGRAM
           #pragma vertex vert
            #pragma fragment frag
            #pragma geometry geom
            #pragma multi_compile_fog

            #include "UnityCG.cginc"
            #include "Lighting.cginc"

            // single-pass
            UNITY_INSTANCING_BUFFER_START(Props)
            UNITY_INSTANCING_BUFFER_START(Props)
            UNITY_DEFINE_INSTANCED_PROP(float, _Radius)
            UNITY_DEFINE_INSTANCED_PROP(float, _MaxFlowRadius)
            UNITY_INSTANCING_BUFFER_END(Props)
            UNITY_INSTANCING_BUFFER_END(Props)
            //end single pass
            #include "GlobeFlow.cginc"	
            
            ENDCG
        }
    }
}
