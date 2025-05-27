using Framework;
using Hotfix;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIPanelBase : UIBase
{
    public UILayerId layerId
    {
        set { _layerId = value; }
        get { return _layerId; }
    }

    public Canvas canvas { get { return _canvas; } }
    public GraphicRaycaster raycaster { get { return _raycaster; } }
    
    [SerializeField]
    private UILayerId _layerId;

    // UI是否需要回到上一级UI，默认需要返回
    // [SerializeField]
    // private bool _needBackToLastUI = true;
    //
    // public bool NeedBackToLastUI
    // {
    //     set => _needBackToLastUI = value;
    //     get => _needBackToLastUI;
    // }
    
    // public bool NeedControl = true;

    private Canvas _canvas;
    private GraphicRaycaster _raycaster;

    private string _uiName = "";
    public string UIName => _uiName;

    public object[] _viewData;

    private bool _isShowComplete;
    public bool IsShowComplete => _isShowComplete;

    private bool _isActive = false;

    private object[] mArgs = null;

    public override void Initialize()
    {
        _canvas = gameObject.AddComponent<Canvas>();
        _raycaster = gameObject.AddComponent<GraphicRaycaster>();
        base.Initialize();
    }
    
    public void Show(string uiName, object[] args = null)
    {
        _uiName = uiName;
        _isActive = true;
        _isShow = true;

        if (args == null && mArgs != null)
        {
            args = mArgs;
        }
        else
        {
            mArgs = args;
        }
        this.gameObject.SetActive(true);
        
        onShow(args);
        if (scaleRoot == null)
        {
            _isShowComplete = true;
            OnShowScaleComplete();
        }
    }
    
    public virtual void OnShowScaleComplete()
    {
        
    }

    // public void Refresh(object[] args = null)
    // {
    //     OnRefresh(args);
    // }

    private DictionaryList<string, UIViewBase> _needReverseView = null;
    
    public void Hide(bool hideAll = false)
    {
        _isActive = false;
        _isShowComplete = false;
        UIManager.Inst.Remove(this, hideAll);
        if (UIViews == null)
        {
            ViewHideInternal();
            return;
        }
        
        //收集所有需要销毁的View
        CollectNeedReverse(UIViews.ValueList);
        if (_needReverseView != null)
        {
            // 播放动画
            for (int i = 0; i < _needReverseView.ValueList.Count; i++)
            {
                _needReverseView.ValueList[i].DoScaleReverse(CheckViewsReverseOver);
            }
        }
        else
        {
            ViewHideInternal();
        }
    }

    public void HideByOpen()
    {
        if (UIViews != null && UIViews.Count != 0)
        {
            for (int i = 0; i < UIViews.Count; i++)
            {
                UIViewBase viewBase = UIViews.ValueList[i];
                viewBase.Hide();
            }
        }

        if (UIExtraArgsViews != null && UIExtraArgsViews.Count != 0)
        {
            for (int i = 0; i < UIExtraArgsViews.Count; i++)
            {
                UIViewBase viewBase = UIExtraArgsViews.ValueList[i];
                viewBase.Hide();
            }
        }

        _isShow = false;
        onHide();
        gameObject.SetActive(false);
        // GameGlobal.UIManager.HideByNextOpen(this);
        // OnDestroy();
        // Object.Destroy(gameObject);
    }
    
    void CheckViewsReverseOver(string viewName)
    {
        _needReverseView.Remove(viewName);
        if (_needReverseView.Count == 0)
        {
            ViewHideInternal();
        }
    }


    public void CollectNeedReverse(List<UIViewBase> views)
    {
        for (int i = 0; i < views.Count; i++)
        {
            if (views[i].scaleRoot != null)
            {
                if (_needReverseView == null) _needReverseView = new DictionaryList<string, UIViewBase>();
                if (!_needReverseView.TryGetValue(views[i].gameObject.name, out UIViewBase viewBase))
                {
                    _needReverseView.Add(views[i].gameObject.name, views[i]);
                }
            }

            if (views[i].UIViews != null && views[i].UIViews.Count != 0)
            {
                CollectNeedReverse(views[i].UIViews.ValueList);
            }
        }
    }

    void ViewHideInternal()
    {
        if (UIViews != null && UIViews.Count != 0)
        {
            for (int i = 0; i < UIViews.Count; i++)
            {
                UIViewBase viewBase = UIViews.ValueList[i];
                viewBase.Hide();
            }
        }

        if (UIExtraArgsViews != null && UIExtraArgsViews.Count != 0)
        {
            for (int i = 0; i < UIExtraArgsViews.Count; i++)
            {
                UIViewBase viewBase = UIExtraArgsViews.ValueList[i];
                viewBase.Hide();
            }
        }
        

        InternalHide();
    }

    void InternalHide()
    {
        if (scaleRoot != null)
        {
            DoScaleReverse(PanelInternalHide);
        }
        else
        {
            // TODO : BUG 新手引导关闭的时候已经找不到页面了？？？？？
            if (gameObject != null)
            {
                PanelInternalHide(gameObject.name);
            }
            else
            {
                KaLog.LogError("我特么新手引导又给干崩了？？？？？？？？？？wtmwtmwtmwtm");
            }
        }
    }

    void PanelInternalHide(string panelName)
    {
        if (panelName != gameObject.name) return;

        _isShow = false;
        // GameGlobal.UIManager.Remove(this);
        onHide();
        OnDestroy();
        Object.Destroy(gameObject);

        mArgs = null;

        // GuideManager.Instance.TriggerGuide?.Invoke(ETriggerType.ClosePanel, new object[] { _uiName });
    }

    public bool CheckPanelHasNeedDoScale()
    {
        bool panelNeedDoScale = false;

        panelNeedDoScale |= scaleRoot != null;
        
        if (UIViews != null && UIViews.Count != 0)
        {
            for (int i = 0; i < UIViews.Count; i++)
            {
                panelNeedDoScale |= UIViews.ValueList[i].scaleRoot != null;
            }
        }

        return panelNeedDoScale;
    }

    public override void onUpdate()
    {
        base.onUpdate();
        if (scaleRoot != null)
        {
            if (CheckPanelScaleOver())
            {
                if (_isActive)
                {
                    _isShowComplete = true;
                    OnShowScaleComplete();
                }
            }
        }
    }

    public bool CheckPanelScaleOver()
    {
        bool scaleOver = false;
        if (scaleRoot != null)
            scaleOver |= IsOpenScaleOver;
        
        if (UIViews != null && UIViews.Count != 0)
        {
            for (int i = 0; i < UIViews.Count; i++)
            {
                if(UIViews.ValueList[i].scaleRoot != null)
                    scaleOver |= UIViews.ValueList[i].IsOpenScaleOver;
            }
        }

        return scaleOver;
    }

    public override void onHide()
    {
        base.onHide();
        _isShowComplete = false;
    }

    public override void OnDestroy()
    {
        base.OnDestroy();
        _isShowComplete = false;
    }

    public void SetSortingOrder()
    {
        // canvas.sortingOrder = SortingOrder;
        var childCanvases = gameObject.GetComponentsInChildren<Canvas>();
        for (int i = 0; i < childCanvases.Length; i++)
        {
            childCanvases[i].sortingOrder += SortingOrder;
        }

        // View不应该加，因为已经都在里面了！上面已经加完了
        // if (UIViews == null)
        //     return;
        //
        // for (int i = 0; i < UIViews.ValueList.Count; i++)
        // {
        //     // var viewCanvas = UIViews.ValueList[i].gameObject.GetComponent<Canvas>();
        //     // if (viewCanvas != null)
        //     //     viewCanvas.sortingOrder += SortingOrder;
        //     
        //     var viewCanvases = UIViews.ValueList[i].gameObject.GetComponentsInChildren<Canvas>();
        //     for (int j = 0; j < viewCanvases.Length; j++)
        //     {
        //         viewCanvases[i].sortingOrder += SortingOrder;
        //     }
        // }
    }

    public void SpecifySortingOrder(int sortingOrder)
    {
        var childCanvases = gameObject.GetComponentsInChildren<Canvas>();
        for (int i = 0; i < childCanvases.Length; i++)
        {
            if (childCanvases[i].sortingOrder >= SortingOrder)
            {
                childCanvases[i].sortingOrder -= SortingOrder;
            }
            
            childCanvases[i].sortingOrder += sortingOrder;
        }

        SortingOrder = sortingOrder;
        
        if (UIViews == null)
            return;
        
        for (int i = 0; i < UIViews.ValueList.Count; i++)
        {
            var viewCanvases = UIViews.ValueList[i].gameObject.GetComponentsInChildren<Canvas>();
            for (int j = 0; j < viewCanvases.Length; j++)
            {
                if (viewCanvases[i].sortingOrder >= SortingOrder)
                {
                    viewCanvases[i].sortingOrder -= SortingOrder;
                }
                
                viewCanvases[i].sortingOrder += sortingOrder;
            }
        }
    }

    public void ClearArgs()
    {
        mArgs = null;
    }
    public void SetArgs(params object[] objs)
    {
        mArgs = objs;
    }
}
