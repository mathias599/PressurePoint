using UnityEngine; // Geeft toegang tot ScriptableObject en CreateAssetMenu.

[CreateAssetMenu(fileName = "New Unit", menuName = "Scriptable Unit")] // Maakt Assets > Create > Scriptable Unit mogelijk.
public class ScriptableUnit : ScriptableObject // Een asset dat zegt welke prefab bij een unit hoort en bij welke factie hij hoort.
{
    public Faction Faction; // Hero of Enemy.
    public BaseUnit UnitPrefab; // De prefab van deze unit.
}

public enum Faction { Hero = 0, Enemy = 1 } // De twee facties.
