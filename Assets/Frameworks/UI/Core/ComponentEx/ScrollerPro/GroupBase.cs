using System.Collections.Generic;
using UnityEngine;
using EnhancedUI.EnhancedScroller;
using UnityEngine.UI;
using static EnhancedUI.EnhancedScroller.EnhancedScroller;
using UnityEngine.UIElements;

namespace Framework
{
    public class GroupBase : EnhancedScrollerCellView
    {
        public int groupIndex;
        public int groupStart;
        public int groupEnd;

        public int gridCount;

        public Vector3 groupSize;

        public ScrollerPro scrollerPro;
        //public List<Transform> transformList = new List<Transform>();

        public void InitGroup(ScrollerPro scrollerPro, Transform itemPrefab)
        {
            this.scrollerPro = scrollerPro;
            RectTransform rect = transform as RectTransform;
            groupSize = rect.sizeDelta;
            if (scrollerPro.isGrid)
            {
                RectTransform itemRect = itemPrefab as RectTransform;
                
                if (scrollerPro.scrollDirection == ScrollDirectionEnum.Vertical)
                {
                    gridCount = (int)(rect.sizeDelta.x / (itemRect.sizeDelta.x + scrollerPro.Scroller.spacing));
                }
                else
                {
                    gridCount = (int)(rect.sizeDelta.y / (itemRect.sizeDelta.y + scrollerPro.Scroller.spacing));
                }

                //for (int i = 0; i < gridCount; i++)
                //{
                //    var item = Instantiate(itemPrefab, transform).transform;
                //    item.gameObject.SetActive(false);
                //    transformList.Add(item);
                //}
            }
            else if (!scrollerPro.isGrid)
            {
                gridCount = 1;
                //var item = Instantiate(itemPrefab, transform).transform;
                //item.gameObject.SetActive(false);
                //transformList.Add(item);
            }
            cellIdentifier = "GroupBase";
        }



        public void SetGroup(int groupIndex, int groupStart, int groupEnd)
        {
            this.groupIndex = groupIndex;
            this.groupStart = groupStart;
            this.groupEnd = groupEnd;
        }

        public override void RefreshCellView()
        {
            int max = scrollerPro.GetDataCountHandler();
            int length = 0;
            for (int i = 0; i < gridCount; i++)
            {
                if (groupStart + i >= max) continue;
                length++;
            }


            for (int i = 0; i < length; i++)
            {
                if (active)
                {
                    scrollerPro.GetObjectHandler?.Invoke(this, groupStart + i, groupStart + length - 1);
                    scrollerPro.ProvideDataHandler?.Invoke(groupStart + i);
                }
                else
                {
                    scrollerPro.ReturnObjectHandler?.Invoke(groupStart + i);
                }
            }
        }
    }
}