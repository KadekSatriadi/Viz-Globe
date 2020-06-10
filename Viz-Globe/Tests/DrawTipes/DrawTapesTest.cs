using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DrawTapesTest : MonoBehaviour
{
    public Globe globe;
    public Vector2 latlon1;
    public Vector2 latlon2;
    public float width;
    public int nSegment;
    public Color color;
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            if (globe.IsGlobeReady())
            {
                globe.ClearTapes();
                globe.DrawTape(latlon1, latlon2, width, color);
            }

        }

    }
}
