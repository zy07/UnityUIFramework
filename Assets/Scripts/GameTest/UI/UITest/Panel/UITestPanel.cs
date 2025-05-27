
using System.Collections.Generic;
using System.Collections;
using UnityEngine.UI;
using System;
using TMPro;
using UnityEngine;
using Framework;
using Hotfix;


[UIBaseHandler(UIPanelName.UITest)]
public partial class UITestPanel : UIPanelBase
{
    public override void InitializeParams()
    {
        base.InitializeParams();
        layerId = UILayerId.Panel;
        // TODO ： 如果没有view，下面这行删除
        ViewNameList ??= new List<string>();
    }
    
    public override IEnumerator Prepare()
    {
        yield break;
    }
    
    public override void onCreate()
    {
        base.onCreate();
    }

    public override void onShow(object[] objs)
    {
        base.onShow(objs);
    }

    public override void onHide()
    {
        base.onHide();
    }

    public override void OnDestroy()
    {
        base.OnDestroy();
    }
    

    
        public void OnClickTestBtn(GameObject obj)
        {
            
        }
        
        

}
