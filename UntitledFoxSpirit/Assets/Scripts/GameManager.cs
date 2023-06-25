using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{

    [SerializeField] GameObject player;
    [SerializeField] GameObject pauseMenu;
    [SerializeField] Collider collider1;
    [SerializeField] Collider collider2;


    public Vector3 playerPos;

    bool isPaused = false;

    void Start()
    {
        playerPos = Vector3.zero;
        Application.targetFrameRate = 60;
        Physics.IgnoreCollision(collider1, collider2, true);
    }

   
    void Update()
    {
        playerPos = player.transform.position;
        
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            isPaused = !isPaused;
            
            PauseGame();
        }



    }

    void PauseGame()
    {
        if (isPaused)
        {
            pauseMenu.SetActive(true);
            Time.timeScale = 0;
        }
        else if (isPaused == false)
        {
            pauseMenu.SetActive(false);
            Time.timeScale = 1;
        }
    }
    
}
