using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System.Linq;

public class EarthquakeDepthAxis : MonoBehaviour
{
    public GameObject textMeshProCanvas;
    public VisualisationBilboardedCircle visualisation;
    public Vector2 latLonPosition;
    public int ntick = 3;
    public float lineWidth = 0.015f;
    public string unit;
    private LineRenderer lineR;

    private GameObject lineG;
    private GameObject labelParent;
    private float lastHeight, maxDomain;
    // Start is called before the first frame update

    public void CreateAxis()
    {
        //add line
        lineG = new GameObject("line");
        lineG.transform.SetParent(transform);
        lineR = lineG.AddComponent<LineRenderer>();
        lineR.useWorldSpace = false;
        lineR.positionCount = 2;

        Material mat = new Material(Shader.Find("Unlit/Transparent"));
        mat.color = Color.white;
        lineR.material = mat;
        lineR.startColor = Color.white;
        lineR.endColor = Color.white;
        lineR.numCapVertices = 18;

        //Get max min
        List<float> heights = visualisation.database.GetFloatRecordsByField(visualisation.heightLabel, "");
        maxDomain = heights.Max();

        //create labels;
        CreateLabels();
        CreateLine();
    }

    void CreateLabels()
    {
        if (labelParent)
        {
            DestroyImmediate(labelParent);
            textMeshProCanvas.SetActive(true);
        }
        labelParent = new GameObject("LabelParent");
        labelParent.transform.position = transform.position;
        labelParent.transform.rotation = transform.rotation;
        labelParent.transform.SetParent(transform);
        float posStep = visualisation.maxHeight * visualisation.globe.radius / ntick;
        float dataStep = maxDomain / ntick;
        Vector3 startPoint = visualisation.globe.GeoToWorldPosition(latLonPosition.x, latLonPosition.y);
        labelParent.transform.localPosition = transform.InverseTransformPoint(startPoint);
        for (int i = 0; i < ntick + 1; i++)
        {
            GameObject g = Instantiate(textMeshProCanvas, labelParent.transform);
            TextMeshProUGUI text = g.GetComponentInChildren<TextMeshProUGUI>();
            Vector3 pos = startPoint + (visualisation.globe.transform.position - startPoint).normalized * posStep * i;
            pos = labelParent.transform.InverseTransformPoint(pos);
            g.transform.localPosition = pos; 
            text.text = (0 + dataStep * i).ToString("F0") + " " + unit;
            //point
            Material mat = new Material(Shader.Find("Unlit/Transparent"));
            GameObject point = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            point.transform.localScale = Vector3.one * 0.025f;
            point.transform.SetParent(labelParent.transform);
            point.transform.localPosition = pos;
            point.GetComponent<MeshRenderer>().material = mat;
        }
        textMeshProCanvas.SetActive(false);
        lastHeight = visualisation.maxHeight;
    }

    void CreateLine()
    {
        //start point
        Vector3 startPoint = visualisation.globe.GeoToWorldPosition(latLonPosition.x, latLonPosition.y);
        Vector3 endPoint = startPoint + (visualisation.globe.transform.position - startPoint).normalized * visualisation.maxHeight * visualisation.globe.radius;
        lineR.SetPosition(0, lineG.transform.InverseTransformPoint(startPoint));
        lineR.SetPosition(1, lineG.transform.InverseTransformPoint(endPoint));
        lineR.widthMultiplier = lineWidth;
    }

    // Update is called once per frame
    void Update()
    {
        if (lineG)
        {
           // UpdateLine();
            //CreateLabels();
        }
    }
}
