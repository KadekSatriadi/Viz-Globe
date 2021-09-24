using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class PopulationDensityYearLabel : MonoBehaviour
{
    public TextMeshProUGUI textLabel;
    public VisualisationNormalBarMultiYear visualisationNormalBarMultiYear;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        float val = visualisationNormalBarMultiYear.activeYear;
        int year = (int)val;
        string text = "";
        switch (year)
        {
            case 0:
                text = "";
                break;
            case 1:
                text = "2000";
                break;
            case 2:
                text = "2005";
                break;
            case 3:
                text = "2010";
                break;
            case 4:
                text = "2015";
                break;
            case 5:
                text = "2020";
                break;
        }
        textLabel.text = text;
    }
}
