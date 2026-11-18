using UnityEngine; // Geeft toegang tot Unity-functionaliteiten zoals ScriptableObject.

[CreateAssetMenu(fileName = "New Unit", menuName = "Scriptable Unit")] // Zorgt ervoor dat je via Assets > Create > Scriptable Unit een nieuw asset kunt aanmaken. De standaardnaam is New Unit.

public class ScriptableUnit : ScriptableObject // Maakt de klasse ScriptableUnit aan. ScriptableObject laat je gegevens opslaan als een apart asset in Unity.
{
    public Faction Faction; // Bewaart tot welke factie deze unit behoort: Hero of Enemy.
    public BaseUnit UnitPrefab; // Verwijst naar de prefab die bij deze Scriptable Unit hoort.
}

public enum Faction { Hero = 0, Enemy = 1 } // Maakt een enum aan met twee mogelijke facties: Hero heeft waarde 0 en Enemy heeft waarde 1.