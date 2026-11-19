using UnityEngine; // Geeft toegang tot Unity-functionaliteiten zoals MonoBehaviour, Mathf en Destroy.
using System.Collections; // Nodig voor IEnumerator (coroutines).
using System.Collections.Generic; // Nodig voor List.

public class BaseUnit : MonoBehaviour // Maakt de klasse BaseUnit aan. Dit is de basisklasse van alle helden en vijanden.
{
    public string unitName; // Bewaart de naam van de unit.
    public Tile occupiedTile; // Bewaart op welke tegel deze unit momenteel staat.
    public Faction faction; // Bewaart tot welke factie deze unit behoort: Hero of Enemy.

    [Header("Health")] // Toont een aparte titel 'Health' in de Unity Inspector.
    public int maxHealth = 10; // Bepaalt de maximale hoeveelheid levenspunten van de unit.
    public int currentHealth; // Bewaart de huidige hoeveelheid levenspunten.
    public int attackDamage = 3; // Bepaalt hoeveel schade deze unit doet bij een aanval.
    public int moveRange = 3; // Hoeveel tegels deze unit per beurt mag lopen. Gebruikt door de route van de held en de vijand-AI.
    public float moveSpeed = 4f; // Hoe snel de unit glijdt (tegels per seconde).
    public bool isMoving; // True zolang de unit aan het lopen is.

    protected virtual void Awake() // Wordt automatisch uitgevoerd wanneer het GameObject wordt geladen. Protected virtual betekent dat afgeleide klassen deze methode kunnen overschrijven.
    {
        currentHealth = maxHealth; // Zet de huidige levenspunten gelijk aan de maximale levenspunten bij het starten.
    }

    public IEnumerator GlideTo(Tile tile) // Verplaatst de unit langzaam naar de tegel
    {
        Vector3 target = tile.transform.position; // Waar de unit naartoe moet
        while (transform.position != target)
        {
            transform.position = Vector3.MoveTowards(transform.position, target, moveSpeed * Time.deltaTime);
            yield return null; // Wacht één frame
        }
        tile.SetUnit(this); // Zet de unit officieel op de tegel
    }

    // ---- Gedeelde acties: helden en vijanden gebruiken exact dezelfde methodes ----

    public void MoveTo(Tile tile) // Verplaatst deze unit naar de opgegeven tegel.
    {
        tile.SetUnit(this); // Laat de tegel de unit plaatsen. SetUnit maakt ook de vorige tegel vrij.
    }

    public void Attack(BaseUnit target) // Valt een andere unit aan.
    {
        target.TakeDamage(attackDamage); // Doet schade aan het doelwit, gelijk aan de attackDamage van deze unit.
    }

    public Vector2 GridPos => occupiedTile.transform.position; // Geeft de positie van deze unit op het raster. De tegels staan precies op hun x- en y-coördinaat.

    public int DistanceTo(BaseUnit other) // Berekent hoeveel stappen (zonder diagonalen) deze unit van een andere unit verwijderd is.
    {
        var d = GridPos - other.GridPos; // Berekent het verschil in x en y tussen beide units.
        return (int)(Mathf.Abs(d.x) + Mathf.Abs(d.y)); // Telt de absolute verschillen op. Dit heet de Manhattan-afstand.
    }

    public bool IsAdjacentTo(BaseUnit other) => DistanceTo(other) == 1; // Geeft true als de andere unit direct boven, onder, links of rechts staat.

    // ------------------------------------------------------------------------------

    public void TakeDamage(int amount) // Methode die schade toebrengt aan de unit. amount bepaalt hoeveel schade de unit krijgt.
    {
        currentHealth = Mathf.Max(currentHealth - amount, 0); // Trekt de schade af van de huidige levenspunten. Mathf.Max zorgt ervoor dat de levenspunten nooit onder 0 zakken.

        if (currentHealth <= 0) // Controleert of de unit geen levenspunten meer heeft.
        {
            Die(); // Roept de Die-methode op wanneer de unit verslagen is.
        }
    }

    public void Heal(int amount) // Methode die de unit geneest. amount bepaalt hoeveel levenspunten erbij komen.
    {
        currentHealth = Mathf.Min(currentHealth + amount, maxHealth); // Voegt levenspunten toe, maar Mathf.Min zorgt ervoor dat de maximale levenspunten niet overschreden worden.
    }

    protected virtual void Die() // Methode die uitgevoerd wordt wanneer de unit sterft. Virtual betekent dat andere klassen deze methode kunnen overschrijven.
    {
        if (occupiedTile != null) occupiedTile.occupiedUnit = null; // Controleert of de unit op een tegel staat en maakt die tegel daarna vrij.
        UnitManager.instance.RemoveUnit(this); // Haalt de unit uit de lijsten van UnitManager zodat dode units geen beurt meer krijgen.
        Destroy(gameObject); // Verwijdert het volledige GameObject van de unit uit de scène.
    }
    public IEnumerator MoveAlongPath(List<Tile> path)
    {
        isMoving = true;

        foreach (Tile tile in path)
        {
            Vector3 targetPosition = tile.transform.position;

            while (transform.position != targetPosition)
            {
                transform.position = Vector3.MoveTowards(transform.position, targetPosition, moveSpeed * Time.deltaTime);
                yield return null;
            }

            tile.SetOnPath(false);
            BaseUnit unitOnTile = tile.occupiedUnit;

            if (unitOnTile != null && unitOnTile.faction != faction)
            {
                unitOnTile.TakeDamage(attackDamage);
            }

            if (tile.occupiedUnit == null)
            {
                tile.SetUnit(this);
            }
        }

        isMoving = false;
    }
}