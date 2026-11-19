using UnityEditor.Rendering; // Rendering-functionaliteiten van de Unity Editor. Wordt hier momenteel niet gebruikt.
using UnityEngine; // Geeft toegang tot Unity-functionaliteiten zoals MonoBehaviour, Debug, GameObject en Vector2.
using UnityEngine.InputSystem; // Geeft toegang tot het nieuwe Unity Input System, hier gebruikt om de muispositie te lezen.

public abstract class Tile : MonoBehaviour // Maakt een abstracte klasse Tile aan. Abstract betekent dat je hiervan niet rechtstreeks een object kunt maken, maar wel andere klassen kunt laten overerven.
{
    public string tileName; // Bewaart de naam van de tegel.
    [SerializeField] protected SpriteRenderer renderer; // Verwijst naar de SpriteRenderer die de afbeelding van de tegel toont. SerializeField maakt deze zichtbaar in de Inspector en protected maakt hem toegankelijk voor afgeleide klassen.
    [SerializeField] private GameObject highlight; // Verwijst naar het highlight-effect dat zichtbaar wordt wanneer je met de muis over een tegel gaat.
    [SerializeField] private bool isWalkable; // Bepaalt of je op dit type tegel kunt staan. Een bool kan true of false zijn.

    public BaseUnit occupiedUnit; // Bewaart welke unit momenteel op deze tegel staat.

    public bool walkable => isWalkable && occupiedUnit == null; // Controleert of de tegel bewandelbaar is EN of er geen unit op staat. Beide voorwaarden moeten true zijn.
    public bool canPassThrough => isWalkable && !(occupiedUnit is BaseHero);
    public bool isOnPath;

    public virtual void Init(int x, int y) // Methode die een tegel initialiseert met zijn x- en y-coördinaten. Virtual betekent dat afgeleide klassen deze methode kunnen overschrijven.
    {

    }

    private void OnMouseEnter() // Wordt automatisch uitgevoerd wanneer de muis boven deze tegel komt.
    {
        highlight.SetActive(true); // Activeert het highlight-effect zodat de tegel oplicht.
        MenuManager.instance.ShowTileInfo(this); // Toont informatie over deze tegel in het menu. this verwijst naar deze tegel.
    }

    private void OnMouseExit() // Wordt automatisch uitgevoerd wanneer de muis de tegel verlaat.
    {
        highlight.SetActive(isOnPath);
        MenuManager.instance.ShowTileInfo(null); // Verbergt de getoonde tegelinformatie in het menu.
    }
    public void SetOnPath(bool onPath)
    {
        isOnPath = onPath;
        highlight.SetActive(onPath);
    }
    private void OnMouseUp()
    {
        UnitManager.instance.FinishDrawingPath();
    }

    private void OnMouseDown() // Wordt automatisch uitgevoerd wanneer je met de muis op deze tegel klikt.
    {
        Debug.Log("Tile werd aangeklikt"); // Toont een bericht in de Unity Console om te controleren of de tegel aangeklikt werd.
        if (GameManager.Instance.State != GameManager.GameState.HeroesTurn) // Controleert of het NIET de beurt van de helden is.
        {
            Debug.Log("Niet de heroes turn"); // Toont een bericht in de Console dat het niet de beurt van de helden is.
            return; // Stopt de methode onmiddellijk zodat de rest van de code niet uitgevoerd wordt.
        }

        if (occupiedUnit != null) // Controleert of er al een unit op deze tegel staat.
        {
            Debug.Log("Tile is niet leeg"); // Toont in de Console dat de tegel bezet is.
            if (occupiedUnit.faction == Faction.Hero) // Controleert of de unit op deze tegel bij de factie van de helden hoort.
            {
                Debug.Log("Hero aangeklikt"); // Toont in de Console dat een held aangeklikt werd.
                UnitManager.instance.StartDrawingPath((BaseHero)occupiedUnit); // Selecteert de held en begint een route te tekenen.
            }

            else // Wordt uitgevoerd wanneer de unit geen held is, dus een vijand.
            {
                Debug.Log("Tegenstander op tile"); // Toont in de Console dat er een tegenstander op de tegel staat. Schade doe je door met de held door de vijand te lopen.
            }
        }
    }

    public void SetUnit(BaseUnit unit) // Methode die een unit op deze tegel plaatst.
    {
        if (unit.occupiedTile != null) unit.occupiedTile.occupiedUnit = null; // Controleert of de unit al op een andere tegel staat en maakt die vorige tegel vrij.
        unit.transform.position = transform.position; // Verplaatst de unit naar de exacte positie van deze tegel.
        occupiedUnit = unit; // Onthoudt dat deze unit nu op deze tegel staat.
        unit.occupiedTile = this; // Onthoudt bij de unit dat hij nu op deze tegel staat.

    }

}