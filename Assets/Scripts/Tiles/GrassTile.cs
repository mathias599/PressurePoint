    using UnityEngine; // Geeft toegang tot Unity-functionaliteiten zoals Color en SerializeField.
using UnityEngine.UIElements; // Geeft toegang tot UI Toolkit. Wordt hier momenteel niet gebruikt.

public class GrassTile : Tile // Maakt de klasse GrassTile aan. Deze erft alles van Tile en is de grastegel van het speelveld.
{
    [SerializeField] private Color baseColor, offsetcolor; // De twee kleuren van het gras. SerializeField maakt ze zichtbaar in de Inspector.

    public override void Init(int x, int y) // Vervangt de Init-methode van Tile. Wordt uitgevoerd wanneer GridManager deze tegel aanmaakt.
    {
        var isOffset = (x + y) % 2 == 1; // Is true wanneer x + y oneven is. Zo ontstaat een dambordpatroon.
        renderer.color = isOffset ? offsetcolor : baseColor; // Geeft de tegel de offsetkleur als isOffset true is, en anders de basiskleur.
    }
}
