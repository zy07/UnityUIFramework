
using Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public static class ComponentEx
{
    [MenuItem("GameObject/UI/空白点击组件")]
    public static void CreateEmptyGraphic()
    {
        if (Selection.activeTransform)
        {
            GameObject obj = new GameObject("EmptyGraphic");
            obj.AddComponent(typeof(CanvasRenderer));
            obj.AddComponent(typeof(EmptyGraphic));
            obj.transform.SetParent(Selection.activeTransform, false);
            Selection.activeTransform = obj.transform;
        }
    }
    
    [MenuItem("GameObject/UI/垂直滑动列表")]
    public static void CreateLoopVerticalScroll()
    {
        if (Selection.activeTransform)
        {
            GameObject obj = new GameObject("t_LoopScroll");
            var ex = obj.AddComponent<ScrollerProEx>();
            var pro = obj.AddComponent<ScrollerPro>();
            ex.horizontal = false;
            ex.vertical = true;
            obj.AddComponent(typeof(RectMask2D));

            //可以拖拽
            var bg = obj.AddComponent<Image>();
            bg.color = new Color(0, 0, 0, 0);
            bg.raycastTarget = true;

            GameObject item = new GameObject("item");
            item.transform.SetParent(obj.transform, false);

            obj.transform.SetParent(Selection.activeTransform, false);
            Selection.activeTransform = obj.transform;
        }
    }
    
    [MenuItem("GameObject/UI/水平滑动列表")]
    public static void CreateLoopHorizontalScroll()
    {
        if (Selection.activeTransform)
        {
            GameObject obj = new GameObject("t_LoopScroll");
            var ex = obj.AddComponent<ScrollerProEx>();
            var pro = obj.AddComponent<ScrollerPro>();
            ex.horizontal = true;
            ex.vertical = false;
            obj.AddComponent(typeof(RectMask2D));

            //可以拖拽
            var bg = obj.AddComponent<Image>();
            bg.color = new Color(0, 0, 0, 0);
            bg.raycastTarget = true;

            GameObject item = new GameObject("item");
            item.AddComponent<RectTransform>();
            item.transform.SetParent(obj.transform, false);

            obj.transform.SetParent(Selection.activeTransform, false);
            Selection.activeTransform = obj.transform;
        }
    }

    [MenuItem("GameObject/UI/不需要滑动的滑动列表")]
    public static void CreateNoneLoopScroll()
    {
        if (Selection.activeTransform)
        {
            GameObject obj = new GameObject("t_LoopScroll");
            var ex = obj.AddComponent<ScrollerProEx>();
            var pro = obj.AddComponent<ScrollerPro>();
            ex.horizontal = false;
            ex.vertical = false;
            pro.needScroller = false;

            obj.AddComponent(typeof(RectMask2D));

            //可以拖拽
            var bg = obj.AddComponent<Image>();
            bg.color = new Color(0, 0, 0, 0);
            bg.raycastTarget = true;

            GameObject item = new GameObject("item");
            item.AddComponent<RectTransform>();
            item.transform.SetParent(obj.transform, false);

            obj.transform.SetParent(Selection.activeTransform, false);
            Selection.activeTransform = obj.transform;
        }
    }
}

[InitializeOnLoad]
public class ComponentAddListener
{
    static ComponentAddListener()
    {
        ObjectFactory.componentWasAdded += ComponentWasAdded;
    }

    private static void ComponentWasAdded(Component obj)
    {
        if (obj.GetType().IsSubclassOf(typeof(MaskableGraphic)))
        {
            ((MaskableGraphic)obj).raycastTarget = false;
        }

        if (obj.GetType() == typeof(Button))
        {
            obj.gameObject.AddComponent<ButtonEx>();
        }
    }
}

