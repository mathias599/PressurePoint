using NUnit.Framework;
using System.Collections; // Geeft toegang tot IEnumerator, nodig voor coroutines.
using System.Collections.Generic; // Geeft toegang tot List, nodig voor het pad van FindPath.
using System.Linq; // Geeft toegang tot LINQ om gegevens te filteren en sorteren.
using System.Threading;
using System.Threading.Tasks;
using UnityEditor.UI;
using UnityEngine; // Geeft toegang tot Unity-functionaliteiten zoals MonoBehaviour, Vector2 en Mathf.

public class BaseEnemy : BaseUnit // Maakt de klasse BaseEnemy aan. Deze erft alles van BaseUnit. Dit is de basisklasse van alle vijanden.
{
    private static readonly Vector2[] directions = // De vier richtingen waarin een vijand kan stappen (geen diagonalen).
        { Vector2.up, Vector2.down, Vector2.left, Vector2.right }; // Omhoog, omlaag, links en rechts.

    public virtual IEnumerator TakeTurn() // Coroutine die UnitManager aanroept tijdens EnemiesTurn. Virtual: een specifieke vijand kan zijn eigen gedrag schrijven.
    {
        var target = GetClosestHero(); // Zoekt de dichtstbijzijnde held.
        if (target == null) yield break; // Geen helden meer: niets te doen.

        for (int step = 0; step < moveRange; step++) // Zet maximaal moveRange stappen, net als de held.
        {
            if (IsAdjacentTo(target)) break; // Staat al naast de held: niet verder lopen.

            var next = GetStepTowards(target); // Zoekt de eerste stap van de kortste route naar de held.
            if (next == null) break; // Geen route gevonden (bv. helemaal ingesloten): stoppen.

            yield return GlideTo(next); // Loopt rustig naar de volgende tegel en wacht tot hij er is.
        }

        if (IsAdjacentTo(target)) // Staat de held nu direct naast deze vijand?
        {
            Attack(target); // Aanvallen met dezelfde Attack-methode die de held gebruikt.
        }
    }

    protected BaseHero GetClosestHero() // Geeft de held terug die het dichtst bij deze vijand staat.
    {
        return UnitManager.instance.heroes // Neemt de lijst met helden uit UnitManager.
            .Where(h => h != null) // Slaat helden over die vernietigd zijn.
            .OrderBy(h => DistanceTo(h)) // Sorteert op afstand tot deze vijand.
            .FirstOrDefault(); // Neemt de dichtstbijzijnde, of null als er geen helden zijn.
    }

    protected Tile GetStepTowards(BaseUnit target) // Geeft de eerste tegel van de kortste route naar een tegel naast het doelwit.
    {
        List<Tile> bestPath = null; // De kortste route tot nu toe. Null = nog niets gevonden.

        foreach (var dir in directions) // Controleert elke tegel rond het doelwit.
        {
            var goal = GridManager.Instance.GetAtPosition(target.GridPos + dir); // De tegel naast het doelwit in die richting.
            if (goal == null || !goal.walkable) continue; // Buiten het raster, berg of bezet: overslaan.

            var path = GridManager.Instance.FindPath(occupiedTile, goal, 100); // Zoekt de kortste route om bergen en units heen. 100 = geen echte limiet.
            if (path == null || path.Count == 0) continue; // Daar kan hij niet komen: overslaan.

            if (bestPath == null || path.Count < bestPath.Count) // Is deze route korter dan de vorige beste?
            {
                bestPath = path; // Nieuwe kortste route onthouden.
            }
        }

        return bestPath != null ? bestPath[0] : null; // Geeft de eerste stap van de kortste route terug, of null als er geen route is.
    }
}
