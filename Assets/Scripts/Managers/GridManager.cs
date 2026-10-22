using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Linq;
using Unity.VisualScripting;

public class GridManager : MonoBehaviour
{
    public static GridManager Instance;
    [SerializeField] private int width, height;

    [SerializeField] private Tile grassTile, mountainTile;

    [SerializeField] private Transform cam;

    private Dictionary<Vector2, Tile> tiles;

    private void Awake()
    {
        Instance = this;
    }

    public void GenerateGrid()
    {
        tiles = new Dictionary<Vector2, Tile>();
        for (int x = 0; x < width; x++)
        {
            for(int y = 0; y < height; y++)
            {
                var randomTile = UnityEngine.Random.Range(0,6) == 3? mountainTile : grassTile;
                var spawnedTile = Instantiate(randomTile, new Vector3(x, y), Quaternion.identity);
                spawnedTile.name = $"Tile {x} {y}";

                spawnedTile.Init(x, y);


                tiles[new Vector2(x, y)] = spawnedTile;
            }
        }
        cam.transform.position = new Vector3((float)width/2 - 0.5f, (float)height / 2 - 0.5f, -10);

        GameManager.Instance.ChangeState(GameManager.GameState.SpawnHeroes);
    }

    public Tile GetHeroSpawnTile()
    {
        return tiles.Where(t => t.Key.x < width / 2 && t.Value.walkable).OrderBy(t => UnityEngine.Random.value).First().Value;
    }

    public Tile GetEnemySpawnTile()
    {
        return tiles.Where(t => t.Key.x > width / 2 && t.Value.walkable).OrderBy(t => UnityEngine.Random.value).First().Value;
    }



    public Tile GetAtPosition(Vector2 pos)
    {
        if (tiles.TryGetValue(pos, out var tile))
        {
            return tile;
        }

        return null;
    }
}
