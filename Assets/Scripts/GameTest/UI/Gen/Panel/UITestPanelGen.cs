
using System.Collections.Generic;
using System.Collections;
using UnityEngine.UI;
using System;
using TMPro;
using UnityEngine;
using Framework;
using Hotfix;


public partial class UITestPanel
{
    #region UIElement声明
    
    [NonSerialized]
    public TextMeshProUGUI TestHintTxt;
    [NonSerialized]
    public Image TestImg;
    [NonSerialized]
    public Button TestBtn;
    #endregion

    public override void BindUI()
    {
        base.BindUI();

        #region 初始化UIElement
        
        TestHintTxt = this.transform.Find("t_Test/t_TestHint").GetComponent<TextMeshProUGUI>();
        TestImg = this.transform.Find("t_Test").GetComponent<Image>();
        TestBtn = this.transform.Find("t_Test").GetComponent<Button>();
        #endregion
    }

    public override void RegisterEvents()
    {
        base.RegisterEvents();
        #region 注册事件
        
        UIEventListener.OnClick(TestBtn.gameObject).AddListener(OnClickTestBtn);
        #endregion
    }
    
    public override void UnRegisterEvents()
    {
        base.UnRegisterEvents();
        #region 反注册事件
        
        UIEventListener.OnClick(TestBtn.gameObject).RemoveListener(OnClickTestBtn);
        #endregion
    }
}

