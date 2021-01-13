using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VisualisationNormalBar : MonoBehaviour
{
    public Globe globe;
    public Shader shader;

    private Vector2[] latLons;
    private float[] values;

    private Material material;
    // Start is called before the first frame update
    void Start()
    {
        globe.Initiate();

        //generate dummy data
        int n = 5000;
        float maxValue = 0;

        latLons = new Vector2[n];
        values = new float[n];

        for(int i = 0; i < n; i++)
        {
            latLons[i] = new Vector2(Random.Range(-90f, 90f), Random.Range(-180f, 180f));
            values[i] = Random.Range(0, 100);
            if (values[i] > maxValue) maxValue = values[i];
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
        Vector2[] normalisedValues = new Vector2[n];
        for (int i = 0; i < n; i++)
        {
            verts[i] = globe.transform.InverseTransformPoint(globe.GeoToWorldPosition(latLons[i].x, latLons[i].y));
            indices[i] = i;
            normalisedValues[i] = new Vector2(values[i] / maxValue, 0f);
        }

        Mesh mesh = new Mesh();
        mesh.SetVertices(verts);
        mesh.SetIndices(indices, MeshTopology.Points, 0);
        mesh.SetUVs(0, latLons);
        mesh.SetUVs(1, normalisedValues);
        mesh.RecalculateBounds();

        mf.mesh = mesh;
    }

    private void Update()
    {
    }

}
