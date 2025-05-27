using UnityEngine;
using UnityEngine.UI;

public class EmptyGraphic : MaskableGraphic
{
    protected override void OnPopulateMesh(VertexHelper vh)
    {
        vh.Clear();
    }
}
