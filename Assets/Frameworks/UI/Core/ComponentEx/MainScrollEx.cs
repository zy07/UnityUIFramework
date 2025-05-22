using System;
using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class MainScrollEx : MonoBehaviour, IBeginDragHandler, IEndDragHandler
{
    public ScrollRect ScrollRect;
    public GameObject Content;
    public bool FlickSnap = false;
    public float Speed;

    public Action<int> OnScroll2Index;

    private RectTransform _contentRectTrans;
    private readonly Vector3[] _corners = new Vector3[4];
    private bool _dragging = false;
    private bool _flicking = false;
    private Coroutine _coroutine;

    private float[] pos = new float[]
    {
        0, -1080, -1080*2, -1080*3, -1080*4
    };

    public void Awake()
    {
        if (ScrollRect == null)
        {
            ScrollRect = this.GetComponent<ScrollRect>();
        }

        if (ScrollRect == null)
        {
            Debug.LogError("请先挂载一个ScrollRect");
            return;
        }

        if (Content == null)
        {
            Content = ScrollRect.transform.Find("Content").gameObject;
        }

        if (Content == null)
        {
            Debug.LogError("请先在ScrollRect下挂一个Content");
            return;
        }

        _contentRectTrans = Content.transform as RectTransform;
        float xSpeed = ScrollRect.horizontal ? Speed : 0;
        float ySpeed = ScrollRect.vertical ? Speed : 0;
        ScrollRect.velocity = new Vector2(xSpeed, ySpeed);
        _flicking = false;
    }

    void LateUpdate()
    {
        if (Content == null || ScrollRect == null)
            return;

        // 检查是否需要跳转
        if (FlickSnap && !_dragging && _flicking)
        {
            float minest = float.MaxValue;
            int minestIdx = -1;
            for (int i = 0; i < pos.Length; i++)
            {
                float min = Mathf.Abs(_contentRectTrans.anchoredPosition.x - pos[i]);
                if (min < minest)
                {
                    minest = min;
                    minestIdx = i;
                }
            }

            if (minestIdx != -1)
            {
                _flicking = false;
                _coroutine = StartCoroutine(ScrollTo(minestIdx, 5000));
            }
        }
    }

    public void StartByIndex(int index)
    {
        _contentRectTrans.anchoredPosition = new Vector2(pos[index], _contentRectTrans.anchoredPosition.y);
    }

    public void ScrollTo(int index)
    {
        if (_coroutine != null)
            StopCoroutine(_coroutine);

        _coroutine = StartCoroutine(ScrollTo(index, 5000));
    }

    private IEnumerator ScrollTo(int index, float speed)
    {
        while (Mathf.Abs(_contentRectTrans.anchoredPosition.x - pos[index]) > 0.1f)
        {
            float displacement = speed * Time.deltaTime;
            int dir = pos[index] < _contentRectTrans.anchoredPosition.x ? -1 : 1;
            float newX = _contentRectTrans.anchoredPosition.x + dir * displacement;
            if ((newX < pos[index] && dir == -1) || (newX > pos[index] && dir == 1))
                newX = pos[index];
            _contentRectTrans.anchoredPosition = new Vector2(newX, _contentRectTrans.anchoredPosition.y);
            yield return null;
        }

        _contentRectTrans.anchoredPosition = new Vector2(pos[index], _contentRectTrans.anchoredPosition.y);
        OnScroll2Index?.Invoke(index);
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (_coroutine != null)
        {
            StopCoroutine(_coroutine);
        }

        _dragging = true;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        _dragging = false;
        _flicking = true;
    }

    private void OnDestroy()
    {
        if (_coroutine != null)
            StopCoroutine(_coroutine);
    }
}
