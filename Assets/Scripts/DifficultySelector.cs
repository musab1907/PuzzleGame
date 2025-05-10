using UnityEngine;
using UnityEngine.UI;
using TMPro;
public class DifficultySelector : MonoBehaviour
{
    public TMP_Dropdown difficultyDropdown; // UI Dropdown Menüsü
    public MazeGenerator mazeGenerator; // MazeGenerator referansı
    public CanvasAnims canvasAnims;
    private GameObject homeButton;

    public void OnDifficultyChanged()
    {
        int selectedIndex = difficultyDropdown.value;
        mazeGenerator.difficulty = (Difficulty)selectedIndex;
    }

    public void StartGame()
    {
        // Zorluğu burada manuel olarak güncelle
        mazeGenerator.difficulty = (Difficulty)difficultyDropdown.value;

        mazeGenerator.StartMaze();   // artık doğru zorlukla çalışır ✅
        canvasAnims.GameStart();
    }
}