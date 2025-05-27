using System;

[AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = true)]
public class UIBaseHandlerAttribute : Hotfix.AttributeBase
{
    public string PrefabName;

    public UIBaseHandlerAttribute(string prefabName)
    {
        this.PrefabName = prefabName;
    }
}