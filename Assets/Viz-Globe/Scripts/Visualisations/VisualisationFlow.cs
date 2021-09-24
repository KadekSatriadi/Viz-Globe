using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class VisualisationFlow : MonoBehaviour
{
    public bool initOnStart = true;
    public Globe globe;
    public Shader shader;
    public ComputeShader computeShader;
    public Database database;
    public string condition = "1 = 1";
    public Texture2D flowTexture;
    public float animationSpeed = 0f;

    [Header("Label")]
    public string latitude1Label;
    public string latitude2Label;
    public string longitude1Label;
    public string longitude2Label;
    public string radiusLabel;
    public string heightLabel;
    public int nTubeSegment = 10;
    public int nCircleSegment = 40;
    [Header("Mapping")]
    public float maxRadius = 1f;
    public float maxHeight = 1f;
    public Color color1 = Color.red;
    public Color color2 = Color.green;

    private Material material;
    private Vector2[] latLons1;
    private Vector2[] latLons2;
    private List<float> latitudes1;
    private List<float> latitudes2;
    private List<float> longitudes1;
    private List<float> longitudes2;
    private List<float> radiusValues;
    private List<float> heighValues;
    private GameObject visParent;

    private ComputeBuffer inputData;
    private ComputeBuffer vertexBuffer;
    private ComputeBuffer normalBuffer;
    private ComputeBuffer colorBuffer;
    private ComputeBuffer uvBuffer;
    private ComputeBuffer indexBuffer;
    private Data[] data;

    private MeshFilter mF;
    private MeshRenderer mR;

    struct Data
    {
        public Vector3 position1;
        public Vector3 position2;
        public float radiusNormalised;
        public float heightNormalised;
    }

    void Start()
    {
        //create visualisation gameobject
        if (visParent != null) Destroy(visParent);
        material = new Material(shader);
        visParent = new GameObject("VisualisationFlow");
        visParent.transform.position = globe.transform.position;
        visParent.transform.SetParent(globe.transform);
        mF = visParent.AddComponent<MeshFilter>();
        mR = visParent.AddComponent<MeshRenderer>();
        mR.material = material;
        if (flowTexture) material.mainTexture = flowTexture;

        if (initOnStart)
        {
            globe.Initiate();
            LoadData();
            CreateMeshCS();
        }
    }

    protected void LoadData()
    {
        latitudes1 = database.GetFloatRecordsByField(latitude1Label, condition);
        latitudes2 = database.GetFloatRecordsByField(latitude2Label, condition);
        longitudes1 = database.GetFloatRecordsByField(longitude1Label, condition);
        longitudes2 = database.GetFloatRecordsByField(longitude2Label, condition);
        radiusValues = database.GetFloatRecordsByField(radiusLabel, condition);
        heighValues = database.GetFloatRecordsByField(heightLabel, condition);


        //prepare data for compute buffer
        data = new Data[latitudes1.Count];

        Debug.Log("N data point = " + data.Length);

        float maxRadiusValue = (radiusLabel != "")? radiusValues.Max() : 1f;
        float maxHeightValue = 0;
        float[] distances = new float[data.Length];

        if (heightLabel != "")
        {
            maxHeightValue = heighValues.Max();
        }
        else
        {
            //calculate distance
            for (int i = 0; i < data.Length; i++)
            {
                Vector2 latlon1 = new Vector2(latitudes1[i], longitudes1[i]);
                Vector2 latlon2 = new Vector2(latitudes2[i], longitudes2[i]);
                float d = globe.GreatCircleDistance(latlon1, latlon2);
                distances[i] = d;
            }
            maxHeightValue = distances.Max();
        }
        

        for (int i = 0; i < data.Length; i++)
        {
            Vector3 p1 = globe.transform.InverseTransformPoint(globe.GeoToWorldPosition(latitudes1[i], longitudes1[i]));
            Vector3 p2 = globe.transform.InverseTransformPoint(globe.GeoToWorldPosition(latitudes2[i], longitudes2[i]));
            float r = (radiusLabel != "") ? radiusValues[i] / maxRadiusValue : 1f;
            float h = 1f;
            if (heightLabel != "")
            {
                h =   heighValues[i] / maxHeightValue;

            }
            else
            {
                h = distances[i] / maxHeightValue;
            }

            data[i] = new Data()
            {
                position1 = p1,
                position2 = p2,
                radiusNormalised = r,
                heightNormalised = h
            };
        }
    }

    void CreateMeshCS()
    {

        int n = data.Length;
        
        int kernel = computeShader.FindKernel("CSGenerateFlowBezier");

        int dataStride = sizeof(float) * 3 + sizeof(float) * 3 + sizeof(float) + sizeof(float);  //stride size position1, position2, radiusNormalised, heighNormalised


        Matrix4x4 unity_ObjectToWorld = visParent.transform.localToWorldMatrix;


        //int nvertices = (nCircleSegment * 2) + (nCircleSegment * (nTubeSegment - 1));
        int nverticesPerFlow = nCircleSegment * nTubeSegment * 4;
        int nindicesPerFlow = nCircleSegment * 6 * nTubeSegment;
        int nvertices = n  * nverticesPerFlow; //duplicate vertex
        int nindices =  n  * nindicesPerFlow;

        inputData = new ComputeBuffer(n, dataStride, ComputeBufferType.Default);
        inputData.SetData(data);

        vertexBuffer = new ComputeBuffer(nvertices, sizeof(float) * 3, ComputeBufferType.Default);
        normalBuffer = new ComputeBuffer(nvertices, sizeof(float) * 3, ComputeBufferType.Default);
        colorBuffer = new ComputeBuffer(nvertices, sizeof(float) * 4, ComputeBufferType.Default);
        uvBuffer = new ComputeBuffer(nvertices, sizeof(float) * 4, ComputeBufferType.Default);
        indexBuffer = new ComputeBuffer(nindices, sizeof(int), ComputeBufferType.Default);


        computeShader.SetFloat("MaxRadius", maxRadius);
        computeShader.SetFloat("MaxHeight", maxHeight);
        computeShader.SetFloat("globeRadius", globe.radius);

        computeShader.SetVector("Color1", color1);
        computeShader.SetVector("Color2", color2);

        computeShader.SetInt("nTubeSegment", nTubeSegment);
        computeShader.SetInt("nCircleSegment", nCircleSegment);
        computeShader.SetInt("nvertices", nverticesPerFlow);
        computeShader.SetInt("nindices", nindicesPerFlow);

        computeShader.SetVector("globeCenter", globe.transform.position);

        computeShader.SetMatrix("unity_ObjectToWorld", unity_ObjectToWorld);
        
        computeShader.SetBuffer(kernel, "InputData", inputData);
        computeShader.SetBuffer(kernel, "OutputVertices", vertexBuffer);
        computeShader.SetBuffer(kernel, "OutputNormals", normalBuffer);
        computeShader.SetBuffer(kernel, "OutputColors", colorBuffer);
        computeShader.SetBuffer(kernel, "OutputUVs", uvBuffer);
        computeShader.SetBuffer(kernel, "OutputIndices", indexBuffer);
        
        computeShader.Dispatch(kernel, n / 10, 1, 1);

        Mesh mesh = new Mesh();
        mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;

        Vector3[] vData = new Vector3[nvertices];
        int[] iData = new int[nindices];
        Vector3[] nData = new Vector3[nvertices];
        Vector2[] uvData = new Vector2[nvertices];
        Color[] cData = new Color[nvertices];

        vertexBuffer.GetData(vData);
        mesh.vertices = vData;

        normalBuffer.GetData(nData);
        mesh.normals = nData;

        colorBuffer.GetData(cData);
        mesh.colors = cData;

        uvBuffer.GetData(uvData);
        mesh.uv = uvData;

        indexBuffer.GetData(iData);
        mesh.triangles = iData;

        //mesh.SetIndices(iData, MeshTopology.Triangles, 0);

        material.SetFloat("_AnimationSpeed", animationSpeed);
        mF.mesh = mesh;

    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            CreateMeshCS();
        }

        material.SetFloat("_AnimationSpeed", animationSpeed);

    }

}
