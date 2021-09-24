using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GetMidPointBetweenTest : MonoBehaviour
{
    public Globe globe;
    public Vector2 latLon1;
    public Vector2 latLon2;

    GameObject g, c, d;
    void Start()
    {
        g = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        g.transform.localScale = Vector3.one * 0.01f;

        c = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        c.transform.localScale = Vector3.one * 0.01f;

        d = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        d.transform.localScale = Vector3.one * 0.01f;
    }

    // Update is called once per frame
    void Update()
    {
        if (globe)
        {
            c.transform.position = globe.GeoToWorldPosition(latLon1);
            d.transform.position = globe.GeoToWorldPosition(latLon2);
            globe.ClearArcs();
            globe.DrawGreatCircleArc(latLon1, latLon2, Color.red, 0.001f);
            g.transform.position = globe.GeoToWorldPosition(globe.GetMidPointBetween(latLon1, latLon2));
        
        }
    }

    private string text;
    private void OnGUI()
    {
        GUI.Label(new Rect(10, 10, 200, 200), text);
    }
}
