using System.Collections.Generic;
using UnityEngine;

public class MazeGenerator : MonoBehaviour
{
    public MazeCell mazeCellPrefab;
    public int mazeWidth;
    public int mazeDepth;

    private MazeCell[,] _mazeGrid;
        
    public MazeCell[,] GetMazeGrid()
    {
        return _mazeGrid;
    }
    private void Start()
    {
        _mazeGrid = new MazeCell[mazeWidth, mazeDepth];

        for (int x = 0; x < mazeWidth; x++)
        {
            for (int z = 0; z < mazeDepth; z++)
            {
                _mazeGrid[x, z] = Instantiate(mazeCellPrefab, new Vector3(x, 0, z), Quaternion.identity);
            }
        }

        GenerateMaze(_mazeGrid[0, 0]);
    }

    private void GenerateMaze(MazeCell currentCell)
    {
        currentCell.Visit();

        List<MazeCell> unvisitedNeighbors = GetUnvisitedNeighbors(currentCell);

        while (unvisitedNeighbors.Count > 0)
        {
            int randomIndex = Random.Range(0, unvisitedNeighbors.Count);
            MazeCell nextCell = unvisitedNeighbors[randomIndex];
            RemoveWalls(currentCell, nextCell);
            GenerateMaze(nextCell);
            unvisitedNeighbors = GetUnvisitedNeighbors(currentCell);
        }
    }

    private List<MazeCell> GetUnvisitedNeighbors(MazeCell currentCell)
    {
        List<MazeCell> unvisitedNeighbors = new List<MazeCell>();

        int x = (int)currentCell.transform.position.x;
        int z = (int)currentCell.transform.position.z;

        if (x + 1 < mazeWidth)
        {
            MazeCell neighbor = _mazeGrid[x + 1, z];
            if (!neighbor.IsVisited)
                unvisitedNeighbors.Add(neighbor);
        }

        if (x - 1 >= 0)
        {
            MazeCell neighbor = _mazeGrid[x - 1, z];
            if (!neighbor.IsVisited)
                unvisitedNeighbors.Add(neighbor);
        }

        if (z + 1 < mazeDepth)
        {
            MazeCell neighbor = _mazeGrid[x, z + 1];
            if (!neighbor.IsVisited)
                unvisitedNeighbors.Add(neighbor);
        }

        if (z - 1 >= 0)
        {
            MazeCell neighbor = _mazeGrid[x, z - 1];
            if (!neighbor.IsVisited)
                unvisitedNeighbors.Add(neighbor);
        }

        return unvisitedNeighbors;
    }

    private void RemoveWalls(MazeCell currentCell, MazeCell nextCell)
    {
        int x1 = (int)currentCell.transform.position.x;
        int z1 = (int)currentCell.transform.position.z;
        int x2 = (int)nextCell.transform.position.x;
        int z2 = (int)nextCell.transform.position.z;

        if (x1 < x2)
        {
            currentCell.ClearRightWall();
            nextCell.ClearLeftWall();
        }
        else if (x1 > x2)
        {
            currentCell.ClearLeftWall();
            nextCell.ClearRightWall();
        }
        else if (z1 < z2)
        {
            currentCell.ClearFrontWall();
            nextCell.ClearBackWall();
        }
        else if (z1 > z2)
        {
            currentCell.ClearBackWall();
            nextCell.ClearFrontWall();
        }
    }
}
