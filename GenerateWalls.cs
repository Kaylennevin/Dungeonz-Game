using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class GenerateWalls
{
    public static void CreateWalls(HashSet<Vector2Int> floorPositions, TilemapPainter tilemapPainter)
    {
        var wallPositions = FindWallsInDirections(floorPositions, MapGenerationAlgorithms.Direction2D.DirectionsList);
        var cornerWallPositions =
            FindWallsInDirections(floorPositions, MapGenerationAlgorithms.Direction2D.DiagonalDirectionsList);
        
        CreateBasicWalls(tilemapPainter, wallPositions, floorPositions);
        CreateCornerWalls(tilemapPainter, cornerWallPositions, floorPositions);
    }

    private static void CreateCornerWalls(TilemapPainter tilemapPainter, HashSet<Vector2Int> cornerWallPositions, HashSet<Vector2Int> floorPositions)
    {
        foreach (var pos in cornerWallPositions)
        {
            string neighbourBinaryValue = "";
            foreach (var direction in MapGenerationAlgorithms.Direction2D.eightDirectionsList)
            {
                var neighbourPos = pos + direction;
                if (floorPositions.Contains(neighbourPos))
                {
                    neighbourBinaryValue += "1";
                }
                else
                {
                    neighbourBinaryValue += "0";
                }
            }
            tilemapPainter.CreateSingleCornerWall(pos, neighbourBinaryValue);
        }
    }

    private static void CreateBasicWalls(TilemapPainter tilemapPainter, HashSet<Vector2Int> wallPositions, HashSet<Vector2Int> floorPositions)
    {
        foreach (var pos in wallPositions)
        {
            string neighboursBinaryValue = "";
            foreach (var direction in MapGenerationAlgorithms.Direction2D.DirectionsList)
            {
                var neighbourPos = pos + direction;
                if (floorPositions.Contains(neighbourPos))
                {
                    neighboursBinaryValue += "1";
                }
                else
                {
                    neighboursBinaryValue += "0";
                }
            }
            tilemapPainter.CreateSingleWall(pos, neighboursBinaryValue);
        }
    }

    private static HashSet<Vector2Int> FindWallsInDirections(HashSet<Vector2Int> floorPositions, List<Vector2Int> directionsList)
    {
        HashSet<Vector2Int> wallPositions = new HashSet<Vector2Int>();

        foreach (var pos in floorPositions)
        {
            foreach (var direction in directionsList)
            {
                var neighbourPos = pos + direction;
                if (floorPositions.Contains(neighbourPos) == false)
                {
                    wallPositions.Add(neighbourPos);
                }
            }
        }

        return wallPositions;
    }
}
