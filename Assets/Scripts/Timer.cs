using UnityEngine;
using UnityEngine.UI;
using TMPro;
public class Timer : MonoBehaviour
{
    public TextMeshProUGUI timerText;
    public MazeGenerator mazeGenerator;

    void Start()
    {
        mazeGenerator.OnTimeUpdated += UpdateTimerUI;
    }

    void UpdateTimerUI(float timeRemaining)
    {
        int minutes = Mathf.FloorToInt(timeRemaining / 60f);
        int seconds = Mathf.FloorToInt(timeRemaining % 60f);
        timerText.text = $"{minutes:00}:{seconds:00}";
    }

    void OnDestroy()
    {
        mazeGenerator.OnTimeUpdated -= UpdateTimerUI;
    }
}