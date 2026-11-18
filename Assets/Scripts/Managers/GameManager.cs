using System; // Geeft toegang tot algemene C#-functionaliteiten, zoals Action en events.
using UnityEngine; // Geeft toegang tot Unity-functionaliteiten zoals MonoBehaviour.

public class GameManager : MonoBehaviour // Maakt de klasse GameManager aan. Deze beheert de algemene spelstatus.
{
    public static GameManager Instance; // Maakt een globale verwijzing zodat andere scripts de GameManager kunnen bereiken.

    public GameState State; // Bewaart de huidige status van het spel.

    public static event Action<GameState> OnGameStateChanged; // Maakt een event aan dat andere scripts kunnen gebruiken om te weten wanneer de spelstatus verandert. Action<GameState> betekent dat het event een GameState meegeeft.

    private void Awake() // Wordt automatisch uitgevoerd wanneer het GameObject wordt geladen.
    {
        Instance = this; // Slaat deze GameManager op als de globale Instance.
    }


    private void Start() // Wordt automatisch één keer uitgevoerd vlak voor de eerste frame.
    {
        ChangeState(GameState.GenerateGrid); // Verandert de spelstatus naar GenerateGrid zodat het raster aangemaakt wordt.
    }

    public void ChangeState(GameState newState) // Methode waarmee de huidige spelstatus veranderd wordt. newState is de nieuwe status.
    {
        State = newState; // Slaat de nieuwe spelstatus op in de variabele State.

        switch (newState) // Controleert welke nieuwe spelstatus actief is en voert de bijbehorende code uit.
        {
            case GameState.GenerateGrid: // Wordt uitgevoerd wanneer de nieuwe status GenerateGrid is.
                GridManager.Instance.GenerateGrid(); // Roept GenerateGrid aan in GridManager om het speelveld aan te maken.
                break; // Stopt deze case zodat de volgende case niet uitgevoerd wordt.

            case GameState.SpawnHeroes: // Wordt uitgevoerd wanneer de nieuwe status SpawnHeroes is.
                UnitManager.instance.SpawnHeroes(); // Roept SpawnHeroes aan om de helden op het speelveld te plaatsen.
                break; // Stopt deze case.

            case GameState.SpawnEnemies: // Wordt uitgevoerd wanneer de nieuwe status SpawnEnemies is.
                UnitManager.instance.SpawnEnemies(); // Roept SpawnEnemies aan om de vijanden op het speelveld te plaatsen.
                break; // Stopt deze case.

            case GameState.HeroesTurn: // Wordt uitgevoerd wanneer het de beurt van de helden is.
                break; // Stopt deze case. Er staat momenteel geen extra code in.

            case GameState.EnemiesTurn: // Wordt uitgevoerd wanneer het de beurt van de vijanden is.
                break; // Stopt deze case. Er staat momenteel geen extra code in.

            default: // Wordt uitgevoerd als geen van de bovenstaande cases overeenkomt.
                break; // Stopt de switch.
        }

        OnGameStateChanged?.Invoke(newState); // Activeert het event en geeft de nieuwe spelstatus door aan alle scripts die erop geabonneerd zijn. ?. zorgt ervoor dat dit alleen gebeurt als er abonnees zijn.
    }



    public enum GameState // Maakt een enum aan met alle mogelijke spelstatussen.
    {
        GenerateGrid = 0, // Status 0: het speelveld wordt aangemaakt.
        SpawnHeroes = 1, // Status 1: de helden worden op het speelveld geplaatst.
        SpawnEnemies = 2, // Status 2: de vijanden worden op het speelveld geplaatst.
        HeroesTurn = 3, // Status 3: de helden zijn aan de beurt.
        EnemiesTurn = 4 // Status 4: de vijanden zijn aan de beurt.
    }
}