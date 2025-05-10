using System.Collections.Generic;
using UnityEngine;
   public enum Difficulty
{
    Easy,
    Medium,
    Hard
}
public class MazeGenerator : MonoBehaviour
{
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
 

    public void StartMaze()
{
    SetMazeSizeByDifficulty();
    GenerateMaze();
    DrawMaze();
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

        // Giriş ve çıkış
        maze[1, 1] = 0; // Giriş
        maze[width - 2, height - 2] = 0; // Çıkış
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

                Instantiate(floorPrefab, pos, Quaternion.identity);

                 if (maze[x, y] == 1)
            {
                Instantiate(wallPrefab, pos + Vector3.up * 0.5f, Quaternion.identity);
            }
            else if (x == 1 && y == 1)
            {
                // Oyuncuyu burada instantiate ediyoruz ve referansını alıyoruz
                playerInstance = Instantiate(playerPrefab, pos + Vector3.up * 1f, Quaternion.identity);
            }
            else if (x == width - 2 && y == height - 2)
            {
                Instantiate(exitPrefab, pos + Vector3.up * 0.5f, Quaternion.identity);
            }
            }
        }
        if (virtualCamera != null && playerInstance != null)
    {
        virtualCamera.Follow = playerInstance.transform;
        virtualCamera.LookAt = playerInstance.transform;
    }
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
}