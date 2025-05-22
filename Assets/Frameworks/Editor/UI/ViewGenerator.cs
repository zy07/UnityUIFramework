
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using Button = UnityEngine.UI.Button;
using File = UnityEngine.Windows.File;
using Image = UnityEngine.UI.Image;
using Toggle = UnityEngine.UI.Toggle;

public enum EGenerateType
{
    Panel,
    PanelGen,
    View,
    ViewGen,
    Adapter,
    ScrollDataItem,
    Cell,
    CellGen,
}



public class GenerateUIWindow : EditorWindow
{
    #region 代码预定义

    // [UILogicHandler(UIDef.#UIDEF#, UIViewDef.#名字#, UIModuleNameDef.#名字#)]

    private static string fileSuffix = ".cs";

    private static string panelNameSuffix = "Panel";

    private static string panelGenSuffix = "PanelGen";
    
    private static string viewNameSuffix = "View";

    private static string fileNameSuffix = "ViewGen";
    
    private static string adapterNameSuffix = "Adapter";
    
    private static string scrollDataNameSuffix = "ScrollData";
    
    private static string scrollItemNameSuffix = "ScrollItem";
    
    private static string cellNameSuffix = "Cell";
    #endregion

    private static GenerateUIWindow _generateUIWindow;
    private static List<ElementInfo> elementInfos = null;

    private static CollectUIData collectUIData;

    private static bool useNameSpace = true;
    private static Vector2 scrollPos;
    private static string codePrivewStr = "";
    private static int deep = 0;

    [MenuItem("Assets/一键生成UI脚本")]
    public static void GenPanel()
    {
        GenPanelView();
    }
    
    // [MenuItem("Assets/GenUI/Cell")]
    // public static void GenEnhancedCellView()
    // {
    //     GenCellView();
    // }

    private static void GenPanelView()
    {
        if (Selection.activeGameObject == null)
        {
            Debug.LogError("请选择一个GameObject！");
            return;
        }

        if (!PrefabUtility.IsPartOfPrefabAsset(Selection.activeGameObject))
        {
            Debug.LogError("请选择一个Prefab！");
            return;
        }
        
        GameObject selectObj = Selection.activeGameObject;
        
        collectUIData = new CollectUIData();
        collectUIData.InitUIData(selectObj);

        string panelStr = GeneratePanelCode();
        string panelEditStr = GenerateEditPanelCode(collectUIData.PanelInfo);
        string panelName = collectUIData.PanelInfo.name;
        if (collectUIData.ViewInfoDic != null)
        {
            foreach (var viewInfo in collectUIData.ViewInfoDic)
            {
                string viewStr = GenerateViewCode(viewInfo.Value);
                string viewEditStr = GenerateEditViewCode(viewInfo.Value);
                SaveFileToUIPath(panelName, viewInfo.Value, EGenerateType.View, viewEditStr);
                SaveFileToUIPath(panelName, viewInfo.Value, EGenerateType.ViewGen, viewStr);
            }
        }

        if (collectUIData.EnhancedScrollAdapterDic != null)
        {
            //foreach (var adapter in collectUIData.EnhancedScrollAdapterDic)
            //{
            //    string adapterStr = GenerateScrollAdapterCode(adapter.Value);
            //    SaveFileToUIPath(adapter.Value, EGenerateType.Adapter, adapterStr);
            //}
        }

        // if (collectUIData.LoopScrollDic != null)
        // {
        //     foreach (var scroll in collectUIData.LoopScrollDic)
        //     {
        //         string scrollStr = GenerateScrollDataItemCode(scroll.Value);
        //         SaveFileToUIPath(panelName, scroll.Value, EGenerateType.ScrollDataItem, scrollStr);
        //     }
        // }
        
        SaveFileToUIPath(panelName, collectUIData.PanelInfo, EGenerateType.Panel, panelEditStr);
        SaveFileToUIPath(panelName, collectUIData.PanelInfo, EGenerateType.PanelGen, panelStr);
        collectUIData = null;
    }

    private static void GenCellView()
    {
        // if (Selection.activeGameObject == null)
        // {
        //     Debug.LogError("请选择一个GameObject！");
        //     return;
        // }
        //
        // if (!PrefabUtility.IsPartOfPrefabAsset(Selection.activeGameObject))
        // {
        //     Debug.LogError("请选择一个Prefab！");
        //     return;
        // }
        //
        // GameObject selectObj = Selection.activeGameObject;
        //
        // collectUIData = new CollectUIData();
        // collectUIData.InitUIData(selectObj);
        //
        // if (collectUIData.EnhancedScrollAdapterDic != null)
        // {
        //     foreach (var adapter in collectUIData.EnhancedScrollAdapterDic)
        //     {
        //         string adapterStr = GenerateScrollAdapterCode(adapter.Value);
        //         SaveFileToUIPath(panelnameadapter.Value, EGenerateType.Adapter, adapterStr);
        //     }
        // }
        //
        // string cellStr = GenerateCellCode(collectUIData.PanelInfo);
        // string cellEditStr = GenerateCellEditCode(collectUIData.PanelInfo);
        // SaveFileToUIPath(collectUIData.PanelInfo, EGenerateType.Cell, cellEditStr);
        // SaveFileToUIPath(collectUIData.PanelInfo, EGenerateType.CellGen, cellStr);
        // collectUIData = null;
    }

    static string GeneratePanelCode()
    {
        if (collectUIData.PanelInfo == null)
        {
            return "";
        }

        string panelStr = "";
        
        panelStr = "";
        panelStr = UsingCodeDefine.Using.Replace("#代码#", PanelGenCodeDefine.Class);

        StringBuilder classNameBuilder = new StringBuilder(collectUIData.PanelInfo.name);
        classNameBuilder.Append(panelNameSuffix);
        panelStr = panelStr.Replace("#类名#", classNameBuilder.ToString());
        string elementPreviewStr = GenerateElementDefineStr(collectUIData.PanelInfo.elementList);
        panelStr = panelStr.Replace("#声明#", elementPreviewStr);
        string elementInitPreviewStr = GenerateInitElementStr(collectUIData.PanelInfo.elementList);
        panelStr = panelStr.Replace("#初始化声明#", elementInitPreviewStr);
        string btnEvtPreviewStr = GenerateBtnEvtStr(collectUIData.PanelInfo.elementList);
        panelStr = panelStr.Replace("#注册事件#", btnEvtPreviewStr);
        string btnUnEvtPreviewStr = GenerateBtnUnEvtStr(collectUIData.PanelInfo.elementList);
        panelStr = panelStr.Replace("#反注册事件#", btnUnEvtPreviewStr);

        return panelStr;
    }
    
    static string GenerateEditPanelCode(ClassInfo classInfo)
    {
        if (classInfo == null)
        {
            return "";
        }

        string panelStr = "";
        
        panelStr = "";
        panelStr = UsingCodeDefine.Using.Replace("#代码#", PanelEditCodeDefine.Class);
        panelStr = panelStr.Replace("#资源名#", classInfo.name);

        StringBuilder classNameBuilder = new StringBuilder(classInfo.name);
        classNameBuilder.Append(panelNameSuffix);
        panelStr = panelStr.Replace("#类名#", classNameBuilder.ToString());
        
        string btnFucPreviewStr = GenerateEditBtnEvtStr(classInfo.elementList);
        panelStr = panelStr.Replace("#事件#", btnFucPreviewStr);

        return panelStr;
    }
    
    static string GenerateEditViewCode(ClassInfo classInfo)
    {
        if (classInfo == null)
        {
            return "";
        }

        string panelStr = "";
        
        panelStr = "";
        panelStr = UsingCodeDefine.Using.Replace("#代码#", ViewEditCodeDefine.Class);

        StringBuilder classNameBuilder = new StringBuilder(classInfo.name);
        classNameBuilder.Append(viewNameSuffix);
        panelStr = panelStr.Replace("#类名#", classNameBuilder.ToString());
        
        string btnFucPreviewStr = GenerateEditBtnEvtStr(classInfo.elementList);
        panelStr = panelStr.Replace("#事件#", btnFucPreviewStr);

        return panelStr;
    }
    
    static string GenerateViewCode(ClassInfo viewInfo)
    {
        string viewStr = "";
        
        viewStr = "";
        viewStr = UsingCodeDefine.Using.Replace("#代码#", ViewGenCodeDefine.Class);

        StringBuilder classNameBuilder = new StringBuilder(viewInfo.name);
        classNameBuilder.Append(viewNameSuffix);
        viewStr = viewStr.Replace("#类名#", classNameBuilder.ToString());
        string elementPreviewStr = GenerateElementDefineStr(viewInfo.elementList);
        viewStr = viewStr.Replace("#声明#", elementPreviewStr);
        string elementInitPreviewStr = GenerateInitElementStr(viewInfo.elementList);
        viewStr = viewStr.Replace("#初始化声明#", elementInitPreviewStr);
        string btnEvtPreviewStr = GenerateBtnEvtStr(viewInfo.elementList);
        viewStr = viewStr.Replace("#注册事件#", btnEvtPreviewStr);
        string btnUnEvtPreviewStr = GenerateBtnUnEvtStr(viewInfo.elementList);
        viewStr = viewStr.Replace("#反注册事件#", btnUnEvtPreviewStr);

        return viewStr;
    }
    
    static string GenerateScrollAdapterCode(ClassInfo viewInfo)
    {
        string adapterStr = "";
        
        adapterStr = "";
        adapterStr = UsingCodeDefine.Using.Replace("#代码#", AdapterCodeDefine.Class);
        StringBuilder classNameBuilder = new StringBuilder(viewInfo.name);
        classNameBuilder.Append(adapterNameSuffix);
        adapterStr = adapterStr.Replace("#类名#", classNameBuilder.ToString());

        return adapterStr;
    }
    
    static string GenerateScrollDataItemCode(ClassInfo viewInfo)
    {
        string scrollDataStr = "";
        
        scrollDataStr = UsingCodeDefine.Using.Replace("#代码#", ScrollDataItemEditCodeDefine.Class);
        scrollDataStr = scrollDataStr.Replace("#类名#", viewInfo.name);

        return scrollDataStr;
    }
    
    static string GenerateCellCode(ClassInfo cellInfo)
    {
        string cellStr = "";
        
        cellStr = "";
        cellStr = UsingCodeDefine.Using.Replace("#代码#", CellGenCodeDefine.Class);

        StringBuilder classNameBuilder = new StringBuilder(cellInfo.name);
        classNameBuilder.Append(cellNameSuffix);
        cellStr = cellStr.Replace("#类名#", classNameBuilder.ToString());
        string elementPreviewStr = GenerateElementDefineStr(cellInfo.elementList);
        cellStr = cellStr.Replace("#声明#", elementPreviewStr);
        string elementInitPreviewStr = GenerateInitElementStr(cellInfo.elementList);
        cellStr = cellStr.Replace("#初始化声明#", elementInitPreviewStr);
        string btnEvtPreviewStr = GenerateBtnEvtStr(cellInfo.elementList);
        cellStr = cellStr.Replace("#注册事件#", btnEvtPreviewStr);
        string btnUnEvtPreviewStr = GenerateBtnUnEvtStr(cellInfo.elementList);
        cellStr = cellStr.Replace("#反注册事件#", btnUnEvtPreviewStr);

        return cellStr;
    }
    
    static string GenerateCellEditCode(ClassInfo cellInfo)
    {
        string cellStr = "";
        
        cellStr = "";
        cellStr = UsingCodeDefine.Using.Replace("#代码#", CellEditCodeDefine.Class);

        StringBuilder classNameBuilder = new StringBuilder(cellInfo.name);
        classNameBuilder.Append(cellNameSuffix);
        cellStr = cellStr.Replace("#类名#", classNameBuilder.ToString());
        
        string btnEditEvtPreviewStr = GenerateEditBtnEvtStr(cellInfo.elementList);
        cellStr = cellStr.Replace("#事件#", btnEditEvtPreviewStr);

        return cellStr;
    }

    static string GenerateElementDefineStr(List<ElementInfo> elementInfoList)
    {
        string resStr = "";
        for (int i = 0; i < elementInfoList.Count; i++)
        {
            if (elementInfoList[i].eleType == null)
            {
                elementInfoList[i].eleType = new List<EElementType>();
                elementInfoList[i].eleType.Add(EElementType.EGAMEOBJECT);
            }

            for (int j = 0; j < elementInfoList[i].eleType.Count; j++)
            {
                switch (elementInfoList[i].eleType[j])
                {
                    case EElementType.EIMAGE:
                        resStr += ElementCodeDefine.imgCompFormat.Replace("#命名#", elementInfoList[i].eleName);
                        break;
                    case EElementType.ETEXT:
                        resStr += ElementCodeDefine.txtCompFormat.Replace("#命名#", elementInfoList[i].eleName);
                        break;
                    case EElementType.EBUTTON:
                        resStr += ElementCodeDefine.btnCompFormat.Replace("#命名#", elementInfoList[i].eleName);
                        break;
                    case EElementType.ETOGGLE:
                        resStr += ElementCodeDefine.togCompFormat.Replace("#命名#", elementInfoList[i].eleName);
                        break;
                    case EElementType.ESCROLLVIEW:
                        resStr += ElementCodeDefine.scrollRectCompFormat.Replace("#命名#", elementInfoList[i].eleName);
                        // resStr += ElementCodeDefine.scrollCellCompFormat.Replace("#命名#", elementInfoList[i].eleName);
                        // resStr += ElementCodeDefine.scrollCompFormat.Replace("#命名#", elementInfoList[i].eleName);
                        break;
                    case EElementType.ERAWIMAGE:
                        resStr += ElementCodeDefine.rawImgCompFormat.Replace("#命名#", elementInfoList[i].eleName);
                        break;
                    case EElementType.ESCROLLERPRO:
                        resStr += ElementCodeDefine.scrollerProCompFormat.Replace("#命名#", elementInfoList[i].eleName);
                        break;
                    case EElementType.EINPUTFIELD:
                        resStr += ElementCodeDefine.inputFieldCompFormat.Replace("#命名#", elementInfoList[i].eleName);
                        break;
                    // case EElementType.ESCROLLDELEGATE:
                    //     resStr += ElementCodeDefine.scrollDelegateCompFormat.Replace("#命名#", elementInfoList[i].eleName);
                    //     break;
                    case EElementType.ESLIDER:
                        resStr += ElementCodeDefine.sliderCompFormat.Replace("#命名#", elementInfoList[i].eleName);
                        break;
                    case EElementType.EDROPDOWN:
                        resStr += ElementCodeDefine.dropDownCompFormat.Replace("#命名#", elementInfoList[i].eleName);
                        break;
                    case EElementType.EGAMEOBJECT:
                        resStr += ElementCodeDefine.objCompFormat.Replace("#命名#", elementInfoList[i].eleName);
                        break;
                    case EElementType.ECONTAINER:
                        resStr += ElementCodeDefine.containerCompFormat.Replace("#命名#", elementInfoList[i].eleName);
                        break;
                    case EElementType.EUI3DDISPLAY:
                        resStr += ElementCodeDefine.ui3DDisplayCompFormat.Replace("#命名#", elementInfoList[i].eleName);
                        break;
                    case EElementType.EIMAGECTRL:
                        resStr += ElementCodeDefine.imageCtrlCompFormat.Replace("#命名#", elementInfoList[i].eleName);
                        break;
                    case EElementType.ETEXTURECTRL:
                        resStr += ElementCodeDefine.textureCtrlCompFormat.Replace("#命名#", elementInfoList[i].eleName);
                        break;
                }
            }
        }

        return resStr;
    }

    static string GenerateInitElementStr(List<ElementInfo> elementInfoList)
    {
        string resStr = "";
        for (int i = 0; i < elementInfoList.Count; i++)
        {
            if (elementInfoList[i].eleType == null)
            {
                elementInfoList[i].eleType = new List<EElementType>();
                elementInfoList[i].eleType.Add(EElementType.EGAMEOBJECT);
            }

            for (int j = 0; j < elementInfoList[i].eleType.Count; j++)
            {
                string str = "";
                switch (elementInfoList[i].eleType[j])
                {
                    case EElementType.EIMAGE:
                        str = ElementCodeInitDefine.imgCompInitFormat.Replace("#名字#", elementInfoList[i].eleName);
                        str = str.Replace("#路径#", string.Format("\"{0}\"", elementInfoList[i].path));
                        break;
                    case EElementType.ETEXT:
                        str = ElementCodeInitDefine.txtCompInitFormat.Replace("#名字#", elementInfoList[i].eleName);
                        str = str.Replace("#路径#", string.Format("\"{0}\"", elementInfoList[i].path));
                        break;
                    case EElementType.EBUTTON:
                        str = ElementCodeInitDefine.btnCompInitFormat.Replace("#名字#", elementInfoList[i].eleName);
                        str = str.Replace("#路径#", string.Format("\"{0}\"", elementInfoList[i].path));
                        break;
                    case EElementType.ETOGGLE:
                        str = ElementCodeInitDefine.togCompInitFormat.Replace("#名字#", elementInfoList[i].eleName);
                        str = str.Replace("#路径#", string.Format("\"{0}\"", elementInfoList[i].path));
                        break;
                    case EElementType.ESCROLLVIEW:
                        str = ElementCodeInitDefine.scrollRectCompInitFormat.Replace("#名字#", elementInfoList[i].eleName);
                        str = str.Replace("#路径#", string.Format("\"{0}\"", elementInfoList[i].path));
                        break;
                    case EElementType.ERAWIMAGE:
                        str = ElementCodeInitDefine.rawImgCompInitFormat.Replace("#名字#", elementInfoList[i].eleName);
                        str = str.Replace("#路径#", string.Format("\"{0}\"", elementInfoList[i].path));
                        break;
                    case EElementType.ESCROLLERPRO:
                        str = ElementCodeInitDefine.scrollerProInitFormat.Replace("#名字#", elementInfoList[i].eleName);
                        str = str.Replace("#路径#", string.Format("\"{0}\"", elementInfoList[i].path));
                        break;
                    case EElementType.EINPUTFIELD:
                        str = ElementCodeInitDefine.inputFieldInitFormat.Replace("#名字#", elementInfoList[i].eleName);
                        str = str.Replace("#路径#", string.Format("\"{0}\"", elementInfoList[i].path));
                        break;
                    // case EElementType.ESCROLLDELEGATE:
                        // str = ElementCodeInitDefine.scrollDelegateInitFormat.Replace("#名字#", elementInfoList[i].eleName);
                        // str = str.Replace("#路径#", string.Format("\"{0}\"", elementInfoList[i].path));
                        // break;
                    case EElementType.ESLIDER:
                        str = ElementCodeInitDefine.sliderInitFormat.Replace("#名字#", elementInfoList[i].eleName);
                        str = str.Replace("#路径#", string.Format("\"{0}\"", elementInfoList[i].path));
                        break;
                    case EElementType.EDROPDOWN:
                        str = ElementCodeInitDefine.dropDownInitFormat.Replace("#名字#", elementInfoList[i].eleName);
                        str = str.Replace("#路径#", string.Format("\"{0}\"", elementInfoList[i].path));
                        break;
                    case EElementType.EGAMEOBJECT:
                        str = ElementCodeInitDefine.objInitFormat.Replace("#名字#", elementInfoList[i].eleName);
                        str = str.Replace("#路径#", string.Format("\"{0}\"", elementInfoList[i].path));
                        break;
                    case EElementType.ECONTAINER:
                        str = ElementCodeInitDefine.containerInitFormat.Replace("#名字#", elementInfoList[i].eleName);
                        str = str.Replace("#路径#", string.Format("\"{0}\"", elementInfoList[i].path));
                        break;
                    case EElementType.EUI3DDISPLAY:
                        str = ElementCodeInitDefine.ui3DDisplayInitFormat.Replace("#名字#", elementInfoList[i].eleName);
                        str = str.Replace("#路径#", string.Format("\"{0}\"", elementInfoList[i].path));
                        break;
                    case EElementType.EIMAGECTRL:
                        str = ElementCodeInitDefine.imageCtrlCompInitFormat.Replace("#名字#", elementInfoList[i].eleName);
                        str = str.Replace("#路径#", string.Format("\"{0}\"", elementInfoList[i].path));
                        break;
                    case EElementType.ETEXTURECTRL:
                        str = ElementCodeInitDefine.textureCtrlCompInitFormat.Replace("#名字#", elementInfoList[i].eleName);
                        str = str.Replace("#路径#", string.Format("\"{0}\"", elementInfoList[i].path));
                        break;
                }

                resStr += str;
            }
        }

        return resStr;
    }

    static string GenerateBtnEvtStr(List<ElementInfo> elementInfoList)
    {
        string resStr = "";
        for (int i = 0; i < elementInfoList.Count; i++)
        {
            if (elementInfoList[i].eleType == null)
            {
                return "";
            }

            for (int j = 0; j < elementInfoList[i].eleType.Count; j++)
            {
                string str = "";
                switch (elementInfoList[i].eleType[j])
                {
                    case EElementType.EBUTTON:
                        str = BtnEventCode.BtnEvtInitFormat.Replace("#名字#", elementInfoList[i].eleName);
                        break;
                }

                resStr += str;
            }
        }

        return resStr;
    }
    
    static string GenerateBtnUnEvtStr(List<ElementInfo> elementInfoList)
    {
        string resStr = "";
        for (int i = 0; i < elementInfoList.Count; i++)
        {
            if (elementInfoList[i].eleType == null)
            {
                return "";
            }

            for (int j = 0; j < elementInfoList[i].eleType.Count; j++)
            {
                string str = "";
                switch (elementInfoList[i].eleType[j])
                {
                    case EElementType.EBUTTON:
                        str = BtnEventCode.BtnEvtUnInitFormat.Replace("#名字#", elementInfoList[i].eleName);
                        break;
                }

                resStr += str;
            }
        }

        return resStr;
    }
    
    static string GenerateEditBtnEvtStr(List<ElementInfo> elementInfoList)
    {
        string resStr = "";
        for (int i = 0; i < elementInfoList.Count; i++)
        {
            if (elementInfoList[i].eleType == null)
            {
                return "";
            }

            for (int j = 0; j < elementInfoList[i].eleType.Count; j++)
            {
                string str = "";
                switch (elementInfoList[i].eleType[j])
                {
                    case EElementType.EBUTTON:
                        str = BtnEventCode.BtnFuncFormat.Replace("#名字#", elementInfoList[i].eleName);
                        break;
                }

                resStr += str;
            }
        }

        return resStr;
    }

    private static string PanelGenDirPath = System.IO.Path.GetFullPath(Application.dataPath + "/Scripts/Hotfix/Logic/GameUI/Generate/UIPanel/");
    private static string PanelDirPath = System.IO.Path.GetFullPath(Application.dataPath + "/Scripts/Hotfix/Logic/GameUI/UI/{0}/Panel/");
    private static string ViewGenDirPath = System.IO.Path.GetFullPath(Application.dataPath + "/Scripts/Hotfix/Logic/GameUI/Generate/UIView/");
    private static string ViewDirPath = System.IO.Path.GetFullPath(Application.dataPath + "/Scripts/Hotfix/Logic/GameUI/UI/{0}/View/");
    private static string AdapterDirPath = System.IO.Path.GetFullPath(Application.dataPath + "/Scripts/Hotfix/Logic/GameUI/ScrollAdapter/");
    private static string CellGenDirPath = System.IO.Path.GetFullPath(Application.dataPath + "/Scripts/Hotfix/Logic/GameUI/Generate/Cell/");
    private static string CellDirPath = System.IO.Path.GetFullPath(Application.dataPath + "/Scripts/Hotfix/Logic/GameUI/Cell/");
    private static string ScrollDataItemDirPath = System.IO.Path.GetFullPath(Application.dataPath + "/Scripts/Hotfix/Logic/GameUI/UI/{0}/ScrollDataItem/");
    

    private static void SaveFileToUIPath(string panelName, ClassInfo classInfo, EGenerateType type, string content)
    {
        string fileName = classInfo.name + type.ToString() + fileSuffix;
        string path = "";
        
        switch (type)
        {
            case EGenerateType.Adapter:
                path = AdapterDirPath;
                break;
            case EGenerateType.Cell:
                path = CellDirPath;
                break;
            case EGenerateType.CellGen:
                path = CellGenDirPath;
                break;
            case EGenerateType.Panel:
                path = string.Format(PanelDirPath, panelName);
                break;
            case EGenerateType.PanelGen:
                path = PanelGenDirPath;
                break;
            case EGenerateType.View:
                path = string.Format(ViewDirPath, panelName);
                break;
            case EGenerateType.ViewGen:
                path = ViewGenDirPath;
                break;
            case EGenerateType.ScrollDataItem:
                path = string.Format(ScrollDataItemDirPath, panelName);
                break;
        }

        if (type == EGenerateType.Panel || type == EGenerateType.View || type == EGenerateType.Adapter 
            || type == EGenerateType.Cell || type == EGenerateType.ScrollDataItem)
        {
            if (File.Exists(path + fileName))
                return;
        }
        
        if (!System.IO.Directory.Exists(path))
            System.IO.Directory.CreateDirectory(path);
        System.IO.File.WriteAllText(path + fileName, content, System.Text.Encoding.UTF8);
        
        Debug.Log("脚本保存成功:" + path + fileName);
        // EditorUtility.DisplayDialog("保存成功", "成功保存在:" + path + fileName, "确认");
        AssetDatabase.Refresh();
    }
}