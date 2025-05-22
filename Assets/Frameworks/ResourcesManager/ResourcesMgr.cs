using CommonLogic;
using UnityEngine;

public class ResourcesMgr : Singleton<ResourcesMgr>
{
    public GameObject LoadGameObject(string path, Transform parent)
    {
        GameObject obj = Resources.Load(path) as GameObject;
        if (obj == null)
            return null;
        obj.transform.SetParent(parent);
        return obj;
    }
}