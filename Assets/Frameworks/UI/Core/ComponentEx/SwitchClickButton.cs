using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Framework
{
    [Serializable]
    public class SwitchClickEvent : UnityEvent<bool> { }
    public class SwitchClickButton : Button
    {

        [SerializeField]
        public SwitchClickEvent switchClick;



        public override void OnPointerDown(PointerEventData eventData)
        {
            base.OnPointerDown(eventData);
            switchClick?.Invoke(true);
        }
        public override void OnPointerUp(PointerEventData eventData)
        {
            base.OnPointerUp(eventData);
            switchClick?.Invoke(false);
        }
        public override void OnPointerExit(PointerEventData eventData)
        {
            base.OnPointerExit(eventData);

        }


    }
}