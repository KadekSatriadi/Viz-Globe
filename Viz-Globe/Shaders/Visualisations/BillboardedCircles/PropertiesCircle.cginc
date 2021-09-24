// single-pass
UNITY_INSTANCING_BUFFER_START(Props)
//using ColorHelper
#include "../ColorHelperProperties.cginc"
//end ColorHelper
UNITY_DEFINE_INSTANCED_PROP(float, _Radius)
UNITY_DEFINE_INSTANCED_PROP(float, _CircleMaxRadius)
UNITY_DEFINE_INSTANCED_PROP(float, _CircleMaxHeight)
UNITY_DEFINE_INSTANCED_PROP(float3, _BrushCenter[10])
UNITY_DEFINE_INSTANCED_PROP(float, _BrushRadius[10])
UNITY_DEFINE_INSTANCED_PROP(float4, _HighlightColor)
UNITY_INSTANCING_BUFFER_END(Props)
//end single pass
