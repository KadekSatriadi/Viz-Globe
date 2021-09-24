Shader "Custom/DoubleSidedUnlit"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
		_Color("Color", Color) = (1,1,1,1)
	   _RimPower("Rim Power", range(0.5, 8.0)) = 2
		_RimColor("Rim Color", Color) = (1,1,1,1)
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100
		
	

        Pass
        {
			Cull Back

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            // make fog work
            #pragma multi_compile_fog

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
				float3 normal : NORMAL;
				UNITY_VERTEX_INPUT_INSTANCE_ID //Insert

            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                UNITY_FOG_COORDS(1)
                float4 vertex : SV_POSITION;
				fixed4 color : COLOR;
				float3 normal : TEXCOORD2;
				float3 viewDir : TEXCOORD1;
				UNITY_FOG_COORDS(1)
				UNITY_VERTEX_INPUT_INSTANCE_ID
				UNITY_VERTEX_OUTPUT_STEREO//Insert
            };
			struct f_output {
				float4 color : COLOR;
			};


			// single-pass
			UNITY_INSTANCING_BUFFER_START(Props)
			UNITY_DEFINE_INSTANCED_PROP(sampler2D, _MainTex)
			UNITY_DEFINE_INSTANCED_PROP(float4, _MainTex_ST)
			UNITY_DEFINE_INSTANCED_PROP(float4, _RimColor)
			UNITY_DEFINE_INSTANCED_PROP(float4, _Color)
			UNITY_DEFINE_INSTANCED_PROP(half, _RimPower)
			UNITY_INSTANCING_BUFFER_END(Props)
			//end single pass

            v2f vert (appdata v)
            {
                v2f o;

				//single-pass
				UNITY_SETUP_INSTANCE_ID(v);
				UNITY_INITIALIZE_OUTPUT(v2f, o);
				UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
				UNITY_TRANSFER_INSTANCE_ID(v, o);
				//end single-pass

				float4x4 modelMatrix = unity_ObjectToWorld;
				float4x4 modelMatrixInverse = unity_WorldToObject;

                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
				o.normal = normalize(mul(float4(v.normal, 0.0), modelMatrixInverse).xyz);
				o.viewDir = normalize(_WorldSpaceCameraPos - mul(modelMatrix, v.vertex).xyz);
                UNITY_TRANSFER_FOG(o,o.vertex);
                return o;
            }

			f_output frag (v2f i) : SV_Target
            {
				 f_output output;
				UNITY_INITIALIZE_OUTPUT(f_output, output);
				UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(i);
				float4 Color = UNITY_ACCESS_INSTANCED_PROP(Props, _Color);
				float4 RimColor = UNITY_ACCESS_INSTANCED_PROP(Props, _RimColor);
				float RimPower = UNITY_ACCESS_INSTANCED_PROP(Props, _RimPower);
				sampler2D MainTex = UNITY_ACCESS_INSTANCED_PROP(Props, _MainTex);
                // sample the texture
				half rim = 1 - saturate(dot(i.viewDir, i.normal));
				fixed4 col = tex2D(MainTex, i.uv) * lerp(Color, RimColor, pow(rim, _RimPower));
				fixed4 final = col ;

				output.color = col;
                return output;
            }
            ENDCG
        }

	Pass
		{

			Cull Front


			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
				// make fog work
				#pragma multi_compile_fog

				#include "UnityCG.cginc"

				struct appdata
				{
					float4 vertex : POSITION;
					float2 uv : TEXCOORD0;
					UNITY_VERTEX_INPUT_INSTANCE_ID //Insert
				};

				struct v2f
				{
					float2 uv : TEXCOORD0;
					UNITY_FOG_COORDS(1)
					float4 vertex : SV_POSITION;
					UNITY_VERTEX_INPUT_INSTANCE_ID
						UNITY_VERTEX_OUTPUT_STEREO//Insert
				};
				struct f_output {
					float4 color : COLOR;
				};
				// single-pass
				UNITY_INSTANCING_BUFFER_START(Props)
					UNITY_DEFINE_INSTANCED_PROP(sampler2D, _MainTex)
					UNITY_DEFINE_INSTANCED_PROP(float4, _MainTex_ST)
					UNITY_DEFINE_INSTANCED_PROP(float4, _Color)
					UNITY_INSTANCING_BUFFER_END(Props)
					//end single pass

				v2f vert(appdata v)
				{
					v2f o;

					//single-pass
					UNITY_SETUP_INSTANCE_ID(v);
					UNITY_INITIALIZE_OUTPUT(v2f, o);
					UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
					UNITY_TRANSFER_INSTANCE_ID(v, o);
					//end single-pass

					o.vertex = UnityObjectToClipPos(v.vertex);
					o.uv = TRANSFORM_TEX(v.uv, _MainTex);
					UNITY_TRANSFER_FOG(o,o.vertex);
					return o;
				}

				f_output frag(v2f i) : SV_Target
				{
					f_output output;
				UNITY_INITIALIZE_OUTPUT(f_output, output);
				UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(i);

					// sample the texture
									float4 Color = UNITY_ACCESS_INSTANCED_PROP(Props, _Color);
				sampler2D MainTex = UNITY_ACCESS_INSTANCED_PROP(Props, _MainTex);
					fixed4 col = tex2D(MainTex, i.uv) * Color;
				
					output.color = col;

					return output;
				}
			ENDCG
		}
    }
		FallBack "VertexLit"
}
