using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorldMap : MonoBehaviour
{
    public Texture2D mapTexture;
    // Start is called before the first frame update
    public Shader shader;

    public System.Action OnReady = delegate { };

    private Material material;
    private MeshFilter meshFilter;
    private MeshRenderer meshRenderer;
    private MeshCollider meshCollider;
    void Start()
    {
        meshFilter = gameObject.AddComponent<MeshFilter>();
        meshRenderer = gameObject.AddComponent<MeshRenderer>();
        meshCollider = gameObject.AddComponent<MeshCollider>();
        material = new Material(shader);
        material.mainTexture = mapTexture;

        CreatePlane();

        OnReady.Invoke();
    }


    public Material GetMaterial()
    {
        return material;
    }

    /// <summary>
    /// Get latitude and longitude at local position x y
    /// </summary>
    /// <param name="x">x (width)</param>
    /// <param name="y">y (height)</param>
    /// <returns></returns>
    public Vector2 GetLatLonAt(float x, float y)
    {
        float lat = Mathf.Lerp(-90f, 90f, y / mapTexture.height * 1f);
        float lon = Mathf.Lerp(-180f, 180f, x / mapTexture.width * 1f);

        return new Vector2(lat, lon);
    }

    /// <summary>
    /// Create mesh plane / map
    /// </summary>
    private void CreatePlane()
    {
        List<Vector3> vertices = new List<Vector3>();
        List<int> triangles = new List<int>();
        List<Vector2> uvs = new List<Vector2>();
        List<Vector2> latLons = new List<Vector2>();

        int w = mapTexture.width;
        int h = mapTexture.height;
        //vertices
        for (int i = 0; i < w + 1; i++)
        {
            for (int j = 0; j < h + 1; j++)
            {
                //vertex
                Vector3 p = new Vector3(i, j, 0);
                vertices.Add(p);

                //uv
                float x = (float)(i) / (float)w;
                float y = (float)(j) / (float)h;
                Vector2 uv = new Vector2(x, y);
                uvs.Add(uv);
            }
        }

        //triangles
        for (int i = 0; i < vertices.Count - h - 2; i++)
        {
            triangles.Add(i);
            triangles.Add(i + 1);
            triangles.Add(i + h + 1);

            triangles.Add(i + 1);
            triangles.Add(i + h + 2);
            triangles.Add(i + h + 1);
        }

        //latlon
        latLons = new List<Vector2>(vertices.Count);
        float halfW = w / 2;
        float halfH = h / 2;
        float lonSpacing = 180f / halfW;
        float latSpacing = 90f / halfH;

        for(int i = 0; i < w + 1; i++)
        {
            float lon = -180f + (i * lonSpacing);
           // Debug.Log("Lon " + lon);

            for (int j = 0; j < h + 1; j++)
            {
                float lat = -90f + (j * latSpacing);

                // Debug.Log("Lat " + lat);
               // Debug.Log("Lon " + lon);

                latLons.Add(new Vector2(lat, lon));
            }
        }

        Mesh mesh = new Mesh();
        mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
        mesh.vertices = vertices.ToArray();
        mesh.triangles = triangles.ToArray();
        mesh.uv = uvs.ToArray();
        mesh.uv2 = latLons.ToArray();
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();
        meshFilter.mesh = mesh;
        meshRenderer.material = material;
        meshCollider.sharedMesh = mesh;
    }
}
