using UnityEngine;
using UnityEngine.UI;

public class DifficultySelector : MonoBehaviour
{
    public Dropdown difficultyDropdown; // UI Dropdown Menüsü
    public MazeGenerator mazeGenerator; // MazeGenerator referansı

    public void OnDifficultyChanged()
    {
        int selectedIndex = difficultyDropdown.value;
        mazeGenerator.difficulty = (Difficulty)selectedIndex;
    }

    public void StartGame()
    {
        
        mazeGenerator.StartMaze(); // Oyunuzmuz başlatılıyo
    }
}