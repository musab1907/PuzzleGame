using System.Collections.Generic;
using TMPro;
using UnityEngine;

// Yardımcı sınıf — sadece veri tutar
[System.Serializable]
public class OutlineAnimData
{
    public TextMeshProUGUI text;
    public float speed = 2f;
    public float minThickness = 0.05f;
    public float maxThickness = 0.15f;
    [HideInInspector] public float currentThickness;
    [HideInInspector] public bool increasing = true;
}

// Asıl animasyon scripti
public class CanvasAnims : MonoBehaviour
{
    public List<OutlineAnimData> animatedTexts = new List<OutlineAnimData>();
    [SerializeField] private GameObject startPanel;
    [SerializeField] private GameObject restartPanel;
    [SerializeField] private GameObject nextPanel;
    [SerializeField] private GameObject gamePanel;
    public MazeGenerator mazeGenerator;
    public GameObject difficultyDropdown;
    void Start()
    {
        if (MazeGenerator.isRestarting)
        {
            gamePanel.SetActive(true);
            restartPanel.SetActive(false);
            startPanel.SetActive(false);
            nextPanel.SetActive(false);
            difficultyDropdown.SetActive(false);

            mazeGenerator.StartMaze(); // Maze'i yeniden başlat
            MazeGenerator.isRestarting = false; // Sadece bir kez kullanılacak

            return;
        }
        else{
            OpenStartPanel(); // Normal açılış
        }
    }
    void Update()
    {
        foreach (var item in animatedTexts)
        {
            if (item.text == null) continue;

            // Kalınlık animasyonu
            if (item.increasing)
            {
                item.currentThickness += Time.deltaTime * item.speed;
                if (item.currentThickness >= item.maxThickness)
                    item.increasing = false;
            }
            else
            {
                item.currentThickness -= Time.deltaTime * item.speed;
                if (item.currentThickness <= item.minThickness)
                    item.increasing = true;
            }

            item.text.fontMaterial.SetFloat(ShaderUtilities.ID_OutlineWidth, item.currentThickness);
        }
    }

    public void OpenRestartPanel()
    {
        restartPanel.SetActive(true);
        nextPanel.SetActive(false);
        startPanel.SetActive(false);
        gamePanel.SetActive(false);
    }
    public void OpenStartPanel()
    {
        restartPanel.SetActive(false);
        nextPanel.SetActive(false);
        startPanel.SetActive(true);
        difficultyDropdown.SetActive(true);
        gamePanel.SetActive(false);
    }
    public void OpenNextPanel()
    {
        restartPanel.SetActive(false);
        startPanel.SetActive(false);
        nextPanel.SetActive(true);
        gamePanel.SetActive(false);
        difficultyDropdown.SetActive(true);
    }

    public void GoHome()
    {
        mazeGenerator.ClearMaze(); // Tüm oyun objelerini sahneden kaldır
                                   // Panel geçişi vs. burada yapılabilir
        OpenStartPanel();
    }
    public void GameStart()
    {
        restartPanel.SetActive(false);
        nextPanel.SetActive(false);
        startPanel.SetActive(false);
        difficultyDropdown.SetActive(false);
        gamePanel.SetActive(true);
    }
}
