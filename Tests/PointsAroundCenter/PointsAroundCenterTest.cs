using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class PointsAroundCenterTest : MonoBehaviour
{
    public Globe globe;
    public Vector2 latlon;
    [Range(0, 90)]
    public float angle;
    public LineRenderer line;
    public int n = 100;

    List<Vector2> latlons = new List<Vector2>();
    List<Vector3> pos = new List<Vector3>();
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (globe)
        {
            line.positionCount = n;
            pos = globe.GetCirclePoints(latlon, angle, n);
            latlons = globe.GetCircleLatLons(latlon, angle, n);
            line.SetPositions(pos.ToArray());
        }    
    }

    private void OnDrawGizmos()
    {
        for(int i = 0; i < n; i++)
        {
            Handles.Label(pos[i], latlons[i].ToString());
        }
    }
}
