using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public enum NeighborsDirEnum
{
    LEFT,
    RIGHT,
    FRONT,
    BACK
}

public class Neighbor
{
    public NeighborsDirEnum dirEnum;
    public Vector3 nodePos;
    public Vector3 dirVector;
    public Vector3 neighborLocation;
    public float mazeWidth;
    public float mazeDepth;
    public MazeCell[,] mazeGrid;

    public Neighbor(Vector3 pos, Vector3 dir, NeighborsDirEnum dirEnum, MazeCell[,] mazeGrid, float width, float depth)
    {
        this.nodePos = pos;
        this.dirVector = dir;
        this.neighborLocation = dir + pos;
        this.dirEnum = dirEnum;
        this.mazeGrid = mazeGrid;
        this.mazeWidth = width;
        this.mazeDepth = depth;
    }
    
    public void SetPos(Vector3 pos)
    {
        neighborLocation = pos + dirVector;
    }

    public bool IsInsideMaze()
    {
        if (neighborLocation.x >= 0 && neighborLocation.x < mazeWidth && neighborLocation.z >= 0 && neighborLocation.z < mazeDepth)
            return true;
        return false;
    }

    public bool NoWalls()
    {
        MazeCell cell = mazeGrid[(int)neighborLocation.x, (int)neighborLocation.z];
        switch (dirEnum)
        {
            case NeighborsDirEnum.RIGHT:
                if (!cell.leftWall.activeInHierarchy)
                    return true;
                break;
            case NeighborsDirEnum.LEFT:
                if (!cell.rightWall.activeInHierarchy)
                    return true;
                break;
            case NeighborsDirEnum.FRONT:
                if (!cell.backWall.activeInHierarchy)
                    return true;
                break;
            case NeighborsDirEnum.BACK:
                if (!cell.frontWall.activeInHierarchy)
                    return true;
                break;
        }
        return false;
    }
}

public class PathMarker
{
    public Vector3 location;
    public float H;
    public float G;
    public float F;
    public GameObject marker;
    public PathMarker parent;

    public PathMarker(Vector3 l, float g, float h, float f, GameObject m, PathMarker p)
    {
        this.location = l;
        this.G = g;
        this.H = h;
        this.F = f;
        this.marker = m;
        this.parent = p;
    }

    public override bool Equals(object obj)
    {
        if (obj == null || !obj.GetType().Equals(this.GetType()))
            return false;
        else
            if (location.Equals(((PathMarker)obj).location))
                return true;

        return false;
    }

    public override int GetHashCode() { return 0; }
}

public class FindPathAlg : MonoBehaviour
{
    public GameObject maze;
    public GameObject startObj;
    public GameObject endObj;
    public GameObject pathObj;
    

    int mazeWidth;
    int mazeDepth;
    List<Vector3> directions = new List<Vector3>();
    bool doneAStar = false;
    bool doneDijkstra = false;
    PathMarker lastPosAStar;
    PathMarker lastPosDijkstra;

    List<Neighbor> neighbors = new List<Neighbor>();
    List<PathMarker> openAStar = new List<PathMarker>();
    List<PathMarker> closedAStar = new List<PathMarker>();
    PathMarker goalNodeAStar;
    PathMarker startNodeAStar;
    MazeCell[,] mazeGridAStar;

    List<PathMarker> openDijkstra = new List<PathMarker>();
    List<PathMarker> closedDijkstra = new List<PathMarker>();
    PathMarker goalNodeDijkstra;
    PathMarker startNodeDijkstra;
    MazeCell[,] mazeGridDijkstra;

    void BeginSearch()
    {
        mazeWidth = maze.GetComponent<MazeGenerator>().mazeWidth;
        mazeDepth = maze.GetComponent<MazeGenerator>().mazeDepth;
        mazeGridAStar = maze.GetComponent<MazeGenerator>().GetMazeGrid();
        mazeGridDijkstra = maze.GetComponent<MazeGenerator>().GetMazeGrid();

        int xCoord = Random.Range(0, mazeWidth);
        int zCoord = Random.Range(0, mazeDepth);

        GameObject start = Instantiate(startObj, new Vector3(xCoord, 0, zCoord), Quaternion.identity);
        startNodeAStar = new PathMarker(start.transform.position, 0, 0, 0, startObj, null);
        startNodeDijkstra = new PathMarker(start.transform.position, 0, 0, 0, startObj, null);

        xCoord = Random.Range(mazeWidth / 2, mazeWidth);
        zCoord = Random.Range(mazeWidth / 2, mazeDepth);

        GameObject end = Instantiate(endObj, new Vector3(xCoord, 0, zCoord), Quaternion.identity);
        goalNodeAStar = new PathMarker(end.transform.position, 0, 0, 0, endObj, null);
        goalNodeDijkstra = new PathMarker(end.transform.position, 0, 0, 0, endObj, null);

        directions.Clear();
        directions.Add(new Vector3(1.0f, 0f, 0f)); //right
        directions.Add(new Vector3(-1.0f, 0f, 0f)); //left
        directions.Add(new Vector3(0.0f, 0f, 1.0f)); //front
        directions.Add(new Vector3(0.0f, 0f, -1.0f)); //back

        openAStar.Clear();
        closedAStar.Clear();
        openAStar.Add(startNodeAStar);

        openDijkstra.Clear();
        closedDijkstra.Clear();
        openDijkstra.Add(startNodeDijkstra);

        lastPosAStar = startNodeAStar;
        lastPosDijkstra = startNodeDijkstra;

        doneAStar = false;
        doneDijkstra = false;

        SearchAStar();
        SearchDijkstra();
    }

    void SearchAStar()
    {
        if (openAStar.Count == 0 || doneAStar) return;

        PathMarker currentNode = openAStar.OrderBy(p => p.F).FirstOrDefault();
        openAStar.Remove(currentNode);

        if (currentNode.Equals(goalNodeAStar))
        {
            doneAStar = true;
            Debug.Log("AStar nodes count: " + closedAStar.Count);
            return;
        }

        CreateNeighbors(currentNode, mazeGridAStar);

        foreach (Neighbor neighbor in neighbors)
        {
            neighbor.SetPos(currentNode.location);

            if (neighbor.IsInsideMaze() && neighbor.NoWalls() && !IsClosed(neighbor, closedAStar))
            {
                GameObject neighborPM = Instantiate(pathObj, neighbor.neighborLocation, Quaternion.identity);
                float G = Vector3.Distance(currentNode.location, neighbor.neighborLocation) + currentNode.G;
                float H = Vector3.Distance(neighbor.neighborLocation, goalNodeAStar.location);
                float F = G + H;
                
                Debug.Log("G: " + G.ToString("0.00"));
                Debug.Log("H: " + H.ToString("0.00"));
                Debug.Log("F: " + F.ToString("0.00"));
                
                if (!UpdateMarker(neighbor.neighborLocation, G, H, F, neighborPM, currentNode, openAStar))
                {
                    openAStar.Add(new PathMarker(neighbor.neighborLocation, G, H, F, neighborPM, currentNode));
                }
            }
        }

        closedAStar.Add(currentNode);
        lastPosAStar = currentNode;
    }

    void SearchDijkstra()
    {
        if (openDijkstra.Count == 0 || doneDijkstra) return;

        PathMarker currentNode = openDijkstra.OrderBy(p => p.G).FirstOrDefault();
        openDijkstra.Remove(currentNode);

        if (currentNode.Equals(goalNodeDijkstra))
        {
            doneDijkstra = true;
            Debug.Log("Dijkstra nodes count: " + closedDijkstra.Count);
            return;
        }

        CreateNeighbors(currentNode, mazeGridDijkstra);

        foreach (Neighbor neighbor in neighbors)
        {
            neighbor.SetPos(currentNode.location);

            if (neighbor.IsInsideMaze() && neighbor.NoWalls() && !IsClosed(neighbor, closedDijkstra))
            {
                GameObject neighborPM = Instantiate(pathObj, neighbor.neighborLocation, Quaternion.identity);
                float G = Vector3.Distance(currentNode.location, neighbor.neighborLocation) + currentNode.G;

                
                Debug.Log("G: " + G.ToString("0.00"));
                
                if (!UpdateMarker(neighbor.neighborLocation, G, 0, G, neighborPM, currentNode, openDijkstra))
                {
                    openDijkstra.Add(new PathMarker(neighbor.neighborLocation, G, 0, G, neighborPM, currentNode));
                }
            }
        }

        closedDijkstra.Add(currentNode);
        lastPosDijkstra = currentNode;
    }

    void CreateNeighbors(PathMarker currentNode, MazeCell[,] mazeGrid)
    {
        neighbors.Clear();
        neighbors.Add(new Neighbor(Vector3.zero, new Vector3(1.0f, 0f, 0f), NeighborsDirEnum.RIGHT, mazeGrid, mazeWidth, mazeDepth));
        neighbors.Add(new Neighbor(Vector3.zero, new Vector3(-1.0f, 0f, 0f), NeighborsDirEnum.LEFT, mazeGrid, mazeWidth, mazeDepth));
        neighbors.Add(new Neighbor(Vector3.zero, new Vector3(0.0f, 0f, 1.0f), NeighborsDirEnum.FRONT, mazeGrid, mazeWidth, mazeDepth));
        neighbors.Add(new Neighbor(Vector3.zero, new Vector3(0.0f, 0f, -1.0f), NeighborsDirEnum.BACK, mazeGrid, mazeWidth, mazeDepth));
    }

    bool UpdateMarker(Vector3 neighborPos, float g, float h, float f, GameObject pBlock, PathMarker currentNode, List<PathMarker> openList)
    {
        foreach (PathMarker p in openList)
        {
            if (p.location.Equals(neighborPos))
            {
                p.G = g;
                p.H = h;
                p.F = f;
                p.marker = pBlock;
                p.parent = currentNode;
                return true;
            }
        }
        return false;
    }

    bool IsClosed(Neighbor n, List<PathMarker> closedList)
    {
        foreach (PathMarker p in closedList)
        {
            if (p.location.Equals(n.neighborLocation))
            {
                return true;
            }
        }
        return false;
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            BeginSearch();
        }

        if (Input.GetKeyDown(KeyCode.R))
        {
            doneAStar = false;
            doneDijkstra = false;
            RemoveAllMarkers();
        }

        if (Input.GetKeyDown(KeyCode.A) && !doneAStar)
        {
            SearchAStar();
        }
        
        if (Input.GetKeyDown(KeyCode.D) && !doneDijkstra)
        {
            SearchDijkstra();
        }

        if ((doneAStar || doneDijkstra) && Input.GetKeyDown(KeyCode.M))
        {
            GetPath();
        }
    }

    void GetPath()
    {
        RemoveAllMarkers();
        
        PathMarker beginAStar = lastPosAStar.parent;

        while (!startNodeAStar.Equals(beginAStar) && beginAStar != null)
        {
            Instantiate(pathObj, beginAStar.location, Quaternion.identity);
            beginAStar = beginAStar.parent;
        }
        
        PathMarker beginDijkstra = lastPosDijkstra.parent;

        while (!startNodeDijkstra.Equals(beginDijkstra) && beginDijkstra != null)
        {
            Instantiate(pathObj, beginDijkstra.location, Quaternion.identity);
            beginDijkstra = beginDijkstra.parent;
        }
    }

    void RemoveAllMarkers()
    {
        GameObject[] markers = GameObject.FindGameObjectsWithTag("marker");
        foreach (GameObject m in markers)
        {
            Destroy(m);
        }
    }
}
