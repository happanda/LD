using System;
using System.Collections;

[AttributeUsage(AttributeTargets.All)]
public class PrefabNameAttribute : Attribute
{
    public PrefabNameAttribute(string prefabName)
    {
        Name = prefabName;
    }

    public string Name
    {
        get;
        set;
    }
}
