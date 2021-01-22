using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VisualisationNormalBar : MonoBehaviour
{
    public Globe globe;
    public Shader shader;
    public Database database;

    [Header("Mapping")]
    public string latitudeLabel;
    public string longitudeLabel;
    public string valueLabel;
    public string colorLabel;

    [Range(0f, 1f)]
    public float maxRange = 1f;
    [Range(0f, 1f)]
    public float barSize = 0.1f;
    public Color maxColor;
    public Color minColor; 

    private Vector2[] latLons;
    private float[] values;

    private Material material;
    // Start is called before the first frame update
    void Start()
    {
        List<float> latitudes = database.GetFloatRecordsByField(latitudeLabel, "");
        List<float> longitudes = database.GetFloatRecordsByField(longitudeLabel, "");
        List<float> values = database.GetFloatRecordsByField(valueLabel, "");
        List<float> colors = database.GetFloatRecordsByField(colorLabel, "");
      
        globe.Initiate();

        //generate dummy data
        int n = latitudes.Count;

        if(n != longitudes.Count  || n != values.Count || n != colors.Count)
        {
            Debug.LogError("Data load failed");
            return;
        }

        Debug.Log("N Points = " + n);

        //get max value and colors
        float maxValue = 0;
        float maxColorValue = 0;
        for (int i = 0; i < n; i++)
        {
            if (values[i] > maxValue) maxValue = values[i];
            if (colors[i] > maxColorValue) maxColorValue = colors[i];
        }

        //create visualisation gameobject
        GameObject g = new GameObject("VisualisationNormalBar");
        g.transform.SetParent(globe.transform);
        MeshFilter mf = g.AddComponent<MeshFilter>();
        MeshRenderer mr = g.AddComponent<MeshRenderer>();
        material = new Material(shader);
        mr.material = material;
        material.SetFloat("_Radius", globe.radius);

        //create mesh
        Vector3[] verts = new Vector3[n];
        int[] indices = new int[n];
        Vector2[] normalisedValuesAndColors = new Vector2[n];

        //normalise values
        for (int i = 0; i < n; i++)
        {
            verts[i] = globe.transform.InverseTransformPoint(globe.GeoToWorldPosition(latitudes[i], longitudes[i]));
            indices[i] = i;
            normalisedValuesAndColors[i] = new Vector2(values[i] / maxValue, colors[i] / maxColorValue);
        }

        Mesh mesh = new Mesh();
        mesh.SetVertices(verts);
        mesh.SetIndices(indices, MeshTopology.Points, 0);
        mesh.SetUVs(0, latLons);
        mesh.SetUVs(1, normalisedValuesAndColors);
        mesh.RecalculateBounds();

        mf.mesh = mesh;
    }

    private void Update()
    {
        material.SetFloat("_MaxRange", maxRange);
        material.SetFloat("_Radius", globe.radius);
        material.SetFloat("_Size", barSize);
        material.SetColor("_MaxColor", maxColor);
        material.SetColor("_MinColor", minColor);
    }

}
