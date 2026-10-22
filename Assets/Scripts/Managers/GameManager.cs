using System;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    public GameState State;

    public static event Action<GameState> OnGameStateChanged;

    private void Awake()
    {
        Instance = this;
    }


    private void Start()
    {
        ChangeState(GameState.GenerateGrid);
    }
    public void ChangeState(GameState newState)
    {
        State = newState;

        switch (newState)
        {
            case GameState.GenerateGrid:
                GridManager.Instance.GenerateGrid();
                break;
            case GameState.SpawnHeroes:
                UnitManager.instance.SpawnHeroes();
                break;
            case GameState.SpawnEnemies:
                break;
            case GameState.HeroesTurn:
                break;
            case GameState.EnemiesTurn:
                break;
            default:
                break;
        }

        OnGameStateChanged(newState);

    }



    public enum GameState
    {
        GenerateGrid = 0, SpawnHeroes = 1, SpawnEnemies = 2, HeroesTurn = 3, EnemiesTurn = 4
    }
}