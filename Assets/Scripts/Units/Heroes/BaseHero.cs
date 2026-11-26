using UnityEngine; // Geeft toegang tot Unity-functionaliteiten zoals GameObject en Header.

public class BaseHero : BaseUnit // Basisklasse van alle helden. Erft alles van BaseUnit en heeft daarnaast een boog.
{
    [Header("Pijl en boog")]
    [Tooltip("Kan deze held de boog gebruiken?")]
    public bool hasBow = true;
    [Tooltip("Schade van één pijl.")]
    [Min(0)] public int bowDamage = 2;
    [Tooltip("Bereik in tegels in elke richting. 2 = vak van 5x5, 3 = 7x7, 4 = 9x9.")]
    [Min(1)] public int bowRange = 2;
    [Tooltip("Hoeveel pijlen de held in totaal heeft.")]
    [Min(0)] public int maxArrows = 2;
    [Tooltip("Het boog-plaatje op de held. Is alleen zichtbaar zolang je richt.")]
    public GameObject bowObject;

    [Header("Status tijdens het spel (niet aanpassen)")]
    public int arrowsLeft; // Hoeveel pijlen er nog over zijn. Wordt bij de start op maxArrows gezet.

    protected override void Awake() // Vult de Awake van BaseUnit aan.
    {
        base.Awake(); // Eerst alles van BaseUnit (health op max, Animator zoeken).
        arrowsLeft = maxArrows; // Begint met alle pijlen.
    }
}
