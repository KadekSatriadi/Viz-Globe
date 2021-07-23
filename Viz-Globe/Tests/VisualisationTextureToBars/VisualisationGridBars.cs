using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VisualisationGridBars : MonoBehaviour
{


    public Texture2D texture;
    public Shader shader;
    public float maxValue;
    public float minValue;
    // Start is called before the first frame update

    public int colorSteps = 2;
    public ColorBru.Code colorCode = ColorBru.Code.Accent;
    public bool reverseColour = false;

    private MeshFilter meshFilter;
    private MeshRenderer meshRenderer;
    void Start()
    {
        //Add components
        meshFilter = gameObject.AddComponent<MeshFilter>();
        meshRenderer = gameObject.AddComponent<MeshRenderer>();

        //Variables
        int width = texture.width;
        int height = texture.height;
        Color[] colors = new Color[width * height];
        Vector3[] vertices = new Vector3[width * height];
        Vector2[] latLon = new Vector2[width * height];
        Vector2[] values = new Vector2[width * height];
        int[] indices = new int[width * height];

        //Read pixel
        for (int i = 0; i < height - 1; i++)
        {
            for(int j = 0; j < width - 1; j++)
            {
                Color col = texture.GetPixel(j, i);
                values[i * width + j] = new Vector2(col.a, 0f);
                colors[i * width + j] = col;
                latLon[i * width + j] = GetLatLonAt(i, j);
                vertices[i * width + j] = new Vector3(j + 0.5f, i + 0.5f, 0);
                indices[i * width + j] = i * width + j;
            }
        }

        //Create Mesh
        Mesh mesh = new Mesh();
        mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
        mesh.SetVertices(vertices);
        mesh.SetUVs(0, latLon);
        mesh.SetUVs(1, values);
        mesh.SetColors(colors);
        mesh.SetIndices(indices, MeshTopology.Points, 0);
        meshFilter.mesh = mesh;
        Material material = new Material(shader);
        meshRenderer.material = material;
    }

    private Vector2 GetLatLonAt(float x, float y)
    {
        float lat = Mathf.Lerp(-90f, 90f, y / texture.height * 1f);
        float lon = Mathf.Lerp(-180f, 180f, x / texture.width * 1f);

        return new Vector2(lat, lon);
    }

    // Update is called once per frame
    void Update()
    {
      Vector4[] colors = ColorBru.ColorsToVector4s(ColorBru.GetColors(colorCode, (byte)colorSteps, reverseColour));
       meshRenderer.material.SetInt("_ColorSteps", colorSteps);
       meshRenderer.material.SetVectorArray("_ColorArray", colors);


    }
}
