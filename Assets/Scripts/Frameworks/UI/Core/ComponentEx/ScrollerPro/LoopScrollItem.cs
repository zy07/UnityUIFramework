using Framework;
using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace Hotfix
{
    public class LoopScrollItem
    {
        public int index = -1;


        public GroupBase groupBase;
        public int groupIndex => groupBase.groupIndex;
        public int groupStart => groupBase.groupStart;


        public GameObject gameObject;

        public ILoopScroll loopScroll;

        public GameObject selectGo;
        public GameObject normalGo;

        private UIBase _uiBase;

        public void Init(ILoopScroll loopScroll, GameObject gameObject, UIBase uiBase)
        {
            this.loopScroll = loopScroll;
            this.gameObject = gameObject;
            _uiBase = uiBase;

            AutoReference(gameObject.transform, this);

            UIEventListener.OnClick(this.gameObject).AddListener(OnItemClick);

            OnInit();
        }

        public void AutoReference(Transform transform, object obj)
        {
            Dictionary<string, FieldInfo> fieldInfoDict = new Dictionary<string, FieldInfo>();
            FieldInfo[] fieldInfos =
                obj.GetType().GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            Type objectType = typeof(Object);
            foreach (FieldInfo item in fieldInfos)
            {
                if (item.FieldType.IsSubclassOf(objectType))
                {
                    fieldInfoDict[item.Name.ToLower()] = item;
                }
            }

            if (fieldInfoDict.Count > 0)
            {
                void AutoReference(Transform transform, Dictionary<string, FieldInfo> fieldInfoDict)
                {
                    string name = transform.name.ToLower();
                    if (fieldInfoDict.ContainsKey(name))
                    {
                        if (fieldInfoDict[name].FieldType.Equals(typeof(GameObject)))
                        {
                            fieldInfoDict[name].SetValue(obj, transform.gameObject);
                        }
                        else if (fieldInfoDict[name].FieldType.Equals(typeof(Transform)))
                        {
                            fieldInfoDict[name].SetValue(obj, transform);
                        }
                        else
                        {
                            fieldInfoDict[name].SetValue(obj, transform.GetComponent(fieldInfoDict[name].FieldType));
                        }
                    }


                    Transform[] childrens = transform.GetComponentsInChildren<Transform>(true);

                    foreach (Transform item in childrens)
                    {
                        string itemName = item.name.ToLower();
                        if (fieldInfoDict.ContainsKey(itemName))
                        {
                            if (fieldInfoDict[itemName].FieldType.Equals(typeof(GameObject)))
                            {
                                fieldInfoDict[itemName].SetValue(obj, item.gameObject);
                            }
                            else if (fieldInfoDict[itemName].FieldType.Equals(typeof(Transform)))
                            {
                                fieldInfoDict[itemName].SetValue(obj, item);
                            }
                            else
                            {
                                fieldInfoDict[itemName].SetValue(obj,
                                    item.GetComponent(fieldInfoDict[itemName].FieldType));
                            }
                        }
                    }
                }

                AutoReference(transform, fieldInfoDict);
            }
        }

        public virtual void OnInit()
        {
        }

        public virtual void Dispose()
        {
        }

        public virtual void OnShow()
        {
        }

        public virtual void OnHide()
        {
        }

        public virtual void UpdateData(object obj)
        {
            OnItemSelect(loopScroll.CurSelect);
        }

        public virtual void OnItemClick(GameObject obj)
        {
            loopScroll.OnItemSelect(index);
            loopScroll.OnItemClick(index);
        }

        public virtual void OnItemSelect(int index)
        {
            UpdateSelectSpriteVisible(this.index == index);
        }

        public virtual void UpdateSelectSpriteVisible(bool visible)
        {
            if (selectGo != null && selectGo.activeSelf != visible)
            {
                selectGo.SetActive(visible);
            }

            if (normalGo != null && normalGo.activeSelf == visible)
            {
                normalGo.SetActive(!visible);
            }
        }

        public void SetSize(Vector2 size)
        {
            loopScroll.SetSize(index, size);
        }

        public void SetSizeX(int x)
        {
            loopScroll.SetSizeX(index, x);
        }

        public void SetSizeY(int y)
        {
            loopScroll.SetSizeY(index, y);
        }

        public void OnCreate()
        {
        }

        public virtual void OnRecycle()
        {
            RectTransform rect = gameObject.transform as RectTransform;
            gameObject.SetActive(false);
            rect.SetParent(groupBase.scrollerPro.transform);
            rect.localPosition = Vector3.zero;
            rect.localRotation = Quaternion.identity;
            rect.localScale = Vector3.one;
        }

        protected void DestroyWidget<T>(ref T widget) where T : UIViewBase
        {
            UIUtil.DestroyWidget(ref widget);
        }

        protected void CreateWidget<T>(GameObject root, string widgetPrefabName, ref T widget)
            where T : UIViewBase, new()
        {
            UIUtil.CreateWidget(root, widgetPrefabName, ref widget);
        }

        protected void ShowWidget(UIViewBase widget, object[] objs = null)
        {
            UIUtil.ShowWidget(widget, objs);
        }
    }
}