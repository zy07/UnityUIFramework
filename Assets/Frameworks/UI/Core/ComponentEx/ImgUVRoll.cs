using System;
using UnityEngine;
using UnityEngine.UI;

public class ImgUVRoll : MonoBehaviour
{
    private RawImage Image = null;

    public float speed_x = 0.1f;
    public float speed_y = 0.1f;

    public void Start()
    {
        Image = this.GetComponent<RawImage>();
    }

    public void Update()
    {
        if (Image == null) return;
        float x = Image.uvRect.x;
        float y = Image.uvRect.y;
        float width = Image.uvRect.width;
        float height = Image.uvRect.height;
        x -= speed_x * Time.deltaTime;
        y -= speed_y * Time.deltaTime;
        Image.uvRect = new Rect(x, y, width, height);
    }
}
