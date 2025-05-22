using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using Framework;
using UnityEngine;
using UnityEngine.UI;
using Hotfix;
using Object = UnityEngine.Object;
using UI;
using Unity.VisualScripting;
using Sequence = DG.Tweening.Sequence;

public enum EUIInitType
{
    Create, // 创建时的UI事件类型，OnDestroy的时候反注册
    Show, // 打开时的UI事件类型，OnHide的时候反注册
}

public abstract partial class UIBase : IScaleable, IPoolable
{
    public bool isShow { get { return _isShow; } }

    protected bool _isShow = false;

    [NonSerialized]
    public bool IsOpenScaleOver = false;

    public GameObject gameObject;
    public Transform root;
    public Transform scaleRoot;
    public string Name;

    public Transform transform => gameObject.transform;

    public List<string> ViewNameList = null;

    // public bool NeedDoScale = false;

    private List<RenderTexture> _rtList;
    private List<Camera> _rtCameraList;

    private List<Material> _grayMaterialList;
    [NonSerialized]
    // 每个UIView对应了一个UIBase, 一个UIBase对应了多个View
    // 放置在UIBase中的意义是，UIView也同样可以有多个View
    public DictionaryList<string, UIViewBase> UIViews;

    // 具有额外参数的View，一般用于新增于界面中的View，比如资源条
    public DictionaryList<string, UIViewBase> UIExtraArgsViews;

    public UIBase Parent = null;

    //private Dictionary<uint, List<UIBaseEffect>> uiEffDict = new Dictionary<uint, List<UIBaseEffect>>();

    public string uiViewName;

    public int SortingOrder;

    // 关闭界面清理字典
    private Dictionary<EUIInitType, List<ICloseClear>> _closeClearDict =
        new Dictionary<EUIInitType, List<ICloseClear>>();

    // Dotween字典，注册清除
    private List<Tween> mTweenList = new List<Tween>();

    private Dictionary<string, object[]> AddViewArgsDict = new Dictionary<string, object[]>();

    /// <summary>
    /// 初始化参数
    /// </summary>
    public virtual void InitializeParams()
    {

    }

    /// <summary>
    /// UI异步加载，按需使用
    /// </summary>
    /// <returns></returns>
    public virtual IEnumerator Prepare()
    {
        yield break;
    }

    protected void AddViewName(List<string> viewNameList)
    {
        if (ViewNameList == null)
        {
            ViewNameList = viewNameList;
        }
        else
        {
            KaLog.LogWarning("【UIBase】 ui view 已经添加过了，检查为何还要继续添加？");
        }
    }

    public virtual void Initialize()
    {

    }

    public void ShowView(string[] viewName)
    {
        for (int i = 0; i < viewName.Length; i++)
        {
            ShowView(viewName[i]);
        }
    }

    public void ShowView(string viewName)
    {
        if (UIViews.TryGetValue(viewName, out UIViewBase viewBase))
        {
            viewBase.Show(null);
        }
        else
        {
            KaLog.LogError($"没有找到名字为{viewName}的ViewBase");
        }
    }

    public void HideView(string[] viewName)
    {
        for (int i = 0; i < viewName.Length; i++)
        {
            HideView(viewName[i]);
        }
    }
    public void HideView(string viewName)
    {
        if (UIViews.TryGetValue(viewName, out UIViewBase viewBase))
        {
            viewBase.Hide();
        }
        else
        {
            KaLog.LogWarning($"没有找到名字为{viewName}的ViewBase");
        }

        //不对称的出现了一个这个，wtm同学来优化
        if (UIExtraArgsViews != null)
        {
            if (UIExtraArgsViews.TryGetValue(viewName, out UIViewBase extraViewBase))
            {
                extraViewBase.Hide();
            }
            else
            {
                KaLog.LogWarning($"没有找到名字为{viewName}的ViewBase");
            }
        }
    }


    public void ShowAllView()
    {
        for (int i = 0; i < UIViews.Count; i++)
        {
            UIViews.ValueList[i].Show(null);
        }
    }

    public void HideAllView()
    {
        for (int i = 0; i < UIViews.Count; i++)
        {
            UIViews.ValueList[i].Hide();
        }
    }

    public bool GetViewShowStatus(string viewName)
    {
        if (UIViews.TryGetValue(viewName, out UIViewBase view))
        {
            return view.isShow;
        }

        return false;
    }

    public UIViewBase GetViewByName(string viewName)
    {
        if (UIViews == null) return null;

        if (UIViews.TryGetValue(viewName, out UIViewBase view))
        {
            return view;
        }

        if (UIExtraArgsViews.TryGetValue(viewName, out UIViewBase extraView))
        {
            return extraView;
        }

        return null;
    }

    //public void Show3dObj(RawImage rawImage, string _3dObjPath, Vector2 pos, Vector3 rot, float scale = 1, bool canDrag = false, bool clear = false)
    //{
    //    if (clear) Clear3dObj();

    //    RenderTexture rt = new RenderTexture(1024, 1024, 0);
    //    if (_rtList == null) _rtList = new List<RenderTexture>();
    //    _rtList.Add(rt);
    //    rt.depthStencilFormat = GraphicsFormat.D16_UNorm;
    //    rawImage.texture = rt;
    //    GameObject camObj = new GameObject("RTCamera");
    //    Camera camera = camObj.AddComponent<Camera>();
    //    if (_rtCameraList == null) _rtCameraList = new List<Camera>();
    //    _rtCameraList.Add(camera);
    //    _3dObjCameraSetting(camera, rt, rawImage);
    //    GameObject obj = GameObjectPool.Get(_3dObjPath);
    //    obj.transform.SetParent(camera.transform, false);
    //    SetChildLayer(obj.transform, LayerMask.NameToLayer("Model"));
    //    Animator animator = obj.GetComponent<Animator>();
    //    if (animator == null)
    //    {
    //        animator = obj.GetComponentInChildren<Animator>();
    //    }

    //    if (animator != null)
    //    {
    //        animator.Play("Idle");
    //    }

    //    obj.transform.localPosition = new Vector3(pos.x, pos.y, 2000);
    //    obj.transform.localRotation = Quaternion.Euler(rot.x, rot.y, rot.z);
    //    obj.transform.localScale = new Vector3(scale, scale, scale);
    //    if (canDrag)
    //    {
    //        _3dObjDrag(rawImage, obj);
    //    }
    //}

    public void SetChildLayer(Transform trans, int layer)
    {
        trans.gameObject.layer = layer;
        if (trans.childCount != 0)
        {
            for (int i = 0; i < trans.childCount; i++)
            {
                SetChildLayer(trans.GetChild(i), layer);
            }
        }
    }

    void _3dObjCameraSetting(Camera camera, RenderTexture rt, RawImage rawImage)
    {
        camera.targetTexture = rt;
        camera.cullingMask = 1 << LayerMask.NameToLayer("Model");
        camera.transform.SetParent(rawImage.transform);
        camera.transform.localPosition = new Vector3(10000, 0, 0);
        camera.transform.localScale = new Vector3(1, 1, 1);
        camera.orthographic = true;
        camera.clearFlags = CameraClearFlags.SolidColor;
    }

    void _3dObjDrag(RawImage rawImage, GameObject obj)
    {
        UIDragEvent uiDragEvent;
        uiDragEvent = rawImage.gameObject.GetComponent<UIDragEvent>();
        if (uiDragEvent == null)
            uiDragEvent = rawImage.gameObject.AddComponent<UIDragEvent>();
        else
            uiDragEvent.ResetDragEvent();
        Vector2 dragStartPos = Vector2.zero;
        Vector2 dragEndPos = Vector2.zero;

        uiDragEvent.OnUIStartDrag = (pos) => { dragStartPos = pos; };
        uiDragEvent.OnUIDrag = (pos) => { On3dObjDrag(obj, ref dragStartPos, pos); };
        uiDragEvent.OnUIEndDrag = (pos) => { dragEndPos = pos; };
    }

    void On3dObjDrag(GameObject obj, ref Vector2 startPos, Vector2 endPos)
    {
        float xOffset = endPos.x - startPos.x;
        float dragSpeed = xOffset * 5;
        Vector3 targetVal = obj.transform.localRotation.eulerAngles;
        targetVal.y += -dragSpeed;
        obj.transform.DOLocalRotate(targetVal, 0.5f);
        startPos = endPos;
    }

    void ClearRT()
    {
        // 删除创建出来的RenderTexture
        if (_rtList != null)
        {
            for (int i = 0; i < _rtList.Count; i++)
            {
                _rtList[i].Release();
            }

            _rtList = null;
        }
    }

    void ClearMaterial()
    {
        if (_grayMaterialList == null) return;
        for (int i = 0; i < _grayMaterialList.Count; i++)
        {
            Object.DestroyImmediate(_grayMaterialList[i], true);
        }

        _grayMaterialList = null;
    }

    public virtual void BindUI()
    {
    }

    public virtual void RegisterEvents() { }

    public virtual void UnRegisterEvents() { }

    public virtual void onCreate()
    {
        RegisterEvents();
        root = transform.Find("Root");
        scaleRoot = transform.Find("Scale");
        DoScale();
    }

    public virtual void onShow(object[] objs)
    {
        _isShow = true;

        if (UIViews != null)
        {
            for (int i = 0; i < UIViews.Count; i++)
            {
                UIViews.ValueList[i].Show(objs);
            }
        }

        if (UIExtraArgsViews != null)
        {
            for (int i = 0; i < UIExtraArgsViews.Count; i++)
            {
                var key = UIExtraArgsViews.KeyList[i];
                if (AddViewArgsDict.TryGetValue(key, out var extraArgs))
                {
                    UIExtraArgsViews.ValueList[i].Show(extraArgs);
                }
                else
                {
                    UIExtraArgsViews.ValueList[i].Show(null);
                }
            }
        }
    }

    public virtual void onUpdate()
    {
        if (UIViews != null)
        {
            for (int i = 0; i < UIViews.Count; i++)
            {
                UIViews.ValueList[i].onUpdate();
            }
        }

        if (UIExtraArgsViews != null)
        {
            for (int i = 0; i < UIExtraArgsViews.Count; i++)
            {
                UIExtraArgsViews.ValueList[i].onUpdate();
            }
        }
    }

    public virtual void onHide()
    {
        _isShow = false;
        ClearRT();
        HandleHideClear();
    }

    public virtual void OnDestroy()
    {
        UnRegisterEvents();
        if (UIViews != null)
        {
            for (int i = 0; i < UIViews.Count; i++)
            {
                UIViews.ValueList[i].OnDestroy();
            }
            UIViews.Clear();
            UIViews = null;
        }

        if (UIExtraArgsViews != null)
        {
            for (int i = 0; i < UIExtraArgsViews.Count; i++)
            {
                UIExtraArgsViews.ValueList[i].OnDestroy();
            }
            UIExtraArgsViews.Clear();
            UIExtraArgsViews = null;
        }
        // ClearMaterial();
        ClearTweenerList();
        
        HandleDestroyClear();
        ClearCloseDict();
        
        AddViewArgsDict.Clear();
    }

    private void ClearTweenerList()
    {
        if (mTweenList.Count != 0)
        {
            for (int i = 0; i < mTweenList.Count; i++)
            {
                if (mTweenList[i] != null)
                {
                    mTweenList[i].Kill();
                }
            }
            
            mTweenList.Clear();
        }
    }

    public void DoScale()
    {
        if (scaleRoot == null) return;
        // if (root == null) return;
        IsOpenScaleOver = false;
        scaleRoot.localScale = new Vector3(0.01f, 0.01f, 1f);
        Sequence s = DOTween.Sequence();
        s.Append(scaleRoot.DOScale(new Vector3(1.15f, 1.15f, 1f), 0.15f));
        s.Append(scaleRoot.DOScale(new Vector3(1f, 1f, 1f), 0.05f));
        s.SetUpdate(true);
        s.onComplete = () =>
        {
            IsOpenScaleOver = true;
        };
        // // sequence.Insert(1, transform.DOScale(new Vector3(1.15f, 1.15f, 1f), 0.05f));
        // // sequence.Insert(2, transform.DOScale(new Vector3(1f, 1f, 1f), 0.05f));
        // //

    }

    public void DoScaleReverse(Action<string> callBack)
    {
        if (scaleRoot == null)
            return;
        Sequence s = DOTween.Sequence();
        s.Append(scaleRoot.transform.DOScale(new Vector3(1.15f, 1.15f, 1f), 0.05f));
        s.Append(scaleRoot.transform.DOScale(new Vector3(0.01f, 0.01f, 1f), 0.15f));
        s.SetUpdate(true);

        s.onComplete = () =>
        {
            callBack?.Invoke(gameObject.name);
        };

        AddTween(s);
    }
    
    /// <summary>
    /// 此接口必须在onCreate里面调用，后面会自动调用onShow
    /// 此接口必须在onCreate里面调用，后面会自动调用onShow
    /// 此接口必须在onCreate里面调用，后面会自动调用onShow
    /// </summary>
    /// <param name="viewName"></param>
    /// <param name="root"></param>
    /// <param name="objs"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public T AddView<T>(string viewName, Transform root, object[] objs) where T : UIViewBase
    {
        return AddView(viewName, root, objs) as T;
    }

    /// <summary>
    /// 此接口必须在onCreate里面调用，后面会自动调用onShow
    /// 此接口必须在onCreate里面调用，后面会自动调用onShow
    /// 此接口必须在onCreate里面调用，后面会自动调用onShow
    /// </summary>
    /// <param name="viewName"></param>
    /// <param name="root"></param>
    /// <param name="objs"></param>
    /// <returns></returns>
    public UIViewBase AddView(string viewName, Transform root, object[] objs)
    {
        if (AddViewArgsDict.TryGetValue(viewName, out var existObjs))
        {
            AddViewArgsDict[viewName] = existObjs;
        }
        else
        {
            AddViewArgsDict.Add(viewName, objs);
        }
        return UIManager.Inst.AddExtraArgsViewSync(viewName, root, this, objs);
    }

    public void AddTween(Tween tween)
    {
        mTweenList.Add(tween);
    }

    public void HandleHideClear()
    {
        if (_closeClearDict.TryGetValue(EUIInitType.Show, out var list))
        {
            if (list == null)
                return;

            for (int i = 0; i < list.Count; i++)
            {
                list[i].ClearOnClose();
            }
            
            list.Clear();
        }
    }

    public void HandleDestroyClear()
    {
        if (_closeClearDict.TryGetValue(EUIInitType.Create, out var list))
        {
            if (list == null)
                return;

            for (int i = 0; i < list.Count; i++)
            {
                list[i].ClearOnClose();
            }
            
            list.Clear();
        }
    }

    public void ClearCloseDict()
    {
        _closeClearDict.Clear();
    }

    public void New()
    {
    }

    public void Free()
    {
    }
}
