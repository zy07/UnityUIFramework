using UnityEngine;

public class UIUtil
{
    public static void DestroyWidget<T>(ref T widget) where T : UIViewBase
    {
        if (widget == null)
            return;
        
        widget.Hide();
        widget.OnDestroy();
        widget = null;
    }
    
    public static void CreateWidget<T>(GameObject root,string widgetPrefabName,ref T widget) where T : UIViewBase,new()
    {
        if (root == null)
        {
            KaLog.LogError($"创建{widgetPrefabName}失败！对应的Root为null");
            return;
        }
        
        var widgetRewardView = root.transform.Find(widgetPrefabName);
        if (widgetRewardView == null)
        {
            KaLog.LogError($"找不到{widgetPrefabName}");
        }
        else
        {
            widget = new();
            widget.gameObject = widgetRewardView.gameObject;
            widget.BindUI();
            widget.onCreate();
        }
    }

    public static void ShowWidget(UIViewBase widget,object[] objs = null)
    {
        if (widget == null)
            return;

        widget.Hide();
        widget.Show(objs);
    }
}