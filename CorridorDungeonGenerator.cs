using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEngine;
using Random = UnityEngine.Random;


public class CorridorDungeonGenerator : DungeonGenerator
{
    [SerializeField] private int corridorLength = 12, corridorCount = 4, corridorWidth = 2;
    [SerializeField] [Range(0f, 1)] private float roomChance = 0.5f;

    public GameObject enemy;
    public GameObject[] enemies;
    public GameObject[] destructibles, largeDestructibles;

    public HashSet<Vector2Int> FloorPositionsUnoccupied = new HashSet<Vector2Int>();
    public List<Vector2Int> tilesNearPlayer = new List<Vector2Int>();

    private void Awake()
    {
        tilemapPainter.ClearTilemap();
        CorridorGeneration();
    }

    protected override void RunGeneration()
    {

        CorridorGeneration();
    }

    protected void CorridorGeneration()
    {
        tilemapPainter = FindObjectOfType<TilemapPainter>();
        tilemapPainter.ClearTilemap();

        HashSet<Vector2Int> floorPositions = new HashSet<Vector2Int>();
        HashSet<Vector2Int> canBeRoomPositions = new HashSet<Vector2Int>();

        CreateCorridors(floorPositions, canBeRoomPositions);

        HashSet<Vector2Int> roomPositions = CreateRooms(canBeRoomPositions);


        List<Vector2Int> deadEnds = GetAllDeadEnds(floorPositions);

        SpawnRoomsAtDeadEnds(deadEnds, roomPositions);

        floorPositions.UnionWith(roomPositions);
        //PopulateFloorWithEnemies(floorPositions);

        tilemapPainter.GenerateFloorTiles(floorPositions);
        GenerateWalls.CreateWalls(floorPositions, tilemapPainter);

    }

    private void SpawnRoomsAtDeadEnds(List<Vector2Int> deadEnds, HashSet<Vector2Int> roomFloors)
    {
        foreach (var pos in deadEnds)
        {
            if (roomFloors.Contains(pos) == false)
            {
                var room = RunRandomWalk(randomWalkParams, pos);
                roomFloors.UnionWith(room);
            }
        }


        PopulateFloor(roomFloors);
    }

    private List<Vector2Int> GetAllDeadEnds(HashSet<Vector2Int> floorPositions)
    {
        List<Vector2Int> deadEnds = new List<Vector2Int>();
        foreach (var pos in floorPositions)
        {
            int neighboursCount = 0;
            foreach (var direction in MapGenerationAlgorithms.Direction2D.DirectionsList)
            {
                if (floorPositions.Contains(pos + direction))
                {
                    neighboursCount++;
                }
            }

            if (neighboursCount <= 2)
            {
                deadEnds.Add(pos);
            }
        }

        return deadEnds;
    }

    private HashSet<Vector2Int> CreateRooms(HashSet<Vector2Int> canBeRoomPositions)
    {
        HashSet<Vector2Int> roomPositions = new HashSet<Vector2Int>();
        int roomCount = Mathf.RoundToInt(canBeRoomPositions.Count * roomChance);

        List<Vector2Int> roomsToCreate = canBeRoomPositions.OrderBy(x => Guid.NewGuid()).Take(roomCount).ToList();

        foreach (var roomPos in roomsToCreate)
        {
            var room = RunRandomWalk(randomWalkParams, roomPos);
            roomPositions.UnionWith(room);
        }

        return roomPositions;
    }

    private void CreateCorridors(HashSet<Vector2Int> floorPositions, HashSet<Vector2Int> canBeRoomPositions)
    {
        var currentPos = startPos;
        canBeRoomPositions.Add(currentPos);

        for (int i = 0; i < corridorCount; i++)
        {
            var corridor = MapGenerationAlgorithms.RandomCorridor(currentPos, corridorLength, corridorWidth);
            currentPos = corridor[corridor.Count - 1];
            canBeRoomPositions.Add(currentPos);
            floorPositions.UnionWith(corridor);

        }
    }

    private void PopulateFloor(HashSet<Vector2Int> floorPositions)
    {
        GetAllTilesNotNeighbouringWalls(floorPositions, FloorPositionsUnoccupied);
        List<Vector2Int> unoccupiedList = FloorPositionsUnoccupied.ToList();

        EnemyController ec = FindObjectOfType<EnemyController>();

        FloorPositionsUnoccupied.Clear();
        SpawnLargeDestructibles(unoccupiedList, ec);
        SpawnSmallDestructible(unoccupiedList, ec);
        //RemoveAllTilesNearPlayer();
        PopulateFloorWithEnemies(unoccupiedList, ec);

    }

    private void PopulateFloorWithEnemies(List<Vector2Int> unoccupiedPositions, EnemyController ec)
    {
        float enemyIncrement = Mathf.Floor((float) Math.Pow(ec.gc.floorNo, 1.18f));
        var floorEnemyCount = unoccupiedPositions.Count * 0.1f + enemyIncrement;

        for (int i = 0; i < floorEnemyCount; i++)
        {
            var randomSpawnLocation = Random.Range(0, unoccupiedPositions.Count);
            var randomEnemyType = Random.Range(0, enemies.Length);
            var enemyToSpawn = Instantiate(enemies[randomEnemyType], (Vector2) unoccupiedPositions[randomSpawnLocation],
                Quaternion.identity);
            ec.enemiesOnCurrentFloor.Add(enemyToSpawn);
            var enemyPosition = Vector2Int.RoundToInt(enemyToSpawn.transform.position);
            unoccupiedPositions.Remove(enemyPosition);
        }
    }

    private void SpawnLargeDestructibles(List<Vector2Int> unoccupiedPositions, EnemyController ec)
    {
        float objectCount = 24f;

        for (int i = 0; i < objectCount; i++)
        {
            var randomSpawnLocation = Random.Range(0, unoccupiedPositions.Count);
            var randomObject = Random.Range(0, largeDestructibles.Length);
            var objectToSpawn = Instantiate(largeDestructibles[randomObject],
                (Vector2) unoccupiedPositions[randomSpawnLocation], Quaternion.identity);
            ec.destructiblesOnFloor.Add(objectToSpawn);

            var objectPos = Vector2Int.RoundToInt(objectToSpawn.transform.position);
            unoccupiedPositions.Remove(objectPos);

            var neighbourTiles = GetNeighbouringTiles(objectPos);

            foreach (var neighbourTile in neighbourTiles)
            {
                unoccupiedPositions.Remove(neighbourTile);
            }
        }
    }
    private void SpawnSmallDestructible(List<Vector2Int> unoccupiedPositions, EnemyController ec)
    {
        float objectCount = 14f;

        for (int i = 0; i < objectCount; i++)
        {
            var randomSpawnLocation = Random.Range(0, unoccupiedPositions.Count);
            var randomObject = Random.Range(0, destructibles.Length);
            var objectToSpawn = Instantiate(destructibles[randomObject],
                (Vector2) unoccupiedPositions[randomSpawnLocation], Quaternion.identity);
            ec.destructiblesOnFloor.Add(objectToSpawn);

            var objectPos = Vector2Int.RoundToInt(objectToSpawn.transform.position);
            unoccupiedPositions.Remove(objectPos);
        }
    }

    private HashSet<Vector2Int> GetNeighbouringTiles(Vector2Int tile)
    {
        HashSet<Vector2Int> neighbourTiles = new HashSet<Vector2Int>();
        foreach (var direction in MapGenerationAlgorithms.Direction2D.eightDirectionsList)
        {
            var neighbourTile = tile + direction;
            neighbourTiles.Add(neighbourTile);
        }
        return neighbourTiles;
    }

    private void GetAllTilesNotNeighbouringWalls(HashSet<Vector2Int> tiles, HashSet<Vector2Int> unoccupiedTiles)
    {
        foreach (var tile in tiles)
        {
            var neighbourCount = 0;
            foreach (var direction in MapGenerationAlgorithms.Direction2D.eightDirectionsList)
            {
                if (tiles.Contains(tile + direction))
                {
                    neighbourCount++;
                }
            }
            if (neighbourCount >= 8)
            {
                unoccupiedTiles.Add(tile);
            }
        }
        RemoveAllTilesNearPlayer();
    }

    public void RemoveAllTilesNearPlayer()
    {

       
        /*foreach (var tile in FloorPositionsUnoccupied)
        {
            Debug.Log(tile);
        }*/
        for (int i = 0; i < 9; i++)
        {
            for (int j = 0; j < 9; j++)
            {
                var tileToRemove = new Vector2Int(i, j);
                var tileToRemoveMinus = new Vector2Int(-i, j);
                var tileToRemoveMinus2 = new Vector2Int(i, -j);
                var tileToRemoveMinus3 = new Vector2Int(-i, -j);
                //Debug.Log(tileToRemove);
                if (FloorPositionsUnoccupied.Contains(tileToRemove))
                {
                    FloorPositionsUnoccupied.Remove(tileToRemove);
                }
                if (FloorPositionsUnoccupied.Contains(tileToRemoveMinus))
                {
                    FloorPositionsUnoccupied.Remove(tileToRemoveMinus);
                }
                if (FloorPositionsUnoccupied.Contains(tileToRemoveMinus2))
                {
                    FloorPositionsUnoccupied.Remove(tileToRemoveMinus2);
                }
                if (FloorPositionsUnoccupied.Contains(tileToRemoveMinus3))
                {
                    FloorPositionsUnoccupied.Remove(tileToRemoveMinus3);
                }
            }
        }
    }
    
}
