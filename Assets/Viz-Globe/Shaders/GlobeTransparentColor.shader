// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Unlit/GlobeTransparentColor"
{
    Properties{
     _MainTex("Base (RGB) Trans (A)", 2D) = "white" {}
     _Color("Color", Color) = (1,1,1,1)
     _BackTransparency("Back Transparency", float) = 0.88
    }

        SubShader{
            Tags {"Queue" = "Transparent" "IgnoreProjector" = "True" "RenderType" = "Transparent"}
            LOD 100

            ZWrite Off
            Blend SrcAlpha OneMinusSrcAlpha

            Pass {
                CGPROGRAM
                    #pragma vertex vert
                    #pragma fragment frag
                    #pragma multi_compile_fog

                    #include "UnityCG.cginc"

                    struct appdata_t {
                        float4 vertex : POSITION;
                        float2 texcoord : TEXCOORD0;
                        UNITY_VERTEX_INPUT_INSTANCE_ID //Insert

                    };

                    struct v2f {
                        float4 vertex : SV_POSITION;
                        half2 texcoord : TEXCOORD0;
                        UNITY_FOG_COORDS(1)
                        UNITY_VERTEX_INPUT_INSTANCE_ID
                        UNITY_VERTEX_OUTPUT_STEREO//Insert
                    };
                    struct f_output {
                        float4 color : COLOR;
                    };
                    sampler2D _MainTex;
                    float4 _MainTex_ST;

                    UNITY_INSTANCING_BUFFER_START(Props)
                    UNITY_DEFINE_INSTANCED_PROP(float, _BackTransparency)
                        UNITY_DEFINE_INSTANCED_PROP(float4, _Color)
                        UNITY_INSTANCING_BUFFER_END(Props)

                    v2f vert(appdata_t v)
                    {
                        v2f o;

                        //single-pass
                        UNITY_SETUP_INSTANCE_ID(v);
                        UNITY_INITIALIZE_OUTPUT(v2f, o);
                        UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
                        UNITY_TRANSFER_INSTANCE_ID(v, o);
                        //end single-pass

                        o.vertex = UnityObjectToClipPos(v.vertex);
                        o.texcoord = TRANSFORM_TEX(v.texcoord, _MainTex);
                        UNITY_TRANSFER_FOG(o,o.vertex);
                        return o;
                    }

                    f_output frag(v2f i) : SV_Target
                    {
                        f_output output;
                        UNITY_INITIALIZE_OUTPUT(f_output, output);
                        UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(i);
                        float4 Color = UNITY_ACCESS_INSTANCED_PROP(Props, _Color);
                        sampler2D MainTex = UNITY_ACCESS_INSTANCED_PROP(Props, _MainTex);

                        output.color = tex2D(MainTex, i.texcoord);
                        if (output.color.x < 1) {
                            output.color = Color;
                        }
                        else {
                            discard;
                        }
                        return output;
                    }
                ENDCG
            }

           Pass {
                        Cull Front

                CGPROGRAM
                    #pragma vertex vert
                    #pragma fragment frag
                    #pragma multi_compile_fog

                    #include "UnityCG.cginc"

                    struct appdata_t {
                        float4 vertex : POSITION;
                        float2 texcoord : TEXCOORD0;
                        UNITY_VERTEX_INPUT_INSTANCE_ID //Insert

                    };

                    struct v2f {
                        float4 vertex : SV_POSITION;
                        half2 texcoord : TEXCOORD0;
                        UNITY_VERTEX_INPUT_INSTANCE_ID
                        UNITY_VERTEX_OUTPUT_STEREO//Insert
                    };
                    struct f_output {
                        float4 color : COLOR;
                    };
                    sampler2D _MainTex;
                    float4 _MainTex_ST;


                    UNITY_INSTANCING_BUFFER_START(Props)
                    UNITY_DEFINE_INSTANCED_PROP(float, _BackTransparency)
                    UNITY_DEFINE_INSTANCED_PROP(float4, _Color)
                    UNITY_INSTANCING_BUFFER_END(Props)
                    //end single pass

                    v2f vert(appdata_t v)
                    {
                        v2f o;

                        //single-pass
                        UNITY_SETUP_INSTANCE_ID(v);
                        UNITY_INITIALIZE_OUTPUT(v2f, o);
                        UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
                        UNITY_TRANSFER_INSTANCE_ID(v, o);
                        //end single-pass

                        o.vertex = UnityObjectToClipPos(v.vertex);
                        o.texcoord = TRANSFORM_TEX(v.texcoord, _MainTex);
                        UNITY_TRANSFER_FOG(o,o.vertex);
                        return o;
                    }

                    f_output frag(v2f i) : SV_Target
                    {
                        f_output output;
                        UNITY_INITIALIZE_OUTPUT(f_output, output);
                        UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(i);
                        float4 Color = UNITY_ACCESS_INSTANCED_PROP(Props, _Color);
                        float Trans = UNITY_ACCESS_INSTANCED_PROP(Props, _BackTransparency);
                        sampler2D MainTex = UNITY_ACCESS_INSTANCED_PROP(Props, _MainTex);

                        output.color = tex2D(MainTex, i.texcoord);
                        if (output.color.x < 1) {
                            output.color = float4(0,0,0,0);
                            output.color = 1.0 - Trans;
                        }
                        else {
                            discard;
                        }
                        
                        return output;
                    }
                ENDCG
            }
    }
}
