using System.Collections.Generic;
using Framework;
using UnityEngine;
using UnityEngine.UI;

public class UIContainer
{
    public GameObject gameObject;
    public Transform transform => gameObject.transform;
    public GameObject Item;
    public int Count;
    public const string Template = "Template";

    private List<UIViewBase> _children = new List<UIViewBase>();
    public List<UIViewBase> Children => _children;
    private List<GameObject> _templateChildren = new List<GameObject>();

    public UIContainer(GameObject gameObject)
    {
        this.gameObject = gameObject;
    }

    public void OnInit<T>(int count, object[] objs) where T : UIViewBase, new()
    {
        this.Count = count;
        Transform itemTrans = FindTemplate();
        if (itemTrans == null)
            return;

        for (int i = 0; i < Count; i++)
        {
            GameObject obj = Object.Instantiate(itemTrans.gameObject, itemTrans.parent, false);
            obj.name = i.ToString();
            obj.SetActive(true);
            T t = new T
            {
                gameObject = obj.transform.GetChild(0).gameObject
            };
            t.BindUI();
            t.onCreate();
            if (objs == null)
            {
                KaLog.LogError("数据不能为null啊朋友！！！！");
                t.onShow(new object[] { null });
            }
            else
            {
                if (objs.Length != Count)
                {
                    KaLog.LogError("数据数量对不上啊朋友！！！！");
                }

                t.onShow(new object[] { objs[i] });
            }

            _children.Add(t);
        }
    }

    public void OnHide()
    {
        if (_children == null)
            return;

        for (int i = 0; i < _children.Count; i++)
        {
            _children[i].onHide();
        }
    }

    public void OnDestroy()
    {
        Count = 0;
        if (_children != null)
        {
            for (int i = 0; i < _children.Count; i++)
            {
                _children[i].OnDestroy();
            }

            _children.Clear();
        }

        _children = null;
    }

    public void Refill<T>(object[] objs, List<Vector3> localPos = null) where T : UIViewBase, new()
    {
        var itemTemplate = FindTemplate();
        if (itemTemplate == null)
            return;

        if (objs == null || objs.Length < 1)
            return;

        if (itemTemplate.childCount < 1)
            return;

        Clear();

        Count = objs.Length;
        var newItemIndex = transform.childCount - 1;
        var countIndex = 0;

        // 在new出一个Container的情况下，如果之前Template被复制过多份，会导致不走_children.Add(),进而不走_children[curDataIndex].onShow
        PreFillExistItems<T>(localPos);

        while (newItemIndex < Count)
        {
            GameObject obj = Object.Instantiate(itemTemplate.gameObject, itemTemplate.parent, false);
            obj.name = newItemIndex.ToString();
            if (localPos != null && newItemIndex < localPos.Count)
            {
                obj.transform.localPosition = localPos[newItemIndex];
            }

            _templateChildren.Add(obj);

            T t = new T
            {
                gameObject = obj.transform.GetChild(0).gameObject
            };
            t.BindUI();
            t.onCreate();
            _children.Add(t);

            ++newItemIndex;
            ++countIndex;
        }

        var containerTrans = itemTemplate.parent;
        // 索引为0的是item的模板，真正的item从1开始
        var curItemIndex = 1;
        var curDataIndex = 0;
        while (curDataIndex < Count && curItemIndex < containerTrans.childCount && curDataIndex < _children.Count)
        {
            containerTrans.GetChild(curItemIndex).gameObject.SetActive(true);
            _children[curDataIndex].onShow(new object[] { objs[curDataIndex] });
            ++curItemIndex;
            ++curDataIndex;
        }

        while (curItemIndex < containerTrans.childCount)
        {
            containerTrans.GetChild(curItemIndex).gameObject.SetActive(false);
            ++curItemIndex;
        }

        LayoutRebuilder.ForceRebuildLayoutImmediate(transform as RectTransform);
    }

    public void RemoveAt(int index)
    {
        if (_children == null || _children.Count < index)
            return;

        if (_templateChildren == null || _templateChildren.Count < index)
            return;

        _children[index].onHide();
        _children[index].OnDestroy();

        Object.DestroyImmediate(_templateChildren[index]);
    }

    public void Append<T>(object param) where T : UIViewBase, new()
    {
        var itemTemplate = FindTemplate();
        if (itemTemplate == null)
            return;

        var newItemIndex = _children.Count;
        GameObject obj = Object.Instantiate(itemTemplate.gameObject, itemTemplate.parent, false);
        obj.name = newItemIndex.ToString();
        _templateChildren.Add(obj);

        T t = new T
        {
            gameObject = obj.transform.GetChild(0).gameObject
        };
        t.BindUI();
        t.onCreate();
        _children.Add(t);
    }

    public void Clear()
    {
        if (_children != null)
        {
            for (int i = 0; i < _children.Count; i++)
            {
                _children[i].onHide();
                _children[i].OnDestroy();
            }

            _children.Clear();
        }

        if (_templateChildren != null)
        {
            for (int i = 0; i < _templateChildren.Count; i++)
            {
                Object.DestroyImmediate(_templateChildren[i]);
            }

            _templateChildren.Clear();
        }
    }

    private void PreFillExistItems<T>(List<Vector3> localPos = null) where T : UIViewBase, new()
    {
        for (int i = 1; i < transform.childCount; i++)
        {
            var curItemObj = transform.GetChild(i);
            curItemObj.name = (i - 1).ToString();

            var localPosIndex = i - 1;
            if (localPos != null && localPosIndex < localPos.Count)
            {
                curItemObj.transform.localPosition = localPos[localPosIndex];
            }

            _templateChildren.Add(curItemObj.gameObject);

            T t = new T
            {
                gameObject = curItemObj.transform.GetChild(0).gameObject
            };
            t.BindUI();
            t.onCreate();
            _children.Add(t);
        }
    }


    private Transform FindTemplate()
    {
        if (this.transform.childCount < 1)
            return null;

        Transform itemTrans = this.transform.GetChild(0);
        if (itemTrans.name != Template)
        {
            KaLog.LogError("UIContainer InitData Error : Do not have 'Template' child.");
            return null;
        }

        itemTrans.gameObject.SetActive(false);
        return itemTrans;
    }

    public UIViewBase GetViewByIndex(int index)
    {
        if (_children == null)
        {
            KaLog.LogError("GetViewByIndex _children == null");
            return null;
        }

        if (index >= _children.Count || index < 0)
        {
            KaLog.LogError($"GetViewByIndex index={index}, _children.Count={_children.Count}");
            return null;
        }

        return _children[index];
    }
}