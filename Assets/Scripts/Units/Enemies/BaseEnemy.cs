using System.Linq; // Geeft toegang tot LINQ om gegevens te filteren en sorteren.
using System.Threading;
using System.Threading.Tasks;
using UnityEngine; // Geeft toegang tot Unity-functionaliteiten zoals MonoBehaviour, Vector2 en Mathf.

public class BaseEnemy : BaseUnit // Maakt de klasse BaseEnemy aan. Deze erft alles van BaseUnit. Dit is de basisklasse van alle vijanden.
{
    private static readonly Vector2[] directions = // De vier richtingen waarin een vijand kan stappen (geen diagonalen).
        { Vector2.up, Vector2.down, Vector2.left, Vector2.right };

    public virtual async Task TakeTurn() // Wordt aangeroepen door UnitManager tijdens EnemiesTurn. Virtual: een specifieke vijand kan zijn eigen gedrag schrijven.
    {
        for (int i = 5; i > 0; i--)
        {
            var target = GetClosestHero(); // Zoekt de dichtstbijzijnde held.
            if (target == null) return; // Geen helden meer: niets te doen.

            if (IsAdjacentTo(target)) // Staat de held direct naast deze vijand?
            {
                Attack(target); // Aanvallen met dezelfde Attack-methode die de held gebruikt.
            }

            for (int j = 0; j < moveRange; j++) // Loopt maximaal moveRange stappen richting de held.
            {
                var next = GetStepTowards(target); // Zoekt de beste buurtegel richting de held.
                if (next == null) break; // Geen betere tegel gevonden (bv. geblokkeerd): stoppen.
                MoveTo(next); // Verplaatsen met dezelfde MoveTo-methode die de held gebruikt.
                if (IsAdjacentTo(target)) break; // Naast de held aangekomen: niet verder lopen.
            }
        }

    }

    protected BaseHero GetClosestHero() // Geeft de held terug die het dichtst bij deze vijand staat.
    {
        return UnitManager.instance.heroes // Neemt de lijst met helden uit UnitManager.
            .Where(h => h != null) // Slaat helden over die vernietigd zijn.
            .OrderBy(h => DistanceTo(h)) // Sorteert op afstand tot deze vijand.
            .FirstOrDefault(); // Neemt de dichtstbijzijnde, of null als er geen helden zijn.
    }

    protected Tile GetStepTowards(BaseUnit target) // Kiest de buurtegel die het dichtst bij het doelwit ligt.
    {
        Tile best = null; // De beste tegel tot nu toe. Null = nog niets gevonden.
        float bestDist = DistanceTo(target); // Een stap moet dichterbij brengen dan waar we nu staan.

        foreach (var dir in directions) // Controleert elke richting.
        {
            var tile = GridManager.Instance.GetAtPosition(GridPos + dir); // Haalt de buurtegel in die richting op.
            if (tile == null || !tile.walkable) continue; // Buiten het raster, berg of bezet: overslaan.

            var d = (Vector2)tile.transform.position - target.GridPos; // Verschil tussen de buurtegel en het doelwit.
            float dist = Mathf.Abs(d.x) + Mathf.Abs(d.y); // Manhattan-afstand vanaf die buurtegel.
            if (dist < bestDist) // Is deze tegel beter dan de vorige beste?
            {
                bestDist = dist; // Nieuwe beste afstand onthouden.
                best = tile; // Nieuwe beste tegel onthouden.
            }
        }
        return best; // Geeft de beste tegel terug, of null als geen enkele stap dichterbij brengt.
    }
}
