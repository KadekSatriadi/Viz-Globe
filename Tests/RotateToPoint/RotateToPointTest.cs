using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotateToPointTest : MonoBehaviour
{
    public Globe globe;
    public Vector2 latLon;

    Vector2 currentLatLon;
    // Start is called before the first frame update
    GameObject g;
    void Start()
    {
        g = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        g.transform.localScale = Vector3.one * 0.01f;
    }

    // Update is called once per frame
    void Update()
    {
        if (globe)
        {
            g.transform.position = globe.GeoToWorldPosition(latLon);
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if(Physics.Raycast(ray, out RaycastHit hit))
            {
                if(hit.transform == globe.transform)
                {
                    currentLatLon = globe.WorldToGeoPosition(hit.point);
                    text = "Lat long= " + latLon.ToString();
                }
            }

            if (Input.GetMouseButtonDown(0))
            {
                globe.RotateToPoint(globe.GeoToWorldPosition(currentLatLon), latLon);
            }
        }
    }

    private string text;
    private void OnGUI()
    {
        GUI.Label(new Rect(10, 10, 200, 200), text);
    }
}
