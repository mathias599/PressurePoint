using System.Collections.Generic; // Geeft toegang tot Dictionary, List en Queue.
using System.Linq; // Geeft toegang tot LINQ (Where, OrderBy, First) om tegels te filteren en sorteren.
using UnityEngine; // Geeft toegang tot Unity-functionaliteiten zoals MonoBehaviour, Vector2 en Random.

public class GridManager : MonoBehaviour // Maakt het speelveld aan en zoekt tegels en routes op.
{
    public static GridManager Instance; // Globale verwijzing zodat andere scripts de GridManager kunnen bereiken.

    [Header("Speelveld")]
    [Tooltip("Aantal tegels in de breedte.")]
    [SerializeField, Min(1)] private int width;
    [Tooltip("Aantal tegels in de hoogte.")]
    [SerializeField, Min(1)] private int height;
    [Tooltip("Kans dat een tegel een berg wordt. 0 = nooit, 1 = altijd. Standaard 1 op 6.")]
    [SerializeField, Range(0f, 1f)] private float mountainChance = 1f / 6f;

    [Header("Tegel-prefabs")]
    [Tooltip("De prefab van een grastegel.")]
    [SerializeField] private Tile grassTile;
    [Tooltip("De prefab van een bergtegel.")]
    [SerializeField] private Tile mountainTile;

    [Header("Camera")]
    [Tooltip("De camera. Wordt na het maken van het speelveld in het midden gezet.")]
    [SerializeField] private Transform cam;
    [Tooltip("Z-positie van de camera. Moet negatief zijn, zodat de camera vóór de tegels staat.")]
    [SerializeField] private float cameraZ = -10f;

    private Dictionary<Vector2, Tile> tiles; // Alle tegels, met hun positie als sleutel.

    private void Awake() // Wordt automatisch uitgevoerd wanneer het GameObject wordt geladen.
    {
        Instance = this; // Slaat deze GridManager op als de globale Instance.
    }

    public void GenerateGrid() // Maakt het volledige speelveld aan.
    {
        tiles = new Dictionary<Vector2, Tile>(); // Begint met een lege verzameling tegels.

        for (int x = 0; x < width; x++) // Elke kolom van het speelveld.
        {
            for (int y = 0; y < height; y++) // Elke rij van het speelveld.
            {
                Tile randomTile = Random.value < mountainChance ? mountainTile : grassTile; // Met kans mountainChance een berg, anders gras.
                Tile spawnedTile = Instantiate(randomTile, new Vector3(x, y), Quaternion.identity); // Maakt de tegel aan op positie (x, y).
                spawnedTile.name = $"Tile {x} {y}"; // Geeft de tegel een naam met zijn coördinaten.
                spawnedTile.Init(x, y); // Laat de tegel zichzelf instellen (bijvoorbeeld de kleur van het gras).

                tiles[new Vector2(x, y)] = spawnedTile; // Bewaart de tegel onder zijn positie.
            }
        }

        cam.transform.position = new Vector3((float)width / 2 - 0.5f, (float)height / 2 - 0.5f, cameraZ); // Zet de camera in het midden, vóór de tegels.

        GameManager.Instance.ChangeState(GameManager.GameState.SpawnHeroes); // Het speelveld is klaar: nu de helden plaatsen.
    }

    public Tile GetHeroSpawnTile() // Geeft een willekeurige vrije tegel op de linkerhelft.
    {
        return tiles.Where(t => t.Key.x < width / 2 && t.Value.walkable).OrderBy(t => Random.value).First().Value; // Vrije tegels links, willekeurig gesorteerd, de eerste.
    }

    public Tile GetEnemySpawnTile() // Geeft een willekeurige vrije tegel op de rechterhelft.
    {
        return tiles.Where(t => t.Key.x > width / 2 && t.Value.walkable).OrderBy(t => Random.value).First().Value; // Vrije tegels rechts, willekeurig gesorteerd, de eerste.
    }

    public Tile GetAtPosition(Vector2 pos) // Geeft de tegel op een positie, of null als daar geen tegel is.
    {
        if (tiles.TryGetValue(pos, out Tile tile)) return tile; // Er ligt een tegel op die positie.
        return null; // Buiten het speelveld.
    }

    public List<Tile> FindPath(Tile start, Tile goal, int maxSteps) // Zoekt de kortste route van start naar goal (zonder diagonalen). Geeft null terug als dat niet lukt.
    {
        Vector2[] directions = { Vector2.up, Vector2.down, Vector2.left, Vector2.right }; // De vier richtingen waarin je kunt stappen.
        var queue = new Queue<Tile>(); // Tegels die we nog moeten bekijken. Wie er eerst in gaat, komt er eerst uit.
        var cameFrom = new Dictionary<Tile, Tile>(); // Per tegel: van welke tegel we kwamen. Zo kunnen we de route terugvinden.
        var steps = new Dictionary<Tile, int>(); // Per tegel: hoeveel stappen hij van de start ligt.

        queue.Enqueue(start); // Begin bij de starttegel...
        steps[start] = 0; // ...die 0 stappen van zichzelf ligt.

        while (queue.Count > 0) // Zolang er nog tegels te bekijken zijn.
        {
            Tile current = queue.Dequeue(); // De volgende tegel.
            if (current == goal) break; // Doel gevonden: stoppen met zoeken.
            if (steps[current] == maxSteps) continue; // Maximum aantal stappen bereikt: vanaf hier niet verder zoeken.

            foreach (Vector2 direction in directions) // Bekijk de buurtegel in elke richting.
            {
                Tile next = GetAtPosition((Vector2)current.transform.position + direction); // De buurtegel.
                if (next == null || !next.walkable || steps.ContainsKey(next)) continue; // Buiten het veld, berg, bezet of al bekeken: overslaan.

                steps[next] = steps[current] + 1; // De buurtegel ligt één stap verder.
                cameFrom[next] = current; // We kwamen er via de huidige tegel.
                queue.Enqueue(next); // Later verder zoeken vanaf de buurtegel.
            }
        }

        if (!steps.ContainsKey(goal)) return null; // Het doel is nooit bereikt: geen route.

        var path = new List<Tile>(); // De route, nog leeg.
        Tile tile = goal; // Begin bij het doel...
        while (tile != start) // ...en loop terug tot de start.
        {
            path.Add(tile); // Tegel in de route zetten.
            tile = cameFrom[tile]; // Naar de tegel waar we vandaan kwamen.
        }
        path.Reverse(); // Omdraaien, zodat de route bij de start begint. De starttegel zelf zit er niet in.
        return path;
    }
}
