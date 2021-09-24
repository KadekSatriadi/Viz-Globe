Shader "TangibleGlobe/TransparentColorMask"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
		_TransparentColor("Transparent Color", Color) = (1,1,1,1)
		_Threshold ("Threshhold", Range(0,0.1)) = 0.1
		_InteriorBrightness("Interior Brightness", Range(0,2)) = 0.1
    }
     SubShader
    {
        Tags { "RenderType"="Transparent" }
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
				UNITY_VERTEX_INPUT_INSTANCE_ID

            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                UNITY_FOG_COORDS(1)
                float4 vertex : SV_POSITION;
				fixed4 color : COLOR;
				float3 normal : TEXCOORD2;
				float3 viewDir : TEXCOORD1;
				UNITY_VERTEX_INPUT_INSTANCE_ID
				UNITY_VERTEX_OUTPUT_STEREO
            };

			struct f_output {
				float4 color : COLOR;

			};

			sampler2D _MainTex; //Insert
			float4 _MainTex_ST;
			// single-pass
			UNITY_INSTANCING_BUFFER_START(Props)
			UNITY_DEFINE_INSTANCED_PROP(float4, _TransparentColor)
			UNITY_DEFINE_INSTANCED_PROP(half, _Threshold)
			UNITY_INSTANCING_BUFFER_END(Props)
				//end single pass
            v2f vert (appdata v)
            {
                v2f o;
				UNITY_SETUP_INSTANCE_ID(v);
				UNITY_INITIALIZE_OUTPUT(v2f, o);
				UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
				UNITY_TRANSFER_INSTANCE_ID(v, o);

				float4x4 modelMatrix = unity_ObjectToWorld;
				float4x4 modelMatrixInverse = unity_WorldToObject;

                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
				o.normal = normalize(mul(float4(v.normal, 0.0), modelMatrixInverse).xyz);
				o.viewDir = normalize(_WorldSpaceCameraPos - mul(modelMatrix, v.vertex).xyz);
                UNITY_TRANSFER_FOG(o,o.vertex);
                return o;
            }

			f_output frag(v2f i) : SV_Target
			{
				f_output output;
				
				UNITY_INITIALIZE_OUTPUT(f_output, output);
				UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(i);
				float4 TransparentColor = UNITY_ACCESS_INSTANCED_PROP(Props, _TransparentColor);
				half Threshold = UNITY_ACCESS_INSTANCED_PROP(Props, _Threshold);

                // sample the texture
					fixed4 col = tex2D(_MainTex, i.uv) ;
					// apply fog
					UNITY_APPLY_FOG(i.fogCoord, col);
					//transparent mask
					half3 diff = col.xyz - TransparentColor.xyz;
					half diffSquared = dot(diff, diff);
					if (diffSquared < Threshold) discard;
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
					UNITY_VERTEX_INPUT_INSTANCE_ID

				};

				struct v2f
				{
					float2 uv : TEXCOORD0;
					UNITY_FOG_COORDS(1)
					float4 vertex : SV_POSITION;
					UNITY_VERTEX_INPUT_INSTANCE_ID
						UNITY_VERTEX_OUTPUT_STEREO
				};

				struct f_output {
					float4 color : COLOR;

				};

				sampler2D _MainTex;
				float4 _MainTex_ST;
				// single-pass
				UNITY_INSTANCING_BUFFER_START(Props)
					UNITY_DEFINE_INSTANCED_PROP(float4, _TransparentColor)
					UNITY_DEFINE_INSTANCED_PROP(half, _Threshold)
					UNITY_DEFINE_INSTANCED_PROP(half, _InteriorBrightness)
				UNITY_INSTANCING_BUFFER_END(Props)
					//end single pass

				v2f vert(appdata v)
				{
					v2f o;
					UNITY_SETUP_INSTANCE_ID(v);
					UNITY_INITIALIZE_OUTPUT(v2f, o);
					UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
					UNITY_TRANSFER_INSTANCE_ID(v, o);

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

				float4 TransparentColor = UNITY_ACCESS_INSTANCED_PROP(Props, _TransparentColor);
				half Threshold = UNITY_ACCESS_INSTANCED_PROP(Props, _Threshold);
				half InteriorBrightness = UNITY_ACCESS_INSTANCED_PROP(Props, _InteriorBrightness);

					// sample the texture
					fixed4 col = tex2D(_MainTex, i.uv) ;

					//transparent mask
					half3 diff = col.xyz - TransparentColor.xyz;
					half diffSquared = dot(diff, diff);
					if(diffSquared < Threshold) discard;

					// apply fog
					UNITY_APPLY_FOG(i.fogCoord, col);
					output.color = col * InteriorBrightness;
					return output;
				}
			ENDCG
		}
    }
		FallBack "VertexLit"
}
