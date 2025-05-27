using UnityEditor;
using UnityEngine;

public class UIEditorHelper : EditorWindow
{
    [MenuItem("GameObject/获取UI节点路径")]
    public static void GetUIPath()
    {
        if (Selection.activeGameObject == null)
        {
            Debug.LogError("请选择一个UI节点！");
            return;
        }

        var selectionActiveObj = Selection.activeGameObject;

        string path = selectionActiveObj.name;

        GetPath(selectionActiveObj, ref path);
        
        GUIUtility.systemCopyBuffer = path;
    }

    public static void GetPath(GameObject obj, ref string path)
    {
        if (obj.transform.parent == null)
            return;

        var parent = obj.transform.parent;
        path = parent.name + "/" + path;
        if (parent.name.StartsWith("UI"))
            return;
        
        GetPath(parent.gameObject, ref path);
    }
}