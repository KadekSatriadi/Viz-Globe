using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ColorProvider : MonoBehaviour
{
    public enum ColorSetting
    {
        MaxMin, Texture, Brewer, CustomList
    }

    public ColorSetting activeColorSetting;
    [Header("MaxMin")]
    public Color maxColor;
    public Color minColor;
    [Range(1f, 100f)]
    public float maxMinLerpscale = 1f;
    [Header("Texture")]
    public Texture2D colorTexture;
    [Header("Brewer")]
    public int colorSteps = 2;
    public ColorBru.Code colorCode = ColorBru.Code.Accent;
    public bool reverseColour = false;
    [Header("CustomList (max 15)")]
    public List<Color> customColors = new List<Color>();
    [Range(1f, 100f)]
    public float customColorLerpScale = 1f;

    public void UpdateColor(Material material)
    {
        material.SetInt("_ColorType", (int) activeColorSetting);
        switch (activeColorSetting)
        {
            case ColorSetting.MaxMin:
                material.SetColor("_MaxColor", maxColor);
                material.SetColor("_MinColor", minColor);
                material.SetFloat("_ColorLerpScale", maxMinLerpscale);
                break;
            case ColorSetting.Brewer:
                Color[] colors = ColorBru.GetColors(colorCode, (byte)colorSteps, reverseColour);
                material.SetColorArray("_ColorArray", colors);
                material.SetInt("_ColorSteps", colorSteps);
                break;
            case ColorSetting.CustomList:
                int count = Mathf.Min(15, customColors.Count);
                material.SetColorArray("_ColorArray", customColors.ToArray());
                material.SetInt("_ColorSteps", count);
                material.SetFloat("_ColorLerpScale", customColorLerpScale);

                break;
        }
    }
}
