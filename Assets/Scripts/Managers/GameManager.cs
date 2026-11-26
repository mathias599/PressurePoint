using System; // Geeft toegang tot Action, nodig voor het event hieronder.
using UnityEngine; // Geeft toegang tot Unity-functionaliteiten zoals MonoBehaviour.

public class GameManager : MonoBehaviour // Beheert de spelstatus: welke fase van het spel nu bezig is.
{
    public static GameManager Instance; // Globale verwijzing zodat andere scripts de GameManager kunnen bereiken.

    public GameState State; // De huidige status van het spel.

    public static event Action<GameState> OnGameStateChanged; // Event voor scripts die willen weten wanneer de spelstatus verandert. Geeft de nieuwe status mee.

    private void Awake() // Wordt automatisch uitgevoerd wanneer het GameObject wordt geladen.
    {
        Instance = this; // Slaat deze GameManager op als de globale Instance.
    }

    private void Start() // Wordt automatisch één keer uitgevoerd vlak voor de eerste frame.
    {
        ChangeState(GameState.GenerateGrid); // Het spel begint met het aanmaken van het speelveld.
    }

    public void ChangeState(GameState newState) // Verandert de spelstatus en start wat bij die status hoort.
    {
        State = newState; // Slaat de nieuwe spelstatus op.

        switch (newState) // Voert de code uit die bij de nieuwe status hoort.
        {
            case GameState.GenerateGrid: // Het speelveld moet aangemaakt worden.
                GridManager.Instance.GenerateGrid(); // GridManager maakt de tegels aan en gaat daarna naar SpawnHeroes.
                break;

            case GameState.SpawnHeroes: // De helden moeten geplaatst worden.
                UnitManager.instance.SpawnHeroes(); // UnitManager plaatst de helden en gaat daarna naar SpawnEnemies.
                break;

            case GameState.SpawnEnemies: // De vijanden moeten geplaatst worden.
                UnitManager.instance.SpawnEnemies(); // UnitManager plaatst de vijanden en gaat daarna naar HeroesTurn.
                break;

            case GameState.HeroesTurn: // De speler is aan de beurt. Er hoeft hier niks te starten: de speler klikt zelf.
                break;

            case GameState.EnemiesTurn: // De vijanden zijn aan de beurt.
                UnitManager.instance.StartEnemyTurn(); // Elke vijand doet om de beurt zijn actie.
                break;
        }

        OnGameStateChanged?.Invoke(newState); // Laat abonnees weten dat de status veranderd is. ?. = alleen als er abonnees zijn.
    }

    public enum GameState // Alle mogelijke spelstatussen, in de volgorde waarin het spel ze doorloopt.
    {
        GenerateGrid = 0, // Het speelveld wordt aangemaakt.
        SpawnHeroes = 1, // De helden worden geplaatst.
        SpawnEnemies = 2, // De vijanden worden geplaatst.
        HeroesTurn = 3, // De helden (de speler) zijn aan de beurt.
        EnemiesTurn = 4 // De vijanden zijn aan de beurt.
    }
}
