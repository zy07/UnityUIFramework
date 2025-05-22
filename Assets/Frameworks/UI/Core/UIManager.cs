using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using CommonLogic;
using Framework;
using Hotfix;
using UnityEngine;
using Object = UnityEngine.Object;

public interface IBindAEntity
{
    public void SetEntityId(int entityId);
}

public class UIManager : Singleton<UIManager>
{
    public UIRoot root
    {
        get { return _root; }
    }

    private const int ORDER_STEP = 10;

    private List<Layer> _layers = new List<Layer>();
    private Dictionary<string, UILayerId> _name2layerDic = new Dictionary<string, UILayerId>();


    /// <summary>
    /// 保存UI的链接关系，使用UIMgr统一管理
    /// </summary>
    private StackList<string> uiStack = new StackList<string>();//只有Panel和Pop需要通过栈管理

    public string GetShowFrontUI()
    {
        if (uiStack.Count > 0)
        {
            return uiStack.Peek();
        }

        return "";
    }

    private List<string> _opendUI = new List<string>();

    private UIRoot _root;

    // UI名和对应的构造函数字典
    public Dictionary<string, ConstructorInfo> UIHandlerDic = new Dictionary<string, ConstructorInfo>();

    public RectTransform GetLayerRoot(UILayerId uiLayerId)
    {
        foreach (var layer in _layers)
        {
            if (layer.id == uiLayerId)
            {
                return layer.root.GetComponent<RectTransform>();
            }
        }

        return null;
    }

    public void Register()
    {
        var rootObj = GameObject.Find("UIRoot");


        var rootTrans = rootObj.transform;
        rootTrans.localScale = Vector3.one;
        rootTrans.localPosition = new Vector3(0, 10000, 0);
        rootTrans.localRotation = Quaternion.identity;

        _root = new UIRoot();
        _root.gameObject = rootObj;
        _root.Init();

        // _root = rootObj.AddComponent<UIRoot>();


        _layers.Add(new Layer() { id = UILayerId.Hud, order = 0 });
        _layers.Add(new Layer() { id = UILayerId.Panel, order = 10000 });
        _layers.Add(new Layer() { id = UILayerId.Pop, order = 20000 });
        _layers.Add(new Layer() { id = UILayerId.Top, order = 25000 });
        _layers.Add(new Layer() { id = UILayerId.OverTop, order = 26000 });
        _layers.Add(new Layer() { id = UILayerId.Fix, order = 30000 });

        for (int i = 0, len = _layers.Count; i < len; i++)
        {
            var layer = _layers[i];
            var go = new GameObject("layer-" + layer.id.ToString());
            var trans = go.AddComponent<RectTransform>();
            trans.SetParent(_root.canvas.transform, false);
            trans.localScale = Vector3.one;
            trans.localPosition = Vector3.zero;
            trans.localRotation = Quaternion.identity;

            trans.anchoredPosition = Vector2.zero;
            trans.anchorMin = Vector2.zero;
            trans.anchorMax = Vector2.one;
            trans.sizeDelta = Vector2.zero;

            layer.root = go;
        }

        CollectUI();
    }
    #region 初始化

    void CollectUI()
    {
        Assembly[] assemblies = System.AppDomain.CurrentDomain.GetAssemblies();
        for (int i = 0; i < assemblies.Length; i++)
        {
            // Type[] types = Assembly.GetExecutingAssembly().GetTypes();
            Type[] types = assemblies[i].GetTypes();
            Type[] array = types;
            foreach (Type type in array)
            {
                object[] customAttributes = type.GetCustomAttributes(typeof(UIBaseHandlerAttribute), inherit: false);
                for (int j = 0; j < customAttributes.Length; j++)
                {
                    if (typeof(UIBase).IsAssignableFrom(type))
                    {
                        UIBaseHandlerAttribute attr = customAttributes[j] as UIBaseHandlerAttribute;
                        ConstructorInfo constructor = type.GetConstructor(Type.EmptyTypes);
                        ReflectionRegisterDic(attr, constructor);
                    }
                    else
                    {
                        KaLog.LogError($"{type} is an invalid UILogicHandler");
                    }
                }
            }
        }
    }

    void ReflectionRegisterDic(UIBaseHandlerAttribute attr, ConstructorInfo ci)
    {
        if (UIHandlerDic.TryGetValue(attr.PrefabName, out ConstructorInfo oldCi))
        {
            KaLog.LogError($"Prefab:{attr.PrefabName} already has a handler {oldCi.DeclaringType}!");
        }
        else
        {
            UIHandlerDic.Add(attr.PrefabName, ci);
        }
    }

    void DebugUIHandlerDic()
    {
        foreach (var uici in UIHandlerDic)
        {
            Debug.Log($"【UIPrefabName】 UIPrefabName is {uici.Key}");
        }
    }

    #endregion

    public void Dispose()
    {
        for (int i = 0; i < _layers.Count; i++)
        {
            GameObject.Destroy(_layers[i].root);
        }

        _layers.Clear();
        _name2layerDic.Clear();
        uiStack.Clear();
        _opendUI.Clear();
        UIHandlerDic.Clear();
    }


    private Layer GetLayer(UILayerId id)
    {
        return _layers[(int)id];
    }


    public T AddView<T>(Transform parentTrans, string name) where T : UIViewBase
    {
        if (parentTrans == null)
        {
            KaLog.LogError("add view without parent");
            return null;
        }

        GameObject obj = new GameObject("res_" + name);
        var prefab = ResourcesMgr.Inst.LoadGameObject(name, obj.transform);
        if (prefab == null)
        {
            KaLog.LogError("invalid ui prefab: " + name);
            return null;
        }

        var go = Object.Instantiate(prefab);
        obj.transform.SetParent(go.transform);
        if (UIHandlerDic.TryGetValue(name, out ConstructorInfo ci))
        {
            T t = ci?.Invoke(null) as T;
            if (t == null)
            {
                KaLog.LogError("View = {0} is not int UIHandler", name);
            }

            t.gameObject = go;
            var trans = go.GetComponent<RectTransform>() ?? go.AddComponent<RectTransform>();
            trans.SetParent(parentTrans, false);
            trans.localScale = Vector3.one;
            trans.localPosition = Vector3.zero;
            trans.localRotation = Quaternion.identity;

            trans.anchoredPosition = Vector2.zero;
            trans.anchorMin = Vector2.zero;
            trans.anchorMax = Vector2.one;
            trans.sizeDelta = Vector2.zero;
            trans.pivot = new Vector2(0.5f, 0.5f);
            //ScreenAdaptationHelper.UIPanelAdaptation(go);
            t.BindUI();
            t.onCreate();
            t.onShow(null);

            return t;
        }
        else
        {
            return null;
        }
    }

    public void SetLayer(UIPanelBase ui)
    {
        var layer = GetLayer(ui.layerId);
        var go = ui.gameObject;

        var curCount = layer.list.Count;
        var last = curCount == 0 ? null : layer.list[curCount - 1];

        var canvas = ui.canvas;
        canvas.overrideSorting = true;
        ui.SortingOrder = last == null ? layer.order : (last.SortingOrder + ORDER_STEP);
        ui.SetSortingOrder();
        // canvas.sortingOrder = last == null ? layer.order : (last.SortingOrder + ORDER_STEP);
        layer.list.Add(ui);
    }

    public UIPanelBase Add(UIPanelBase ui)
    {
        var layer = GetLayer(ui.layerId);

        var go = ui.gameObject;
        var trans = go.GetComponent<RectTransform>() ?? go.AddComponent<RectTransform>();
        trans.SetParent(layer.root.transform, false);
        trans.localScale = Vector3.one;
        trans.localPosition = Vector3.zero;
        trans.localRotation = Quaternion.identity;

        trans.anchoredPosition = Vector2.zero;
        trans.anchorMin = Vector2.zero;
        trans.anchorMax = Vector2.one;
        trans.sizeDelta = Vector2.zero;
        trans.pivot = new Vector2(0.5f, 0.5f);

        //ScreenAdaptationHelper.UIPanelAdaptation(go);

        return ui;
    }

    public void Remove(UIPanelBase ui,bool hideAll)
    {
        var layer = GetLayer(ui.layerId);
        layer.list.Remove(ui);

        HidePopup(ui);
        
        CheckNeedOpenLastUI(ui);
        ui.gameObject.SetActive(false);
        _opendUI.Remove(ui.UIName);
    }

    public void HideByNextOpen(string uiName, UIPanelBase open)
    {
        // 打开弹窗不需要隐藏，打开界面就需要把之前的UI都隐藏了。
        
        // 如果要打开的UI不是Panel，就不关心了
        if (open.layerId != UILayerId.Panel)
            return;

        for (int i = 0; i < uiStack.DataList.Count; i++)
        {
            if (uiStack.DataList[i] == open.UIName)
                continue;

            var stackUIName = uiStack.DataList[i];
            var stackUI = Find(stackUIName);
            if (stackUI == null)
            {
                KaLog.LogWarning($"发现异常：UIManager.HideByNextOpen({uiName}) stackUI == null,  i={i}, stackUIName={stackUIName}, uiStack.DataList.Count={uiStack.DataList.Count}");
                continue;
            }
            if(stackUI.isShow)
                stackUI.HideByOpen();
        }
    }

    public UIPanelBase Find(string name)
    {
        UILayerId id = UILayerId.Hud;
        if (!_name2layerDic.TryGetValue(name, out id))
            return null;
        var layer = _layers[(int)id];
        return layer.Find(name);
    }

    public bool IsPanelActive(string name)
    {
        if (_opendUI.Contains(name))
        {
            return true;
        }
        return false;
    }


    public T Find<T>(string name) where T : UIPanelBase
    {
        return Find(name) as T;
    }

    #region HUD

    //public T AddHUDSync<T>(string name, Vector3 worldPos, int entityId) where T : UIPanelBase, IBindAEntity
    //{
    //    var prefab = AssetManager.Instance.StartLoadGameObject(_root.gameObject, name).resGameObject;

    //    T ret = PrepareAddHUDSync<T>(name);

    //    // var ret = ui as T;
    //    ret?.SetEntityId(entityId);

    //    ret.Show(name);

    //    var layer = GetLayer(ret.layerId);
    //    var trans = ret.gameObject.GetComponent<RectTransform>() ?? ret.gameObject.AddComponent<RectTransform>();
    //    trans.SetParent(layer.root.transform, false);
    //    trans.localScale = Vector3.one;
    //    trans.localPosition = Vector3.zero;
    //    trans.localRotation = Quaternion.identity;

    //    trans.anchoredPosition = Vector2.zero;
    //    trans.anchorMin = Vector2.zero;
    //    trans.anchorMax = Vector2.one;
    //    trans.sizeDelta = Vector2.zero;

    //    Camera cam = GameGlobal.CameraManager.CurrentCamera;
    //    Vector3 screenPos = cam.WorldToScreenPoint(worldPos);

    //    var uiCame = root.camera;
    //    Vector3 uiPos = uiCame.ScreenToWorldPoint(screenPos);

    //    trans.position = uiPos;


    //    ((UIBase)ret).Initialize();
    //    return ret;
    //}

    //T PrepareAddHUDSync<T>(string name) where T : UIPanelBase, IBindAEntity
    //{
    //    if (UIHandlerDic.TryGetValue(name, out ConstructorInfo ci))
    //    {
    //        UIPanelBase uiPanelBase = (UIPanelBase)ci?.Invoke(null);
    //        if (uiPanelBase == null)
    //        {
    //            Log.Error("[UIManager] UIPanelBase reflection failed!");
    //            return null;
    //        }
    //        uiPanelBase.InitializeParams();

    //        uiPanelBase.Prepare();

    //        var prefab = AssetManager.Instance.StartLoadGameObject(root.gameObject,name).resGameObject;

    //        var go = Object.Instantiate(prefab);

    //        var layer = GetLayer(uiPanelBase.layerId);
    //        go.name = name;
    //        uiPanelBase.gameObject = go;
    //        _name2layerDic[name] = uiPanelBase.layerId;

    //        HandleUINeedBackToLastUI(uiPanelBase);
    //        uiPanelBase.BindUI();

    //        ConstructViewBaseSync(uiPanelBase);

    //        uiPanelBase.onCreate();

    //        return uiPanelBase as T;
    //    }
    //    else
    //    {
    //        Log.Error("[UIManager] UI Name : {0} has no Attribute!", name);
    //        return null;
    //    }
    //}

    //public void UpdateHUD(UIPanelBase hud, Vector3 worldPos)
    //{
    //    Camera cam = GameGlobal.CameraManager.CurrentCamera;
    //    Vector3 screenPos = cam.WorldToScreenPoint(worldPos);

    //    var uiCame = root.camera;
    //    Vector3 uiPos = uiCame.ScreenToWorldPoint(screenPos);

    //    var trans = hud.gameObject.GetComponent<RectTransform>();
    //    trans.position = uiPos;
    //}

    #endregion

    public T ShowSync<T>(string name) where T : UIPanelBase
    {
        return ShowSync(name) as T;
    }

    public void Show(string name, object[] args = null)
    {
        if (uiStack.Count > 0)
        {
            if (uiStack.Peek() == name)
            {
                // 2024年6月3日14:51:06 版本上线前改动
                // 主要是为了跳转，跳转过程中会给一些参数
                // 但是当前界面正在打开状态，所以需要重新onshow
                var peekUI = Find(name);
                if (peekUI != null && !peekUI.isShow)
                {
                    peekUI.onShow(args);
                }
                return;
            }
        }
        
        var cur = Find(name);
        
        if (cur != null)
        {
            HandleStackByReopenUI(name, cur, args);
            HideByNextOpen(name, cur);
            
            //HandleUINeedBackToLastUI(cur);
            // if (!cur.isShow)
            // {
            //     cur.Show(name, args);
            // }
            // else
            // {
            //     cur.Refresh(args);
            // }

            return;
        }

        if (UIHandlerDic.TryGetValue(name, out ConstructorInfo ci))
        {
            cur = (UIPanelBase)ci?.Invoke(null);
            if (cur == null)
            {
                KaLog.LogError("[UIManager] UIPanelBase reflection failed!");
                return;
            }

            cur.InitializeParams();
            _opendUI.Add(name);
            
            HideByNextOpen(name, cur);
            Push2Stack(name, cur);
            TaskManager.Inst.StartTask(PrepareShowUI(name, cur, args));
        }
        else
        {
            KaLog.LogError("[UIManager] UI Name : {0} has no Attribute!", name);
            return;
        }
        
    }

    /// <summary>
    /// 同步打开UI，慎用，常用的是异步 ↑
    /// </summary>
    public UIPanelBase ShowSync(string name, object[] args = null)
    {
        var cur = Find(name);
        if (cur != null)
        {
            if (uiStack.Count > 0)
            {
                if (uiStack.Peek() == name)
                {
                    var peekUI = Find(name);
                    if (peekUI != null && !peekUI.isShow)
                    {
                        peekUI.onShow(args);
                    }
                    return cur;
                }
            }
            
            HideByNextOpen(name, cur);
            HandleStackByReopenUI(name, cur, args);
            
            //HandleUINeedBackToLastUI(cur);
            // if (!cur.isShow)
            //     cur.Show(name, args);
            return cur;
        }

        if (UIHandlerDic.TryGetValue(name, out ConstructorInfo ci))
        {
            cur = (UIPanelBase)ci?.Invoke(null);
            if (cur == null)
            {
                KaLog.LogError("[UIManager] UIPanelBase reflection failed!");
                return null;
            }

            cur.InitializeParams();
            _opendUI.Add(name);
            HideByNextOpen(name, cur);
            
            PrepareShowUISync(name, cur, args);
            Push2Stack(name, cur);
            return cur;
        }
        else
        {
            KaLog.LogError("[UIManager] UI Name : {0} has no Attribute!", name);
            return null;
        }
        
    }

    IEnumerator PrepareShowUI(string name, UIPanelBase uiPanelBase, object[] args)
    {
        yield return uiPanelBase.Prepare();

        GameObject obj = new GameObject("res_" + name);

        var prefab = ResourcesMgr.Inst.LoadGameObject(name, obj.transform);

        var go = Object.Instantiate(prefab);
        obj.transform.SetParent(go.transform);

        var layer = GetLayer(uiPanelBase.layerId);
        go.name = name;
        uiPanelBase.gameObject = go;
        uiPanelBase.uiViewName = name;
        _name2layerDic[name] = uiPanelBase.layerId;
        uiPanelBase.Initialize();


        HandleUINeedBackToLastUI(uiPanelBase);
        uiPanelBase.BindUI();

        yield return ConstructViewBase(uiPanelBase);

        Add(uiPanelBase);

        SetLayer(uiPanelBase);
        uiPanelBase._viewData = args;
        uiPanelBase.onCreate();
        uiPanelBase.Show(name, args);
    }

    void PrepareShowUISync(string name, UIPanelBase uiPanelBase, object[] args = null)
    {
        uiPanelBase.Prepare();
        GameObject obj = new GameObject("res_" + name);
        var prefab = ResourcesMgr.Inst.LoadGameObject(name, obj.transform);


        var go = Object.Instantiate(prefab);
        obj.transform.SetParent(go.transform);

        var layer = GetLayer(uiPanelBase.layerId);
        go.name = name;
        uiPanelBase.gameObject = go;
        uiPanelBase.uiViewName = name;
        uiPanelBase.Initialize();
        _name2layerDic[name] = uiPanelBase.layerId;


        HandleUINeedBackToLastUI(uiPanelBase);
        uiPanelBase.BindUI();

        ConstructViewBaseSync(uiPanelBase);

        Add(uiPanelBase);

        SetLayer(uiPanelBase);

        uiPanelBase.onCreate();
        uiPanelBase.Show(name, args);
    }

    void ConstructViewBaseSync(UIBase uiBase)
    {
        if (uiBase.ViewNameList != null)
        {
            for (int i = 0; i < uiBase.ViewNameList.Count; i++)
            {
                var viewName = uiBase.ViewNameList[i];
                if (UIHandlerDic.TryGetValue(viewName, out ConstructorInfo ci))
                {
                    UIViewBase viewBase = (UIViewBase)ci?.Invoke(null);
                    if (viewBase == null)
                    {
                        KaLog.LogError("[UIManager] UIBase -> {0}, has no attribute", viewName);
                        continue;
                    }
                    viewBase.Parent = uiBase;

                    viewBase.uiViewName = viewName;
                    viewBase.InitializeParams();
                    viewBase.Prepare();
                    BindViewBase2Obj(uiBase.transform, viewName, viewBase);
                    if (viewBase.gameObject == null)
                    {
                        KaLog.LogError("[UIManager] UIName UIBase -> {0}, has no prefab -> {1}", uiBase.gameObject.name, viewName);
                    }
                    else
                    {
                        uiBase.UIViews ??= new DictionaryList<string, UIViewBase>();
                        uiBase.UIViews.Add(viewName, viewBase);

                        viewBase.BindUI();
                        viewBase.onCreate();
                    }

                    ConstructViewBaseSync(viewBase);
                }
            }
        }
    }

    /// <summary>
    /// 递归加载View
    /// 绑定View2Obj
    /// </summary>
    /// <param name="uiBase">view名字</param>
    /// <returns></returns>
    private IEnumerator ConstructViewBase(UIBase uiBase)
    {
        if (uiBase.ViewNameList != null)
        {
            for (int i = 0; i < uiBase.ViewNameList.Count; i++)
            {
                var viewName = uiBase.ViewNameList[i];
                if (UIHandlerDic.TryGetValue(viewName, out ConstructorInfo ci))
                {
                    UIViewBase viewBase = (UIViewBase)ci?.Invoke(null);
                    viewBase.Parent = uiBase;
                    viewBase.uiViewName = viewName;
                    viewBase.InitializeParams();
                    yield return viewBase.Prepare();
                    BindViewBase2Obj(uiBase.transform, viewName, viewBase);
                    if (viewBase.gameObject == null)
                    {
                        KaLog.LogError("[UIManager] UIName UIBase -> {0}, has no prefab -> {1}", uiBase.gameObject.name, viewName);
                    }
                    else
                    {
                        uiBase.UIViews ??= new DictionaryList<string, UIViewBase>();
                        uiBase.UIViews.Add(viewName, viewBase);

                        viewBase.BindUI();
                        viewBase.onCreate();
                    }

                    yield return ConstructViewBase(viewBase);
                }
            }
        }
    }

    public IEnumerator AddView(string name, Transform root, UIBase uiBase, object[] objs, Action onAddView = null)
    {
        if (UIHandlerDic.TryGetValue(name, out ConstructorInfo ci))
        {
            UIViewBase viewBase = (UIViewBase)ci?.Invoke(null);
            viewBase.Parent = uiBase;
            viewBase.uiViewName = name;
            viewBase.InitializeParams();
            yield return viewBase.Prepare();

            var obj = ResourcesMgr.Inst.LoadGameObject(name, uiBase.transform);

            if (obj == null)
            {
                KaLog.LogError("ui name:{0} is null", name);
                yield break;
            }
            else
            {
                GameObject go = GameObject.Instantiate(obj);
                viewBase.gameObject = go;
                go.transform.SetParent(root);
                go.transform.localPosition = Vector3.zero;
                go.transform.rotation = Quaternion.identity;
                go.transform.localScale = Vector3.one;
                uiBase.UIViews ??= new DictionaryList<string, UIViewBase>();
                uiBase.UIViews.Add(name, viewBase);
                onAddView?.Invoke();

                viewBase.BindUI();
                viewBase.onCreate();
                viewBase.onShow(objs);
            }

            yield return ConstructViewBase(viewBase);
        }
    }


    public UIViewBase AddExtraArgsViewSync(string name, Transform root, UIBase uiBase, object[] objs)
    {
        if (UIHandlerDic.TryGetValue(name, out ConstructorInfo ci))
        {
            UIViewBase viewBase = (UIViewBase)ci?.Invoke(null);
            viewBase.Parent = uiBase;
            viewBase.uiViewName = name;
            viewBase.InitializeParams();
            viewBase.Prepare();

            var obj = ResourcesMgr.Inst.LoadGameObject($"v_{name}", uiBase.transform);

            if (obj == null)
            {
                KaLog.LogError("ui name:{0} is null", name);
                return null;
            }
            else
            {
                GameObject go = GameObject.Instantiate(obj);
                viewBase.gameObject = go;
                go.transform.SetParent(root);
                go.transform.localPosition = Vector3.zero;
                go.transform.rotation = Quaternion.identity;
                go.transform.localScale = Vector3.one;

                uiBase.UIExtraArgsViews ??= new DictionaryList<string, UIViewBase>();
                uiBase.UIExtraArgsViews.Add(name, viewBase);

                viewBase.BindUI();
                viewBase.onCreate();
            }

            ConstructViewBaseSync(viewBase);

            return viewBase;
        }

        return null;
    }

    private void BindViewBase2Obj(Transform parent, string viewName, UIViewBase viewBase)
    {
        string fixedViewName = $"v_{viewName}";

        for (int i = 0; i < parent.childCount; i++)
        {
            if (parent.GetChild(i).name == fixedViewName)
            {
                viewBase.gameObject = parent.GetChild(i).gameObject;
                return;
            }
            else
            {
                BindViewBase2Obj(parent.GetChild(i), viewName, viewBase);
            }
        }
    }

    private void Push2Stack(string uiName, UIPanelBase ui)
    {
        var uiLayerId = ui.layerId;
        
        // 只有Panel和Popup需要通过栈管理，别的不需要，比如飞金币这种功能性的UI、新手引导这种功能性的UI，都不需要
        if (uiLayerId != UILayerId.Panel && uiLayerId != UILayerId.Pop)
            return;
        
        uiStack.Push(uiName);
    }

    private void HandleStackByReopenUI(string uiName, UIPanelBase uiPanel, object[] args)
    {
        var stackList = uiStack.DataList;
        var layerId = uiPanel.layerId;
        
        bool find = false;

        for (int i = 0; i < stackList.Count; i++)
        {
            if (stackList[i] == uiPanel.gameObject.name)
            {
                find = true;
                break;
            }
        }

        StackList<string> tempStack = new StackList<string>();
        
        // 如果找到了目标UI，根据LayerId
        // 1. 更改层级到当前相同LayerId的栈最高的UI层级
        // 2. 其他相同LayerId的层级都减去对应的LAYER_STEP
        // 3. 通过第二个临时stack整理当前stack
        if (find)
        {
            HandleStackInternal(layerId, uiName, tempStack, 0, args);
        }
        else
        {
            if(!uiPanel.isShow)
                uiPanel.Show(uiName, args);
        }
    }

    private void HandleStackInternal(UILayerId layerId, string uiName, StackList<string> stack, int sortingOrder, object[] args)
    {
        if (uiStack.Peek() == uiName)
        {
            // 找到了
            // 把其他塞回去
            var curPanel = uiStack.Pop();

            while (stack.Count > 0)
            {
                var temp = stack.Pop();
                uiStack.Push(temp);
            }
            
            var uiPanel = Find(uiName);
            uiPanel.SpecifySortingOrder(sortingOrder);  
            if(!uiPanel.isShow)
                uiPanel.Show(uiName, args);
            
            uiStack.Push(curPanel);
            return;
        }
        
        // 异常情况
        if (uiStack.Count <= 0)
        {
            while (stack.Count > 0)
            {
                var temp = stack.Pop();
                uiStack.Push(temp);
            }
            KaLog.LogError("重新打开UI，没找到？wtm");
            return;
        }

        // 开始找
        var pop = uiStack.Pop();
        var popPanel = Find(pop);
        if (popPanel != null)
        {
            if (popPanel.layerId == layerId)
            {
                if (popPanel.SortingOrder > sortingOrder)
                    sortingOrder = popPanel.SortingOrder;
            
                // 相同LayerId的，需要-当前step，重新指定所有SortingOrder
                popPanel.SpecifySortingOrder(popPanel.SortingOrder - ORDER_STEP);
            }
        }
        else
        {
            KaLog.LogWarning($"【分析情况】这里跳转的过程，丢失了一个界面：{pop}，栈内还有，资源没了。");
        }
        
        stack.Push(pop);

        HandleStackInternal(layerId, uiName, stack, sortingOrder, args);
    }

    private void HandleUINeedBackToLastUI(UIPanelBase ui)
    {
        // if (!ui.NeedControl)
        //     return;

        // if (ui.NeedBackToLastUI)
        // {
        //     // 检查Stack中是否已经打开过此UI
        //
        //     if (uiStack.Count != 0)
        //     {
        //         UIPanelBase lastUi = Find<UIPanelBase>(uiStack.Peek());
        //         if (lastUi != null)
        //             lastUi.HideByOpen();
        //     }
        //     // uiStack.Push(ui.gameObject.name);
        //     // 这里暂时不处理，因为目前没有A打开BCD再打开A的情况，框架中不允许
        //     // else if (uiStack.Peek() != ui.name) // 如果Stack中打开过并且不是最上层的UI，就重新规划一下顺序
        //     // {
        //     //     uiStack.RemoveData(ui.name);
        //     //     uiStack.Push(ui.name);
        //     // }
        // }
        //
        // if (!uiStack.Contains(ui.gameObject.name))
        // {
        //     uiStack.Push(ui.gameObject.name);
        // }
    }

    public void Hide(string name)
    {
        var cur = Find(name);
        if (cur != null)
        {
            cur.Hide();

            CheckNeedOpenLastUI(cur);
        }

        // GuideManager.Instance
        //     .TriggerGuide(ETriggerType.ClosePanel, new object[] { name });
    }

    private void HidePopup(UIPanelBase ui)
    {
        if (uiStack.Count <= 0)
            return;
        
        if (ui.layerId == UILayerId.Panel || ui.layerId == UILayerId.Pop)
        {
            if (ui.UIName == uiStack.Peek())
            {
                uiStack.Pop();
            }
            else // 有些地方先打开了UI，再关闭的UI，需要检查一下
            {
                if (uiStack.DataList.Contains(ui.UIName))
                {
                    StackList<string> tempStack = new StackList<string>();
                    PopupPanelNotPeek(tempStack, ui);
                }
            }
        }
    }

    private void PopupPanelNotPeek(StackList<string> stack, UIPanelBase ui)
    {
        if (uiStack.Count <= 0)
            return;
        
        // 找到了
        if (uiStack.Peek() == ui.UIName)
        {
            uiStack.Pop();
            
            while (stack.Count > 0)
            {
                var temp = stack.Pop();
                uiStack.Push(temp);
            }
            
            return;
        }
        
        // 异常情况
        if (uiStack.Count <= 0)
        {
            while (stack.Count > 0)
            {
                var temp = stack.Pop();
                uiStack.Push(temp);
            }
            KaLog.LogError("关闭UI，没找到？wtm");
            return;
        }

        var uiName = uiStack.Pop();
        stack.Push(uiName);
        PopupPanelNotPeek(stack, ui);
    }

    public bool HaveNeedOpenLastUI(UIPanelBase curHideBase)
    {
        if (uiStack.Count <= 0)
            return false;
        
        for (int i = uiStack.DataList.Count - 1; i >= 0; i--)
        {
            var uiName = uiStack.DataList[i];
            var ui = Find(uiName);
            if (ui == null)
            {
                continue;
            }
            if (ui.layerId == UILayerId.Panel)
            {
                if(!ui.isShow)
                    return true;
            }
        }
        return false;
    }

    public void CheckNeedOpenLastUI(UIPanelBase curHideBase)
    {
        if (uiStack.Count <= 0)
            return;

        if (curHideBase.layerId != UILayerId.Panel && curHideBase.layerId != UILayerId.Pop)
            return;
        
        // 如果下一个UI的LayerId 是 panel 就可以正常显示，
        // 如果不是，就直到找到LayerId是Panel的，显示，如果一个没找到就拉倒吧。
        // 如果没找到期间，有Popup类型的，也一起显示出来。
        for (int i = uiStack.DataList.Count - 1; i >= 0; i--)
        {
            var uiName = uiStack.DataList[i];
            var ui = Find(uiName);
            if (ui == null)
            {
                KaLog.LogWarning($"UIManager.CheckNeedOpenLastUI ui == null, uiName={uiName}");
                continue;
            }
            if (ui.layerId == UILayerId.Panel && curHideBase.layerId != UILayerId.Pop)
            {
                if(!ui.isShow)
                    ui.Show(uiName);
                break;
            }
            
            if (ui.layerId == UILayerId.Pop)
            {
                if(!ui.isShow)
                    ui.Show(uiName);
            }
        }
    }

    public void HideAll(List<string> ignoreUIPanel = null)
    {
        ClearUIStack(ignoreUIPanel);
        for (int i = 0; i < _layers.Count - 1; i++)
        {
            var layer = GetLayer(_layers[i].id);
            for (int j = layer.list.Count - 1; j >= 0; j--)
            {
                var ui = layer.list[j];
                if (ignoreUIPanel != null && ignoreUIPanel.Contains(ui.UIName))
                    continue;
                ui.Hide(true);
                //ui.OnDestroy();
            }
        }


    }
    
    public void HideAllLayer(UILayerId uiLayerId)
    {
        for (int i = 0; i < _layers.Count - 1; i++)
        {
            var layer = GetLayer(_layers[i].id);
            if (layer.id == uiLayerId)
            {
                for (int j = layer.list.Count - 1; j >= 0; j--)
                {
                    var ui = layer.list[j];
                    ui.Hide(true);
                }

                break;
            }
        }


    }

    /// <summary>
    /// 切换场景/主动释放
    /// </summary>
    public void ClearUIStack(List<string> ignoreUIPanel = null)
    {
        if (ignoreUIPanel == null)
        {
            // uiStack.Clear();
        }
        else
        {
            // List<string> tempStrList = new List<string>();
            // string popUIStr = uiStack.Pop();
            // while (!string.IsNullOrEmpty(popUIStr))
            // {
            //     if (ignoreUIPanel.Contains(popUIStr))
            //     {
            //         tempStrList.Add(popUIStr);
            //     }
            //     popUIStr = uiStack.Pop();
            // }
            // tempStrList.Reverse();
            // foreach (var uiStr in tempStrList)
            // {
            //     uiStack.Push(uiStr);
            // }
        }
    }

    public void Clear()
    {
        uiStack.Clear();
    }

    // public void ShowTips(string content)
    // {
    //     UITips uiTips = Find("UITips") as UITips;
    //     if (uiTips == null) return;
    //     uiTips.ShowTips(content);
    // }
    //
    // public void ShowRewards(List<RewardItemData> dataList, string title = "REWARD LIST",
    //     string clickTips = "Click To Collect")
    // {
    //     UIGetRewardPanel uiGetRewardPanel = Find<UIGetRewardPanel>(UIPanelName.UIGetReward);
    //     if (uiGetRewardPanel == null) return;
    //     uiGetRewardPanel.ShowRewards(dataList, title, clickTips);
    // }
    //
    // public void ShowFlyItem(EFlyItemType type, Vector3 generatePos, float radius, int count)
    // {
    //     UIFlyItemPanel uiFlyItemPanel = Find<UIFlyItemPanel>(UIPanelName.UIFlyItem);
    //     if (uiFlyItemPanel == null) return;
    //     uiFlyItemPanel.SetData(type, generatePos, radius, count);
    // }
    //
    // public void ShowMutiFlyItem(EFlyItemType[] type, float radius, int[] count)
    // {
    //     UIFlyItemPanel uiFlyItemPanel = Find<UIFlyItemPanel>(UIPanelName.UIFlyItem);
    //     if (uiFlyItemPanel == null) return;
    //     uiFlyItemPanel.SetMutiData(type, radius, count);
    // }
    //
    // public void ShowMaskLoading(UnityAction callback)
    // {
    //     UIMaskLoadingPanel uiMaskLoadingPanel = Find<UIMaskLoadingPanel>(UIPanelName.UIMaskLoading);
    //     if (uiMaskLoadingPanel == null) return;
    //     uiMaskLoadingPanel.ShowMaskLoading(callback);
    // }
    //
    // public void CloseMaskLoading(Action onClosed = null)
    // {
    //     UIMaskLoadingPanel uiMaskLoadingPanel = Find<UIMaskLoadingPanel>(UIPanelName.UIMaskLoading);
    //     if (uiMaskLoadingPanel == null) return;
    //     uiMaskLoadingPanel.CloseMaskLoading(onClosed);
    // }
    //
    // public void ShowFlower(bool showFlower, string contenStr = "Loading", FlowerType type = FlowerType.Other)
    // {
    //     UIMaskFlowerPanel uiMaskPanel = ShowSync<UIMaskFlowerPanel>(UIPanelName.UIMaskFlower);
    //     if (uiMaskPanel == null) return;
    //     uiMaskPanel.ShowFlower(showFlower, contenStr, type);
    // }

    public bool IsPanelScaleOver(string name)
    {
        UIPanelBase panelBase = Find(name);
        if (panelBase == null) return true;
        return panelBase.CheckPanelScaleOver() || !panelBase.CheckPanelHasNeedDoScale();
    }

    private class Layer
    {
        public UILayerId id;
        public int order;
        public GameObject root;
        public List<UIPanelBase> list = new List<UIPanelBase>();

        public UIPanelBase Find(string name)
        {
            return list.Find(ui => name == ui.gameObject.name);
        }
    }


    private Dictionary<ItemBgColor, Sprite> ItemBgPool = new Dictionary<ItemBgColor, Sprite>();

    public enum ItemBgColor
    {
        Brown,
        Green,
        Blue,
        Purple,
        Red,
        Yellow,
    }

    public void Update()
    {
        for (int i = _opendUI.Count - 1; i >= 0; i--)
        {
            UIPanelBase uiPanelBase = Find<UIPanelBase>(_opendUI[i]);
            if (uiPanelBase == null)
                continue;
            uiPanelBase.onUpdate();
        }

        //for (int i = 0; i < _opendUI.Count; i++)
        //{

        //    UIPanelBase uiPanelBase = Find<UIPanelBase>(_opendUI[i]);
        //    if (uiPanelBase == null)
        //        continue;
        //    uiPanelBase.onUpdate();
        //}
    }

    #region 工具
    public bool CheckMainOnTheTop()
    {
        return CheckIsOnTheTop(UIPanelName.UIBowMain) || CheckIsOnTheTop(UIPanelName.UISLGWorld);
    }

    public bool CheckIsOnTheTop(string panelName)
    {
        if (_layers[(int)UILayerId.Pop].list.Count > 0)
        {
            if (_layers[(int)UILayerId.Pop].list.Last().UIName == panelName)
                return true;
        }

        return uiStack.Count == 0 ? false : uiStack.Peek() == panelName && !CheckIsPopping();
    }

    public bool CheckIsPopping()
    {
        bool res = false;
        if (_layers[(int)UILayerId.Pop].list.Count <= 0)
            return false;

        // 排除对话界面
        for (int i = 0; i < _layers[(int)UILayerId.Pop].list.Count; i++)
        {
            if (_layers[(int)UILayerId.Pop].list[i].UIName == UIPanelName.UIDialogueNew)
                continue;
            res = true;
        }

        return res;
    }
    public bool CheckIsShop()
    {
        if (_layers[(int)UILayerId.Panel].list.Count <= 0)
            return false;


        for (int i = 0; i < _layers[(int)UILayerId.Panel].list.Count; i++)
        {
            if (_layers[(int)UILayerId.Panel].list[i].UIName == UIPanelName.UIShop)
            {
                return true;
           

            }
        }

        return false;
    }
    
    #endregion
}