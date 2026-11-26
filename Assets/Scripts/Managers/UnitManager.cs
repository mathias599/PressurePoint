using System.Collections; // Geeft toegang tot IEnumerator, nodig voor coroutines.
using System.Collections.Generic; // Geeft toegang tot List.
using System.Linq; // Geeft toegang tot LINQ (Where, OrderBy, ToList).
using UnityEngine; // Geeft toegang tot Unity-functionaliteiten zoals MonoBehaviour, Instantiate en Random.
using UnityEngine.InputSystem; // Geeft toegang tot het nieuwe Input System (muis en toetsenbord).

// Beheert alle units: spawnen, de route van de held, de boog en de beurten van de vijanden.
public class UnitManager : MonoBehaviour
{
    public static UnitManager instance; // Globale verwijzing zodat andere scripts de UnitManager kunnen bereiken.

    [Header("Spawnen")]
    [Tooltip("Hoeveel helden er bij de start op het speelveld komen.")]
    [SerializeField, Min(0)] private int heroCount = 1;
    [Tooltip("Hoeveel vijanden er bij de start op het speelveld komen.")]
    [SerializeField, Min(0)] private int enemyCount = 1;

    [Header("Beurten")]
    [Tooltip("Wachttijd in seconden tussen de acties van de vijanden, zodat je kunt volgen wat er gebeurt.")]
    [SerializeField, Min(0f)] private float enemyActionDelay = 0.5f;

    [Header("Besturing")]
    [Tooltip("Toets om de boog tevoorschijn te halen of weg te doen.")]
    [SerializeField] private Key bowKey = Key.B;

    [Header("Status tijdens het spel (niet aanpassen)")]
    public List<BaseHero> heroes = new List<BaseHero>(); // Alle levende helden.
    public List<BaseEnemy> enemies = new List<BaseEnemy>(); // Alle levende vijanden.
    public BaseHero selectedHero; // De held die nu geselecteerd is.
    public List<Tile> drawnPath = new List<Tile>(); // De route die de speler tekent. De eerste tegel is de tegel van de held.
    public bool isDrawingPath; // True zolang de speler een route aan het tekenen is.
    public bool isAiming; // True als de speler met de boog aan het richten is.

    private List<ScriptableUnit> units; // Alle units uit de map Resources/Units waaruit gekozen kan worden.
    private BaseHero aimingHero; // De held die aan het richten is.
    private List<Tile> rangeTiles = new List<Tile>(); // Alle tegels in het schietbereik.

    private void Awake() // Wordt automatisch uitgevoerd wanneer het GameObject wordt geladen.
    {
        instance = this; // Slaat deze UnitManager op als de globale instance.
        units = Resources.LoadAll<ScriptableUnit>("Units").ToList(); // Laadt alle ScriptableUnit-assets uit Resources/Units.
    }

    private void Update() // Wordt elke frame uitgevoerd.
    {
        if (Keyboard.current[bowKey].wasPressedThisFrame) ToggleBow(); // Boog-toets (standaard B): boog tevoorschijn halen of wegdoen.
        if (isDrawingPath) FollowMouseWithPath(); // Tijdens het tekenen volgt de route de muis.
    }

    // ---------------------------------------------------------------- Spawnen

    public void SpawnHeroes() // Maakt de helden aan en zet ze op het speelveld.
    {
        for (int i = 0; i < heroCount; i++)
        {
            BaseHero spawnedHero = Instantiate(GetRandomUnit<BaseHero>(Faction.Hero)); // Maakt een willekeurige held aan.
            Tile spawnTile = GridManager.Instance.GetHeroSpawnTile(); // Een vrije tegel op de linkerhelft.

            spawnTile.SetUnit(spawnedHero); // Zet de held op die tegel.
            heroes.Add(spawnedHero); // Voegt hem toe aan de lijst met helden.
        }

        GameManager.Instance.ChangeState(GameManager.GameState.SpawnEnemies); // Daarna zijn de vijanden aan de beurt om geplaatst te worden.
    }

    public void SpawnEnemies() // Maakt de vijanden aan en zet ze op het speelveld.
    {
        for (int i = 0; i < enemyCount; i++)
        {
            BaseEnemy spawnedEnemy = Instantiate(GetRandomUnit<BaseEnemy>(Faction.Enemy)); // Maakt een willekeurige vijand aan.
            Tile spawnTile = GridManager.Instance.GetEnemySpawnTile(); // Een vrije tegel op de rechterhelft.

            spawnTile.SetUnit(spawnedEnemy); // Zet de vijand op die tegel.
            enemies.Add(spawnedEnemy); // Voegt hem toe aan de lijst met vijanden.
        }

        GameManager.Instance.ChangeState(GameManager.GameState.HeroesTurn); // Alles staat klaar: de speler begint.
    }

    private T GetRandomUnit<T>(Faction faction) where T : BaseUnit // Kiest een willekeurige unit-prefab van een factie. T is het type (BaseHero of BaseEnemy).
    {
        return (T)units.Where(u => u.Faction == faction).OrderBy(o => Random.value).First().UnitPrefab; // Units van die factie, willekeurig gesorteerd, de eerste.
    }

    public void RemoveUnit(BaseUnit unit) // Haalt een gestorven unit uit de juiste lijst.
    {
        if (unit is BaseHero hero) heroes.Remove(hero); // Een held uit de heldenlijst.
        if (unit is BaseEnemy enemy) enemies.Remove(enemy); // Een vijand uit de vijandenlijst.
    }

    // ---------------------------------------------------------------- Held selecteren en route tekenen

    public void SetSelectedHero(BaseHero hero) // Selecteert een held, of deselecteert met null.
    {
        selectedHero = hero; // Onthoudt de geselecteerde held.
        MenuManager.instance.ShowSelectedHero(hero); // Toont zijn naam, of verbergt de info bij null.
    }

    public void StartDrawingPath(BaseHero hero) // Begint een route te tekenen voor de aangeklikte held.
    {
        if (hero.isMoving) return; // Nog aan het lopen: geen nieuwe route.
        if (GameManager.Instance.State != GameManager.GameState.HeroesTurn) return; // Alleen tijdens de beurt van de speler.

        SetSelectedHero(hero); // Selecteert de held.
        drawnPath.Clear(); // Gooit een oude route weg.
        drawnPath.Add(hero.occupiedTile); // De route begint op de tegel van de held.
        isDrawingPath = true; // Vanaf nu wordt er getekend.
    }

    private void FollowMouseWithPath() // Vult de route stap voor stap aan tot aan de tegel onder de muis, zodat er geen tegels overgeslagen worden.
    {
        Vector2 mouseWorld = Camera.main.ScreenToWorldPoint(Mouse.current.position.ReadValue()); // Muispositie in de spelwereld.
        Vector2 mouseTilePos = new Vector2(Mathf.Round(mouseWorld.x), Mathf.Round(mouseWorld.y)); // Afgerond, want tegels staan op hele getallen.

        for (int i = 0; i < 20; i++) // Maximaal 20 stappen per frame, zodat de lus nooit eindeloos doorgaat.
        {
            Vector2 lastPos = drawnPath[drawnPath.Count - 1].transform.position; // Positie van de laatste tegel van de route.
            if (lastPos == mouseTilePos) break; // De route is bij de muis.

            Vector2 difference = mouseTilePos - lastPos; // Hoe ver de muis nog weg is, in x en y.
            Vector2 step; // Eén stap naar links, rechts, omhoog of omlaag.
            if (Mathf.Abs(difference.x) >= Mathf.Abs(difference.y)) // Horizontaal verder weg?
                step = new Vector2(Mathf.Sign(difference.x), 0); // Dan een stap naar links of rechts. Mathf.Sign geeft -1 of 1.
            else
                step = new Vector2(0, Mathf.Sign(difference.y)); // Anders een stap omhoog of omlaag.

            Tile nextTile = GridManager.Instance.GetAtPosition(lastPos + step); // De tegel van die stap.
            if (nextTile == null) break; // Buiten het speelveld.

            int countBefore = drawnPath.Count; // Lengte van de route vóór deze stap.
            AddTileToPath(nextTile); // Probeert de tegel toe te voegen (of haalt de laatste weg bij een stap terug).
            if (drawnPath.Count == countBefore) break; // Niks veranderd (berg, buiten bereik, ...): stoppen.
        }
    }

    public void AddTileToPath(Tile tile) // Voegt een tegel toe aan de route, als dat mag.
    {
        if (!isDrawingPath) return; // Er wordt niet getekend.

        int last = drawnPath.Count - 1; // Index van de laatste tegel van de route.

        if (last > 0 && tile == drawnPath[last - 1]) // Gaat de muis terug naar de vorige tegel?
        {
            drawnPath[last].SetOnPath(false); // Laatste tegel uitzetten...
            drawnPath.RemoveAt(last); // ...en uit de route halen (stap ongedaan maken).
            return;
        }

        if (drawnPath.Contains(tile)) return; // Zit al in de route.
        if (last >= selectedHero.moveRange) return; // Alle stappen zijn op.
        if (!tile.canPassThrough) return; // Berg of een andere held.

        Vector2 difference = tile.transform.position - drawnPath[last].transform.position; // Afstand tot de laatste tegel van de route.
        if (difference.magnitude > 1.1f) return; // Niet direct ernaast (te ver of schuin).

        drawnPath.Add(tile); // Voegt de tegel toe...
        tile.SetOnPath(true); // ...en laat hem oplichten.
    }

    public void FinishDrawingPath() // Wordt uitgevoerd als de speler de muis loslaat: de held loopt de route.
    {
        if (!isDrawingPath) return; // Er werd niet getekend.
        isDrawingPath = false; // Het tekenen is klaar.

        drawnPath.RemoveAt(0); // De starttegel hoeft de held niet te lopen.

        while (drawnPath.Count > 0 && !drawnPath[drawnPath.Count - 1].walkable) // Zolang de laatste tegel bezet is (bijvoorbeeld door een vijand)...
        {
            drawnPath[drawnPath.Count - 1].SetOnPath(false); // ...zet hem uit...
            drawnPath.RemoveAt(drawnPath.Count - 1); // ...en haal hem weg, want de held mag niet op een bezette tegel eindigen.
        }

        if (drawnPath.Count == 0) return; // Geen route over: de held blijft staan.

        StartCoroutine(HeroMoveRoutine(selectedHero, new List<Tile>(drawnPath))); // Laat de held lopen (met een kopie van de route).
        SetSelectedHero(null); // Deselecteert de held.
    }

    private IEnumerator HeroMoveRoutine(BaseHero hero, List<Tile> path) // Wacht tot de held klaar is met lopen en geeft dan de beurt door.
    {
        yield return hero.StartCoroutine(hero.MoveAlongPath(path)); // De held loopt de route (en doet schade aan vijanden onderweg).
        HeroActionDone(); // Daarna zijn de vijanden aan de beurt.
    }

    // ---------------------------------------------------------------- Beurten

    public void HeroActionDone() // De held heeft zijn actie gedaan (lopen of schieten): de vijanden zijn aan de beurt.
    {
        SetSelectedHero(null); // Deselecteert de held.
        GameManager.Instance.ChangeState(GameManager.GameState.EnemiesTurn); // Geeft de beurt aan de vijanden.
    }

    public void StartEnemyTurn() // Start de beurt van de vijanden. Wordt aangeroepen door GameManager.
    {
        StartCoroutine(EnemyTurnRoutine()); // Coroutine, zodat we tussen de acties kunnen wachten.
    }

    private IEnumerator EnemyTurnRoutine() // Laat alle vijanden één voor één hun beurt doen.
    {
        yield return new WaitForSeconds(enemyActionDelay); // Even wachten voor de eerste vijand.

        foreach (BaseEnemy enemy in enemies.ToList()) // Over een kopie van de lijst, want units kunnen sterven tijdens de beurt.
        {
            if (enemy == null) continue; // Ondertussen vernietigd: overslaan.
            yield return enemy.StartCoroutine(enemy.TakeTurn()); // De vijand doet zijn beurt; we wachten tot hij klaar is.
            yield return new WaitForSeconds(enemyActionDelay); // Even wachten voor de volgende vijand.
        }

        if (heroes.Count == 0) // Geen helden meer?
        {
            Debug.Log("Game over"); // Alle helden zijn dood.
            yield break; // De beurt gaat niet terug naar de speler.
        }

        GameManager.Instance.ChangeState(GameManager.GameState.HeroesTurn); // De speler is weer aan de beurt.
    }

    // ---------------------------------------------------------------- Pijl en boog

    public void ToggleBow() // Haalt de boog tevoorschijn, of doet hem weg als je al aan het richten was.
    {
        if (isAiming) { StopAiming(); return; } // Al aan het richten: B doet de boog weg.

        if (GameManager.Instance.State != GameManager.GameState.HeroesTurn) return; // Alleen tijdens jouw beurt.
        if (isDrawingPath || heroes.Count == 0) return; // Niet tijdens het tekenen, en alleen als er een held is.

        BaseHero hero = heroes[0]; // De eerste held (de Knight).
        if (!hero.hasBow || hero.isMoving) return; // Geen boog, of nog aan het lopen.
        if (hero.arrowsLeft <= 0) return; // Geen pijlen meer.

        aimingHero = hero; // Onthoudt welke held richt.
        isAiming = true; // Vanaf nu wordt er gericht.
        if (hero.bowObject != null) hero.bowObject.SetActive(true); // Laat het boog-plaatje zien.

        for (int x = -hero.bowRange; x <= hero.bowRange; x++) // Alle kolommen van het schietvak (bowRange 3 = 7x7).
        {
            for (int y = -hero.bowRange; y <= hero.bowRange; y++) // Alle rijen van het schietvak.
            {
                if (x == 0 && y == 0) continue; // De tegel van de held zelf niet.

                Tile tile = GridManager.Instance.GetAtPosition(hero.GridPos + new Vector2(x, y)); // De tegel op die plek.
                if (tile == null) continue; // Buiten het speelveld.

                tile.SetInRange(true); // Laat de tegel oplichten.
                rangeTiles.Add(tile); // Onthoudt hem, zodat hij later weer uit kan.
            }
        }
    }

    private void StopAiming() // Stopt met richten en doet de boog weg.
    {
        foreach (Tile tile in rangeTiles) tile.SetInRange(false); // Zet alle tegels van het bereik weer uit.
        rangeTiles.Clear(); // Maakt de lijst leeg.

        if (aimingHero != null && aimingHero.bowObject != null) aimingHero.bowObject.SetActive(false); // Verbergt het boog-plaatje.
        aimingHero = null; // Er richt geen held meer.
        isAiming = false; // Niet meer aan het richten.
    }

    public void TryShoot(Tile target) // Probeert te schieten op de aangeklikte tegel.
    {
        if (!rangeTiles.Contains(target)) return; // Buiten bereik.
        if (!(target.occupiedUnit is BaseEnemy enemy)) return; // Daar staat geen vijand.

        if (!HasLineOfSight(aimingHero.occupiedTile, target)) // Staat er iets tussen?
        {
            Debug.Log("Er staat iets in de weg!");
            return;
        }

        enemy.TakeDamage(aimingHero.bowDamage); // Doet schade aan de vijand.
        aimingHero.arrowsLeft--; // Eén pijl minder. Moet vóór StopAiming, want die maakt aimingHero leeg.
        StopAiming(); // Doet de boog weg en zet het bereik uit.
        HeroActionDone(); // Schieten kost je beurt.
    }

    private bool HasLineOfSight(Tile from, Tile to) // Kijkt of er niks (berg of unit) tussen twee tegels staat.
    {
        Vector2 start = from.transform.position; // Positie van de held.
        Vector2 end = to.transform.position; // Positie van de vijand.
        int samples = Mathf.CeilToInt(Vector2.Distance(start, end) * 10); // 10 controles per tegel afstand.

        for (int i = 1; i < samples; i++) // Punten op de lijn tussen held en vijand.
        {
            Vector2 point = Vector2.Lerp(start, end, (float)i / samples); // Een punt op de lijn.
            Vector2 tilePos = new Vector2(Mathf.Round(point.x), Mathf.Round(point.y)); // De tegel waar dat punt op ligt.
            Tile tile = GridManager.Instance.GetAtPosition(tilePos);

            if (tile == null || tile == from || tile == to) continue; // Start- en doeltegel tellen niet mee.
            if (tile.blocksArrows || tile.occupiedUnit != null) return false; // Berg of unit in de weg.
        }
        return true; // Niks in de weg.
    }
}
