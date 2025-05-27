using System;
using UnityEngine;
using UnityEngine.EventSystems;

public class UIDragEvent : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler
{

    public Action<Vector2> OnUIStartDrag;

    public Action<Vector2> OnUIDrag;

    public Action<Vector2> OnUIEndDrag;
    
    public void OnPointerDown(PointerEventData eventData)
    {
        OnUIStartDrag?.Invoke(eventData.position);
    }

    public void OnDrag(PointerEventData eventData)
    {
        OnUIDrag?.Invoke(eventData.position);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        OnUIEndDrag?.Invoke(eventData.position);
    }

    public void ResetDragEvent()
    {
        OnUIStartDrag = null;
        OnUIDrag = null;
        OnUIEndDrag = null;
    }
}