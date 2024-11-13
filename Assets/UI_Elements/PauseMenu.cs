using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseMenu : MonoBehaviour
{
    public static bool GamePaused = false;
    public GameObject PauseUI;
    public GameObject PauseButtonPrompt;

    public string scene;
    public Color loadToColor = Color.white;

    // Update is called once per frame
    void Update()

    {
        // Check all joystick buttons (0 to 19 is a common range, but you can extend it if needed)
        for (int i = 0; i <= 19; i++)
        {
            KeyCode keyCode = (KeyCode)System.Enum.Parse(typeof(KeyCode), "JoystickButton" + i);
            if (Input.GetKeyDown(keyCode))
            {
                Debug.Log("Pressed: " + keyCode);
            }
        }

        if (Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.JoystickButton9) || Input.GetKeyDown(KeyCode.JoystickButton7))
        {
            Debug.Log("escape");
            if (GamePaused)
            {

                    ContinueGame();
            }
            else
            {

                    PauseGame();
            }
        }
        
    }

    public void PauseGame()
    {
            PauseUI.SetActive(true);
            Time.timeScale = 0f;
            GamePaused = true;


    }

    public void ContinueGame()
    {
            PauseUI.SetActive(false);
            Time.timeScale = 1f;
            GamePaused = false;

    }

    public void LoadMainMenu()
    {
        //SceneManager.LoadScene("MainMenuScene");
        Time.timeScale = 1f;
        GamePaused = false;
        Initiate.Fade(scene, loadToColor, 0.5f);

    }



}
