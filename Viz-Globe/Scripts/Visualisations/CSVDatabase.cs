using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CSVDatabase : Database
{
    public TextAsset csv;

    private void Awake()
    {
        data = CSVReader.Read(csv);
    }

    public override List<float> GetFloatRecordsByField(string fieldname, string con)
    {
        if (data == null)
        {
            data = CSVReader.Read(csv);
        }

        List<float> r = new List<float>();
        foreach(Dictionary<string, object> d in data)
        {
            foreach(KeyValuePair<string, object> kv in d)
            {
                if (kv.Key.Equals(fieldname))
                {
                    Debug.Log(kv.Value.ToString());
                    Debug.Log(kv.Key.ToString());
                    r.Add(float.Parse(kv.Value.ToString()));
                }
            }          
        }

        return r;
    }

    public override List<Dictionary<string, object>> GetRecordsByField(string fieldname)
    {
        if (data == null)
        {
            data = CSVReader.Read(csv);
        }

        List<Dictionary<string, object>> r = new List<Dictionary<string, object>>();
        foreach (Dictionary<string, object> d in data)
        {
            foreach (KeyValuePair<string, object> kv in d)
            {
                if (kv.Key.Equals(fieldname))
                {
                    r.Add(d);
                }
            }
        }

        return r;
    }

}
