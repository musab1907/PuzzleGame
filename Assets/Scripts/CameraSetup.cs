using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class CameraSetup : MonoBehaviour
{
    public CinemachineVirtualCamera virtualCam;
    public GameObject player;
    

    void Start()
    {
        //virtualCam.Follow = player.transform;
        //virtualCam.LookAt = player.transform;
    }
}

