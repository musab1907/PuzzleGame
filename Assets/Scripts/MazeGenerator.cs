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
    private bool colorsChanged = false;
    private Vector3 playerStartPos;

    public int width = 15;
    public int height = 15;
    public GameObject wallPrefab;
    public GameObject floorPrefab;
    public GameObject playerPrefab;
    public GameObject exitPrefab;
    public Cinemachine.CinemachineVirtualCamera virtualCamera;
    public GameObject playerInstance;

    private int[,] maze;
    public Difficulty difficulty = Difficulty.Easy;

    public float levelTime = 180f;//İlk süremiz
    private float timeRemaining;
    private bool isTiming = false;
    public delegate void TimeUpdated(float remaining);
    public event TimeUpdated OnTimeUpdated;
    private List<GameObject> spawnedObjects = new List<GameObject>();
    public DynamicJoystick dynamicJoystick;

    public void StartMaze()
    {
        SetMazeSizeByDifficulty();
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
                isTiming = false;
                //RestartLevel();
            }
        }
    }
    public void RestartLevel()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    void GenerateMaze()
    {
        maze = new int[width, height];
        // Başta hepsi duvar (1)
        for (int x = 0; x < width; x++)
            for (int y = 0; y < height; y++)
                maze[x, y] = 1;

        // Recursive Backtracking ile yol aç
        CarvePath(1, 1);
        AddExtraDeadEnds(GetDeadEndCountByDifficulty());

        // Giriş ve çıkış
        maze[1, 1] = 0; // Giriş
        Vector2Int exit = FindFarthestPointFromStart();
        maze[exit.x, exit.y] = 0;// Çıkış
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

            if (nx > 0 && nx < width && ny > 0 && ny < height && maze[nx, ny] == 1)
            {
                maze[x + dx[dir] / 2, y + dy[dir] / 2] = 0;
                CarvePath(nx, ny);
            }
        }
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
                else if (x == width - 2 && y == height - 2)
                {
                    var exit = Instantiate(exitPrefab, pos + Vector3.up * 0.5f, Quaternion.identity);
                    spawnedObjects.Add(exit);
                }
            }
        }
        if (virtualCamera != null && playerInstance != null)
        {
            virtualCamera.Follow = playerInstance.transform;
            virtualCamera.LookAt = playerInstance.transform;
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
            int r = Random.Range(i, list.Count);
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
            int x = Random.Range(1, width - 1);
            int y = Random.Range(1, height - 1);
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