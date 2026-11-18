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
        highlight.SetActive(false); // Deactiveert het highlight-effect zodat de tegel niet meer oplicht.
        MenuManager.instance.ShowTileInfo(null); // Verbergt de getoonde tegelinformatie in het menu.
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
                UnitManager.instance.SetSelectedHero((BaseHero)occupiedUnit); // Selecteert de aangeklikte unit als held. (BaseHero) zet de BaseUnit om naar BaseHero.
            }

            else // Wordt uitgevoerd wanneer de unit geen held is, dus een vijand.
            {
                Debug.Log("Tegenstander op tile"); // Toont in de Console dat er een tegenstander op de tegel staat.
                if (UnitManager.instance.selectedHero != null) // Controleert of er momenteel een held geselecteerd is.
                {
                    Debug.Log("Damaged Enemy"); // Toont in de Console dat de vijand schade krijgt.
                    var enemy = (BaseEnemy)occupiedUnit; // Zet de bezette unit om naar BaseEnemy zodat we de vijand-specifieke klasse kunnen gebruiken.
                    enemy.TakeDamage(UnitManager.instance.selectedHero.attackDamage); // Doet schade aan de vijand, gelijk aan de attackDamage van de geselecteerde held.
                    UnitManager.instance.SetSelectedHero(null); // Deselecteert de held nadat de aanval uitgevoerd is.
                }
            }
        }
        else // Wordt uitgevoerd wanneer er GEEN unit op deze tegel staat.
        {
            if (UnitManager.instance.selectedHero != null) // Controleert of er een held geselecteerd is.
            {
                Vector2 mouseWorldPos = Camera.main.ScreenToWorldPoint(Mouse.current.position.ReadValue()); // Leest de positie van de muis op het scherm en zet die om naar wereldcoördinaten.
                RaycastHit2D hit = Physics2D.Raycast(mouseWorldPos, Vector2.zero); // Voert een 2D Raycast uit op de muispositie om te controleren welk object geraakt wordt.
                if (hit.collider != null) // Controleert of de Raycast een collider gevonden heeft.
                {
                    Tile clickedTile = hit.collider.GetComponent<Tile>(); // Probeert de Tile-component op het aangeklikte object te vinden.

                    if (clickedTile != null) // Controleert of het aangeklikte object daadwerkelijk een Tile-component heeft.
                    {
                        if (clickedTile.isWalkable) // Controleert of je op dit type tegel kunt staan. Dat de tegel leeg is, weten we hier al.
                        {
                            SetUnit(UnitManager.instance.selectedHero); // Verplaatst de geselecteerde held naar deze tegel.
                            UnitManager.instance.SetSelectedHero(null); // Deselecteert de held nadat hij verplaatst is.

                        }
                    }
                }

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