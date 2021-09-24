using UnityEngine;

public class VisualisationNormalBar : Visualisation
{        
    [Header("Mapping")]
    [Range(0f, 1f)]
    public float maxLength = 1f;
    [Range(0f, 0.5f)]
    public float barSize = 0.1f;

    void Start()
    {
        if (initOnStart)
        {
            Initiate();
        }      
    }

   

    

    private void Update()
    {
        material.SetFloat("_MaxRange", maxLength);
        material.SetFloat("_Radius", globe.radius);
        material.SetFloat("_Size", barSize);
        colorProvider.UpdateColor(material);
    }

}
