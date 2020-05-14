using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

//based on https://wiki.unity3d.com/index.php/ProceduralPrimitives#C.23_-_Sphere
public class Globe: MonoBehaviour
{
    [Header("General settings")]
    public int meshResolution = 3;
    public float diameter = 1f;
    public Material material;

    public bool initOnStart = false;
    [Header("Egocentric")]
    public bool egocentric = false;

    [Header("LatLong Lines")]
    public bool showLatLonLines = false;
    public int nLat = 5;
    public int nLon = 10;
    public float tickness = 0.001f;

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
    private List<GameObject> lines = new List<GameObject>();

    //debugs
    Vector2 debug_latLong;

    #region MONOS
    private void Start()
    {
        if (initOnStart)
        {
            Initiate();
        }        
    }
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
    #endregion

    #region PRIVATES
    public void CreateLatLonLines()
    {
        float R = diameter + 0.001f;
        float segment = R / nLat;
        int lineResolution = 100;
        Material lineMaterial = new Material(Shader.Find("Standard"));
        GameObject lineParent = new GameObject("LatLonLines");
        lineParent.transform.position = transform.position;
        lineParent.transform.SetParent(transform);
        for (int i = 0; i < nLat; i++)
        {
            float h = R - (segment * i);
            GameObject l = new GameObject();
            l.transform.position = transform.position;
            l.transform.position -= new Vector3(0, R - h, 0);
            l.transform.SetParent(lineParent.transform);
            lines.Add(l);
            float r = Mathf.Sqrt((R * 2 * h) - (h * h));
            DrawCircle(l, r, tickness, lineResolution, lineMaterial);
        }

        for (int i = 0; i < nLat; i++)
        {
            float h = R + (segment * i);
            GameObject l = new GameObject();
            l.transform.position = transform.position;
            l.transform.position -= new Vector3(0, R - h, 0);
            l.transform.SetParent(lineParent.transform);
            lines.Add(l);
            float r = Mathf.Sqrt((R * 2 * h) - (h * h));
            DrawCircle(l, r, tickness, lineResolution, lineMaterial);
        }

        for (int i = nLon * 2; i >= 0; i--)
        {
            float h = R - (segment * i);
            GameObject l = new GameObject();
            l.transform.position = transform.position;
            l.transform.rotation = Quaternion.Euler(-90, (360 / nLon) * i, 0);
            l.transform.SetParent(lineParent.transform);
            lines.Add(l);
            DrawCircle(l, R, tickness, lineResolution, lineMaterial);
        }

    }

    //https://loekvandenouweland.com/content/use-linerenderer-in-unity-to-draw-a-circle.html
    public static void DrawCircle(GameObject container, float radius, float lineWidth, int segments, Material mat)
    {
        var line = container.AddComponent<LineRenderer>();
        line.useWorldSpace = false;
        line.startWidth = lineWidth;
        line.endWidth = lineWidth;
        line.positionCount = segments + 1;
        line.material = mat;

        var pointCount = segments + 1; // add extra point to make startpoint and endpoint the same to close the circle
        var points = new Vector3[pointCount];

        for (int i = 0; i < pointCount; i++)
        {
            var rad = Mathf.Deg2Rad * (i * 360f / segments);
            points[i] = new Vector3(Mathf.Sin(rad) * radius, 0, Mathf.Cos(rad) * radius);

        }

        line.SetPositions(points);
    }
    #endregion

    #region PUBLIC

    /// <summary>
    /// Is point visible
    /// </summary>
    /// <param name="viewpoint"></param>
    /// <returns></returns>
    public bool IsPointVisible(Vector3 viewpoint, Vector2 latLon)
    {
        Vector3 worldPos = GeoToWorldPosition(latLon);
        Ray ray = new Ray(viewpoint, worldPos - viewpoint); //camera to point

        RaycastHit hit; //point to camera
        Physics.Raycast(ray, out hit);

        if( hit.transform == transform)
        {
            float hitDistance = Vector3.Distance(hit.point, viewpoint);
            float pointDistance = Vector3.Distance(worldPos, viewpoint);
            if (hitDistance + 0.0001f >= pointDistance) // the 0.0001f constant to remove flickering ...
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        else
        {
            return false;
        }      
    }

    /// <summary>
    /// Initiate, create sphere
    /// </summary>
    public void Initiate()
    {
        if (filter == null) filter = gameObject.AddComponent<MeshFilter>();
        if (render == null) render = gameObject.AddComponent<MeshRenderer>();
        render.material = material;
        if(egocentric) render.material.SetTextureScale("_MainTex", new Vector2(-1, 1));
        CreateSphere();
        if(showLatLonLines) CreateLatLonLines();
    }

    /// <summary>
    /// Return geo position from world position Vector2 (lat, lon)
    /// </summary>
    /// <param name="pos">World coordinate</param>
    /// <returns></returns>
    public Vector2 WorldToGeoPosition(Vector3 pos)
    {
        pos = transform.InverseTransformPoint(pos);

        float lat = 90f - (Mathf.Acos(pos.y / diameter)) * 180f / Mathf.PI;
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

        if (egocentric)
        {
            lat *= 1f;
            lon *= -1f;
        }
        float x = diameter * Mathf.Sin(lat) * Mathf.Cos(lon);
        float y = diameter * Mathf.Sin(lat) * Mathf.Sin(lon);
        float z = diameter * Mathf.Cos(lat);

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

        vertices[0] = Vector3.up * diameter;
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

                vertices[lon + lat * (nbLong + 1) + 1] = new Vector3(sin1 * cos2, cos1, sin1 * sin2) * diameter;
            }
        }
        vertices[vertices.Length - 1] = Vector3.up * -diameter;
        #endregion

        #region Normales		
        Vector3[] normales = new Vector3[vertices.Length];
        for (int n = 0; n < vertices.Length; n++)
        {
            normales[n] = vertices[n].normalized;
            if (egocentric) normales[n] = -1f * normales[n];
        }
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
    #endregion

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