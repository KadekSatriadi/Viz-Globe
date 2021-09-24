

struct appdata
{
	float4 vertex : POSITION;
	float2 uv : TEXCOORD0;
	float2 uv2 : TEXCOORD1;
	float4 color: COLOR;
	UNITY_VERTEX_INPUT_INSTANCE_ID //Insert

};

struct v2g
{
	float4 vertex : SV_POSITION;
	float2 uv : TEXCOORD0;
	float2 uv2 : TEXCOORD1;
	float4 color: COLOR;
	UNITY_VERTEX_INPUT_INSTANCE_ID
	UNITY_VERTEX_OUTPUT_STEREO//Insert

};

struct g2f
{
	float4 vertex : SV_POSITION;
	float2 uv : TEXCOORD0;
	float4 color: COLOR;
	UNITY_VERTEX_INPUT_INSTANCE_ID
	UNITY_VERTEX_OUTPUT_STEREO
};


struct f_output {
	float4 color : COLOR;
};
//vertex shader
v2g vert(appdata v)
{
	v2g o;

	//single-pass
	UNITY_SETUP_INSTANCE_ID(v);
	UNITY_INITIALIZE_OUTPUT(v2g, o);
	UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
	UNITY_TRANSFER_INSTANCE_ID(v, o);
	//end single-pass

	o.vertex = v.vertex;
	o.uv = v.uv;
	o.uv2 = v.uv2;
	o.color = v.color;

	return o;
}

//calculate normal in world space. p1, p2, p3 in local space.
float3 CalculateNormal(float3 p1, float3 p2, float3 p3) {
	float3 normal = normalize(cross(
		normalize(mul(unity_ObjectToWorld, p1) - mul(unity_ObjectToWorld, p2)),
		normalize(mul(unity_ObjectToWorld, p1) - mul(unity_ObjectToWorld, p3))
	));

	return normal;
}

void AddTriangle(float4 color, float3 p1, float3 p2, float3 p3, float2 uv1, float2 uv2, float2 uv3, inout TriangleStream<g2f> triStream, point v2g input[1])
{
	g2f o;
	//single-pass
	UNITY_SETUP_INSTANCE_ID(input[0]);
	UNITY_INITIALIZE_OUTPUT(g2f, o);
	UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input[0]);
	// get vertex normal in world space
	half3 worldNormal = CalculateNormal(p1,p2,p3);
	half nl = max(0, dot(worldNormal, _WorldSpaceLightPos0.xyz));
	o.color = nl * _LightColor0 * color;

	// the only difference from previous shader:
	// in addition to the diffuse lighting from the main light,
	// add illumination from ambient or light probes
	// ShadeSH9 function from UnityCG.cginc evaluates it,
	// using world space normal
	o.color.rgb += ShadeSH9(half4(worldNormal, 1));

	o.vertex = UnityObjectToClipPos(p1);
	o.uv = uv1;
	UNITY_TRANSFER_VERTEX_OUTPUT_STEREO(input[0], o);
	triStream.Append(o);

	o.vertex = UnityObjectToClipPos(p2);
	o.uv = uv2;
	UNITY_TRANSFER_VERTEX_OUTPUT_STEREO(input[0], o);
	triStream.Append(o);

	o.vertex = UnityObjectToClipPos(p3);
	o.uv = uv3;
	UNITY_TRANSFER_VERTEX_OUTPUT_STEREO(input[0], o);
	triStream.Append(o);
	triStream.RestartStrip();
}

//geometry shader
[maxvertexcount(36)]
void geom(point v2g input[1], inout TriangleStream<g2f> triStream)
{

	// Access instanced variables
	float MaxRange = UNITY_ACCESS_INSTANCED_PROP(Props, _MaxRange);
	float Size = UNITY_ACCESS_INSTANCED_PROP(Props, _Size);
	float4 MaxColor = UNITY_ACCESS_INSTANCED_PROP(Props, _MaxColor);
	float4 MinColor = UNITY_ACCESS_INSTANCED_PROP(Props, _MinColor);
	float Radius = UNITY_ACCESS_INSTANCED_PROP(Props, _Radius);
	//single-pass

	float2 latLon = input[0].uv;
	float value = input[0].uv2.x;
	float col = input[0].uv2.y;

	float3 vertex = input[0].vertex.xyz;
	//float range = value * MaxRange *(1 + _SinTime.w) / 2;
	float range = value * MaxRange;

	//tangents
	float3 fromCenter = vertex;
	float3 northPole = float3(0, Radius, 0);
	float3 polarTangent = normalize(northPole - vertex) * Size;
	float3 horizontalTangent = normalize(cross(polarTangent, fromCenter)) * Size;

	//bottom quad
	float3 p1, p2, p3, p4;

	p1 = vertex - horizontalTangent;
	p1 = p1 + normalize(cross(vertex - p1, -p1)) * Size;
	p2 = vertex + horizontalTangent;
	p2 = p2 - normalize(cross(vertex - p2, -p2)) * Size;
	p3 = vertex + horizontalTangent;
	p3 = p3 + normalize(cross(vertex - p3, -p3)) * Size;
	p4 = vertex - horizontalTangent;
	p4 = p4 - normalize(cross(vertex - p4, -p4)) * Size;

	//top quad
	float3 p5, p6, p7, p8;
	p5 = p1 + normalize(fromCenter) * range;
	p6 = p2 + normalize(fromCenter) * range;
	p7 = p3 + normalize(fromCenter) * range;
	p8 = p4 + normalize(fromCenter) * range;

	//o.uv = input[0].uv;

	float4 color = getColor(col);

	//BOTTOM
	//triangle 1, p1, p2, p4
	AddTriangle(color, p1, p2, p4, float2(0, 0), float2(0, 1), float2(1, 0), triStream, input);

	//triangle 2, p4, p2, p3
	AddTriangle(color, p4, p2, p3, float2(1, 0), float2(0, 1), float2(1, 1), triStream, input);

	//TOP
	//triangle 11, p5, p6, p7
	AddTriangle(color, p8, p6, p5, float2(1, 0), float2(0, 1), float2(1, 1), triStream, input);

	//triangle p5, p7, p8
	AddTriangle(color, p7, p6, p8, float2(0, 0), float2(0, 1), float2(1, 0), triStream, input);

	
	//SIDES
	AddTriangle(color, p2, p6, p7, float2(1, 1), float2(1, 0), float2(0, 0), triStream, input);
	AddTriangle(color, p2, p7, p3, float2(0, 0), float2(1, 1), float2(0, 1), triStream, input);
	//end 

	AddTriangle(color, p7, p8, p3, float2(1, 1), float2(1, 0), float2(0, 1), triStream, input);
	AddTriangle(color, p8, p4, p3, float2(1, 0), float2(1, 1), float2(0, 1), triStream, input);
	//end

	AddTriangle(color, p4, p5, p1, float2(0, 0), float2(1, 1), float2(0, 1), triStream, input);
	AddTriangle(color, p8, p5, p4, float2(1, 0), float2(0, 0), float2(0, 1), triStream, input);
	//end

	AddTriangle(color, p1, p5, p6, float2(1, 0), float2(0, 0), float2(0, 1), triStream, input);
	AddTriangle(color, p1, p6, p2, float2(1, 0), float2(0, 1), float2(1, 1), triStream, input);
	//end
}

f_output frag(g2f i) : SV_Target
{
	f_output output;
	UNITY_INITIALIZE_OUTPUT(f_output, output);
	UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(i);
	sampler2D MainTex = UNITY_ACCESS_INSTANCED_PROP(Props, _MainTex);

	fixed4 col = tex2D(MainTex, i.uv);

	output.color = i.color * col;

	return output;
}
