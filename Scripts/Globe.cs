﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

//based on https://wiki.unity3d.com/index.php/ProceduralPrimitives#C.23_-_Sphere
public class Globe: MonoBehaviour
{
    [Header("General settings")]
    public int meshResolution = 3;
    public float radius = 1f;
    public Material material;

    public bool initOnStart = false;
    [Header("Egocentric")]
    public bool egocentric = false;

    [Header("Graticules Lines")]
    public bool showLines = false;
    public Color graticuleColor;
    public int latitudeSpacing = 10;
    public int longitudeSpacing = 30;
    public float tickness = 0.001f;
    public Shader lineShader;
    public int lineResolution = 50;

    [Header("Animation")]
    public AnimationCurve animationCurve;
    public float animationDuration = 1f;

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
    private bool isReady = false;
    private List<LineRenderer> arcs = new List<LineRenderer>();
    //debugs

    #region MONOS
    private void Start()
    {
        if (initOnStart)
        {
            Initiate();
        }        
    }

    #endregion

    #region PRIVATES
    //https://loekvandenouweland.com/content/use-linerenderer-in-unity-to-draw-a-circle.html
    private static void DrawCircle(GameObject container, float radius, float lineWidth, int segments, Material mat)
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

    /// <summary>
    /// Get points between two latlons
    /// https://math.stackexchange.com/questions/329749/draw-an-arc-in-3d-coordinate-system
    /// </summary>
    /// <param name="latLon1"></param>
    /// <param name="latLon2"></param>
    /// <param name="n"></param>
    /// <returns>List of points in local coordinate</returns>
    private List<Vector3> GetPointsBetween(Vector2 latLon1, Vector2 latLon2, int n)
    {
        //using slerp interpolation 
        List<Vector3> points = new List<Vector3>();
        Vector3 center = transform.position;
        Vector3 startPoint = GeoToWorldPosition(latLon1);
        Vector3 endPoint = GeoToWorldPosition(latLon2);

        Vector3 u = startPoint - transform.position;
        Vector3 v = endPoint - transform.position;

        for (int i = 0; i < n; i++)
        {
            float t = (float) i / (n - 1f);
            //final point
            Vector3 p = center + Vector3.Slerp(u, v, t);
            //add offset
            Vector3 off = (p - transform.position).normalized * 0.001f;
            points.Add(p + off);
        }

        return points;
    }
    
    private IEnumerator RotationTween(Quaternion s, Quaternion e, Action a)
    {
        float journey = 0f;
        while (journey <= animationDuration)
        {
            journey = journey + Time.deltaTime;
            float percent = Mathf.Clamp01(journey / animationDuration);
            Quaternion current = Quaternion.Lerp(s, e, animationCurve.Evaluate(percent));
            transform.rotation = current;
            yield return null;
        }
        a.Invoke();
    }
    #endregion

    #region PUBLIC
    /// <summary>
    /// Yaw globe
    /// </summary>
    /// <param name="angle"></param>
    /// 
    public void Yaw(float angle)
    {
        transform.Rotate(0, angle, 0, Space.Self);
    }

    /// <summary>
    /// Roll
    /// </summary>
    /// <param name="cameraPosition">Camera vector</param>
    /// <param name="angle"></param>
    public void Roll(Vector3 cameraPosition, float angle)
    {
        Vector3 axis = Vector3.Cross(cameraPosition - transform.position, transform.up);
        transform.Rotate(axis, angle, Space.World);
    }
    public void RollLimit(Vector3 cameraPosition, float angle)
    {
        Quaternion rot = transform.rotation;
        Vector3 axis = Vector3.Cross(cameraPosition - transform.position, transform.up);
        transform.Rotate(axis, angle, Space.World);
        if(Vector3.Angle(Vector3.up, transform.up) > 50f)
        {
            transform.rotation = rot;
        }
    }


    /// <summary>
    /// [UNTESTED - USE WITH CAUTION] Calculating central angle between two lat lon.
    /// Source: https://en.wikipedia.org/wiki/Great-circle_distance
    /// </summary>
    /// <param name="latLon1"></param>
    /// <param name="latLon2"></param>
    /// <returns></returns>
    private float GetCentralAngle(Vector2 latLon1, Vector2 latLon2)
    {
        float lat1 = latLon1.x;
        float lat2 = latLon2.x;
        float lon1 = latLon1.y;
        float lon2 = latLon2.y;
        float absDiffLon = Mathf.Abs(lon1 - lon2);

        return Mathf.Acos(
            Mathf.Sin(lat1) * Mathf.Sin(lat2) +
            Mathf.Cos(lat1) * Mathf.Cos(lat2) * Mathf.Cos(absDiffLon)
            );
    }

    /// <summary>
    ///Calculate the distance between two lat lon
    /// </summary>
    /// <param name="latLon1"></param>
    /// <param name="latLon2"></param>
    /// <returns></returns>
    public float GreatCircleDistance(Vector2 latLon1, Vector2 latLon2)
    {
        return radius * 0.5f * GetCentralAngle(latLon1, latLon2);
    }

    /// <summary>
    /// Drawing line from latLon1 to latLon2
    /// </summary>
    /// <param name="latLon1"></param>
    /// <param name="latLon2"></param>
    public void DrawGreatCircleArc(Vector2 latLon1, Vector2 latLon2, Color color, float width)
    {
        GameObject g = new GameObject("Line");
        g.transform.position = transform.position;
        g.transform.SetParent(transform);

        LineRenderer line = g.AddComponent<LineRenderer>();
        line.useWorldSpace = false;
        line.material = new Material(lineShader);
        line.widthMultiplier = width;
        line.material.color = color;
        List<Vector3> points = GetPointsBetween(latLon1, latLon2, 50);
        line.positionCount = points.Count;
        //add points, transform to local coordinate
        foreach (Vector3 p in points)
        {
            line.SetPosition(points.IndexOf(p), g.transform.InverseTransformPoint(p));
        }

        //add to list
        arcs.Add(line);
    }

    /// <summary>
    /// Remove all arcs
    /// </summary>
    public void RemoveArcs()
    {
        foreach(LineRenderer l in arcs)
        {
            Destroy(l.gameObject, 0);
        }
        arcs = new List<LineRenderer>();
    }

    /// <summary>
    /// Return the mid point in  between two locations
    /// </summary>
    /// <param name="latlon1"></param>
    /// <param name="latlon2"></param>
    /// <returns></returns>
    public Vector2 GetMidPointBetween(Vector2 latlon1, Vector2 latlon2)
    {
        Vector3 p1 = GeoToWorldPosition(latlon1);
        Vector3 p2 = GeoToWorldPosition(latlon2);
        Vector3 center = (p1 + p2) * 0.5f;

        return WorldToGeoPosition(transform.position + (center - transform.position).normalized * radius);
    }

    /// <summary>
    /// [DOES NOT WORK WELL] Rotating the globe such that the latLon is at the give world point
    /// </summary>
    /// <param name="worldPoint">the point to which latLon should be moved</param>
    /// <param name="latLon">the target lat lon</param>
    ///
    private Vector3 toTarget, toWorld, upVector, worldPoint, targetPoint;
    public void RotateToPoint(Vector3 worldPoint, Vector2 latLon, bool doit)
    {
        Vector3 target = GeoToWorldPosition(latLon);

        //animate
        toTarget = target - transform.position;
        toWorld = worldPoint - transform.position;
        upVector = Vector3.Cross(toWorld, toTarget);

        Quaternion currentRotation = transform.rotation;

        Quaternion finalRot = currentRotation * Quaternion.FromToRotation(toTarget, toWorld);
        if (doit)
        {
            StartCoroutine(RotationTween(currentRotation, finalRot, delegate
            {
            }));
        }

    }

    //private void OnDrawGizmos()
    //{
    //    Gizmos.color = Color.red;
    //    Gizmos.DrawLine(transform.position + toTarget, transform.position);
    //    Gizmos.color = Color.green;
    //    Gizmos.DrawLine(transform.position + toWorld, transform.position);
    //    Gizmos.color = Color.blue;
    //    Gizmos.DrawLine(transform.position + upVector, transform.position);
    //}

    private List<Vector3> GetSmallCirclePointsLatitude(float lat, int n)
    {
        List<Vector3> points = new List<Vector3>();
        float spacing = 360f / n;
        for(int i = 0; i < n; i++)
        {
            float lon = i * spacing;
            Vector3 point = GeoToWorldPosition(new Vector2(lat, lon));
            points.Add(transform.InverseTransformPoint(point + (point - transform.position).normalized * 0.001f));
        }
        return points;
    }
    private List<Vector3> GetSmallCirclePointsLongitude(float lon, int n)
    {
        List<Vector3> points = new List<Vector3>();
        float spacing = 360f / n;
        for (int i = 0; i < n; i++)
        {
            float lat = i * spacing;
            Vector3 point = GeoToWorldPosition(new Vector2(lat, lon));
            points.Add(transform.InverseTransformPoint(point + (point - transform.position).normalized * 0.001f));
        }
        return points;
    }

    public void DrawLongitudeSmallCircle(float lon)
    {
        GameObject g = new GameObject("LongitudeLine");
        LineRenderer l = g.AddComponent<LineRenderer>();
        l.material = new Material(lineShader);
        l.material.color = graticuleColor;
        l.widthMultiplier = tickness;
        l.loop = true;
        l.useWorldSpace = false;

        g.transform.position = transform.position;
        g.transform.rotation = transform.rotation;

        List<Vector3> points = GetSmallCirclePointsLongitude(lon, lineResolution);
        l.positionCount = points.Count;
        l.SetPositions(points.ToArray());

        g.transform.SetParent(transform);
    }

    public void DrawLatitudeSmallCircle(float lat)
    {
        GameObject g = new GameObject("LatitudeLine");
        LineRenderer l = g.AddComponent<LineRenderer>();
        l.material = new Material(lineShader);
        l.material.color = graticuleColor;
        l.widthMultiplier = tickness;
        l.loop = true;
        l.useWorldSpace = false;

        g.transform.position = transform.position;
        g.transform.rotation = transform.rotation;

        List<Vector3> points = GetSmallCirclePointsLatitude(lat, lineResolution);
        l.positionCount = points.Count;
        l.SetPositions(points.ToArray());

        g.transform.SetParent(transform);
    }

    public void CreateGraticuleLines()
    {
        //Draw equator
        DrawLatitudeSmallCircle(0);

        int n = 90 / latitudeSpacing;
        //Draw latitude lines
        for(int i = 1; i < n; i++)
        {
            DrawLatitudeSmallCircle(i * latitudeSpacing);
            DrawLatitudeSmallCircle(i * -latitudeSpacing);
        }

        //Draw longitude lines
        n = 360 / longitudeSpacing;
        for(int i = 1; i < n; i++)
        {
            DrawLongitudeSmallCircle(i * longitudeSpacing);
        }

    }

    /// <summary>
    /// Is globe initiated
    /// </summary>
    /// <returns></returns>
    public bool IsGlobeReady()
    {
        return isReady;
    }

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
        if(showLines) CreateGraticuleLines();
        isReady = true;
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

        if (egocentric)
        {
            lat *= 1f;
            lon *= -1f;
        }
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
}