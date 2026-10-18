    using UnityEngine;
using UnityEngine.UIElements;

public class GrassTile : Tile
{
    [SerializeField] private Color baseColor, offsetcolor;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public override void Init(int x, int y)
    {
        var isOffset = (x + y) % 2 == 1;
        renderer.color = isOffset ? offsetcolor : baseColor;
    }
}
