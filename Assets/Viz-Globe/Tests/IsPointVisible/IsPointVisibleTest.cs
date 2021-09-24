using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class IsPointVisibleTest : MonoBehaviour
{
    public Transform viewPoint;
    public Globe globe;
    public List<Vector2> latLong = new List<Vector2>();

    int n = 100;

    void Update()
    {
        for(int i = 0; i < n; i++)
        {
            latLong.Add(new Vector2(Random.Range(-90f, 90), Random.Range(-180f, 180f)));
        }
    }


    private void OnDrawGizmos()
    {
        if(latLong.Count > 0)
        for (int i = 0; i < n; i++)
        {
                Gizmos.DrawSphere(globe.GeoToWorldPosition(latLong[i]), 0.005f);
                if (globe.IsPointVisible(viewPoint.position, latLong[i]))
                { 
                    Gizmos.DrawLine(viewPoint.position, globe.GeoToWorldPosition(latLong[i]));
                  //  Handles.Label(globe.GeoToWorldPosition(latLong[i]), latLong[i].ToString());
                  //  Handles.Label(viewPoint.position, globe.IsPointVisible(viewPoint.position, latLong[i]).ToString());
                }
           
        }
        //Handles.Label(viewPoint.position, "Visible points");
    }
}
