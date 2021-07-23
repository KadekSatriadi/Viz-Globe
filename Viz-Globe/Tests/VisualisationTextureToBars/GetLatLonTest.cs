using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GetLatLonTest : MonoBehaviour
{
    public WorldMap map;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    private string text = "";
    // Update is called once per frame
    void Update()
    {
        RaycastHit hit;
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        if (Physics.Raycast(ray, out hit))
        {
            Transform objectHit = hit.transform;
            WorldMap map = objectHit.gameObject.GetComponent<WorldMap>();
            if (map)
            {
                Vector3 localPos = objectHit.InverseTransformPoint(hit.point);
               text =   map.GetLatLonAt(localPos.x, localPos.y).ToString("F4");
            }
            
        }
    }

    void OnGUI()
    {
        GUI.Label(new Rect(10, 10, 1000, 200), text);
    }
}
