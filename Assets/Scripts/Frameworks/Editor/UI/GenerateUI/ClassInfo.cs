using System.Collections.Generic;
using UnityEngine;

public class ClassInfo
{
    public string name;
    public string baseName;
    public bool show = true;
    public GameObject gameObject;
    public List<ElementInfo> elementList = new List<ElementInfo>();

    public ClassInfo()
    {
    }

    public ClassInfo(string name, string baseName)
    {
        this.name = name;
        this.baseName = baseName;
    }
}

public class ElementInfo
{
    public List<EElementType> eleType;
    public string eleName;
    public string path;

    public ElementInfo()
    {
    }

    public ElementInfo(List<EElementType> eleType, string eleName, string path)
    {
        this.eleName = eleName;
        this.eleType = eleType;
        this.path = path;
    }
}