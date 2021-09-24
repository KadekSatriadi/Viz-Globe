using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DrawLineTest : MonoBehaviour
{
    public Globe globe;
    public Vector2 latlon1;
    public Vector2 latlon2;

    // Start is called before the first frame update
    void Start()
    {
        globe.OnReady += delegate
        {
            GameObject g1 = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            g1.transform.localScale = Vector3.one * 0.01f;
            g1.transform.position = globe.GeoToWorldPosition(latlon1);

            GameObject g2 = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            g2.transform.localScale = Vector3.one * 0.01f;
            g2.transform.position = globe.GeoToWorldPosition(latlon2);

            g1.transform.SetParent(globe.transform);
            g2.transform.SetParent(globe.transform);

            Debug.Log("Distance = " + globe.GreatCircleDistance(latlon1, latlon2));
            globe.DrawGreatCircleArc(latlon1, latlon2, Color.black, 0.0025f);
        };
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
