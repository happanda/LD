using System;
using System.Collections;


public class TilePrefabNameAttribute : Attribute
{
    public TilePrefabNameAttribute(string prefabName)
    {
        Name = prefabName;
    }

    public string Name
    {
        get;
        set;
    }
}

public class FragPrefabNameAttribute : Attribute
{
    public FragPrefabNameAttribute(string prefabName)
    {
        Name = prefabName;
    }

    public string Name
    {
        get;
        set;
    }
}
