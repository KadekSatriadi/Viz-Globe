Shader "Custom/Flow"
{
	Properties{
		 _MainTex("Albedo (RGB)", 2D) = "white" {}
		_AnimationSpeed("_AnimationSpeed",Range(0,5)) = 0.1

	}
	SubShader{
		 Tags {"Queue" = "Transparent" "RenderType" = "Transparent" }
		 LOD 200

		 CGPROGRAM
		 #pragma surface surf Lambert vertex:vert
		 #pragma target 3.0

		 struct Input {
			 float4 vertColor;
			 float2 uv_MainTex;
		 };

		sampler2D _MainTex;
		fixed _AnimationSpeed;

		 void vert(inout appdata_full v, out Input o) {
			 UNITY_INITIALIZE_OUTPUT(Input, o);
			 o.vertColor = v.color;
		 }

		 void surf(Input IN, inout SurfaceOutput o) {
			// o.Albedo = IN.vertColor.rgb;

			 fixed2 scrolledUV = IN.uv_MainTex; //***

			 fixed yScrollValue = frac(_AnimationSpeed * _Time.y); //***
			 scrolledUV -= fixed2(0, yScrollValue); //***

			 // Albedo comes from a texture tinted by color
			 fixed4 c = tex2D(_MainTex, scrolledUV) * IN.vertColor.rgba; //***
			 o.Albedo = c.rgb;
			 // Metallic and smoothness come from slider variables
			// o.Metallic = _Metallic;
			 //o.Smoothness = _Glossiness;
			 if (c.a < 0.5) discard;
			 o.Alpha = c.a;
		 }
		 ENDCG
	}
		FallBack "Diffuse"
}
