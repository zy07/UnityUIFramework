using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIRoot
{
    [SerializeField]
    private Camera _camera;
    public new Camera camera { get { return _camera; } }
    [SerializeField]
    private Canvas _canvas;
    public Canvas canvas { get { return _canvas; } }

    public GameObject gameObject;

    public Transform transform => gameObject.transform;

    public void Init()
    {
        _camera = _camera ?? this.transform.GetComponentInChildren<Camera>();
        _canvas = _canvas ?? this.transform.GetComponentInChildren<Canvas>();
    }
}
