using UnityEngine; // Geeft toegang tot Unity-functionaliteiten zoals MonoBehaviour, Debug en GameObject.

// Basisklasse van alle tegels. Abstract: je maakt altijd een GrassTile of MountainTile, nooit een losse Tile.
public abstract class Tile : MonoBehaviour
{
    [Header("Tegel")]
    [Tooltip("Naam van de tegel, getoond in het menu.")]
    public string tileName;
    [Tooltip("Kun je op dit type tegel staan? Gras wel, berg niet. Een tegel die niet bewandelbaar is, houdt ook pijlen tegen.")]
    [SerializeField] private bool isWalkable;

    [Header("Onderdelen")]
    [Tooltip("De SpriteRenderer van de tegel.")]
    [SerializeField] protected SpriteRenderer renderer;
    [Tooltip("Het object dat de tegel laat oplichten.")]
    [SerializeField] private GameObject highlight;

    [Header("Status tijdens het spel (niet aanpassen)")]
    public BaseUnit occupiedUnit; // De unit die nu op deze tegel staat, of null.

    public bool walkable => isWalkable && occupiedUnit == null; // Je kunt hier staan én de tegel is leeg.
    public bool canPassThrough => isWalkable && !(occupiedUnit is BaseHero); // Een held mag hier doorheen: geen berg en geen andere held. Een vijand mag er wel staan.
    public bool blocksArrows => !isWalkable; // Bergen houden pijlen tegen.

    public bool isOnPath; // Ligt deze tegel op de getekende route?
    public bool isInRange; // Ligt deze tegel in het schietbereik van de boog?

    public virtual void Init(int x, int y) // Wordt aangeroepen als GridManager de tegel aanmaakt. Virtual: GrassTile vult dit zelf in.
    {
    }

    public void SetUnit(BaseUnit unit) // Zet een unit op deze tegel.
    {
        if (unit.occupiedTile != null) unit.occupiedTile.occupiedUnit = null; // Maakt de vorige tegel van de unit vrij.
        unit.transform.position = transform.position; // Zet de unit precies op deze tegel.
        occupiedUnit = unit; // Deze tegel weet welke unit erop staat...
        unit.occupiedTile = this; // ...en de unit weet op welke tegel hij staat.
    }

    // ---------------------------------------------------------------- Oplichten

    public void SetOnPath(bool onPath) // Zet de tegel op de route of haalt hem eraf.
    {
        isOnPath = onPath;
        UpdateHighlight();
    }

    public void SetInRange(bool inRange) // Zet de tegel in het schietbereik of haalt hem eruit.
    {
        isInRange = inRange;
        UpdateHighlight();
    }

    private void UpdateHighlight() // Licht op zolang de tegel op de route of in het schietbereik ligt.
    {
        highlight.SetActive(isOnPath || isInRange);
    }

    // ---------------------------------------------------------------- Muis

    private void OnMouseEnter() // De muis komt boven deze tegel.
    {
        highlight.SetActive(true); // Laat de tegel oplichten.
        MenuManager.instance.ShowTileInfo(this); // Toont de info van deze tegel.
    }

    private void OnMouseExit() // De muis verlaat deze tegel.
    {
        UpdateHighlight(); // Blijft alleen oplichten als hij op de route of in het bereik ligt.
        MenuManager.instance.ShowTileInfo(null); // Verbergt de tegelinfo.
    }

    private void OnMouseDown() // Er wordt op deze tegel geklikt.
    {
        Debug.Log("Tile werd aangeklikt");

        if (GameManager.Instance.State != GameManager.GameState.HeroesTurn) // Niet de beurt van de speler?
        {
            Debug.Log("Niet de heroes turn");
            return;
        }

        if (UnitManager.instance.isAiming) // Aan het richten met de boog?
        {
            UnitManager.instance.TryShoot(this); // Dan schieten op deze tegel...
            return; // ...en geen route tekenen.
        }

        if (occupiedUnit == null) return; // Lege tegel: niks te doen.

        Debug.Log("Tile is niet leeg");
        if (occupiedUnit.faction == Faction.Hero) // Een held?
        {
            Debug.Log("Hero aangeklikt");
            UnitManager.instance.StartDrawingPath((BaseHero)occupiedUnit); // Selecteert de held en begint een route te tekenen.
        }
        else // Een vijand. Schade doe je door er met de held doorheen te lopen of te schieten.
        {
            Debug.Log("Tegenstander op tile");
        }
    }

    private void OnMouseUp() // De muis wordt losgelaten na een klik op deze tegel.
    {
        UnitManager.instance.FinishDrawingPath(); // De held loopt de getekende route.
    }
}
