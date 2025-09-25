using System.Collections.Generic;
using UnityEngine;
using Random = System.Random;

public class MapGenerator : MonoBehaviour
{
    [SerializeField] private int mapWidth  = 40;
    [SerializeField] private int mapHeight = 40;

    [Header("Prefabs")] [SerializeField] private GameObject pathPrefab;
    [SerializeField]                     private GameObject wallPrefab;
    [SerializeField]                     private GameObject destinationPrefab;
    [SerializeField]                     private GameObject npcPrefab;
    [SerializeField]                     private GameObject solvedPathPrefab;

    private string[,]        mapMatrix;
    private List<GameObject> spawnedObjects = new List<GameObject>();
    private List<GameObject> spawnedPath    = new List<GameObject>();
    private Agent            agent;
    private bool             isCurrentMapSolved;

    void Start()
    {
        Observer.Instance.Subscribe(GameConstants.ObserverKey.OnSolveNotify, this.Solve);
        Observer.Instance.Subscribe(GameConstants.ObserverKey.OnReplayNotify, this.Replay);
        this.mapMatrix = new string[this.mapWidth, this.mapHeight];
        this.GenerateMap();
        this.GenerateMapObject();
    }

    private void GenerateMap()
    {
        // set current map solved = false
        this.isCurrentMapSolved = false;

        // fill all walls
        for (int y = 0; y < this.mapHeight; y++)
        {
            for (int x = 0; x < this.mapWidth; x++)
            {
                mapMatrix[x, y] = GameConstants.TileKey.Wall;
            }
        }

        Vector2Int start = new Vector2Int(1, 1);
        mapMatrix[start.x, start.y] = GameConstants.TileKey.Npc;

        Random           rng           = new Random();
        Vector2Int       current       = start;
        List<Vector2Int> mainPathCells = new List<Vector2Int>();

        // main path
        while (true)
        {
            if (mapMatrix[current.x, current.y] != GameConstants.TileKey.Npc &&
                mapMatrix[current.x, current.y] != GameConstants.TileKey.Destination)
            {
                mapMatrix[current.x, current.y] = GameConstants.TileKey.Empty;
                mainPathCells.Add(current);
            }

            if (current.y == mapHeight - 2 || current.x == mapWidth - 2)
            {
                mapMatrix[current.x, current.y] = GameConstants.TileKey.Destination;
                mainPathCells.Add(current);

                break;
            }

            List<Vector2Int> dirs = new List<Vector2Int>
            {
                new Vector2Int(1, 0),
                new Vector2Int(-1, 0),
                new Vector2Int(0, 1),
                new Vector2Int(0, -1)
            };

            Shuffle(dirs, rng);

            bool moved = false;

            foreach (var d in dirs)
            {
                int nx = current.x + d.x;
                int ny = current.y + d.y;

                if (nx > 0 && nx < mapWidth - 1 &&
                    ny > 0 && ny < mapHeight - 1 &&
                    mapMatrix[nx, ny] == GameConstants.TileKey.Wall &&
                    CountEmptyNeighbors(nx, ny) <= 1)
                {
                    current = new Vector2Int(nx, ny);
                    moved   = true;

                    break;
                }
            }

            if (!moved)
            {
                bool foundAlternative = false;

                // track nguoc lai xem co tile nao di duoc tiep de hon khong
                for (int i = mainPathCells.Count - 1; i >= 0; i--)
                {
                    var cell = mainPathCells[i];

                    foreach (var d in dirs)
                    {
                        int nx = cell.x + d.x;
                        int ny = cell.y + d.y;

                        if (nx > 0 && nx < mapWidth - 1 &&
                            ny > 0 && ny < mapHeight - 1 &&
                            mapMatrix[nx, ny] == GameConstants.TileKey.Wall &&
                            CountEmptyNeighbors(nx, ny) <= 1)
                        {
                            current          = new Vector2Int(nx, ny);
                            foundAlternative = true;

                            break;
                        }
                    }

                    if (foundAlternative) break;
                }

                if (!foundAlternative)
                {
                    Debug.Log("Main path completely blocked, setting Destination");
                    mapMatrix[current.x, current.y] = GameConstants.TileKey.Destination;
                    mainPathCells.Add(current);

                    break;
                }
            }
        }

        DebugDrawMainPath(mainPathCells);

        // branch flood-fill expansion
        List<Vector2Int> candidates = new List<Vector2Int>(mainPathCells);

        while (candidates.Count > 0)
        {
            int        idx  = rng.Next(candidates.Count);
            Vector2Int cell = candidates[idx];

            List<Vector2Int> dirs = new List<Vector2Int>
            {
                new Vector2Int(1, 0),
                new Vector2Int(-1, 0),
                new Vector2Int(0, 1),
                new Vector2Int(0, -1)
            };

            Shuffle(dirs, rng);
            bool carved = false;

            foreach (var d in dirs)
            {
                int nx = cell.x + d.x;
                int ny = cell.y + d.y;

                if (nx > 0 && nx < mapWidth - 1 &&
                    ny > 0 && ny < mapHeight - 1 &&
                    mapMatrix[nx, ny] == GameConstants.TileKey.Wall &&
                    CountEmptyNeighbors(nx, ny) == 1)
                {
                    mapMatrix[nx, ny] = GameConstants.TileKey.Empty;
                    candidates.Add(new Vector2Int(nx, ny));
                    carved = true;

                    if (nx == 1 || nx == mapWidth - 2 || ny == 1 || ny == mapHeight - 2)
                    {
                        candidates.RemoveAt(idx);
                    }

                    break;
                }
            }

            if (!carved)
            {
                candidates.RemoveAt(idx);
            }
        }

        // border walls
        for (int x = 0; x < this.mapWidth; x++)
        {
            mapMatrix[x, 0]                  = GameConstants.TileKey.Wall;
            mapMatrix[x, this.mapHeight - 1] = GameConstants.TileKey.Wall;
        }

        for (int y = 0; y < this.mapHeight; y++)
        {
            mapMatrix[0, y]                 = GameConstants.TileKey.Wall;
            mapMatrix[this.mapWidth - 1, y] = GameConstants.TileKey.Wall;
        }

        Observer.Instance.Notify(GameConstants.ObserverKey.OnMapModified, new Vector2Int(this.mapWidth, this.mapHeight));
    }

    private void Shuffle<T>(List<T> list, Random rng)
    {
        for (int i = 0; i < list.Count; i++)
        {
            int j = rng.Next(i, list.Count);
            (list[i], list[j]) = (list[j], list[i]);
        }
    }

    private int CountEmptyNeighbors(int x, int y)
    {
        int count = 0;
        if (mapMatrix[x - 1, y] == GameConstants.TileKey.Empty) count++;
        if (mapMatrix[x + 1, y] == GameConstants.TileKey.Empty) count++;
        if (mapMatrix[x, y - 1] == GameConstants.TileKey.Empty) count++;
        if (mapMatrix[x, y + 1] == GameConstants.TileKey.Empty) count++;

        return count;
    }

    #region Output

    private void GenerateMapObject()
    {
        float offsetX = this.mapWidth / 2f;
        float offsetY = this.mapHeight / 2f;

        bool isNpcPrefab = false;

        for (int y = 0; y < this.mapHeight; y++)
        {
            for (int x = 0; x < this.mapWidth; x++)
            {
                Vector3 pos = new Vector3(x - offsetX, y - offsetY, 0);

                string     id     = mapMatrix[x, y];
                GameObject prefab = null;
                isNpcPrefab = false;

                if (id == GameConstants.TileKey.Empty) prefab     = pathPrefab;
                else if (id == GameConstants.TileKey.Wall) prefab = wallPrefab;
                else if (id == GameConstants.TileKey.Npc)
                {
                    prefab      = npcPrefab;
                    isNpcPrefab = true;
                }
                else if (id == GameConstants.TileKey.Destination) prefab = destinationPrefab;

                if (prefab != null)
                {
                    var go = Instantiate(prefab, pos, Quaternion.identity, this.transform);

                    if (isNpcPrefab)
                    {
                        this.agent = go.GetComponent<Agent>();
                    }

                    spawnedObjects.Add(go);
                }
            }
        }
    }

    #endregion

    #region Unrelated Services

    public void Dispose()
    {
        foreach (var obj in spawnedObjects)
        {
            if (obj != null) Destroy(obj);
        }

        spawnedObjects.Clear();
        this.spawnedPath.Clear();
    }

    private void Replay(object param)
    {
        Dispose();
        this.mapMatrix = new string[this.mapWidth, this.mapHeight];
        this.GenerateMap();
        this.GenerateMapObject();
    }

    private void Solve(object param)
    {
        if (this.isCurrentMapSolved)
            return;

        this.isCurrentMapSolved = true;
        Vector2Int start = Vector2Int.zero;
        Vector2Int goal  = Vector2Int.zero;

        for (int y = 0; y < mapHeight; y++)
        {
            for (int x = 0; x < mapWidth; x++)
            {
                if (mapMatrix[x, y] == GameConstants.TileKey.Npc)
                    start = new Vector2Int(x, y);
                else if (mapMatrix[x, y] == GameConstants.TileKey.Destination)
                    goal = new Vector2Int(x, y);
            }
        }

        Queue<Vector2Int>                  q       = new Queue<Vector2Int>();
        Dictionary<Vector2Int, Vector2Int> parent  = new Dictionary<Vector2Int, Vector2Int>();
        bool[,]                            visited = new bool[mapWidth, mapHeight];

        q.Enqueue(start);
        visited[start.x, start.y] = true;

        int[] dx = { 1, -1, 0, 0 };
        int[] dy = { 0, 0, 1, -1 };

        bool found = false;

        while (q.Count > 0 && !found)
        {
            var cur = q.Dequeue();

            if (cur == goal)
            {
                found = true;

                break;
            }

            for (int i = 0; i < 4; i++)
            {
                int nx = cur.x + dx[i];
                int ny = cur.y + dy[i];

                if (nx >= 0 && nx < mapWidth &&
                    ny >= 0 && ny < mapHeight &&
                    !visited[nx, ny] &&
                    (mapMatrix[nx, ny] == GameConstants.TileKey.Empty ||
                     mapMatrix[nx, ny] == GameConstants.TileKey.Destination))
                {
                    visited[nx, ny] = true;
                    q.Enqueue(new Vector2Int(nx, ny));
                    parent[new Vector2Int(nx, ny)] = cur;
                }
            }
        }

        if (!found)
        {
            Debug.Log("No path found to Destination");

            return;
        }

        List<Vector2Int> path = new List<Vector2Int>();
        Vector2Int       step = goal;

        while (step != start)
        {
            path.Add(step);
            step = parent[step];
        }

        path.Add(start);
        path.Reverse();

        float offsetX = this.mapWidth / 2f;
        float offsetY = this.mapHeight / 2f;

        foreach (var p in path)
        {
            if (mapMatrix[p.x, p.y] == GameConstants.TileKey.Empty)
            {
                Vector3 pos = new Vector3(p.x - offsetX, p.y - offsetY, 0);
                var     go  = Instantiate(solvedPathPrefab, pos, Quaternion.identity, this.transform);
                spawnedObjects.Add(go);
                this.spawnedPath.Add(go);
            }
        }

        if (this.agent != null)
        {
            this.agent.Move(this.spawnedPath);
        }
        else
        {
            Debug.Log("agent is null");
        }
    }

    #endregion

#if UNITY_EDITOR
    private void DebugDrawMainPath(List<Vector2Int> mainPathCells)
    {
        if (mainPathCells.Count < 2) return;

        float offsetX = this.mapWidth / 2f;
        float offsetY = this.mapHeight / 2f;

        for (int i = 0; i < mainPathCells.Count - 1; i++)
        {
            Vector3 a = new Vector3(mainPathCells[i].x - offsetX, mainPathCells[i].y - offsetY, 0);
            Vector3 b = new Vector3(mainPathCells[i + 1].x - offsetX, mainPathCells[i + 1].y - offsetY, 0);
            Debug.DrawLine(a, b, Color.blue, 10f); // giữ line trong 10s
        }
    }
#endif
}