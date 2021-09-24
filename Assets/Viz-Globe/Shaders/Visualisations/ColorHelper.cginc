//include these on Instance Buffer
//UNITY_DEFINE_INSTANCED_PROP(uint, _ColorType)
//UNITY_DEFINE_INSTANCED_PROP(float4, _MaxColor)
//UNITY_DEFINE_INSTANCED_PROP(float4, _MinColor)
//UNITY_DEFINE_INSTANCED_PROP(int, _ColorSteps)
//UNITY_DEFINE_INSTANCED_PROP(float4, _ColorArray[15])
//end include


//return color based on given colour pallete, val ranges from 0 - 1
float4 getColorFromColorArray(float val) {
    int i = floor(_ColorSteps * val);
    return _ColorArray[i];
}

//adapted from https://gamedev.stackexchange.com/questions/98740/how-to-color-lerp-between-multiple-colors
float4 getColorFromColorArrayBlend(float val) {
    float scaledTime = val * (float)(_ColorSteps - 1);
    float4 oldColor = _ColorArray[(int)scaledTime];
    float4 newColor = _ColorArray[(int)(scaledTime + 1)];
    float newT = scaledTime - floor(scaledTime);

    return lerp(oldColor, newColor, newT * _ColorLerpScale);
}



float4 getColor(float val) {
	float4 color;
    [branch] switch (_ColorType)
    {
    case 0: //MinMax
        color = lerp(_MinColor, _MaxColor, val * _ColorLerpScale);
        break;
    case 1: //Texture
        color = lerp(_MinColor, _MaxColor, val);
        break;
    case 2: //Brewer
        color = getColorFromColorArray(val);
        break;
    case 3: //Custom
        color = getColorFromColorArrayBlend(val);
        break;
    default:
        color = float4(0, 0, 0, 0);
        break;
    }

    return color;
}
