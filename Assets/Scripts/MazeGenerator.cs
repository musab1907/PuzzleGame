using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using DG.Tweening;
public enum Difficulty
{
    Easy,
    Medium,
    Hard
}
public class MazeGenerator : MonoBehaviour
{
    private static int[,] savedMazeState = null;
    private System.Random deterministicRNG;
    public static int lastMazeSeed = -1;
    public static Difficulty restartDifficulty = Difficulty.Easy;
    private int mazeSeed;
    public static bool isRestarting = false;
    public Vector3 easyTopCamPosition;
    public Vector3 mediumTopCamPosition;
    public Vector3 hardTopCamPosition;
    [Header("Top Cam Body Offsets By Difficulty")]
    public Vector3 easyTopCamOffset = new Vector3(0, 20, 0);
    public Vector3 mediumTopCamOffset = new Vector3(0, 30, 0);
    public Vector3 hardTopCamOffset = new Vector3(0, 40, 0);
    // How long to show the solution path in seconds
    public float solutionDisplayTime = 5f;
    // Editable solution path line width
    public float solutionLineStartWidth = 0.1f;
    public float solutionLineEndWidth = 0.1f;
    private bool colorsChanged = false;
    private Vector3 playerStartPos;

    public int width = 15;
    public int height = 15;
    public GameObject wallPrefab;
    public GameObject floorPrefab;
    public GameObject playerPrefab;
    public GameObject exitPrefab;
    public Cinemachine.CinemachineVirtualCamera virtualCamera;
    public Cinemachine.CinemachineVirtualCamera topCam;
    public GameObject playerInstance;

    private int[,] maze;
    public Difficulty difficulty = Difficulty.Easy;

    public float levelTime = 180f;//İlk süremiz
    private float timeRemaining;
    private bool isTiming = false;
    public delegate void TimeUpdated(float remaining);
    public event TimeUpdated OnTimeUpdated;
    private List<GameObject> spawnedObjects = new List<GameObject>();
    public FloatingJoystick floatingJoystick;

    private List<Vector2Int> solutionPath = new List<Vector2Int>();
    public GameObject pathMarkerPrefab;

    public Color solutionStartColor = Color.cyan;
    public Color solutionEndColor = Color.cyan;

    private Vector2Int exitPosition;

    public void StartMaze()
    {
        if (isRestarting)
        {
            difficulty = restartDifficulty;
        }
        SetMazeSizeByDifficulty();
        if (!isRestarting)
        {
            mazeSeed = Random.Range(0, int.MaxValue);
            lastMazeSeed = mazeSeed;
        }
        else
        {
            mazeSeed = lastMazeSeed;
        }

        deterministicRNG = new System.Random(mazeSeed);

        switch (difficulty)
        {
            case Difficulty.Easy:
                levelTime = 180f;
                break;
            case Difficulty.Medium:
                levelTime = 150f;
                break;
            case Difficulty.Hard:
                levelTime = 120f;
                break;
        }
        timeRemaining = levelTime;
        isTiming = true;
        GenerateMaze();
        DrawMaze();
    }

    void Update()
    {
        if (isTiming)
        {
            timeRemaining -= Time.deltaTime;

            if (difficulty == Difficulty.Hard && !colorsChanged && timeRemaining <= 60f)
            {
                foreach (var obj in spawnedObjects)
                {
                    if (obj.CompareTag("Wall"))
                    {
                        Renderer rend = obj.GetComponent<Renderer>();
                        if (rend != null)
                        {
                            Color[] targetColors = { Color.red, Color.green, Color.yellow, Color.magenta };
                            Color targetColor = targetColors[Random.Range(0, targetColors.Length)];
                            rend.material.DOColor(targetColor, 1f);
                        }
                    }
                }

                if (playerInstance != null)
                {
                    playerInstance.transform.DOMove(playerStartPos, 1f);
                }

                colorsChanged = true;
            }

            // UI’ya bildir
            OnTimeUpdated?.Invoke(timeRemaining);

            if (timeRemaining <= 0f)
            {
                // Süre bittiğinde sayaçı durdur ve 00:00 olarak güncelle
                timeRemaining = 0f;
                isTiming = false;
                // Restart panelini aç
                CanvasAnims canvas = FindObjectOfType<CanvasAnims>();
                if (canvas != null)
                    canvas.OpenRestartPanel();
                return;
            }
        }
    }
    public void RestartLevel()
    {
        isRestarting = true;
        restartDifficulty = difficulty;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    void GenerateMaze()
    {
        if (MazeGenerator.isRestarting && savedMazeState != null)
        {
            maze = (int[,])savedMazeState.Clone();
            // Çözüm yolunu ve çıkış noktasını yeniden hesapla
            exitPosition = FindFarthestPointFromStart();
            FindSolutionPath();
            return;
        }
        maze = new int[width, height];
        solutionPath.Clear();
        // Başta hepsi duvar (1)
        for (int x = 0; x < width; x++)
            for (int y = 0; y < height; y++)
                maze[x, y] = 1;

        // Recursive Backtracking ile yol aç
        CarvePath(1, 1);
        AddExtraDeadEnds(GetDeadEndCountByDifficulty());

        // Giriş ve çıkış
        maze[1, 1] = 0; // Giriş
        exitPosition = FindFarthestPointFromStart();
        maze[exitPosition.x, exitPosition.y] = 0; // Çıkış

        FindSolutionPath();
        savedMazeState = (int[,])maze.Clone();
    }
    Vector2Int FindFarthestPointFromStart()
    {
        int maxDist = 0;
        Vector2Int farthest = new Vector2Int(1, 1);
        for (int x = 1; x < width - 1; x++)
        {
            for (int y = 1; y < height - 1; y++)
            {
                if (maze[x, y] == 0)
                {
                    int dist = Mathf.Abs(x - 1) + Mathf.Abs(y - 1);
                    if (dist > maxDist)
                    {
                        maxDist = dist;
                        farthest = new Vector2Int(x, y);
                    }
                }
            }
        }
        return farthest;
    }

    void CarvePath(int x, int y)
    {
        int[] dx = { 0, 0, -2, 2 };
        int[] dy = { -2, 2, 0, 0 };
        List<int> dirs = new List<int> { 0, 1, 2, 3 };
        Shuffle(dirs);

        maze[x, y] = 0;

        foreach (int dir in dirs)
        {
            int nx = x + dx[dir];
            int ny = y + dy[dir];

            if (nx > 0 && ny > 0 && nx < width - 1 && ny < height - 1 && maze[nx, ny] == 1)
            {
                maze[x + dx[dir] / 2, y + dy[dir] / 2] = 0;
                CarvePath(nx, ny);
            }
        }
    }

    void FindSolutionPath()
    {
        Vector2Int start = new Vector2Int(1, 1);
        Vector2Int end = exitPosition;
        Dictionary<Vector2Int, Vector2Int> cameFrom = new Dictionary<Vector2Int, Vector2Int>();
        Queue<Vector2Int> queue = new Queue<Vector2Int>();
        HashSet<Vector2Int> visited = new HashSet<Vector2Int>();

        queue.Enqueue(start);
        visited.Add(start);

        int[] dx = { 0, 1, 0, -1 };
        int[] dy = { 1, 0, -1, 0 };

        while (queue.Count > 0)
        {
            Vector2Int current = queue.Dequeue();
            if (current == end) break;

            for (int i = 0; i < 4; i++)
            {
                int nx = current.x + dx[i];
                int ny = current.y + dy[i];
                Vector2Int neighbor = new Vector2Int(nx, ny);

                if (nx >= 0 && nx < width && ny >= 0 && ny < height && maze[nx, ny] == 0 && !visited.Contains(neighbor))
                {
                    visited.Add(neighbor);
                    cameFrom[neighbor] = current;
                    queue.Enqueue(neighbor);
                }
            }
        }

        // Reconstruct path
        solutionPath.Clear();
        Vector2Int pathCursor = end;
        while (pathCursor != start)
        {
            solutionPath.Add(pathCursor);
            pathCursor = cameFrom[pathCursor];
        }
        solutionPath.Add(start);
        solutionPath.Reverse();
    }

    public void ShowSolutionPath()
    {
        Transform topViewTarget = new GameObject("TopViewTarget").transform;
        topViewTarget.position = new Vector3(width / 2f, 0f, height / 2f);

        LineRenderer lr = GetComponent<LineRenderer>();
        if (lr == null)
        {
            lr = gameObject.AddComponent<LineRenderer>();
            lr.material = new Material(Shader.Find("Sprites/Default"));
            lr.startColor = solutionStartColor;
            lr.endColor = solutionEndColor;
            lr.startWidth = solutionLineStartWidth;
            lr.endWidth = solutionLineEndWidth;
            lr.useWorldSpace = true;
        }
        else
        {
            lr.startWidth = solutionLineStartWidth;
            lr.endWidth = solutionLineEndWidth;
        }

        lr.positionCount = solutionPath.Count;
        for (int i = 0; i < solutionPath.Count; i++)
        {
            lr.SetPosition(i, new Vector3(solutionPath[i].x, 0.2f, solutionPath[i].y));
        }

        // Switch cameras for solution path view
        topCam.Follow = topViewTarget;
        topCam.LookAt = null;
        if (topCam != null) topCam.Priority = 20;
        if (virtualCamera != null) virtualCamera.Priority = 10;
        if (topCam != null)
        {
            var body = topCam.GetCinemachineComponent<Cinemachine.CinemachineFramingTransposer>();
            if (body != null)
            {
                switch (difficulty)
                {
                    case Difficulty.Easy:
                        body.m_TrackedObjectOffset = easyTopCamOffset;
                        break;
                    case Difficulty.Medium:
                        body.m_TrackedObjectOffset = mediumTopCamOffset;
                        break;
                    case Difficulty.Hard:
                        body.m_TrackedObjectOffset = hardTopCamOffset;
                        break;
                }
            }
        }

        // Start coroutine to hide it after a delay
        StartCoroutine(HideSolutionPathAfterDelay());
    }

    private System.Collections.IEnumerator HideSolutionPathAfterDelay()
    {
        yield return new WaitForSeconds(solutionDisplayTime);
        LineRenderer lr = GetComponent<LineRenderer>();
        if (lr != null)
        {
            lr.positionCount = 0;
        }
        // Restore cameras (priority swap)
        if (topCam != null) topCam.Priority = 10;
        if (virtualCamera != null) virtualCamera.Priority = 20;
    }

    void DrawMaze()
    {
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                Vector3 pos = new Vector3(x, 0, y);

                var floor = Instantiate(floorPrefab, pos, Quaternion.identity);
                spawnedObjects.Add(floor);

                if (maze[x, y] == 1)
                {
                    var wall = Instantiate(wallPrefab, pos + Vector3.up * 0.5f, Quaternion.identity);
                    spawnedObjects.Add(wall);
                    wall.tag = "Wall";
                    Renderer rend = wall.GetComponent<Renderer>();
                    if (rend != null)
                    {
                        rend.material.color = Color.blue;
                    }
                }
                else if (x == 1 && y == 1)
                {
                    // Oyuncuyu burada instantiate ediyoruz ve referansını alıyoruz
                    playerInstance = Instantiate(playerPrefab, pos + Vector3.up * 1f, Quaternion.identity);
                    spawnedObjects.Add(playerInstance);
                    playerStartPos = playerInstance.transform.position;
                }
                else if (x == exitPosition.x && y == exitPosition.y)
                {
                    var exit = Instantiate(exitPrefab, pos + Vector3.up * 1.4f, Quaternion.Euler(0,43,0));
                    spawnedObjects.Add(exit);
                }
            }
        }
        if (virtualCamera != null && playerInstance != null)
        {
            virtualCamera.Follow = playerInstance.transform;
            virtualCamera.LookAt = null;
        }
    }

    public void ClearMaze()//Tüm instantiate edilen objeleri kaldırma için performans önemi
    {
        foreach (var obj in spawnedObjects)
        {
            if (obj != null)
                Destroy(obj);
        }
        spawnedObjects.Clear();
    }



    void Shuffle(List<int> list)
    {
        for (int i = 0; i < list.Count; i++)
        {
            int tmp = list[i];
            int r = deterministicRNG.Next(i, list.Count);
            list[i] = list[r];
            list[r] = tmp;
        }
    }

    void SetMazeSizeByDifficulty() //Zorluk seçimi
    {
        switch (difficulty)
        {
            case Difficulty.Easy:
                width = 11;
                height = 11;
                break;
            case Difficulty.Medium:
                width = 17;
                height = 17;
                break;
            case Difficulty.Hard:
                width = 25;
                height = 25;
                break;
        }
    }

    void AddExtraDeadEnds(int count)
    {
        int attempts = 0;
        while (count > 0 && attempts < 1000)
        {
            int x = deterministicRNG.Next(1, width - 1);
            int y = deterministicRNG.Next(1, height - 1);
            if (maze[x, y] == 1)
            {
                int openNeighbors = 0;
                if (maze[x + 1, y] == 0) openNeighbors++;
                if (maze[x - 1, y] == 0) openNeighbors++;
                if (maze[x, y + 1] == 0) openNeighbors++;
                if (maze[x, y - 1] == 0) openNeighbors++;

                if (openNeighbors == 1)
                {
                    maze[x, y] = 0;
                    count--;
                }
            }
            attempts++;
        }
    }

    int GetDeadEndCountByDifficulty()
    {
        switch (difficulty)
        {
            case Difficulty.Easy: return 10;
            case Difficulty.Medium: return 25;
            case Difficulty.Hard: return 50;
            default: return 15;
        }
    }
}