Shader "VizGlobe/VisualisationNormalBar"
{
    Properties
    {
        _MaxColor ("MaxColor", Color) = (1,1,1,1)
        _MinColor ("MinColor", Color) = (1,1,1,1)
		_Radius("Radius", float) = 1
		_Size("Size", float) = 1
		_MaxRange("MaxRange", float) = 1
		_MainTex("Texture", 2D) = "white" {}
	}
		SubShader
	{

		Tags { "Queue" = "Transparent" "RenderType" = "Transparent" "IgnoreProjector" = "True" }
		Pass
		{
		CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma geometry geom


			sampler2D _MainTex;
			float4 _MaxColor;
			float4 _MinColor;
			float _Radius;
			float _Size;
			float _MaxRange;
			float3 _GlobeWorldPos;

			struct appdata
			{
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
				float2 uv2 : TEXCOORD1;
				float4 color: COLOR;
			};

			struct v2g
			{
				float4 vertex : SV_POSITION;
				float2 uv : TEXCOORD0;
				float2 uv2 : TEXCOORD1;
				float4 color: COLOR;
			};

			struct g2f
			{
				float4 vertex : SV_POSITION;
				float2 uv : TEXCOORD0;
				float4 color: COLOR;
			};

			v2g vert(appdata v)
			{
				v2g o;
				o.vertex = v.vertex;
				o.uv = v.uv;
				o.uv2 = v.uv2;
				o.color = v.color;
				return o;
			}

			[maxvertexcount(36)]
			void geom(point v2g input[1], inout TriangleStream<g2f> triStream)
			{
				float2 latLon = input[0].uv;
				float value = input[0].uv2.x * 5;
				float color = input[0].uv2.y;

				float3 vertex = input[0].vertex.xyz;
				float range = value * _MaxRange;

				//tangents
				float3 fromCenter = vertex;
				float3 northPole = float3(0, _Radius, 0);
				float3 polarTangent = normalize(northPole - vertex) * _Size;
				float3 horizontalTangent = normalize(cross(polarTangent, fromCenter)) * _Size;

				//bottom quad
				float3 p1, p2, p3, p4;

				p1 = vertex - horizontalTangent;
				p1 = p1 + normalize(cross(vertex - p1, -p1)) * _Size;
				p2 = vertex + horizontalTangent;
				p2 = p2 - normalize(cross(vertex - p2, -p2)) * _Size;
				p3 = vertex + horizontalTangent;
				p3 = p3 + normalize(cross(vertex - p3, -p3)) * _Size;
				p4 = vertex - horizontalTangent;
				p4 = p4 - normalize(cross(vertex - p4, -p4)) * _Size;

				//top quad
				float3 p5, p6, p7, p8;
				p5 = p1 + normalize(fromCenter) * range;
				p6 = p2 + normalize(fromCenter) * range;
				p7 = p3 + normalize(fromCenter) * range;
				p8 = p4 + normalize(fromCenter) * range;

				g2f o;
				o.uv = input[0].uv;
				o.color = lerp(_MinColor, _MaxColor, color);

				//BOTTOM
				//triangle 1, p1, p2, p4
				o.vertex = UnityObjectToClipPos(p1);
				triStream.Append(o);
				o.vertex = UnityObjectToClipPos(p2);
				triStream.Append(o);
				o.vertex = UnityObjectToClipPos(p4);
				triStream.Append(o);
				triStream.RestartStrip();

				//triangle 2, p4, p2, p3
				o.vertex = UnityObjectToClipPos(p4);
				triStream.Append(o);
				o.vertex = UnityObjectToClipPos(p2);
				triStream.Append(o);
				o.vertex = UnityObjectToClipPos(p3);
				triStream.Append(o);
				triStream.RestartStrip();


				//TOP
				//triangle 11, p5, p6, p7
				o.vertex = UnityObjectToClipPos(p8);
				triStream.Append(o);
				o.vertex = UnityObjectToClipPos(p6);
				triStream.Append(o);
				o.vertex = UnityObjectToClipPos(p5);
				triStream.Append(o);
				triStream.RestartStrip();

				//triangle p5, p7, p8
				o.vertex = UnityObjectToClipPos(p7);
				triStream.Append(o);
				o.vertex = UnityObjectToClipPos(p6);
				triStream.Append(o);
				o.vertex = UnityObjectToClipPos(p8);
				triStream.Append(o);
				triStream.RestartStrip();

				//SIDES
				o.vertex = UnityObjectToClipPos(p2);
				triStream.Append(o);
				o.vertex = UnityObjectToClipPos(p6);
				triStream.Append(o);
				o.vertex = UnityObjectToClipPos(p7);
				triStream.Append(o);
				triStream.RestartStrip();
				o.vertex = UnityObjectToClipPos(p2);
				triStream.Append(o);
				o.vertex = UnityObjectToClipPos(p7);
				triStream.Append(o);
				o.vertex = UnityObjectToClipPos(p3);
				triStream.Append(o);
				triStream.RestartStrip();

				o.vertex = UnityObjectToClipPos(p7);
				triStream.Append(o);
				o.vertex = UnityObjectToClipPos(p8);
				triStream.Append(o);
				o.vertex = UnityObjectToClipPos(p3);
				triStream.Append(o);
				triStream.RestartStrip();
				o.vertex = UnityObjectToClipPos(p8);
				triStream.Append(o);
				o.vertex = UnityObjectToClipPos(p4);
				triStream.Append(o);
				o.vertex = UnityObjectToClipPos(p3);
				triStream.Append(o);
				triStream.RestartStrip();

				o.vertex = UnityObjectToClipPos(p4);
				triStream.Append(o);
				o.vertex = UnityObjectToClipPos(p5);
				triStream.Append(o);
				o.vertex = UnityObjectToClipPos(p1);
				triStream.Append(o);
				triStream.RestartStrip();
				o.vertex = UnityObjectToClipPos(p8);
				triStream.Append(o);
				o.vertex = UnityObjectToClipPos(p5);
				triStream.Append(o);
				o.vertex = UnityObjectToClipPos(p4);
				triStream.Append(o);
				triStream.RestartStrip();

				o.vertex = UnityObjectToClipPos(p1);
				triStream.Append(o);
				o.vertex = UnityObjectToClipPos(p5);
				triStream.Append(o);
				o.vertex = UnityObjectToClipPos(p6);
				triStream.Append(o);
				triStream.RestartStrip();
				o.vertex = UnityObjectToClipPos(p1);
				triStream.Append(o);
				o.vertex = UnityObjectToClipPos(p6);
				triStream.Append(o);
				o.vertex = UnityObjectToClipPos(p2);
				triStream.Append(o);
				triStream.RestartStrip();

			}

			fixed4 frag(g2f input) : SV_Target
			{
				// sample the texture
				fixed4 col = tex2D(_MainTex, input.uv) * input.color;
				return col;
			}

			
		ENDCG
		}
    }
    FallBack "Diffuse"
}
