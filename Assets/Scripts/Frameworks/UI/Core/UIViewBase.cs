using UnityEngine;

public abstract class UIViewBase : UIBase
{
    private Transform _parent;

    public override void Initialize()
    {
        base.Initialize();
        Transform selfTrans = this.gameObject.transform;
        selfTrans.SetParent(_parent, false);
        selfTrans.localScale = new Vector3(1, 1, 1);
        Show(null);
    }

    public void SetParent(Transform parent)
    {
        this._parent = parent;
    }

    public void Show(object[] args)
    {
        if (!this.gameObject.activeSelf)
        {
            this.gameObject.SetActive(true);
        }

        _isShow = true;
        onShow(args);
    }

    // public void Refresh(object[] args)
    // {
    //     OnRefresh(args);
    // }

    public void Hide()
    {
        if (this.gameObject.activeSelf)
        {
            this.gameObject.SetActive(false);
        }

        _isShow = false;
        onHide();
    }

    public void ShowWithoutActive()
    {
        _isShow = true;
        onShow(null);
    }

    public void HideWithoutInactive()
    {
        _isShow = false;
        onHide();
    }
}
