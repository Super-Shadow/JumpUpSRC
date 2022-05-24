using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Inst;

    public void Awake()
    {
        Inst = this;
    }

    public void Start()
    {
        Player = FindObjectOfType<PlayerController>();
        GameCamera = FindObjectOfType<CameraController>();
    }

    public CameraController GameCamera;
    public PlayerController Player;

    // Camerapoints for where a camera can move in transitions
    [SerializeField] protected List<Transform> CameraPoints;
    public Transform CameraPos(int ID) { return CameraPoints[ID]; }


}
