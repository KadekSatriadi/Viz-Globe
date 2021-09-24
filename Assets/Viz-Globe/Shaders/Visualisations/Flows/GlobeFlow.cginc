sampler2D _MainTex;
float4 _MainTex_ST;

struct appdata
{
	float4 vertex : POSITION;
	float2 uv : TEXCOORD0;
	float2 uv2 : TEXCOORD1;
	float2 uv3 : TEXCOORD2;
	float4 color: COLOR;
	UNITY_VERTEX_INPUT_INSTANCE_ID //Insert

};

struct v2g
{
	float4 vertex : SV_POSITION;
	float2 uv : TEXCOORD0;
	float2 uv2 : TEXCOORD1;
	float2 uv3 : TEXCOORD2;
	float4 color: COLOR;
	UNITY_VERTEX_INPUT_INSTANCE_ID
	UNITY_VERTEX_OUTPUT_STEREO//Insert

};

struct g2f
{
	float4 vertex : SV_POSITION;
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
	o.uv3 = v.uv3;
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

void AddTriangle(float4 color, float3 p1, float3 p2, float3 p3, inout TriangleStream<g2f> triStream) 
{
	g2f o;

	half3 worldNormal = CalculateNormal(p1, p2, p3);
	half nl = max(0, dot(worldNormal, _WorldSpaceLightPos0.xyz));
	o.color = nl * _LightColor0 * color;

	// the only difference from previous shader:
	// in addition to the diffuse lighting from the main light,
	// add illumination from ambient or light probes
	// ShadeSH9 function from UnityCG.cginc evaluates it,
	// using world space normal
	o.color.rgb += ShadeSH9(half4(worldNormal, 1));

	o.vertex = UnityObjectToClipPos(p1);
	UNITY_TRANSFER_VERTEX_OUTPUT_STEREO(input[0], o);
	triStream.Append(o);

	o.vertex = UnityObjectToClipPos(p2);
	UNITY_TRANSFER_VERTEX_OUTPUT_STEREO(input[0], o);
	triStream.Append(o);

	o.vertex = UnityObjectToClipPos(p3);
	UNITY_TRANSFER_VERTEX_OUTPUT_STEREO(input[0], o);
	triStream.Append(o);
	triStream.RestartStrip();
}

//geometry shader
[maxvertexcount(128)]
void geom(point v2g input[1], inout TriangleStream<g2f> triStream)
{
	g2f o;

	//single-pass
	UNITY_SETUP_INSTANCE_ID(input[0]);
	UNITY_INITIALIZE_OUTPUT(g2f, o);
	UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input[0]);
	// Access instanced variables
	float Radius = UNITY_ACCESS_INSTANCED_PROP(Props, _Radius);
	float MaxFlowRadius = UNITY_ACCESS_INSTANCED_PROP(Props, _MaxFlowRadius);
	//single-pass

	float3 vertex1 = input[0].vertex.xyz;
	float3 vertex2 = input[0].color.rgab;
	float2 latLon1 = input[0].uv;
	float2 latLon2 = input[0].uv;
	float value = input[0].uv2.x;	
	float r = 0.1;

	//triangles
	float3 direction = (vertex2 - vertex1);
	float3 side = normalize(cross(direction, float3(0, 1, 0)));
	float4 color = float4(1, 1, 1, 1);

	uint nTubeSegment = 10;
	uint nCircleSegment = 10;
	float stepSeg = 1.0 / nTubeSegment;
	float stepDeg = 360.0 / nCircleSegment;
	float3 pivot1 = vertex1;
	float3 pivot2 = vertex1 + (normalize(direction) * stepSeg);

	float3 p1, p2, p3, p4;
	for (uint i = 0; i < nTubeSegment; i++) {
		direction = pivot2 - pivot1;
		for (uint j = 0; j < nCircleSegment; j++) // circle
		{
			//https://stackoverflow.com/questions/27714014/3d-point-on-circumference-of-a-circle-with-a-center-radius-and-normal-vector
			//create a new coordinate system (v1, v2, v3) whose normal is pivot2 - pivot1
			//p = centerPoint + R * (cos(a) * v1 + sin(a) * v2)

			float3 v3 = normalize(direction);
			float3 v1 = normalize(float3(v3.z, 0, -v3.x));
			float3 v2 = cross(v3, v1);

			float angle1 = radians(stepDeg * j);
			float angle2 = radians(stepDeg * (j + 1));

			p1 = pivot1 + r * (cos(angle1) * v1 + sin(angle1) * v2);
			p2 = pivot1 + r * (cos(angle2) * v1 + sin(angle2) * v2);
			p3 = pivot2 + r * (cos(angle2) * v1 + sin(angle2) * v2);
			p4 = pivot2 + r * (cos(angle1) * v1 + sin(angle1) * v2);

			AddTriangle(color, p1, p2, p3, triStream);
			AddTriangle(color, p1, p3, p4, triStream);
		}
		pivot1 = pivot2;
		pivot2 = pivot1 + (normalize(direction) * stepSeg * (i + 1));
	}
	
}

f_output frag(g2f i) : SV_Target
{
	f_output output;
	UNITY_INITIALIZE_OUTPUT(f_output, output);
	UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(i);
	output.color = i.color;

	return output;
}
