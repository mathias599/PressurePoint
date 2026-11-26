using UnityEngine; // Geeft toegang tot Unity-functionaliteiten zoals Color.

public class GrassTile : Tile // De grastegel. Erft alles van Tile.
{
    [Header("Kleuren")]
    [Tooltip("Kleur van de ene helft van het dambordpatroon.")]
    [SerializeField] private Color baseColor;
    [Tooltip("Kleur van de andere helft van het dambordpatroon.")]
    [SerializeField] private Color offsetcolor;

    public override void Init(int x, int y) // Wordt aangeroepen als GridManager deze tegel aanmaakt.
    {
        bool isOffset = (x + y) % 2 == 1; // True als x + y oneven is: zo ontstaat een dambordpatroon.
        renderer.color = isOffset ? offsetcolor : baseColor; // Om en om de ene of de andere kleur.
    }
}
