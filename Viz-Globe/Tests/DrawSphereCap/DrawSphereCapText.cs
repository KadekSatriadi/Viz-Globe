using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DrawSphereCapText : MonoBehaviour
{
    public Globe globe;
    public Vector2 latLon;
    [Range(5, 180)]
    public float centralAngle;
    [Range(10,200)]
    public int n;
    [Range(2,200)]
    public int nSegments;
    public Color color;
    public bool live = false;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (globe.IsGlobeReady())
        {
            if (Input.GetKeyDown(KeyCode.Space))
            {
                globe.ClearSphereCaps();
                globe.DrawSphereCap(latLon, centralAngle, n, nSegments, color);
            }
            if (live)
            {
                globe.ClearSphereCaps();
                globe.DrawSphereCap(latLon, centralAngle, n, nSegments, color);
            }
        }
    }
}
