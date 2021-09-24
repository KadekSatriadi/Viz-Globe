using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class VisualisationBilboardedCircle : Visualisation
{
    [Header("Additional label for height of the circle. The Value Label above will be mapped to radius")]
    public string heightLabel;
    [Header("Mapping")]
    public float maxHeight = 1f;
    public float maxRadius = 0.1f;
    [Header("Texture")]
    public Texture2D texture;

    void Start()
    {
        if (initOnStart)
        {
            Initiate();
            OnReady.Invoke();
        }
    }

    protected override void Initiate()
    {
        LoadData();

        globe.Initiate();

        //generate dummy data
        int n = latitudes.Count;

        if (n != longitudes.Count || (n != values.Count && valueLabel != "") || (n != colors.Count && colorLabel != ""))
        {
            Debug.LogError("Data load failed");
            return;
        }

        Debug.Log("N Points = " + n);

        //get max value and colors
        List<float> heights = database.GetFloatRecordsByField(heightLabel, condition);
        float maxHeightValue = float.PositiveInfinity;
        float minHeightValue = float.NegativeInfinity;
        float minColorValue = colors.Min();

        if (valueLabel != "") maxValue = (relativeValue) ? values.Max() : maxValue;
        if (colorLabel != "") maxColorValue = (relativeColor) ? colors.Max() : maxColorValue;
        if (heightLabel != "") maxHeightValue = heights.Max();
        if (heightLabel != "") minHeightValue = heights.Min();
        //create visualisation gameobject
        if (visParent != null) Destroy(visParent);

        visParent = new GameObject("Visualisation" + this.gameObject.GetType().ToString());
        visParent.transform.position = globe.transform.position;
        visParent.transform.SetParent(globe.transform);
        MeshFilter mf = visParent.AddComponent<MeshFilter>();
        MeshRenderer mr = visParent.AddComponent<MeshRenderer>();
        material = new Material(shader);
        material.mainTexture = texture;

        mr.material = material;
        material.SetFloat("_Radius", globe.radius);

        //create mesh
        Vector3[] verts = new Vector3[n];
        int[] indices = new int[n];
        Vector2[] normalisedValuesAndColors = new Vector2[n];
        Vector2[] normalisedHeights = new Vector2[n];
        latLons = new Vector2[n];
        //normalise values
        for (int i = 0; i < n; i++)
        {
            verts[i] = globe.transform.InverseTransformPoint(globe.GeoToWorldPosition(latitudes[i], longitudes[i]));
            indices[i] = i;
            float val = (valueLabel != "") ? Mathf.Pow(values[i] / maxValue, 0.5f) : 1f;
            //this is only for this demo purpose, needs to be removed later
            if (values[i] > 7f && values[i] < 9f) 
            {
                val = 2f;
            }
            else if(values[i] >= 9f)
            {
                val = 3.5f;
            }

            float col = (colorLabel != "") ? (colors[i] - minColorValue) / (maxColorValue - minColorValue) : 1f;
            float height = (heightLabel != "")? (heights[i] - minHeightValue) / (maxHeightValue - minHeightValue) : 1f;
            normalisedValuesAndColors[i] = new Vector2(val, col);
            normalisedHeights[i] = new Vector2(height, 0f);
            latLons[i] = new Vector2(latitudes[i], longitudes[i]);
        }

        Mesh mesh = new Mesh();
        mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
        mesh.SetVertices(verts);
        mesh.SetIndices(indices, MeshTopology.Points, 0);
        mesh.SetUVs(0, latLons);
        mesh.SetUVs(1, normalisedValuesAndColors);
        mesh.SetUVs(2, normalisedHeights);
        mesh.RecalculateBounds();

        mf.sharedMesh = mesh;
    }

    public Material GetMaterial()
    {
        return material;
    }

    private void Update()
    {
        material.SetFloat("_CircleMaxRadius", maxRadius);
        material.SetFloat("_Radius", globe.radius);
        material.SetFloat("_CircleMaxHeight", maxHeight);
        material.SetVector("_GlobeWorldPosition", globe.transform.position);
        colorProvider.UpdateColor(material);
    }
}
