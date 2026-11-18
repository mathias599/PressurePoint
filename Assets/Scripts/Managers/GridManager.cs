using UnityEngine; // Haalt de Unity-functionaliteiten binnen.
using System.Collections; // Maakt werken met collecties en IEnumerator mogelijk. Wordt hier momenteel niet gebruikt.
using System.Collections.Generic; // Maakt gebruik van bijvoorbeeld Dictionary mogelijk.
using System; // Haalt algemene C#-functionaliteiten binnen. Daardoor moet je hieronder UnityEngine.Random voluit schrijven.
using System.Runtime.InteropServices.WindowsRuntime; // Functionaliteiten voor communicatie met Windows Runtime. Wordt hier momenteel niet gebruikt.
using System.Linq; // Maakt het mogelijk om lijsten en dictionaries te filteren en sorteren.
using Unity.VisualScripting; // Haalt de Unity Visual Scripting-functionaliteiten binnen. Wordt hier momenteel niet gebruikt.

public class GridManager : MonoBehaviour // Maakt de klasse GridManager aan, die aan een Unity GameObject gekoppeld kan worden.
{
    public static GridManager Instance; // Maakt een statische variabele zodat andere scripts GridManager kunnen bereiken.

    [SerializeField] private int width, height; // Bepaalt de breedte en hoogte van het speelveld. SerializeField maakt deze zichtbaar in de Inspector.

    [SerializeField] private Tile grassTile, mountainTile; // Verwijst naar de grass- en mountain-tile prefabs die in Unity ingesteld worden.

    [SerializeField] private Transform cam; // Verwijst naar de Transform van de camera.

    private Dictionary<Vector2, Tile> tiles; // Bewaart alle tegels met hun positie als sleutel en hun Tile als waarde.

    private void Awake() // Wordt automatisch uitgevoerd wanneer het object wordt geladen.
    {
        Instance = this; // Slaat deze GridManager op zodat andere scripts hem kunnen gebruiken.
    }

    public void GenerateGrid() // Methode die het volledige speelveld aanmaakt.
    {
        tiles = new Dictionary<Vector2, Tile>(); // Maakt een nieuwe Dictionary aan om alle tegels in op te slaan.

        for (int x = 0; x < width; x++) // Herhaalt de code voor elke positie in de breedte.
        {
            for (int y = 0; y < height; y++) // Herhaalt de code voor elke positie in de hoogte.
            {
                var randomTile = UnityEngine.Random.Range(0, 6) == 3 ? mountainTile : grassTile; // Kiest een getal van 0 t/m 5. Bij 3 wordt het een berg (kans van 1 op 6), anders gras.

                var spawnedTile = Instantiate(randomTile, new Vector3(x, y), Quaternion.identity); // Maakt de gekozen tegel aan op positie x en y, zonder rotatie.

                spawnedTile.name = $"Tile {x} {y}"; // Geeft de aangemaakte tegel een naam met zijn x- en y-coördinaten.

                spawnedTile.Init(x, y); // Initialiseert de tegel en geeft zijn positie door.


                tiles[new Vector2(x, y)] = spawnedTile; // Slaat de aangemaakte tegel op in de Dictionary met zijn positie als sleutel.
            }
        }

        cam.transform.position = new Vector3((float)width / 2 - 0.5f, (float)height / 2 - 0.5f, -10); // Zet de camera in het midden van het speelveld. De -10 plaatst hem vóór de tegels zodat hij ze kan zien.

        GameManager.Instance.ChangeState(GameManager.GameState.SpawnHeroes); // Verandert de gamestate zodat de helden op het speelveld geplaatst kunnen worden.
    }

    public Tile GetHeroSpawnTile() // Methode die een willekeurige vrije starttegel voor de held zoekt.
    {
        return tiles.Where(t => t.Key.x < width / 2 && t.Value.walkable).OrderBy(t => UnityEngine.Random.value).First().Value; // Zoekt bewandelbare tegels op de linkerhelft, sorteert ze willekeurig en kiest de eerste.
    }

    public Tile GetEnemySpawnTile() // Methode die een willekeurige vrije starttegel voor de vijand zoekt.
    {
        return tiles.Where(t => t.Key.x > width / 2 && t.Value.walkable).OrderBy(t => UnityEngine.Random.value).First().Value; // Zoekt bewandelbare tegels op de rechterhelft, sorteert ze willekeurig en kiest de eerste.
    }



    public Tile GetAtPosition(Vector2 pos) // Methode die de tegel op een bepaalde positie opzoekt.
    {
        if (tiles.TryGetValue(pos, out var tile)) // Controleert of er een tegel bestaat op de opgegeven positie en slaat die op in tile.
        {
            return tile; // Geeft de gevonden tegel terug.
        }

        return null; // Geeft null terug als er geen tegel op die positie bestaat.
    }

    public List<Tile> FindPath(Tile start, Tile goal, int maxSteps)
    {
        Vector2[] directions = { Vector2.up, Vector2.down, Vector2.left, Vector2.right };
        var queue = new Queue<Tile>();
        var cameFrom = new Dictionary<Tile, Tile>();
        var steps = new Dictionary<Tile, int>();

        queue.Enqueue(start);
        steps[start] = 0;

        while (queue.Count > 0)
        {
            Tile current = queue.Dequeue();
            if (current == goal) break;
            if (steps[current] == maxSteps) continue;

            foreach (Vector2 direction in directions)
            {
                Tile next = GetAtPosition((Vector2)current.transform.position + direction);
                if (next == null || !next.walkable || steps.ContainsKey(next)) continue;

                steps[next] = steps[current] + 1;
                cameFrom[next] = current;
                queue.Enqueue(next);
            }
        }

        if (!steps.ContainsKey(goal)) return null;

        var path = new List<Tile>();
        Tile tile = goal;
        while (tile != start)
        {
            path.Add(tile);
            tile = cameFrom[tile];
        }
        path.Reverse();
        return path;
    }
}