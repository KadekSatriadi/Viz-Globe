using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DrawCriclesTest : MonoBehaviour
{
    public Globe globe;
    public Vector2 center;
    public float angularDiameter;
    public Color color;
    public float width = 0.001f;

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            globe.DrawCircle(center, angularDiameter * 0.5f, color, width);
        }
    }
}
