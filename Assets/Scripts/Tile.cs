using UnityEngine;

public class Tile : MonoBehaviour
{
    [SerializeField] private Color baseColor, offsetcolor;
    [SerializeField] private SpriteRenderer renderer;

    public void Init(bool isOffset)
    {
        renderer.color = isOffset ? offsetcolor : baseColor;
    }
}
