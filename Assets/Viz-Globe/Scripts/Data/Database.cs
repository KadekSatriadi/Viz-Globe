using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Database : MonoBehaviour
{
    protected List<Dictionary<string, object>> data;


    public abstract List<Dictionary<string, object>> GetRecordsByField(string fieldname);
    public abstract List<float> GetFloatRecordsByField(string fieldname, string con);

    public abstract List<string> GetStringRecordsByField(string fieldname, string con);

    public abstract List<object> GetUniqueValues(string fieldname);

    public virtual List<Dictionary<string, object>> GetData()
    {
        return data;
    }

}
