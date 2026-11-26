using System.Collections; // Geeft toegang tot IEnumerator, nodig voor coroutines.
using System.Collections.Generic; // Geeft toegang tot List.
using UnityEngine; // Geeft toegang tot Unity-functionaliteiten zoals MonoBehaviour, Mathf en Destroy.

// Basisklasse van alle helden en vijanden: health, lopen en aanvallen.
public class BaseUnit : MonoBehaviour
{
    [Header("Algemeen")]
    [Tooltip("Naam van de unit, getoond in het menu.")]
    public string unitName;
    [Tooltip("Hero of Enemy. Bepaalt wie vriend en wie vijand is.")]
    public Faction faction;

    [Header("Health")]
    [Tooltip("Levenspunten waarmee de unit begint.")]
    [Min(1)] public int maxHealth = 10;

    [Header("Aanval")]
    [Tooltip("Schade per aanval (ook als de held door een vijand heen loopt).")]
    [Min(0)] public int attackDamage = 3;

    [Header("Lopen")]
    [Tooltip("Hoeveel tegels de unit per beurt mag lopen.")]
    [Min(0)] public int moveRange = 3;
    [Tooltip("Loopsnelheid, in tegels per seconde.")]
    [Min(0.1f)] public float moveSpeed = 4f;

    [Header("Animatie")]
    [Tooltip("Speelt de aanvalsanimatie af. Leeg laten = Awake zoekt zelf een Animator op deze unit.")]
    [SerializeField] private Animator animator;
    [Tooltip("Naam van de trigger in de Animator die de aanvalsanimatie start.")]
    [SerializeField] private string attackTrigger = "AttackLeft";

    [Header("Status tijdens het spel (niet aanpassen)")]
    public int currentHealth; // Huidige levenspunten. Wordt bij de start op maxHealth gezet.
    public Tile occupiedTile; // De tegel waar de unit nu op staat.
    public bool isMoving; // True zolang de unit een route aan het lopen is.

    public Vector2 GridPos => occupiedTile.transform.position; // Positie op het raster (tegels staan precies op hun x en y).

    protected virtual void Awake() // Wordt automatisch uitgevoerd bij het laden. Virtual: BaseHero vult dit aan.
    {
        currentHealth = maxHealth; // Begint met volle health.
        if (animator == null) animator = GetComponentInChildren<Animator>(); // Zoekt zelf een Animator als er geen gekoppeld is.
    }

    // ---------------------------------------------------------------- Afstand

    public int DistanceTo(BaseUnit other) // Aantal stappen (zonder diagonalen) tot een andere unit.
    {
        Vector2 difference = GridPos - other.GridPos; // Verschil in x en y.
        return (int)(Mathf.Abs(difference.x) + Mathf.Abs(difference.y)); // Stappen in x plus stappen in y.
    }

    public bool IsAdjacentTo(BaseUnit other) => DistanceTo(other) == 1; // Staat de andere unit direct boven, onder, links of rechts?

    // ---------------------------------------------------------------- Health

    public void Attack(BaseUnit target) // Valt een andere unit aan.
    {
        if (animator != null)
            animator.SetTrigger(attackTrigger); // Speelt de aanvalsanimatie af.
        else
            Debug.LogWarning(name + " heeft geen Animator!", this);

        target.TakeDamage(attackDamage); // Doet schade aan het doelwit.
    }

    public void TakeDamage(int amount) // Verliest levenspunten. Bij 0 gaat de unit dood.
    {
        currentHealth = Mathf.Max(currentHealth - amount, 0); // Nooit onder 0.
        if (currentHealth <= 0) Die();
    }

    public void Heal(int amount) // Krijgt levenspunten terug, maar nooit meer dan maxHealth.
    {
        currentHealth = Mathf.Min(currentHealth + amount, maxHealth);
    }

    protected virtual void Die() // De unit sterft. Virtual: een unit kan hier zijn eigen gedrag aan toevoegen.
    {
        if (occupiedTile != null) occupiedTile.occupiedUnit = null; // Maakt zijn tegel vrij.
        UnitManager.instance.RemoveUnit(this); // Haalt hem uit de lijsten, zodat hij geen beurt meer krijgt.
        Destroy(gameObject); // Verwijdert de unit uit de scène.
    }

    // ---------------------------------------------------------------- Lopen

    public IEnumerator GlideTo(Tile tile) // Schuift de unit rustig naar één tegel en zet hem er daarna op. Gebruikt door de vijanden.
    {
        Vector3 target = tile.transform.position; // Waar de unit naartoe moet.
        while (transform.position != target) // Tot de unit er is...
        {
            transform.position = Vector3.MoveTowards(transform.position, target, moveSpeed * Time.deltaTime); // ...een klein stukje opschuiven...
            yield return null; // ...en een frame wachten.
        }
        tile.SetUnit(this); // Zet de unit officieel op de tegel.
    }

    public IEnumerator MoveAlongPath(List<Tile> path) // Laat de held tegel voor tegel een route lopen. Vijanden onderweg krijgen schade.
    {
        isMoving = true;

        foreach (Tile tile in path) // Elke tegel van de route.
        {
            Vector3 target = tile.transform.position; // De volgende tegel.
            while (transform.position != target) // Tot de unit er is...
            {
                transform.position = Vector3.MoveTowards(transform.position, target, moveSpeed * Time.deltaTime); // ...een klein stukje opschuiven...
                yield return null; // ...en een frame wachten.
            }

            tile.SetOnPath(false); // Deze tegel is bereikt: niet meer oplichten.

            BaseUnit unitOnTile = tile.occupiedUnit; // Staat hier iemand?
            if (unitOnTile != null && unitOnTile.faction != faction) Attack(unitOnTile); // Een vijand: aanvallen.

            if (tile.occupiedUnit == null) tile.SetUnit(this); // Tegel leeg (of de vijand is net gestorven)? Dan erop gaan staan. Anders loopt de unit er gewoon doorheen.
        }

        isMoving = false;
    }
}
