using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Exit : MonoBehaviour
{
    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            // Oyuncu çıkış noktasına ulaştıysa Next paneli aç
            CanvasAnims canvas = FindObjectOfType<CanvasAnims>();
            if (canvas != null)
                canvas.OpenNextPanel();
        }
    }
}
