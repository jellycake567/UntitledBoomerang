using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{

    [SerializeField] GameObject player;
    //[SerializeField] GameObject spear;

    public Vector3 playerPos;

    void Start()
    {
        playerPos = Vector3.zero;
        Application.targetFrameRate = 60;
    }

    // Update is called once per frame
    void Update()
    {
        playerPos = player.transform.position;
    }
}
