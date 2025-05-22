using Framework;
using System.Collections.Generic;

using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CollectUIData
{
    public GameObject Prefab;
    public ClassInfo PanelInfo;
    public Dictionary<string, ClassInfo> ViewInfoDic;
    public Dictionary<string, ClassInfo> EnhancedScrollAdapterDic;
    public Dictionary<string, ClassInfo> LoopScrollDic;

    private Stack<string> uiNodeStack = new Stack<string>();
    private int deep;

    public void InitUIData(GameObject prefab)
    {
        Prefab = prefab;
        PanelInfo = new ClassInfo();
        PanelInfo.name = prefab.name;
        PanelInfo.gameObject = prefab;
        PanelInfo.baseName = "UIPanelBase";
        LoopCollectData(Prefab.transform);
    }

    public void LoopCollectData(Transform trans)
    {
        deep++;
        int chdCount = trans.childCount;
        for (int i = 0; i < chdCount; i++)
        {
            Transform chd = trans.GetChild(i);
            uiNodeStack.Push(chd.name);
            LoopCollectData(chd);
            string chdName = chd.name;
            string[] splChdNames = chdName.Split('_');
            if (splChdNames.Length > 1)
            {
                if (splChdNames[0] == "t") // Component
                {
                    string eleName = chdName.Remove(0, 2);
                    List<EElementType> eleTypes = null;
                    TryGetAllElement(ref eleTypes, chd);
                    bool isView = false;
                    string viewName = "";
                    string path = GetTransPath(chd, ref isView, ref viewName);
                    if (!isView)
                    {
                        bool hasAdded = false;
                        for (int j = 0; j < PanelInfo.elementList.Count; j++)
                        {
                            if (PanelInfo.elementList[j].path == path)
                            {
                                hasAdded = true;
                                break;
                            }
                        }

                        if (!hasAdded)
                        {
                            ElementInfo info = new ElementInfo();
                            info.path = path;
                            info.eleName = eleName;
                            info.eleType = eleTypes;
                            PanelInfo.elementList.Add(info);
                        }
                    }
                    else
                    {
                        ClassInfo viewInfo = GetViewInfo(viewName);
                        bool hasAdded = false;
                        for (int j = 0; j < viewInfo.elementList.Count; j++)
                        {
                            if (viewInfo.elementList[j].path == path)
                            {
                                hasAdded = true;
                                break;
                            }
                        }

                        if (!hasAdded)
                        {
                            ElementInfo info = new ElementInfo();
                            info.path = path;
                            info.eleName = eleName;
                            info.eleType = eleTypes;
                            viewInfo.elementList.Add(info);
                        }
                    }
                }
                else if (splChdNames[0] == "c")
                {
                    string eleName = chdName.Remove(0, 2);
                    bool isView = false;
                    string viewName = "";
                    string path = GetTransPath(chd, ref isView, ref viewName);
                    if (!isView)
                    {
                        bool hasAdded = false;
                        for (int j = 0; j < PanelInfo.elementList.Count; j++)
                        {
                            if (PanelInfo.elementList[j].path == path)
                            {
                                hasAdded = true;
                                break;
                            }
                        }

                        if (!hasAdded)
                        {
                            ElementInfo info = new ElementInfo();
                            info.path = path;
                            info.eleName = eleName;
                            info.eleType = new List<EElementType>() { EElementType.ECONTAINER };
                            PanelInfo.elementList.Add(info);
                        }
                    }
                    else
                    {
                        ClassInfo viewInfo = GetViewInfo(viewName);
                        bool hasAdded = false;
                        for (int j = 0; j < viewInfo.elementList.Count; j++)
                        {
                            if (viewInfo.elementList[j].path == path)
                            {
                                hasAdded = true;
                                break;
                            }
                        }

                        if (!hasAdded)
                        {
                            ElementInfo info = new ElementInfo();
                            info.path = path;
                            info.eleName = eleName;
                            info.eleType = new List<EElementType>() { EElementType.ECONTAINER };
                            viewInfo.elementList.Add(info);
                        }
                    }
                }
            }

            uiNodeStack.Pop();
        }

        deep--;
    }

    #region GetAllElement

    void TryGetAllElement(ref List<EElementType> typeList, Transform trans)
    {
        TryGetImageComp(ref typeList, trans);
        TryGetButtonComp(ref typeList, trans);
        TryGetRawImageComp(ref typeList, trans);
        TryGetInputFieldComp(ref typeList, trans);
        TryGetTextComp(ref typeList, trans);

        TryGetScrollerProComp(ref typeList, trans);

        TryGetToggleComp(ref typeList, trans);
        // TryGetScrollDelegateViewComp(ref typeList, trans);
        TryGetSliderViewComp(ref typeList, trans);
        // TryGetViewBaseComp(ref typeList, trans);
        TryGetDropDownComp(ref typeList, trans);
    }

    void TryGetImageComp(ref List<EElementType> typeList, Transform trans)
    {
        if (trans.TryGetComponent<Image>(out Image imageComp))
        {
            if (typeList != null)
            {
                if (!typeList.Contains(EElementType.EIMAGE))
                {
                    typeList.Add(EElementType.EIMAGE);
                }
            }
            else
            {
                typeList = new List<EElementType>();
                typeList.Add(EElementType.EIMAGE);
            }
        }
    }

    void TryGetButtonComp(ref List<EElementType> typeList, Transform trans)
    {
        if (trans.TryGetComponent<Button>(out Button imageComp))
        {
            if (typeList != null)
            {
                if (!typeList.Contains(EElementType.EBUTTON))
                {
                    typeList.Add(EElementType.EBUTTON);
                }
            }
            else
            {
                typeList = new List<EElementType>();
                typeList.Add(EElementType.EBUTTON);
            }
        }
    }

    void TryGetRawImageComp(ref List<EElementType> typeList, Transform trans)
    {
        if (trans.TryGetComponent<RawImage>(out RawImage imageComp))
        {
            if (typeList != null)
            {
                if (!typeList.Contains(EElementType.ERAWIMAGE))
                {
                    typeList.Add(EElementType.ERAWIMAGE);
                }
            }
            else
            {
                typeList = new List<EElementType>();
                typeList.Add(EElementType.ERAWIMAGE);
            }
        }
    }

    void TryGetInputFieldComp(ref List<EElementType> typeList, Transform trans)
    {
        if (trans.TryGetComponent<InputField>(out InputField imageComp) ||
            trans.TryGetComponent<TMP_InputField>(out TMP_InputField inputField))
        {
            if (typeList != null)
            {
                if (!typeList.Contains(EElementType.EINPUTFIELD))
                {
                    typeList.Add(EElementType.EINPUTFIELD);
                }
            }
            else
            {
                typeList = new List<EElementType>();
                typeList.Add(EElementType.EINPUTFIELD);
            }
        }
    }

    void TryGetTextComp(ref List<EElementType> typeList, Transform trans)
    {
        if (trans.TryGetComponent<Text>(out Text imageComp) || trans.TryGetComponent<TMP_Text>(out TMP_Text tmpText))
        {
            if (typeList != null)
            {
                if (!typeList.Contains(EElementType.ETEXT))
                {
                    typeList.Add(EElementType.ETEXT);
                }
            }
            else
            {
                typeList = new List<EElementType>();
                typeList.Add(EElementType.ETEXT);
            }
        }
    }

    void TryGetScrollerProComp(ref List<EElementType> typeList, Transform trans)
    {
        if (trans.TryGetComponent<ScrollerPro>(out ScrollerPro proComp))
        {
            if (typeList != null)
            {
                if (!typeList.Contains(EElementType.ESCROLLERPRO))
                {
                    typeList.Add(EElementType.ESCROLLERPRO);
                }
            }
            else
            {
                typeList = new List<EElementType>();
                typeList.Add(EElementType.ESCROLLERPRO);
            }
        }
    }

    void TryGetScrollDelegateViewComp(ref List<EElementType> typeList, Transform trans)
    {
        //if (trans.TryGetComponent<BaseScrollDelegate>(out BaseScrollDelegate imageComp))
        //{
        //    if (typeList != null)
        //    {
        //        if (!typeList.Contains(EElementType.ESCROLLDELEGATE))
        //        {
        //            typeList.Add(EElementType.ESCROLLDELEGATE);
        //        }
        //    }
        //    else
        //    {
        //        typeList = new List<EElementType>();
        //        typeList.Add(EElementType.ESCROLLDELEGATE);
        //    }

        //    string[] transNameSplit = trans.name.Split('_');
            
        //    ClassInfo adapterInfo = new ClassInfo();
        //    adapterInfo.name = transNameSplit[1];
        //    InitEnhancedScrollAdapterInfo(trans.name, adapterInfo);
        //}
    }

    void TryGetSliderViewComp(ref List<EElementType> typeList, Transform trans)
    {
        if (trans.TryGetComponent<Slider>(out Slider imageComp))
        {
            if (typeList != null)
            {
                if (!typeList.Contains(EElementType.ESLIDER))
                {
                    typeList.Add(EElementType.ESLIDER);
                }
            }
            else
            {
                typeList = new List<EElementType>();
                typeList.Add(EElementType.ESLIDER);
            }
        }
    }

    //static void TryGetViewBaseComp(ref List<EElementType> typeList, Transform trans)
    //{
    //    if (trans.TryGetComponent<UIViewBase>(out UIViewBase imageComp))
    //    {
    //        if (typeList != null)
    //        {
    //            if (!typeList.Contains(EElementType.EVIEWBASE))
    //            {
    //                typeList.Add(EElementType.EVIEWBASE);
    //            }
    //        }
    //        else
    //        {
    //            typeList = new List<EElementType>();
    //            typeList.Add(EElementType.EVIEWBASE);
    //        }
    //    }
    //}

    void TryGetToggleComp(ref List<EElementType> typeList, Transform trans)
    {
        if (trans.TryGetComponent<Toggle>(out Toggle imageComp))
        {
            if (typeList != null)
            {
                if (!typeList.Contains(EElementType.ETOGGLE))
                {
                    typeList.Add(EElementType.ETOGGLE);
                }
            }
            else
            {
                typeList = new List<EElementType>();
                typeList.Add(EElementType.ETOGGLE);
            }
        }
    }

    void TryGetDropDownComp(ref List<EElementType> typeList, Transform trans)
    {
        if (trans.TryGetComponent<TMP_Dropdown>(out TMP_Dropdown imageComp))
        {
            if (typeList != null)
            {
                if (!typeList.Contains(EElementType.EDROPDOWN))
                {
                    typeList.Add(EElementType.EDROPDOWN);
                }
            }
            else
            {
                typeList = new List<EElementType>();
                typeList.Add(EElementType.EDROPDOWN);
            }
        }
    }

    string GetTransPath(Transform trans, ref bool isView, ref string viewName)
    {
        List<string> reversePath = new List<string>();
        while (uiNodeStack.Count > 0)
        {
            reversePath.Add(uiNodeStack.Pop());
        }

        string res = "";
        isView = false;
        
        for (int i = reversePath.Count - 1; i >= 0; i--)
        {
            uiNodeStack.Push(reversePath[i]);
            if (reversePath[i].Contains("v_"))
            {
                isView = true;
                res = "";
                viewName = reversePath[i].Remove(0, 2);
                continue;
            }
            
            if (i == 0)
            {
                res += reversePath[i];
            }
            else
            {
                res += $"{reversePath[i]}/";
            }
        }

        

        // Transform tempTrans = trans;
        // string path = "";
        // for (int j = 0; j < deep; j++)
        // {
        //     if (j == deep - 1)
        //     {
        //         path += tempTrans.name;
        //     }
        //     else
        //     {
        //         path += string.Format("{0}/", tempTrans.name);
        //     }
        //
        //     tempTrans = tempTrans.parent;
        // }
        //
        // string[] pathArr = path.Split('/');
        // string res = "";
        // for (int i = pathArr.Length - 1; i >= 0; i--)
        // {
        //     if (i == 0)
        //     {
        //         res += pathArr[i];
        //     }
        //     else
        //     {
        //         res += string.Format("{0}/", pathArr[i]);
        //     }
        // }

        return res;
    }

    #endregion

    public ClassInfo GetViewInfo(string viewName)
    {
        ViewInfoDic ??= new Dictionary<string, ClassInfo>();

        if (ViewInfoDic.TryGetValue(viewName, out ClassInfo existViewInfo))
        {
            return existViewInfo;
        }
        else
        {
            ClassInfo viewInfo = new ClassInfo();
            viewInfo.name = viewName;
            ViewInfoDic.Add(viewName, viewInfo);
            return viewInfo;
        }
    }

    public void InitEnhancedScrollAdapterInfo(string enhancedName, ClassInfo adapterInfo)
    {
        EnhancedScrollAdapterDic ??= new Dictionary<string, ClassInfo>();
        if (EnhancedScrollAdapterDic.TryGetValue(enhancedName, out ClassInfo existAdapterInfo))
        {
            Debug.LogError($"{enhancedName} 已存在，请检查，Panel下滑动列表的命名是否重复");
            return;
        }

        EnhancedScrollAdapterDic.Add(enhancedName, adapterInfo);
    }
    
    public void InitLoopScrollInfo(string scrollName, ClassInfo scrollInfo)
    {
        LoopScrollDic ??= new Dictionary<string, ClassInfo>();
        if (LoopScrollDic.TryGetValue(scrollName, out ClassInfo existAdapterInfo))
        {
            Debug.LogError($"{scrollName} 已存在，请检查，Panel下滑动列表的命名是否重复");
            return;
        }

        LoopScrollDic.Add(scrollName, scrollInfo);
    }

    public void Release()
    {
        PanelInfo = null;
        if (ViewInfoDic != null)
        {
            ViewInfoDic.Clear();
            ViewInfoDic = null;
        }

        if (EnhancedScrollAdapterDic != null)
        {
            EnhancedScrollAdapterDic.Clear();
            EnhancedScrollAdapterDic = null;
        }

        if (LoopScrollDic != null)
        {
            LoopScrollDic.Clear();
            LoopScrollDic = null;
        }
    }
}