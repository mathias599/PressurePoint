using UnityEngine;

public class Tile : MonoBehaviour
{
    [SerializeField] private Color baseColor, offsetcolor;
    [SerializeField] private SpriteRenderer renderer;
    [SerializeField] private GameObject highlight;
    public void Init(bool isOffset)
    {
        renderer.color = isOffset ? offsetcolor : baseColor;
    }

    private void OnMouseEnter()
    {
          highlight.SetActive(true);
    }

    private void OnMouseExit()
    {
        highlight.SetActive(false);
    }

}
