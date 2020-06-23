using System;
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
        if (Input.GetKeyDown(KeyCode.Space))
        {
            Debug.Log("Pairs");
            string text = "lat1,lon1,lat2,lon2,angular_dist, lat_center_offset" + Environment.NewLine;
            for(int i = 0; i < n / 2; i++)
            {
                if(i > 0 && i != (n / 4))
                    text += latlons[i].x.ToString("F4") + "," + latlons[i].y.ToString("F4") + "," + latlons[i + (n / 2)].x.ToString("F4") + "," + latlons[i + (n / 2)].y.ToString("F4") + "," + angle * 2 + "," + latlon.x + Environment.NewLine;
            }

            System.IO.StreamWriter file = new System.IO.StreamWriter(@"lat_lon_pool_"+ angle * 2 +"_lat_" + latlon.x + " .csv", true);
            file.Write(text);
            file.Close();
        }
    }

    //private void OnDrawGizmos()
    //{
    //    for(int i = 0; i < n; i++)
    //    {
    //        Handles.Label(pos[i], latlons[i].ToString("F4"));
    //    }
    //}
}
