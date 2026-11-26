using UnityEngine; // Geeft toegang tot Unity-functionaliteiten zoals MonoBehaviour en GameObject.
using UnityEngine.UI; // Geeft toegang tot UI-elementen zoals Text.

public class MenuManager : MonoBehaviour // Maakt de klasse MenuManager aan. Deze beheert de menu's en informatie die op het scherm verschijnen.
{
    public static MenuManager instance; // Maakt een globale verwijzing zodat andere scripts de MenuManager kunnen gebruiken.
    [Header("Info-vakjes")]
    [Tooltip("Vakje met de naam van de geselecteerde held.")]
    [SerializeField] private GameObject selectedHeroObject;
    [Tooltip("Vakje met de naam van de tegel onder de muis.")]
    [SerializeField] private GameObject tileObject;
    [Tooltip("Vakje met de naam van de unit op de tegel onder de muis.")]
    [SerializeField] private GameObject tileUnitObject;

    [Header("Pijlenteller")]
    [Tooltip("De tekst linksboven die het aantal pijlen toont.")]
    [SerializeField] private Text arrowText;
    [Tooltip("Tekst vóór het aantal pijlen. \"Pijlen: \" geeft bijvoorbeeld \"Pijlen: 1 / 2\".")]
    [SerializeField] private string arrowTextPrefix = "Pijlen: ";

    private void Awake() // Wordt automatisch uitgevoerd wanneer het GameObject wordt geladen.
    {
        instance = this; // Slaat deze MenuManager op als de globale instance.
    }

    private void Update() // Wordt elke frame uitgevoerd. Houdt de pijltekst linksboven bij.
    {
        if (arrowText == null) return; // Er is geen tekst gekoppeld: niks doen.

        if (UnitManager.instance.heroes.Count == 0) // Geen held meer (game over)?
        {
            arrowText.text = ""; // Dan de tekst leegmaken.
            return; // Stopt de methode.
        }

        BaseHero hero = UnitManager.instance.heroes[0]; // De Knight.
        arrowText.text = arrowTextPrefix + hero.arrowsLeft + " / " + hero.maxArrows; // Bijvoorbeeld "Pijlen: 1 / 2".
    }

    public void ShowTileInfo(Tile tile) // Methode die informatie toont over de tegel waar de muis op staat. tile is die tegel, of null om de informatie te verbergen.
    {
        if (tile == null) // Controleert of er geen tegel is doorgegeven.
        {
            tileObject.SetActive(false); // Verbergt het UI-object met de informatie over de tegel.
            tileUnitObject.SetActive(false); // Verbergt het UI-object met informatie over de unit op de tegel.
            return; // Stopt de methode zodat de rest niet uitgevoerd wordt.
        }

        tileObject.GetComponentInChildren<Text>().text = tile.tileName; // Zoekt een Text-component in de children van het UI-object en verandert de tekst naar de naam van de tegel.
        tileObject.SetActive(true); // Maakt het UI-object met de tegelinformatie zichtbaar.

        if (tile.occupiedUnit) // Controleert of er een unit op de tegel staat.
        {
            tileUnitObject.GetComponentInChildren<Text>().text = tile.occupiedUnit.unitName; // Haalt de naam van de unit op en toont die in het bijbehorende Text-element.
            tileUnitObject.SetActive(true); // Maakt het UI-object met de unitinformatie zichtbaar.
        }
    }

    public void ShowSelectedHero(BaseHero hero) // Methode die informatie over de geselecteerde held toont. hero is die held, of null om de informatie te verbergen.
    {
        if (hero == null) // Controleert of er momenteel geen held geselecteerd is.
        {
            selectedHeroObject.SetActive(false); // Verbergt het UI-object met de informatie over de geselecteerde held.
            return; // Stopt de methode zodat de rest niet uitgevoerd wordt.
        }

        selectedHeroObject.GetComponentInChildren<Text>().text = hero.unitName; // Zoekt een Text-component in de children van het UI-object en stelt de tekst in op de naam van de geselecteerde held.
        selectedHeroObject.SetActive(true); // Maakt het UI-object met de informatie over de geselecteerde held zichtbaar.
    }
}