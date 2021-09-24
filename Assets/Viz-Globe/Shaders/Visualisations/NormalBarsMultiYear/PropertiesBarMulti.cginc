// single-pass
UNITY_INSTANCING_BUFFER_START(Props)
//using ColorHelper
#include "../ColorHelperProperties.cginc"
//end ColorHelper
UNITY_DEFINE_INSTANCED_PROP(float, _ActiveYear)
UNITY_DEFINE_INSTANCED_PROP(float, _Radius)
UNITY_DEFINE_INSTANCED_PROP(float, _Size)
UNITY_DEFINE_INSTANCED_PROP(float, _MaxRange)
UNITY_DEFINE_INSTANCED_PROP(sampler2D, _MainTex)
UNITY_DEFINE_INSTANCED_PROP(float4, _MainTex_ST)
UNITY_INSTANCING_BUFFER_END(Props)
//end single pass
