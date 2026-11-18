using NUnit.Framework; // Geeft toegang tot NUnit-testfunctionaliteiten. Wordt hier momenteel niet gebruikt.
using System.Collections.Generic; // Geeft toegang tot collecties zoals List.
using System.Linq; // Geeft toegang tot LINQ om gegevens te filteren en sorteren.
using UnityEngine; // Geeft toegang tot Unity-functionaliteiten zoals MonoBehaviour, Instantiate en Random.

public class UnitManager : MonoBehaviour // Maakt de klasse UnitManager aan. Deze beheert het aanmaken en selecteren van units.
{
    public static UnitManager instance; // Maakt een globale verwijzing zodat andere scripts de UnitManager kunnen gebruiken.

    private List<ScriptableUnit> units; // Maakt een lijst waarin alle beschikbare ScriptableUnit-objecten worden opgeslagen.
    public BaseHero selectedHero; // Bewaart welke held momenteel geselecteerd is.

    private void Awake() // Wordt automatisch uitgevoerd wanneer het GameObject wordt geladen.
    {
        instance = this; // Slaat deze UnitManager op als de globale instance.

        units = Resources.LoadAll<ScriptableUnit>("Units").ToList(); // Laadt alle ScriptableUnit-assets uit de map Resources/Units en zet ze om naar een List.
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

            GameManager.Instance.ChangeState(GameManager.GameState.SpawnEnemies); // Verandert de spelstatus naar SpawnEnemies zodat daarna de vijanden aangemaakt worden.

        }
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

            GameManager.Instance.ChangeState(GameManager.GameState.HeroesTurn); // Verandert de spelstatus naar HeroesTurn zodat de helden aan de beurt zijn.
        }
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
}