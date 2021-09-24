using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEngine.Events;

public class Visualisation : MonoBehaviour
{
    public bool initOnStart = true;
    public Globe globe;
    public Shader shader;
    public Database database;
    public string condition = "1 = 1";
    [Header("Labels")]
    public string latitudeLabel;
    public string longitudeLabel;
    public string valueLabel;
    public string colorLabel;
    [Header("Relative vs Absolute")]
    [Tooltip("if True, use max value from the data. If False, set the Max Value manualy")]
    public bool relativeValue = true;
    [Tooltip("Ignore if above is False")]
    public float maxValue;
    [Tooltip("if True, use max value from the data. If False, set the Max Value manualy")]
    public bool relativeColor = true;
    [Tooltip("Ignore if above is False")]
    public float maxColorValue;
    public ColorProvider colorProvider;

    [Header("Events")]
    public UnityEvent OnReady;

    protected Material material;
    protected Vector2[] latLons;
    protected List<float> latitudes;
    protected List<float> longitudes;
    protected List<float> values;
    protected List<float> colors;
    protected GameObject visParent;

    protected MeshFilter mf;
    protected MeshRenderer mr;

    [ContextMenu("Vis Normal Bar/Create")]
    protected virtual void Initiate()
    {
        LoadData();

        globe.Initiate();

        CreateVisualisationGameobject();

        CreateMesh();

        OnReady.Invoke();
    }

    protected virtual void CreateMesh()
    {
        //generate dummy data
        int n = latitudes.Count;

        if (n != longitudes.Count || (n != values.Count && valueLabel != "") || (n != colors.Count && colorLabel != ""))
        {
            Debug.LogError("Data load failed");
            return;
        }

        Debug.Log("N Points = " + n);

        //get max value and colors
        if (valueLabel != "") maxValue = (relativeValue) ? values.Max() : maxValue;
        if (colorLabel != "") maxColorValue = (relativeColor) ? colors.Max() : maxColorValue;

        //create mesh
        Vector3[] verts = new Vector3[n];
        int[] indices = new int[n];
        Vector2[] normalisedValuesAndColors = new Vector2[n];

        //normalise values
        for (int i = 0; i < n; i++)
        {
            verts[i] = globe.transform.InverseTransformPoint(globe.GeoToWorldPosition(latitudes[i], longitudes[i]));
            indices[i] = i;
            float val = (valueLabel != "") ? values[i] / maxValue : -99f;
            float col = (colorLabel != "") ? colors[i] / maxColorValue : -99f;
            //if (col > 0.5f) Debug.Log(col);
            // Debug.Log("Col " + col);
            normalisedValuesAndColors[i] = new Vector2(val, col);
        }

        Mesh mesh = new Mesh();
        mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
        mesh.SetVertices(verts);
        mesh.SetIndices(indices, MeshTopology.Points, 0);
        mesh.SetUVs(0, latLons);
        mesh.SetUVs(1, normalisedValuesAndColors);
        mesh.RecalculateBounds();

        mf.sharedMesh = mesh;
    }

    public GameObject GetVisualisatiParent()
    {
        return visParent;
    }

    protected void CreateVisualisationGameobject()
    {
        if (visParent != null) Destroy(visParent);

        visParent = new GameObject("Visualisation" + this.gameObject.GetType().Name.ToString());
        visParent.transform.position = globe.transform.position;
        visParent.transform.SetParent(globe.transform);
        mf = visParent.AddComponent<MeshFilter>();
        mr = visParent.AddComponent<MeshRenderer>();
        material = new Material(shader);

        mr.material = material;
        material.SetFloat("_Radius", globe.radius);
    }

    protected virtual void LoadData()
    {
        latitudes = database.GetFloatRecordsByField(latitudeLabel, condition);
        longitudes = database.GetFloatRecordsByField(longitudeLabel, condition);
        values = database.GetFloatRecordsByField(valueLabel, condition);
        colors = database.GetFloatRecordsByField(colorLabel, condition);
    }
}
