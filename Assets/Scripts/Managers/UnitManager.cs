using System.Collections; // Geeft toegang tot IEnumerator, nodig voor coroutines.
using System.Collections.Generic; // Geeft toegang tot collecties zoals List.
using System.Linq; // Geeft toegang tot LINQ om gegevens te filteren en sorteren.
using UnityEngine; // Geeft toegang tot Unity-functionaliteiten zoals MonoBehaviour, Instantiate en Random.
using UnityEngine.InputSystem; // Geeft toegang tot het nieuwe Input System, hier gebruikt om de muispositie te lezen.

public class UnitManager : MonoBehaviour // Maakt de klasse UnitManager aan. Deze beheert het aanmaken en selecteren van units.
{
    public static UnitManager instance; // Maakt een globale verwijzing zodat andere scripts de UnitManager kunnen gebruiken.

    private List<ScriptableUnit> units; // Maakt een lijst waarin alle beschikbare ScriptableUnit-objecten worden opgeslagen.
    public BaseHero selectedHero; // Bewaart welke held momenteel geselecteerd is.
    public List<Tile> drawnPath = new List<Tile>(); // De route die de speler met de muis tekent. De eerste tegel is de tegel waar de held staat.
    public bool isDrawingPath; // Is true zolang de speler een route aan het tekenen is.
    public bool isAiming; // Is true als de speler met de boog aan het richten is.
    private BaseHero aimingHero; // De held die aan het richten is.
    private List<Tile> rangeTiles = new List<Tile>(); // Alle tiles in het schietbereik.

    public List<BaseHero> heroes = new List<BaseHero>(); // Lijst met alle levende helden op het speelveld.
    public List<BaseEnemy> enemies = new List<BaseEnemy>(); // Lijst met alle levende vijanden op het speelveld.

    [SerializeField] private float enemyActionDelay = 0.5f; // Wachttijd in seconden tussen de acties van de vijanden, zodat de speler kan volgen wat er gebeurt.

    private void Awake() // Wordt automatisch uitgevoerd wanneer het GameObject wordt geladen.
    {
        instance = this; // Slaat deze UnitManager op als de globale instance.

        units = Resources.LoadAll<ScriptableUnit>("Units").ToList(); // Laadt alle ScriptableUnit-assets uit de map Resources/Units en zet ze om naar een List.
    }

    private void Update() // Wordt elke frame uitgevoerd. Volgt de muis terwijl je een route tekent.
    {
        if (Keyboard.current.bKey.wasPressedThisFrame) ToggleBow(); // B = boog tevoorschijn halen of wegdoen.
        if (!isDrawingPath) return; // Als er geen route getekend wordt, hoeft er niks te gebeuren.

        Vector2 mouseWorld = Camera.main.ScreenToWorldPoint(Mouse.current.position.ReadValue()); // Zet de muispositie op het scherm om naar een positie in de spelwereld.
        Vector2 mouseTilePos = new Vector2(Mathf.Round(mouseWorld.x), Mathf.Round(mouseWorld.y)); // Rondt af naar hele getallen, want tiles staan op hele posities. Zo weten we op welke tile de muis staat.

        for (int i = 0; i < 20; i++) // Zet stap voor stap tiles bij tot aan de muis. 20 is een veiligheidsgrens zodat de lus nooit eindeloos doorgaat.
        {
            Vector2 lastPos = drawnPath[drawnPath.Count - 1].transform.position; // Positie van de laatste tile in de route.
            if (lastPos == mouseTilePos) break; // De route is bij de muis aangekomen, dus stoppen.

            Vector2 difference = mouseTilePos - lastPos; // Hoe ver de muis nog van de laatste tile af is, in x en y.
            Vector2 step; // De richting van de volgende stap: 1 tile naar links, rechts, omhoog of omlaag.
            if (Mathf.Abs(difference.x) >= Mathf.Abs(difference.y)) // Als de muis horizontaal verder weg is dan verticaal...
                step = new Vector2(Mathf.Sign(difference.x), 0); // ...zet dan een stap naar links of rechts. Mathf.Sign geeft -1 of 1.
            else // Anders is de muis verticaal verder weg...
                step = new Vector2(0, Mathf.Sign(difference.y)); // ...dus zet een stap omhoog of omlaag.

            Tile nextTile = GridManager.Instance.GetAtPosition(lastPos + step); // Zoekt de tile op de plek van de volgende stap.
            if (nextTile == null) break; // Buiten het speelveld, dus stoppen.

            int countBefore = drawnPath.Count; // Onthoudt hoe lang de route was vóór deze stap.
            AddTileToPath(nextTile); // Probeert de tile toe te voegen. Bij een stap terug wordt de laatste tile juist weggehaald.
            if (drawnPath.Count == countBefore) break; // Er is niks veranderd (berg, buiten bereik, ...), dus stoppen.
        }
    }

    public void SpawnHeroes() // Methode die de helden aanmaakt en op het speelveld plaatst.
    {
        var heroCount = 1; // Bepaalt hoeveel helden er aangemaakt worden. Momenteel is dat 1.

        for (int i = 0; i < heroCount; i++) // Herhaalt de code zolang i kleiner is dan heroCount.
        {
            var randomPrefab = GetRandomUnit<BaseHero>(Faction.Hero); // Kiest een willekeurige held uit de beschikbare units.
            var spawnedHero = Instantiate(randomPrefab); // Maakt een nieuwe instantie van de gekozen held aan.
            var randomSpawnTile = GridManager.Instance.GetHeroSpawnTile(); // Vraagt aan GridManager om een willekeurige vrije spawntegel voor de held.

            randomSpawnTile.SetUnit(spawnedHero); // Plaatst de aangemaakte held op de gekozen tegel.
            heroes.Add(spawnedHero); // Voegt de held toe aan de lijst met helden.
        }

        GameManager.Instance.ChangeState(GameManager.GameState.SpawnEnemies); // Verandert de spelstatus naar SpawnEnemies zodat daarna de vijanden aangemaakt worden. Staat buiten de loop zodat dit maar één keer gebeurt.
    }

    public void SpawnEnemies() // Methode die de vijanden aanmaakt en op het speelveld plaatst.
    {
        var enemyCount = 1; // Bepaalt hoeveel vijanden er aangemaakt worden. Momenteel is dat 1.

        for (int i = 0; i < enemyCount; i++) // Herhaalt de code zolang i kleiner is dan enemyCount.
        {
            var randomPrefab = GetRandomUnit<BaseEnemy>(Faction.Enemy); // Kiest een willekeurige vijand uit de beschikbare units.
            var spawnedEnemy = Instantiate(randomPrefab); // Maakt een nieuwe instantie van de gekozen vijand aan.
            var randomSpawnTile = GridManager.Instance.GetEnemySpawnTile(); // Vraagt aan GridManager om een willekeurige vrije spawntegel voor de vijand.

            randomSpawnTile.SetUnit(spawnedEnemy); // Plaatst de aangemaakte vijand op de gekozen tegel.
            enemies.Add(spawnedEnemy); // Voegt de vijand toe aan de lijst met vijanden.
        }

        GameManager.Instance.ChangeState(GameManager.GameState.HeroesTurn); // Verandert de spelstatus naar HeroesTurn zodat de helden aan de beurt zijn. Staat buiten de loop zodat dit maar één keer gebeurt.
    }


    private T GetRandomUnit<T>(Faction faction) where T : BaseUnit // Generieke methode die een willekeurige unit teruggeeft. T moet een klasse zijn die overerft van BaseUnit.
    {
        return (T)units.Where(u => u.Faction == faction).OrderBy(o => Random.value).First().UnitPrefab; // Filtert de units op basis van hun factie, sorteert ze willekeurig, neemt de eerste en geeft de bijbehorende prefab terug.
    }

    public void SetSelectedHero(BaseHero hero) // Methode waarmee een held geselecteerd of gedeselecteerd wordt.
    {
        selectedHero = hero; // Slaat de doorgegeven held op als de momenteel geselecteerde held.
        MenuManager.instance.ShowSelectedHero(hero); // Geeft de held door aan MenuManager, die zijn naam toont of de informatie verbergt als hero null is.
    }

    public void StartDrawingPath(BaseHero hero) // Methode die begint met het tekenen van een route voor de aangeklikte held.
    {
        if (hero.isMoving) return; // Als de held nog aan het lopen is, mag je geen nieuwe route tekenen.
        if (GameManager.Instance.State != GameManager.GameState.HeroesTurn) return; // Alleen tekenen tijdens de beurt van de helden.

        SetSelectedHero(hero); // Selecteert de held.
        drawnPath.Clear(); // Maakt de oude route leeg.
        drawnPath.Add(hero.occupiedTile); // Zet de tegel van de held als eerste in de route.
        isDrawingPath = true; // Vanaf nu wordt er een route getekend.
    }

    public void AddTileToPath(Tile tile) // Methode die een tegel aan de getekende route toevoegt, als dat mag.
    {
        if (!isDrawingPath) return; // Als er geen route getekend wordt, niks doen.

        int last = drawnPath.Count - 1; // De plek van de laatste tegel in de route.

        if (last > 0 && tile == drawnPath[last - 1]) // Gaat de muis terug naar de vorige tegel?
        {
            drawnPath[last].SetOnPath(false); // Zet de highlight van de laatste tegel uit.
            drawnPath.RemoveAt(last); // Haalt de laatste tegel uit de route (een stap ongedaan maken).
            return; // Stopt de methode.
        }

        if (drawnPath.Contains(tile)) return; // Deze tegel zit al in de route, dus niet nog eens toevoegen.
        if (last >= selectedHero.moveRange) return; // De held heeft al zijn stappen gebruikt.
        if (!tile.canPassThrough) return; // Op deze tegel kan de held niet komen (berg of een andere held).

        // Alleen tiles direct naast de vorige, dus niet schuin
        Vector2 difference = tile.transform.position - drawnPath[last].transform.position; // Verschil in positie tussen deze tegel en de laatste tegel van de route.
        if (difference.magnitude > 1.1f) return; // De tegel ligt niet direct naast de vorige (te ver of schuin), dus niet toevoegen.

        drawnPath.Add(tile); // Voegt de tegel toe aan de route.
        tile.SetOnPath(true); // Laat de tegel oplichten.
    }

    public void FinishDrawingPath() // Methode die wordt uitgevoerd als de speler de muis loslaat. Laat de held de route lopen.
    {
        if (!isDrawingPath) return; // Als er geen route getekend werd, niks doen.
        isDrawingPath = false; // Het tekenen is gestopt.

        drawnPath.RemoveAt(0); // De starttile hoeft hij niet te lopen

        while (drawnPath.Count > 0 && !drawnPath[drawnPath.Count - 1].walkable) // Zolang de laatste tegel bezet is (bv. door de Ghoul)...
        {
            drawnPath[drawnPath.Count - 1].SetOnPath(false); // ...zet zijn highlight uit...
            drawnPath.RemoveAt(drawnPath.Count - 1); // ...en haal hem uit de route, want de held mag niet eindigen op een bezette tegel.
        }

        if (drawnPath.Count == 0) return; // Er is geen route over, dus de held blijft staan.

        StartCoroutine(HeroMoveRoutine(selectedHero, new List<Tile>(drawnPath))); // Laat de held de route lopen en geeft daarna de beurt door.
        SetSelectedHero(null); // Deselecteert de held.
    }

    private IEnumerator HeroMoveRoutine(BaseHero hero, List<Tile> path) // Wacht tot de held klaar is met lopen en geeft dan de beurt aan de vijanden.
    {
        yield return hero.StartCoroutine(hero.MoveAlongPath(path)); // Laat de held de getekende route aflopen (en schade doen aan vijanden onderweg).
        HeroActionDone(); // De held heeft zijn actie gedaan: de beurt gaat naar de vijanden.
    }

    public void RemoveUnit(BaseUnit unit) // Haalt een gestorven unit uit de juiste lijst.
    {
        if (unit is BaseHero h) heroes.Remove(h); // Als de unit een held is, wordt hij uit de heldenlijst verwijderd.
        if (unit is BaseEnemy e) enemies.Remove(e); // Als de unit een vijand is, wordt hij uit de vijandenlijst verwijderd.
    }

    public void HeroActionDone() // Wordt aangeroepen wanneer de held zijn actie gedaan heeft (verplaatsen of aanvallen).
    {
        SetSelectedHero(null); // Deselecteert de held.
        GameManager.Instance.ChangeState(GameManager.GameState.EnemiesTurn); // Geeft de beurt door aan de vijanden.
    }

    public void StartEnemyTurn() // Start de beurt van de vijanden. Wordt aangeroepen door GameManager.
    {
        StartCoroutine(EnemyTurnRoutine()); // Start een coroutine zodat we tussen de acties kunnen wachten.
    }

    private IEnumerator EnemyTurnRoutine() // Coroutine die alle vijanden één voor één hun beurt laat doen.
    {
        yield return new WaitForSeconds(enemyActionDelay); // Wacht even voordat de eerste vijand begint.

        foreach (var enemy in enemies.ToList()) // Loopt over een kopie van de lijst, omdat units kunnen sterven tijdens de beurt.
        {
            if (enemy == null) continue; // Slaat vijanden over die ondertussen vernietigd zijn.
            yield return enemy.StartCoroutine(enemy.TakeTurn()); // Laat de vijand zijn beurt doen en wacht tot hij helemaal klaar is.
            yield return new WaitForSeconds(enemyActionDelay); // Wacht even voor de volgende vijand.
        }

        if (heroes.Count == 0) // Controleert of er nog helden over zijn.
        {
            Debug.Log("Game over"); // Alle helden zijn dood.
            yield break; // Stopt de coroutine; de beurt gaat niet terug naar de helden.
        }

        GameManager.Instance.ChangeState(GameManager.GameState.HeroesTurn); // Geeft de beurt terug aan de helden.
    }
    public void ToggleBow() // Haalt de boog tevoorschijn of doet hem weg.
    {
        if (isAiming) { StopAiming(); return; } // Was je al aan het richten? Dan doet B de boog weg.

        if (GameManager.Instance.State != GameManager.GameState.HeroesTurn) return; // Alleen tijdens jouw beurt.
        if (isDrawingPath || heroes.Count == 0) return; // Niet tijdens het tekenen van een route, en alleen als er een held is.

        BaseHero hero = heroes[0]; // De eerste held (de Knight).
        if (!hero.hasBow || hero.isMoving) return; // Geen boog of nog aan het lopen? Dan niks doen.
        if (hero.arrowsLeft <= 0) return; // Geen pijlen meer: de boog kan niet tevoorschijn komen.

        aimingHero = hero; // Onthoudt welke held richt.
        isAiming = true; // Vanaf nu ben je aan het richten.
        if (hero.bowObject != null) hero.bowObject.SetActive(true); // Laat het boog-plaatje zien.

        for (int x = -hero.bowRange; x <= hero.bowRange; x++) // Loopt over alle kolommen van het schietvak (bij bowRange 3 is dat 7x7).
        {
            for (int y = -hero.bowRange; y <= hero.bowRange; y++) // Loopt over alle rijen van het schietvak.
            {
                if (x == 0 && y == 0) continue; // De tile waar de held zelf staat, slaan we over.

                Tile tile = GridManager.Instance.GetAtPosition(hero.GridPos + new Vector2(x, y)); // Zoekt de tile op die plek.
                if (tile == null) continue; // Buiten het speelveld: overslaan.

                tile.SetInRange(true); // Laat de tile oplichten.
                rangeTiles.Add(tile); // Onthoudt de tile, zodat we hem later weer uit kunnen zetten.
            }
        }
    }

    private void StopAiming() // Stopt met richten en doet de boog weg.
    {
        foreach (Tile tile in rangeTiles) tile.SetInRange(false); // Zet alle tiles in het bereik weer uit.
        rangeTiles.Clear(); // Maakt de lijst leeg.

        if (aimingHero != null && aimingHero.bowObject != null) aimingHero.bowObject.SetActive(false); // Verbergt het boog-plaatje.
        aimingHero = null; // Er richt geen held meer.
        isAiming = false; // Niet meer aan het richten.
    }

    public void TryShoot(Tile target) // Probeert te schieten op de aangeklikte tile.
    {
        if (!rangeTiles.Contains(target)) return; // De tile ligt buiten het bereik.
        if (!(target.occupiedUnit is BaseEnemy enemy)) return; // Er staat geen vijand op die tile.

        if (!HasLineOfSight(aimingHero.occupiedTile, target)) // Staat er iets tussen de held en de vijand?
        {
            Debug.Log("Er staat iets in de weg!"); // Toont in de Console dat het schot geblokkeerd is.
            return;
        }

        enemy.TakeDamage(aimingHero.bowDamage); // Doet schade aan de vijand.
        aimingHero.arrowsLeft--; // Haalt één pijl af. Moet vóór StopAiming, want die maakt aimingHero leeg.
        StopAiming(); // Doet de boog weg en zet het bereik uit.
        HeroActionDone(); // Schieten kost je beurt, daarna is de vijand aan de beurt.
    }

    private bool HasLineOfSight(Tile from, Tile to) // Controleert of er niks tussen twee tiles staat.
    {
        Vector2 start = from.transform.position; // Positie van de held.
        Vector2 end = to.transform.position; // Positie van de vijand.
        int samples = Mathf.CeilToInt(Vector2.Distance(start, end) * 10); // 10 controles per tile afstand.

        for (int i = 1; i < samples; i++) // Loopt over punten op de lijn tussen held en vijand.
        {
            Vector2 point = Vector2.Lerp(start, end, (float)i / samples); // Een punt op de lijn.
            Vector2 tilePos = new Vector2(Mathf.Round(point.x), Mathf.Round(point.y)); // Op welke tile ligt dat punt?
            Tile tile = GridManager.Instance.GetAtPosition(tilePos); // Haalt die tile op.

            if (tile == null || tile == from || tile == to) continue; // De start- en doeltile tellen niet mee.
            if (tile.blocksArrows || tile.occupiedUnit != null) return false; // Berg of unit in de weg: niet schieten.
        }
        return true; // Niks in de weg: je kunt schieten.
    }
}