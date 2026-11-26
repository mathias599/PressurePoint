using System.Collections; // Geeft toegang tot IEnumerator, nodig voor coroutines.
using System.Collections.Generic; // Geeft toegang tot List.
using System.Linq; // Geeft toegang tot LINQ (Where, OrderBy, FirstOrDefault).
using UnityEngine; // Geeft toegang tot Unity-functionaliteiten zoals Vector2.

// Basisklasse van alle vijanden: loopt naar de dichtstbijzijnde held en valt aan.
public class BaseEnemy : BaseUnit
{
    [Header("Vijand-AI")]
    [Tooltip("Hoe ver (in stappen) de vijand maximaal een route naar de held zoekt. Hoger = beter om bergen heen, maar trager.")]
    [SerializeField, Min(1)] private int pathSearchLimit = 100;

    private static readonly Vector2[] directions = { Vector2.up, Vector2.down, Vector2.left, Vector2.right }; // De vier richtingen (geen diagonalen).

    public virtual IEnumerator TakeTurn() // De beurt van deze vijand. Wordt door UnitManager gestart. Virtual: een vijand kan eigen gedrag krijgen.
    {
        BaseHero target = GetClosestHero(); // De dichtstbijzijnde held.
        if (target == null) yield break; // Geen helden meer.

        for (int step = 0; step < moveRange; step++) // Maximaal moveRange stappen.
        {
            if (IsAdjacentTo(target)) break; // Staat al naast de held.

            Tile next = GetStepTowards(target); // Eerste stap van de kortste route naar de held.
            if (next == null) break; // Geen route (bijvoorbeeld ingesloten).

            yield return GlideTo(next); // Loopt naar die tegel en wacht tot hij er is.
        }

        if (IsAdjacentTo(target)) Attack(target); // Naast de held? Aanvallen.
    }

    protected BaseHero GetClosestHero() // De held die het dichtst bij staat, of null.
    {
        return UnitManager.instance.heroes
            .Where(hero => hero != null) // Vernietigde helden overslaan.
            .OrderBy(hero => DistanceTo(hero)) // Dichtstbijzijnde eerst.
            .FirstOrDefault(); // De eerste, of null als de lijst leeg is.
    }

    protected Tile GetStepTowards(BaseUnit target) // Eerste tegel van de kortste route naar een vrije tegel naast het doelwit, of null.
    {
        List<Tile> bestPath = null; // Kortste route tot nu toe.

        foreach (Vector2 direction in directions) // Elke tegel rond het doelwit.
        {
            Tile goal = GridManager.Instance.GetAtPosition(target.GridPos + direction); // Tegel naast het doelwit.
            if (goal == null || !goal.walkable) continue; // Buiten het veld, berg of bezet.

            List<Tile> path = GridManager.Instance.FindPath(occupiedTile, goal, pathSearchLimit); // Kortste route om bergen en units heen.
            if (path == null || path.Count == 0) continue; // Daar kan hij niet komen.

            if (bestPath == null || path.Count < bestPath.Count) bestPath = path; // Korter dan de vorige beste? Onthouden.
        }

        return bestPath != null ? bestPath[0] : null; // Eerste stap, of null als er geen route is.
    }
}
