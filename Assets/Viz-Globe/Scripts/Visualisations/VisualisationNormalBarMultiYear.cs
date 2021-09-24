using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class VisualisationNormalBarMultiYear : Visualisation
{
    [Header("Specific Settings")]
    public string year1Label;
    public string year2Label;
    public string year3Label;
    public string year4Label;
    public string year5Label;

    [Range(0f, 5f)]
    public float activeYear = 1f;
    public bool animate = false;
    [Range(0f, 4f)]
    public float animationStartFromYear = 0;
    [Header("Mapping")]
    [Range(0f, 5f)]
    public float maxLength = 1f;
    [Range(0f, 0.5f)]
    public float barSize = 0.1f;

    private List<float> year1Values = new List<float>();
    private List<float> year2Values = new List<float>();
    private List<float> year3Values = new List<float>();
    private List<float> year4Values = new List<float>();
    private List<float> year5Values = new List<float>();

    void Start()
    {
        if (initOnStart)
        {
            Initiate();
        }
    }

    protected override void Initiate()
    {
        LoadData();

        globe.Initiate();
      
        //create visualisation gameobject
        CreateVisualisationGameobject();

        //create mesh
        CreateMesh();
    }

    protected override void CreateMesh()
    {
        //generate dummy data
        int n = latitudes.Count;

        Debug.Log("N Points = " + n);

        //Get max value accross years
        List<float> maxvalues = new List<float>()
        {
            year1Values.Max(),
            year2Values.Max(),
            year3Values.Max(),
            year4Values.Max(),
            year5Values.Max()
        };
        float maxValue = maxvalues.Max();

        if (colorLabel != "") maxColorValue = (relativeColor) ? colors.Max() : maxColorValue;


        Vector3[] verts = new Vector3[n];
        int[] indices = new int[n];
        Vector2[] normalisedVal1Val2 = new Vector2[n];
        Vector2[] normalisedVal3Val4 = new Vector2[n];
        Vector2[] normalisedVal5 = new Vector2[n];

        //normalise values
        for (int i = 0; i < n; i++)
        {
            verts[i] = globe.transform.InverseTransformPoint(globe.GeoToWorldPosition(latitudes[i], longitudes[i]));
            indices[i] = i;
            float val1 = year1Values[i] / maxValue;
            float val2 = year2Values[i] / maxValue;
            float val3 = year3Values[i] / maxValue;
            float val4 = year4Values[i] / maxValue;
            float val5 = year5Values[i] / maxValue;

            normalisedVal1Val2[i] = new Vector2(val1, val2);
            normalisedVal3Val4[i] = new Vector2(val3, val4);
            normalisedVal5[i] = new Vector2(val5,0);
        }

        Mesh mesh = new Mesh();
        mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
        mesh.SetVertices(verts);
        mesh.SetIndices(indices, MeshTopology.Points, 0);
        mesh.SetUVs(0, normalisedVal1Val2);
        mesh.SetUVs(1, normalisedVal3Val4);
        mesh.SetUVs(2, normalisedVal5);
        mesh.RecalculateBounds();

        mf.sharedMesh = mesh;
    }

    protected override void LoadData()
    {
        base.LoadData();
        year1Values = database.GetFloatRecordsByField(year1Label, condition);
        year2Values = database.GetFloatRecordsByField(year2Label, condition);
        year3Values = database.GetFloatRecordsByField(year3Label, condition);
        year4Values = database.GetFloatRecordsByField(year4Label, condition);
        year5Values = database.GetFloatRecordsByField(year5Label, condition);
    }


    private void Update()
    {
        material.SetFloat("_MaxRange", maxLength);
        material.SetFloat("_Radius", globe.radius);
        material.SetFloat("_Size", barSize);
        if(animate) activeYear = animationStartFromYear + (4f * (1f +  Mathf.Sin(Time.unscaledTime)) * 0.5f);
        material.SetFloat("_ActiveYear", activeYear);
        colorProvider.UpdateColor(material);
    }
}
