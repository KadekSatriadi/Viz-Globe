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
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                UNITY_FOG_COORDS(1)
                float4 vertex : SV_POSITION;
				fixed4 color : COLOR;
				float3 normal : TEXCOORD2;
				float3 viewDir : TEXCOORD1;
            };

			sampler2D _MainTex;
			float4 _MainTex_ST;
			float4 _Color;
			float4 _RimColor;
			half _RimPower;

            v2f vert (appdata v)
            {
                v2f o;

				float4x4 modelMatrix = unity_ObjectToWorld;
				float4x4 modelMatrixInverse = unity_WorldToObject;

                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
				o.normal = normalize(mul(float4(v.normal, 0.0), modelMatrixInverse).xyz);
				o.viewDir = normalize(_WorldSpaceCameraPos - mul(modelMatrix, v.vertex).xyz);
                UNITY_TRANSFER_FOG(o,o.vertex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // sample the texture
				half rim = 1 - saturate(dot(i.viewDir, i.normal));
				fixed4 col = tex2D(_MainTex, i.uv) * lerp(_Color, _RimColor, pow(rim, _RimPower));
				fixed4 final = col ;
                // apply fog
                UNITY_APPLY_FOG(i.fogCoord, final);
                return final;
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
				};

				struct v2f
				{
					float2 uv : TEXCOORD0;
					UNITY_FOG_COORDS(1)
					float4 vertex : SV_POSITION;
				};

				sampler2D _MainTex;
				float4 _MainTex_ST;
				float4 _Color;

				v2f vert(appdata v)
				{
					v2f o;
					o.vertex = UnityObjectToClipPos(v.vertex);
					o.uv = TRANSFORM_TEX(v.uv, _MainTex);
					UNITY_TRANSFER_FOG(o,o.vertex);
					return o;
				}

				fixed4 frag(v2f i) : SV_Target
				{
					// sample the texture
					fixed4 col = tex2D(_MainTex, i.uv) * _Color;
				// apply fog
				UNITY_APPLY_FOG(i.fogCoord, col);
				return col;
				}
			ENDCG
		}
    }
		FallBack "VertexLit"
}
