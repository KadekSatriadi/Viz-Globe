using System;
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
    public int latitudeSpacing = 30;
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
    private List<GameObject> graticules = new List<GameObject>();
    private bool isReady = false;
    private List<LineRenderer> arcs = new List<LineRenderer>();
    private List<GameObject> tapes = new List<GameObject>();
    private List<GameObject> sphereCaps = new List<GameObject>();

    private const float ROLL_LIMIT = 60f;
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
    /// Drawing a sphere cap on the globe
    /// </summary>
    /// <param name="latLon"></param>
    /// <param name="angularRadius">Radius!</param>
    /// <param name="nPoints"></param>
    /// <param name="nSegments"></param>
    /// <param name="color"></param>
    public void DrawSphereCap(Vector2 latLon, float angularRadius, int nPoints, int nSegments, Color color)
    {
      
        GameObject sphereCap = new GameObject("SphereCap");
        sphereCap.transform.position = transform.position;
        sphereCap.transform.rotation = transform.rotation;
        sphereCap.transform.SetParent(transform);
        MeshFilter mf = sphereCap.AddComponent<MeshFilter>();
        MeshRenderer mr = sphereCap.AddComponent<MeshRenderer>();
        mf.mesh = CreateSpherCapMesh(latLon, angularRadius, nPoints, nSegments);
        mr.material = new Material(lineShader);
        mr.material.color = color;

        sphereCaps.Add(sphereCap);
    }


    /// <summary>
    /// Create sphere cap meash mesh
    /// </summary>
    /// <param name="latLon"></param>
    /// <param name="angularRadius">Radius!</param>
    /// <param name="nPoints"></param>
    /// <param name="nSegments"></param>
    /// <param name=""></param>
    /// <returns></returns>
    public Mesh CreateSpherCapMesh(Vector2 latLon, float angularRadius, int nPoints, int nSegments)
    {
        List<Vector3> points = new List<Vector3>();
        List<Vector3> normals = new List<Vector3>();
        List<int> triangles = new List<int>();

        points.AddRange(GetCirclePoints(latLon, 0f, 1));
        for (int i = 1; i < nSegments; i++)
        {
            points.AddRange(GetCirclePoints(latLon, i * (angularRadius / (nSegments - 1)), nPoints));
        }

        foreach (Vector3 p in points)
        {
            //DEBUG
            //GameObject g = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            //g.transform.position = transform.position;
            //g.transform.localScale *= 0.01f;
            //g.transform.SetParent(transform);
            //g.transform.localPosition = p;
            //g.name = points.IndexOf(p).ToString();

            normals.Add(p - transform.localPosition);
        }

        //Triangles for the inner circle
        int tri = 1;
        for (int i = 0; i < nPoints - 1; i++)
        {
            triangles.Add(0);
            triangles.Add(tri + 1);
            triangles.Add(tri);
            tri++;
        }
        // Debug.Log(tri);
        triangles.Add(0);
        triangles.Add(1);
        triangles.Add(tri);
        tri++;

        //Debug.Log("New line");

        ////Triangles for outter circles
        for (int i = 0; i < nSegments - 2; i++)
        {
            for (int j = 0; j < nPoints - 1; j++)
            {
                triangles.Add(tri);//21
                triangles.Add(tri - nPoints); //1
                triangles.Add(tri + 1); //22

                triangles.Add(tri - nPoints);
                triangles.Add(tri - nPoints + 1);
                triangles.Add(tri + 1);

                // Debug.Log(tri);



                tri++;

            }
            triangles.Add(tri);//40
            triangles.Add(tri - nPoints); //20
            triangles.Add(tri - nPoints + 1); //21

            triangles.Add(tri - nPoints); //20
            triangles.Add(tri - (nPoints * 2) + 1);//1
            triangles.Add(tri - nPoints + 1);//21

            //Debug.Log("New line");
            tri++;

        }


        Mesh mesh = new Mesh();
        mesh.vertices = points.ToArray();
        mesh.triangles = triangles.ToArray();
        mesh.normals = normals.ToArray();
        mesh.RecalculateBounds();
        mesh.Optimize();

        return mesh;
    }

    /// <summary>
    /// Clear all sphere caps
    /// </summary>
    public void ClearSphereCaps()
    {
        foreach(GameObject g in sphereCaps)
        {
            DestroyImmediate(g);
        }

        sphereCaps = new List<GameObject>();
    }

    /// <summary>
    /// Draw a tape on the sphere surface
    /// </summary>
    /// <param name="latLon1">point 1</param>
    /// <param name="latLon2">point 2</param>
    /// <param name="width">width of the tape</param>
    public void DrawTape(Vector2 latLon1, Vector2 latLon2, float width, Color color, int nSegments)
    {
        GameObject g = new GameObject("Tape");
        g.transform.rotation = transform.rotation;
        g.transform.SetParent(transform);
        g.transform.localPosition = Vector3.zero;
        MeshFilter meshFilter = g.AddComponent<MeshFilter>();
        MeshRenderer meshRenderer = g.AddComponent<MeshRenderer>();

        //update component
        meshFilter.mesh = CreateTapeMesh(latLon1, latLon2, width, nSegments);
        meshRenderer.material = new Material(lineShader);
        meshRenderer.material.color = color;

        //add to list
        tapes.Add(g);
    }

    /// <summary>
    /// Create Tape Mesh
    /// </summary>
    /// <param name="latLon1">point 1</param>
    /// <param name="latLon2">point 2</param>
    /// <param name="width">width</param>
    /// <returns></returns>
    public Mesh CreateTapeMesh(Vector2 latLon1, Vector2 latLon2, float width, int nSegments)
    {
        //Creat the tape mesh
        //Get the points between these two
        Vector3[] points = GetPointsBetween(latLon1, latLon2, nSegments + 1).ToArray();
        
        //Build mesh vertices
        Vector3[] verts = new Vector3[nSegments * 2 + 2];
        Vector3[] normals = new Vector3[verts.Length];
        int[] triangles = new int[nSegments * 6];
        int p = 0;
        for (int i = 0; i < points.Length; i++)
        {
            Vector3 anchor = points[i] + (points[i] - transform.position).normalized * 0.005f;

            Vector3 toCenterVect = (transform.position - anchor).normalized;
            Vector3 forwardVect = (i < points.Length - 1)? (points[i + 1] - anchor).normalized : (anchor - points[i - 1]).normalized;
            Vector3 sideVect = Vector3.Cross(toCenterVect, forwardVect);

            Vector3 leftVert = anchor - (sideVect.normalized * width * 0.5f);
            Vector3 rightVert = anchor + (sideVect.normalized * width * 0.5f);

            verts[p] = transform.InverseTransformPoint(leftVert);
            verts[p + 1] = transform.InverseTransformPoint(rightVert);           

            //normal
            normals[p] = (leftVert - transform.position).normalized;
            normals[p + 1] = (rightVert - transform.position).normalized;

            p += 2;
        }

        //triangles
        int c = 0;
        int tri = 0;
        for (int i = 0; i < nSegments; i++)
        {
            //if not last segment, update triangle
            triangles[tri] = c;
            triangles[tri + 1] = c + 1;
            triangles[tri + 2] = c + 2;
            triangles[tri + 3] = c + 1;
            triangles[tri + 4] = c + 3;
            triangles[tri + 5] = c + 2;
            c += 2;
            tri += 6;
        }

        //create Mesh
        Mesh mesh = new Mesh();
        mesh.vertices = verts;
        mesh.triangles = triangles;
        mesh.normals = normals;
        mesh.RecalculateBounds();
        mesh.Optimize();

        return mesh;
    }

    public void ClearTapes()
    {
        foreach(GameObject g in tapes)
        {
            Destroy(g, 0);
        }
        tapes = new List<GameObject>();
    }
    /// <summary>
    /// Get the circle points around the center in local coordinate
    /// </summary>
    /// <param name="center"></param>
    /// <param name="angularRadius">Angle between the point to spherecenter and center to spherecenter</param>
    /// <returns>Local coordinate</returns>
    public List<Vector3> GetCirclePoints(Vector2 center, float angularRadius, int n)
    {
        List<Vector3> points = new List<Vector3>();
        Vector3 worldCenter = transform.InverseTransformPoint(GeoToWorldPosition(center));
        Vector3 center2World = worldCenter - Vector3.zero;
        //Create points on the pole
        List<Vector3> polarPoints = GetSmallCirclePointsLatitude(90f - angularRadius,  n);
        //rotate center
        Quaternion rot = Quaternion.FromToRotation(transform.InverseTransformVector(transform.up) * radius, center2World);
        foreach(Vector3 p in polarPoints)
        {
            //rotate to the center
            Vector3 rotatedV = rot * p;
            points.Add(rotatedV);
        }

        return points;
    }
    /// <summary>
    /// Get the circle lat lons around the center 
    /// </summary>
    /// <param name="center"></param>
    /// <param name="angularRadius"></param>
    /// <param name="n"></param>
    /// <returns></returns>
    public List<Vector2> GetCircleLatLons(Vector2 center, float angularRadius, int n)
    {
        List<Vector3> list = GetCirclePoints(center, angularRadius, n);
        List<Vector2> latLons = new List<Vector2>();
        foreach(Vector3 p in list)
        {
            latLons.Add(WorldToGeoPosition(transform.TransformPoint(p)));
        }

        return latLons;
    }

    /// <summary>
    /// Draw circle on the sphere
    /// </summary>
    /// <param name="center"></param>
    /// <param name="angularRadius"></param>
    /// <param name="color"></param>
    /// <param name="width"></param>
    public void DrawCircle(Vector2 center, float angularRadius, Color color, float width)
    {
        GameObject g = new GameObject("Circle");
        g.transform.rotation = transform.rotation;
        g.transform.SetParent(transform);
        g.transform.localPosition = Vector3.zero;

        LineRenderer line = g.AddComponent<LineRenderer>();
        line.widthMultiplier = width;
        line.material = new Material(lineShader);
        line.material.color = color;
        line.useWorldSpace = false;
        line.loop = true;

        List<Vector3> points = GetCirclePoints(center, angularRadius, 100);
        List<Vector3> local = new List<Vector3>();
        foreach(Vector3 p in points)
        {
            local.Add(g.transform.InverseTransformPoint(p));
        }

        line.positionCount = points.Count;
        line.SetPositions(points.ToArray());
    }

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
        if(IsBeyonRollLimit())
        {
            transform.rotation = rot;
        }
    }
    public bool IsBeyonRollLimit()
    {
        if (Vector3.Angle(Vector3.up, transform.up) > ROLL_LIMIT)
        {
            return true;
        }
        else
        {
            return false;
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
    ///Calculate the distance between two lat lon, source: https://stackoverflow.com/questions/6544286/calculate-distance-of-two-geo-points-in-km-c-sharp
    /// </summary>
    /// <param name="latLon1"></param>
    /// <param name="latLon2"></param>
    /// <returns></returns>
    public float GreatCircleDistance(Vector2 latLon1, Vector2 latLon2)
    {
        float lat1 = latLon1.x;
        float lon1 = latLon1.y;
        float lat2 = latLon2.x;
        float lon2 = latLon2.y;

        float sLat1 = Mathf.Sin(Mathf.Deg2Rad * (lat1));
        float sLat2 = Mathf.Sin(Mathf.Deg2Rad * (lat2));
        float cLat1 = Mathf.Cos(Mathf.Deg2Rad * (lat1));
        float cLat2 = Mathf.Cos(Mathf.Deg2Rad * (lat2));
        float cLon = Mathf.Cos(Mathf.Deg2Rad * (lon1) - Mathf.Deg2Rad * (lon2));

        float cosD = sLat1 * sLat2 + cLat1 * cLat2 * cLon;

        float d = Mathf.Acos(cosD);

        float dist = radius * d;

        return dist;
    }

    /// <summary>
    /// Drawing line from latLon1 to latLon2
    /// </summary>
    /// <param name="latLon1"></param>
    /// <param name="latLon2"></param>
    public void DrawGreatCircleArc(Vector2 latLon1, Vector2 latLon2, Color color, float width)
    {
        GameObject g = new GameObject("Arc");
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
    public void ClearArcs()
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
            transform.rotation = finalRot;
        }

    }

    /// <summary>
    /// [DOES NOT WORK WELL] Rotating the globe such that the latLon is at the give world point
    /// </summary>
    /// <param name="worldPoint">the point to which latLon should be moved</param>
    /// <param name="latLon">the target lat lon</param>
    ///
    public void RotateToPointAnimate(Vector3 worldPoint, Vector2 latLon)
    {
      

        StartCoroutine(YawAnimate(worldPoint, latLon));
       
    }

    private IEnumerator YawAnimate(Vector3 worldPoint, Vector2 latLon)
    {
        Vector2 latLonWorld = WorldToGeoPosition(worldPoint);

        //animate
        float inc = 2f;
        bool isYawValid= true;
        bool isRollValid = true;

        float y = 1f;
        float r = 1f;
        if (latLonWorld.y > latLon.y) y = -1f;
        if (latLonWorld.x < latLon.x) r = -1f;

        while (isYawValid || isRollValid)
        {
            if(isYawValid) Yaw(y * inc);
            if(isRollValid) RollLimit(Camera.main.transform.position, r * inc);

            latLonWorld = WorldToGeoPosition(worldPoint);

            if (Mathf.Abs(latLonWorld.y - latLon.y) <= 3f)
            {
                isYawValid = false;
            }
            if (Mathf.Abs(latLonWorld.x - latLon.x) <= 3f)
            {
                isRollValid = false;
            }
            yield return new WaitForSeconds(0.005f);
        }
    }

    /// <summary>
    /// NOT TESTED! Rotate globe such that latlonorigin at latlontarget
    /// </summary>
    /// <param name="latLonTarget"></param>
    /// <param name="latLonOrigin"></param>
    public void RotateToPointLatitude(Vector2 latLonTarget, Vector2 latLonOrigin)
    {
        float diff = latLonOrigin.x - latLonTarget.x;
        Vector3 rot = transform.rotation.eulerAngles;
        transform.rotation = Quaternion.Euler(rot.x, rot.y, diff);       
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
    /// <summary>
    /// Return in local coordinate
    /// </summary>
    /// <param name="lat"></param>
    /// <param name="n"></param>
    /// <returns>Local space</returns>
    private List<Vector3> GetSmallCirclePointsLatitude(float lat, int n)
    {
        List<Vector3> points = new List<Vector3>();
        float spacing = 360f / n;
        for(int i = 0; i < n; i++)
        {
            float lon = i * spacing;
            Vector3 point = GeoToWorldPosition(new Vector2(lat, lon));
            points.Add(transform.InverseTransformPoint(point + (point - transform.position).normalized * 0.002f));
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
            points.Add(transform.InverseTransformPoint(point + (point - transform.position).normalized * 0.002f));
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
    /// <summary>
    /// Create graticules
    /// </summary>
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