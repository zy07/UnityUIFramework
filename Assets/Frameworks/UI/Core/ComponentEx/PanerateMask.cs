using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections.Generic;

public class PanerateMask : MonoBehaviour,IPointerClickHandler, IPointerDownHandler, IDragHandler
{
    private List<RaycastResult> _rawRaycastResults = new List<RaycastResult>();

    public List<GameObject> TargetObj = null;
    
    public void OnPointerClick(PointerEventData eventData)
    {
        Raycast(eventData);
    }

    private void Raycast(PointerEventData eventData)
    {
        _rawRaycastResults.Clear();
        EventSystem.current.RaycastAll(eventData, _rawRaycastResults);
        foreach (var rlt in _rawRaycastResults)
        {
            // Debug.Log(rlt.gameObject);
            //遮罩层自身需要添加该脚本，否则会导致ExecuteEvents.Execute再次触发遮罩层自身的IPointerClickHandler导致死循环
            if (rlt.gameObject.GetComponent<IgnoreEventRaycast>())
            {
                continue;
            }

            if (TargetObj != null)
            {
                if (TargetObj.Count == 0) return;
                if(TargetObj.Contains(rlt.gameObject))
                    ExecuteEvents.ExecuteHierarchy(rlt.gameObject, eventData, ExecuteEvents.pointerClickHandler);
            }
                
        }
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        _rawRaycastResults.Clear();
        EventSystem.current.RaycastAll(eventData, _rawRaycastResults);
        foreach (var rlt in _rawRaycastResults)
        {
            Debug.Log(rlt.gameObject);
            //遮罩层自身需要添加该脚本，否则会导致ExecuteEvents.Execute再次触发遮罩层自身的IPointerClickHandler导致死循环
            if (rlt.gameObject.GetComponent<IgnoreEventRaycast>())
            {
                continue;
            }

            if (TargetObj != null)
            {
                if (TargetObj.Count == 0) return;
                if (TargetObj.Contains(rlt.gameObject))
                {
                    ExecuteEvents.ExecuteHierarchy(rlt.gameObject, eventData, ExecuteEvents.pointerDownHandler);
                    ExecuteEvents.ExecuteHierarchy(rlt.gameObject, eventData, ExecuteEvents.pointerUpHandler);
                }
            }
                
        }
    }

    public void OnDrag(PointerEventData eventData)
    {
        _rawRaycastResults.Clear();
        EventSystem.current.RaycastAll(eventData, _rawRaycastResults);
        foreach (var rlt in _rawRaycastResults)
        {
            Debug.Log(rlt.gameObject);
            //遮罩层自身需要添加该脚本，否则会导致ExecuteEvents.Execute再次触发遮罩层自身的IPointerClickHandler导致死循环
            if (rlt.gameObject.GetComponent<IgnoreEventRaycast>())
            {
                continue;
            }

            if (TargetObj != null)
            {
                if (TargetObj.Count == 0) return;
                if(TargetObj.Contains(rlt.gameObject))
                    ExecuteEvents.ExecuteHierarchy(rlt.gameObject, eventData, ExecuteEvents.dragHandler);
            }
                
        }
    }
}