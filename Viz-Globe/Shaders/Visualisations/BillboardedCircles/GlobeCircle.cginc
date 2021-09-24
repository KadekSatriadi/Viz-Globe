sampler2D _MainTex; //Insert
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
	o.uv3 = v.uv3;
	o.color = v.color;

	return o;
}


float3 CalculateNormal(float3 p1, float3 p2, float3 p3) {
	float3 normal = normalize(cross(
		normalize(p1 - p2),
		normalize(p1 - p3)
	));

	return normal;
}


//void AddTriangle(float4 color, float3 p1, float3 p2, float3 p3, float2 uv1, float2 uv2, float2 uv3, inout TriangleStream<g2f> triStream, v2g input[1])
//{
//	g2f o;
//
//	//single-pass
//	UNITY_SETUP_INSTANCE_ID(input[0]);
//	UNITY_INITIALIZE_OUTPUT(g2f, o);
//	UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input[0]);
//
//
//	UNITY_TRANSFER_VERTEX_OUTPUT_STEREO(input[0], o);
//	//o.normal = CalculateNormal(p1, p2, p3);
//	o.color = color;
//	o.vertex = UnityObjectToClipPos(p1);
//	o.uv = TRANSFORM_TEX(uv1, _MainTex);;
//	triStream.Append(o);
//
//	UNITY_TRANSFER_VERTEX_OUTPUT_STEREO(input[0], o);
//	//o.normal = CalculateNormal(p1, p2, p3);
//	o.color = color;
//	o.vertex = UnityObjectToClipPos(p2);
//	o.uv = TRANSFORM_TEX(uv2, _MainTex);;
//	triStream.Append(o);
//
//	UNITY_TRANSFER_VERTEX_OUTPUT_STEREO(input[0], o);
//	//o.normal = CalculateNormal(p1, p2, p3);
//	o.color = color;
//	o.vertex = UnityObjectToClipPos(p3);
//	o.uv = TRANSFORM_TEX(uv3, _MainTex);;
//	triStream.Append(o);
//	triStream.RestartStrip();
//}

void AddTriangle(float4 color, float3 p1, float3 p2, float3 p3, float2 uv1, float2 uv2, float2 uv3, inout TriangleStream<g2f> triStream, point v2g input[1])
{
	g2f o;

	//single-pass
	UNITY_SETUP_INSTANCE_ID(input[0]);
	UNITY_INITIALIZE_OUTPUT(g2f, o);
	UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input[0]);

	o.color = color;
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

bool isHighlighted(float3 vertex) {
	float3 BrushCenter[10] = UNITY_ACCESS_INSTANCED_PROP(Props, _BrushCenter);
	float BrushRadius[10] = UNITY_ACCESS_INSTANCED_PROP(Props, _BrushRadius);
	float GlobeRadius = UNITY_ACCESS_INSTANCED_PROP(Props, _Radius);
	bool result = false;
	for (uint i = 0; i < 10; i++) {
		//inside the brush
		if (distance(BrushCenter[i], vertex) <= BrushRadius[i] * GlobeRadius) {
			result = true;
			break;
		}
	}

	return result;
}

bool isHighlightEmpty() {
	float BrushRadius[10] = UNITY_ACCESS_INSTANCED_PROP(Props, _BrushRadius);
	bool result = true;
	for (uint i = 0; i < 10; i++) {
		if (BrushRadius[i] > 0) {
			result = false;
			break;
		}
	}

	return result;
}


//geometry shader
[maxvertexcount(6)]
void geom(point v2g input[1], inout TriangleStream<g2f> triStream)
{

	// Access instanced variables
	float CircleHeight = UNITY_ACCESS_INSTANCED_PROP(Props, _CircleMaxHeight);
	float CircleRadius = UNITY_ACCESS_INSTANCED_PROP(Props, _CircleMaxRadius);
	float4 MaxColor = UNITY_ACCESS_INSTANCED_PROP(Props, _MaxColor);
	float4 MinColor = UNITY_ACCESS_INSTANCED_PROP(Props, _MinColor);
	float Radius = UNITY_ACCESS_INSTANCED_PROP(Props, _Radius);

	float4 HighlightColor = UNITY_ACCESS_INSTANCED_PROP(Props, _HighlightColor);
	//single-pass

	float3 vertex = input[0].vertex.xyz;
	float2 latLon = input[0].uv;
	float value = input[0].uv2.x;
	float col = input[0].uv2.y;
	float height = input[0].uv3.x;

	//normal
	//float3 cameraUp = normalize(UNITY_MATRIX_IT_MV[1].xyz);
	//float3 cameraForward = UNITY_MATRIX_IT_MV[2].xyz;
	//float3 right = normalize(cross(cameraUp, cameraForward));

	float3 up = (transpose(mul(unity_WorldToObject, unity_MatrixInvV)))[1].xyz;
	float3 toCamera = (transpose(mul(unity_WorldToObject, unity_MatrixInvV)))[2].xyz;
	float3 right = normalize(cross(up, toCamera));

	//triangles
	float circleRadius = value * _CircleMaxRadius;

	float circleHeight = height * _CircleMaxHeight;
	//height
	float3 toCenterVector = -vertex;
	float3 position = vertex + (toCenterVector * circleHeight);
	float3 p1, p2, p3, p4;

	p1 = position - (right * circleRadius) + (up * circleRadius);
	p2 = position + (right * circleRadius) + (up * circleRadius);
	p3 = p2 - (up * circleRadius * 2);
	p4 = p1 - (up * circleRadius * 2);

	float4 color;
	color = getColor(col);
	if (!isHighlighted(vertex) && !isHighlightEmpty()) {
		color.rgb *= 0.35;
		color.a = 0.51;
	}

	//QUAD
	AddTriangle(color, p1, p3, p2, float2(0, 0), float2(1, 1), float2(1, 0), triStream, input);
	AddTriangle(color, p1, p4, p3, float2(0, 0), float2(0, 1), float2(1, 1), triStream, input);
}

f_output frag(g2f i) : SV_Target
{
	f_output output;
	UNITY_INITIALIZE_OUTPUT(f_output, output);
	UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(i);

	fixed4 col = tex2D(_MainTex, i.uv);;

	output.color = i.color *col;
	return output;
}
