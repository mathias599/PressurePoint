using UnityEngine;

public class BaseHero : BaseUnit // Maakt de klasse BaseHero aan. Deze erft alles van BaseUnit. Dit is de basisklasse van alle helden.
{
    [Header("Pijl en boog")] // Toont een aparte titel 'Pijl en boog' in de Inspector.
    public bool hasBow = true; // Bepaalt of deze held een boog heeft.
    public int bowDamage = 2; // Hoeveel schade één pijl doet.
    public int bowRange = 2; // Aantal tiles in elke richting. 2 = een vak van 5x5, 3 = 7x7 (de Knight-prefab staat op 3).
    public GameObject bowObject; // Het boog-plaatje op de held. Wordt zichtbaar als je op B drukt.
    public int maxArrows = 2; // Hoeveel pijlen de held in totaal heeft.
    public int arrowsLeft; // Hoeveel pijlen de held nog over heeft.

    protected override void Awake() // Vervangt de Awake van BaseUnit.
    {
        base.Awake(); // Doet eerst alles van BaseUnit (health op max zetten).
        arrowsLeft = maxArrows; // Begint met alle pijlen.
    }
}
