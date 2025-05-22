using System;
using System.Collections;
using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;




public class ButtonEx : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
{

    public bool scalable = true;
    // 缩放初始值
    // 缩放结束值
    public Vector3 downTo = new Vector3(1f, 1f, 1f);
    // 缩放初始值
    // 缩放中间过度值
    public Vector3 upMidTo = new Vector3(0.9f, 0.9f, 1);
    // 缩放结束值
    public Vector3 upTo = new Vector3(1f, 1f, 1f);
    // 缩放时间
    public float Duration = 0.1f;

    public bool hasSound = true;

    public string soundName = "ui_click";

    private Tweener _startTweener;
    private Tweener _midTweener;
    private Tweener _endTweener;

    public Func<string> SetSound;
    public Action onPointDown;
    public Action onPointUp;
    public Action onPointEnter;
    public Action onPointExit;
    public bool scaleConfig=false;//是否根据初始大小进行调整缩放值

    public void Awake()
    {
        //不知道为啥要指定，不能自定义 为避免影响别的组件 加个参数 给需要设置值的组件 读取配置
        // downTo = new Vector3(1, 1, 1f);
        // upMidTo = new Vector3(0.9f, 0.9f, 1);
        // upTo = new Vector3(1f, 1f, 1f);

        if(scaleConfig)
        {
            var scale=transform.localScale.x;
            downTo = new Vector3(scale, scale, 1f);
            upMidTo = new Vector3(scale * 0.9f, scale * 0.9f, 1);
            upTo = new Vector3(scale, scale, 1f);

        }else
        {
            downTo = new Vector3(1, 1, 1f);
            upMidTo = new Vector3(0.9f, 0.9f, 1);
            upTo = new Vector3(1f, 1f, 1f);
        }
    }
    public void OnPointerDown(PointerEventData eventData)
    {
        onPointDown?.Invoke();

        if (scalable)
        {
            if (transform != null)
            {
                if (_startTweener != null)
                {
                    _startTweener.Kill();
                    _startTweener = null;
                }
                _startTweener = this.transform.DOScale(downTo, Duration).SetUpdate(true);
            }
        }
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        onPointUp?.Invoke();

        if (scalable)
        {
            if (transform != null)
            {
                if (_midTweener != null)
                {
                    _midTweener.Kill();
                    _midTweener = null;
                }
                _midTweener = this.transform.DOScale(upMidTo, Duration).SetUpdate(true);
                _midTweener.onComplete = () =>
                {
                    if (transform != null)
                    {
                        if (_endTweener != null)
                        {
                            _endTweener.Kill();
                            _endTweener = null;
                        }
                        _endTweener = this.transform.DOScale(upTo, Duration).SetUpdate(true);
                    }
                };
            }
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        hasSound = true;
        if (hasSound)
        {
            if (soundName.Equals("UI_Click"))
            {
                soundName = "ui_click";
            }
            var realSoundName = (SetSound == null) ? soundName : SetSound?.Invoke();
            // TODO ： 播放音效
            // HotfixFunc.CallPublicStaticMethod("Hotfix", "GameUtil", "PlayUISound", realSoundName);
        }
    }

    private void OnDestroy()
    {
        Clear();
    }
    public void Clear() 
    {
        gameObject.transform.localScale = new Vector3(1f, 1f, 1f);
        if (_startTweener != null) _startTweener.Kill();
        if (_midTweener != null) _midTweener.Kill();
        if (_endTweener != null) _endTweener.Kill();
    }
    public void OnPointerEnter(PointerEventData eventData)
    {
        onPointEnter?.Invoke();
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        onPointExit?.Invoke();
    }
}
