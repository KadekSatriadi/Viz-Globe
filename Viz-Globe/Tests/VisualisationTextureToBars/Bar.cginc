
sampler2D _MainTex;
float4  _MainTex_ST;

// single-pass
UNITY_INSTANCING_BUFFER_START(Props)
UNITY_DEFINE_INSTANCED_PROP(float4, _MaxColor)
UNITY_DEFINE_INSTANCED_PROP(float4, _MinColor)
UNITY_DEFINE_INSTANCED_PROP(float, _Size)
UNITY_DEFINE_INSTANCED_PROP(float, _MaxRange)
UNITY_DEFINE_INSTANCED_PROP(int, _ColorSteps)
UNITY_DEFINE_INSTANCED_PROP(float4, _ColorArray[15])
UNITY_INSTANCING_BUFFER_END(Props)
//end single pass

struct appdata
{
	float4 vertex : POSITION;
	float2 uv : TEXCOORD0;
	float2 uv2 : TEXCOORD1;
	float4 color: COLOR;
	float3 normal : NORMAL;
	UNITY_VERTEX_INPUT_INSTANCE_ID //Insert

};

struct v2g
{
	float4 vertex : SV_POSITION;
	float2 uv : TEXCOORD0;
	float2 uv2 : TEXCOORD1;
	float4 color: COLOR;
	float3 normal : NORMAL;
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
	o.normal = v.normal;

	return o;
}

//return color based on given colour pallete, val ranges from 0 - 1
float4 getColor(float val) {
	int i = floor(_ColorSteps * val);
	return _ColorArray[i];
}

//calculate normal in world space. p1, p2, p3 in local space.
float3 CalculateNormal(float3 p1, float3 p2, float3 p3) {
	float3 normal = normalize(cross(
		normalize(mul(unity_ObjectToWorld, p1) - mul(unity_ObjectToWorld, p2)),
		normalize(mul(unity_ObjectToWorld, p1) - mul(unity_ObjectToWorld, p3))
	));

	return normal;
}

void AddVertex(float3 v, float2 uv, float4 col, float3 normal, inout TriangleStream<g2f> triStream)
{
	g2f o;
	o.vertex = UnityObjectToClipPos(v);
	o.uv = uv;

	// get vertex normal in world space
	half3 worldNormal = normal;
	half nl = max(0, dot(worldNormal, _WorldSpaceLightPos0.xyz));
	o.color = nl * _LightColor0 * col;

	// the only difference from previous shader:
	// in addition to the diffuse lighting from the main light,
	// add illumination from ambient or light probes
	// ShadeSH9 function from UnityCG.cginc evaluates it,
	// using world space normal
	o.color.rgb += ShadeSH9(half4(worldNormal, 1));
	triStream.Append(o);
}

//geometry shader
[maxvertexcount(36)]
void geom(point v2g input[1], inout TriangleStream<g2f> triStream)
{
	g2f o;

	//single-pass
	UNITY_SETUP_INSTANCE_ID(input[0]);
	UNITY_INITIALIZE_OUTPUT(g2f, o);
	UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input[0]);
	// Access instanced variables
	float MaxRange = UNITY_ACCESS_INSTANCED_PROP(Props, _MaxRange);
	float Size = UNITY_ACCESS_INSTANCED_PROP(Props, _Size);
	float4 MaxColor = UNITY_ACCESS_INSTANCED_PROP(Props, _MaxColor);
	float4 MinColor = UNITY_ACCESS_INSTANCED_PROP(Props, _MinColor);
	//single-pass

	float2 latLon = input[0].uv;
	float value = input[0].uv2.x;

	float3 c = input[0].vertex.xyz;
	float height = lerp(0, _MaxRange, value);

	float3 p1, p2, p3, p4;
	float3 p5, p6, p7, p8;

	//bottom
	p1 = float3(c.x - _Size, c.y + _Size, 0);
	p2 = float3(c.x + _Size, c.y + _Size, 0);
	p3 = float3(c.x + _Size, c.y - _Size, 0);
	p4 = float3(c.x - _Size, c.y - _Size, 0);

	//top
	p5 = p1 + float3(0, 0, -height);
	p6 = p2 + float3(0, 0, -height);
	p7 = p3 + float3(0, 0, -height);
	p8 = p4 + float3(0, 0, -height);

	float4 color = getColor(value);
	float2 uv;
	float3 normal;
	//BOTTOM
		//triangle 1, p1, p2, p3
	normal = CalculateNormal(p1, p2, p3);

	uv = TRANSFORM_TEX(float2(0, 1), _MainTex);
	AddVertex(p1, uv, color, normal, triStream);
	uv = TRANSFORM_TEX(float2(1, 1), _MainTex);
	AddVertex(p2, uv, color, normal, triStream);
	uv = TRANSFORM_TEX(float2(1, 0), _MainTex);
	AddVertex(p3, uv, color, normal, triStream);
	triStream.RestartStrip();

	//triangle 2, p1, p3, p4
	uv = TRANSFORM_TEX(float2(0, 1), _MainTex);
	AddVertex(p1, uv, color, normal, triStream);
	uv = TRANSFORM_TEX(float2(1, 0), _MainTex);
	AddVertex(p3, uv, color, normal, triStream);
	uv = TRANSFORM_TEX(float2(0, 0), _MainTex);
	AddVertex(p4, uv, color, normal, triStream);
	triStream.RestartStrip();


	//TOP
	//triangle 1, p5, p6, p7
	normal = CalculateNormal(p5, p6, p7);

	uv = TRANSFORM_TEX(float2(0, 1), _MainTex);
	AddVertex(p5, uv, color, normal, triStream);
	uv = TRANSFORM_TEX(float2(1, 1), _MainTex);
	AddVertex(p6, uv, color, normal, triStream);
	uv = TRANSFORM_TEX(float2(1, 0), _MainTex);
	AddVertex(p7, uv, color, normal, triStream);
	triStream.RestartStrip();


	//triangle 2, p5, p7, p8
	uv = TRANSFORM_TEX(float2(0, 1), _MainTex);
	AddVertex(p5, uv, color, normal, triStream);
	uv = TRANSFORM_TEX(float2(1, 0), _MainTex);
	AddVertex(p7, uv, color, normal, triStream);
	uv = TRANSFORM_TEX(float2(0, 0), _MainTex);
	AddVertex(p8, uv, color, normal, triStream);
	triStream.RestartStrip();


	//SIDE 1
	//triangle 1, p1, p2, p5
	normal = CalculateNormal(p1, p2, p5);

	uv = TRANSFORM_TEX(float2(0, 1), _MainTex);
	AddVertex(p1, uv, color, normal, triStream);
	uv = TRANSFORM_TEX(float2(1, 1), _MainTex);
	AddVertex(p2, uv, color, normal, triStream);
	uv = TRANSFORM_TEX(float2(1, 0), _MainTex);
	AddVertex(p5, uv, color, normal, triStream);
	triStream.RestartStrip();


	//triangle 2, p2, p6, p5
	uv = TRANSFORM_TEX(float2(0, 1), _MainTex);
	AddVertex(p2, uv, color, normal, triStream);
	uv = TRANSFORM_TEX(float2(1, 0), _MainTex);
	AddVertex(p6, uv, color, normal, triStream);
	uv = TRANSFORM_TEX(float2(0, 0), _MainTex);
	AddVertex(p5, uv, color, normal, triStream);
	triStream.RestartStrip();


	//SIDE 2
	//triangle 1, p1, p5, p8
	normal = CalculateNormal(p1, p5, p8);

	uv = TRANSFORM_TEX(float2(0, 1), _MainTex);
	AddVertex(p1, uv, color, normal, triStream);
	uv = TRANSFORM_TEX(float2(1, 1), _MainTex);
	AddVertex(p5, uv, color, normal, triStream);
	uv = TRANSFORM_TEX(float2(1, 0), _MainTex);
	AddVertex(p8, uv, color, normal, triStream);
	triStream.RestartStrip();

	//triangle 2, p1, p8, p4
	uv = TRANSFORM_TEX(float2(0, 1), _MainTex);
	AddVertex(p1, uv, color, normal, triStream);
	uv = TRANSFORM_TEX(float2(1, 0), _MainTex);
	AddVertex(p8, uv, color, normal, triStream);
	uv = TRANSFORM_TEX(float2(0, 0), _MainTex);
	AddVertex(p4, uv, color, normal, triStream);
	triStream.RestartStrip();


	//SIDE 3
	//triangle 1, p4, p8, p7
	normal = CalculateNormal(p4, p8, p7);

	uv = TRANSFORM_TEX(float2(0, 1), _MainTex);
	AddVertex(p4, uv, color, normal, triStream);
	uv = TRANSFORM_TEX(float2(1, 1), _MainTex);
	AddVertex(p8, uv, color, normal, triStream);
	uv = TRANSFORM_TEX(float2(1, 0), _MainTex);
	AddVertex(p7, uv, color, normal, triStream);
	triStream.RestartStrip();


	//triangle 2, p4, p7, p3
	uv = TRANSFORM_TEX(float2(0, 1), _MainTex);
	AddVertex(p4, uv, color, normal, triStream);
	uv = TRANSFORM_TEX(float2(1, 0), _MainTex);
	AddVertex(p7, uv, color, normal, triStream);
	uv = TRANSFORM_TEX(float2(0, 0), _MainTex);
	AddVertex(p3, uv, color, normal, triStream);
	triStream.RestartStrip();


	//SIDE 4
	//triangle 1, p3, p7, p6
	normal = CalculateNormal(p3, p7, p6);

	uv = TRANSFORM_TEX(float2(0, 1), _MainTex);
	AddVertex(p3, uv, color, normal, triStream);
	uv = TRANSFORM_TEX(float2(1, 1), _MainTex);
	AddVertex(p7, uv, color, normal, triStream);
	uv = TRANSFORM_TEX(float2(1, 0), _MainTex);
	AddVertex(p6, uv, color, normal, triStream);
	triStream.RestartStrip();


	//triangle 2, p3, p6, p2
	uv = TRANSFORM_TEX(float2(0, 1), _MainTex);
	AddVertex(p3, uv, color, normal, triStream);
	uv = TRANSFORM_TEX(float2(1, 0), _MainTex);
	AddVertex(p6, uv, color, normal, triStream);
	uv = TRANSFORM_TEX(float2(0, 0), _MainTex);
	AddVertex(p2, uv, color, normal, triStream);
	triStream.RestartStrip();

}

f_output frag(g2f i) : SV_Target
{
	f_output output;
	UNITY_INITIALIZE_OUTPUT(f_output, output);
	UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(i);

	fixed4 col = tex2D(_MainTex, i.uv);

	output.color = i.color * col;
	return output;
}
