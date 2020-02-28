using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

//based on https://wiki.unity3d.com/index.php/ProceduralPrimitives#C.23_-_Sphere
public class Globe: MonoBehaviour
{

    public int meshResolution = 3;
    public float radius = 1f;
    public Material material;
    public bool initOnStart = false;

    #region EVENTS
    public Action OnReady = delegate { };
    #endregion

    private struct TriangleIndices
    {
        public int v1;
        public int v2;
        public int v3;

        public TriangleIndices(int v1, int v2, int v3)
        {
            this.v1 = v1;
            this.v2 = v2;
            this.v3 = v3;
        }
    }

    private MeshFilter filter;
    private MeshRenderer render;
    private SphereCollider collider;

    private void Start()
    {
        if (initOnStart)
        {
            Initiate();
        }        
    }

    public void Initiate()
    {
       if(filter == null) filter = gameObject.AddComponent<MeshFilter>();
       if(render == null) render = gameObject.AddComponent<MeshRenderer>();
        render.material = material;
        CreateSphere();
    }

    Vector2 latLong;
    private void Update()
    {
        #region DEBUG
        //Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        //if (Physics.Raycast(ray, out RaycastHit hit))
        //{
        //    latLong = WorldToGeoPosition(hit.point);
        //}
        #endregion
    }

    private void OnGUI()
    {
       //GUI.Label(new Rect(100, 100, 200, 200), "LatLong = " + latLong.ToString());
    }

    /// <summary>
    /// Return geo position from world position Vector2 (lat, lon)
    /// </summary>
    /// <param name="pos">World coordinate</param>
    /// <returns></returns>
    public Vector2 WorldToGeoPosition(Vector3 pos)
    {
        pos = transform.InverseTransformPoint(pos);

        float lat = 90f - (Mathf.Acos(pos.y / radius)) * 180f / Mathf.PI;
        float lon = ((270f + (Mathf.Atan2(pos.x, pos.z)) * 180f / Mathf.PI) % 360f) - 180f;

        return new Vector2(lat, -lon);
    }


    /// <summary>
    /// Return world position from lat lon
    /// </summary>
    /// <param name="lat"></param>
    /// <param name="lon"></param>
    /// <returns></returns>
    public Vector3 GeoToWorldPosition(float lat, float lon)
    {
        lat = (90f - lat) * Mathf.Deg2Rad;
        lon *= Mathf.Deg2Rad;

        float x = radius * Mathf.Sin(lat) * Mathf.Cos(lon);
        float y = radius * Mathf.Sin(lat) * Mathf.Sin(lon);
        float z = radius * Mathf.Cos(lat);

        Vector3 position = new Vector3(-x, z, -y);

        return transform.TransformPoint(position);
    }
    public Vector3 GeoToWorldPosition(Vector2 latLon)
    {
        return GeoToWorldPosition(latLon.x, latLon.y);
    }

    //https://wiki.unity3d.com/index.php/ProceduralPrimitives#C.23_-_Sphere
    public void CreateSphere()
    {
        Mesh mesh = filter.mesh;
        mesh.Clear();

        // Longitude |||
        int nbLong = meshResolution;
        //int nbLong = 24;
        // Latitude ---
        int nbLat = meshResolution;
        //int nbLat = 16;

        #region Vertices
        Vector3[] vertices = new Vector3[(nbLong + 1) * nbLat + 2];
        float _pi = Mathf.PI;
        float _2pi = _pi * 2f;

        vertices[0] = Vector3.up * radius;
        for (int lat = 0; lat < nbLat; lat++)
        {
            float a1 = _pi * (float)(lat + 1) / (nbLat + 1);
            float sin1 = Mathf.Sin(a1);
            float cos1 = Mathf.Cos(a1);

            for (int lon = 0; lon <= nbLong; lon++)
            {
                float a2 = _2pi * (float)(lon == nbLong ? 0 : lon) / nbLong;
                float sin2 = Mathf.Sin(a2);
                float cos2 = Mathf.Cos(a2);

                vertices[lon + lat * (nbLong + 1) + 1] = new Vector3(sin1 * cos2, cos1, sin1 * sin2) * radius;
            }
        }
        vertices[vertices.Length - 1] = Vector3.up * -radius;
        #endregion

        #region Normales		
        Vector3[] normales = new Vector3[vertices.Length];
        for (int n = 0; n < vertices.Length; n++)
            normales[n] = vertices[n].normalized;
        #endregion

        #region UVs
        Vector2[] uvs = new Vector2[vertices.Length];
        uvs[0] = Vector2.up;
        uvs[uvs.Length - 1] = Vector2.zero;
        for (int lat = 0; lat < nbLat; lat++)
            for (int lon = 0; lon <= nbLong; lon++)
                uvs[lon + lat * (nbLong + 1) + 1] = new Vector2((float)lon / nbLong, 1f - (float)(lat + 1) / (nbLat + 1));
        #endregion

        #region Triangles
        int nbFaces = vertices.Length;
        int nbTriangles = nbFaces * 2;
        int nbIndexes = nbTriangles * 3;
        int[] triangles = new int[nbIndexes];

        //Top Cap
        int i = 0;
        for (int lon = 0; lon < nbLong; lon++)
        {
            triangles[i++] = lon + 2;
            triangles[i++] = lon + 1;
            triangles[i++] = 0;
        }

        //Middle
        for (int lat = 0; lat < nbLat - 1; lat++)
        {
            for (int lon = 0; lon < nbLong; lon++)
            {
                int current = lon + lat * (nbLong + 1) + 1;
                int next = current + nbLong + 1;

                triangles[i++] = current;
                triangles[i++] = current + 1;
                triangles[i++] = next + 1;

                triangles[i++] = current;
                triangles[i++] = next + 1;
                triangles[i++] = next;
            }
        }

        //Bottom Cap
        for (int lon = 0; lon < nbLong; lon++)
        {
            triangles[i++] = vertices.Length - 1;
            triangles[i++] = vertices.Length - (lon + 2) - 1;
            triangles[i++] = vertices.Length - (lon + 1) - 1;
        }
        #endregion

        mesh.vertices = vertices;
        mesh.normals = normales;
        mesh.uv = uvs;
        mesh.triangles = triangles;

        mesh.RecalculateBounds();
        mesh.Optimize();

        collider = gameObject.AddComponent<SphereCollider>();

        OnReady.Invoke();
    }

    #region DEBUG

    //private void OnDrawGizmos()
    //{
    //    Vector2 latLon1 = new Vector2(0, 0);
    //    Vector2 latLon2 = new Vector2(90f, 0);
    //    Vector2 latLon3 = new Vector2(-90f, 0);
    //    Vector2 latLon4 = new Vector2(35.102100f, 33.255587f);

    //    Vector3 pos1 = GeoToWorldPosition(latLon1);
    //    Vector3 pos2 = GeoToWorldPosition(latLon2);
    //    Vector3 pos3 = GeoToWorldPosition(latLon3);
    //    Vector3 pos4 = GeoToWorldPosition(latLon4);

    //    Gizmos.DrawSphere(pos1, 0.0015f);
    //    Gizmos.DrawSphere(pos2, 0.0015f);
    //    Gizmos.DrawSphere(pos3, 0.0015f);
    //    Gizmos.DrawSphere(pos4, 0.0015f);
    //    Handles.Label(pos1, latLon1.ToString());
    //    Handles.Label(pos2, latLon2.ToString());
    //    Handles.Label(pos3, latLon3.ToString());
    //    Handles.Label(pos4, latLon4.ToString());

    //    int res = 10;
    //    for(int i = -90; i < 90; i += res)
    //    {
    //        for(int j = -180; j < 180; j += res)
    //        {
    //            Vector2 latLonA = new Vector2(i, j);
    //            Vector3 pos = GeoToWorldPosition(latLonA);
    //           // Gizmos.DrawSphere(pos, 0.0015f);
    //           // if(i % 30 == 0 && j % 60 == 0) Handles.Label(pos, latLonA.ToString());
    //        }
    //    }
    //}

    #endregion

}