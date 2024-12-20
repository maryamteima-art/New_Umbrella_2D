using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;

public class PauseMenu : MonoBehaviour
{
    public static bool GamePaused = false;
    public static bool firstPaused = false;
    public GameObject PauseUI;
    public GameObject resumeButton;
    public GameObject pausePrompt;

    public Animator pausePromptAni;
    //public AudioSource audioSource;

    public string scene;
    public Color loadToColor = Color.white;
    // Update is called once per frame
    void Update()
    {

        //PauseUI.SetActive(false);
        // Check all joystick buttons (0 to 19)
        for (int i = 0; i <= 19; i++)
        {
            KeyCode keyCode = (KeyCode)System.Enum.Parse(typeof(KeyCode), "JoystickButton" + i);
            if (Input.GetKeyDown(keyCode))
            {
                Debug.Log("Pressed: " + keyCode); //
                //if (Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.JoystickButton9) || Input.GetKeyDown(KeyCode.JoystickButton7))
                //https://docs.unity3d.com/Packages/com.unity.inputsystem@1.0/api/UnityEngine.InputSystem.Gamepad.html
            }
        }

        if (Input.GetKeyDown(KeyCode.Escape) || Gamepad.current.startButton.wasPressedThisFrame)
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
            InputSystem.PauseHaptics();
            firstPaused = true;
            // EventSystem.current.SetSelectedGameObject(resumeButton);

            // Show the cursor
            Cursor.visible = true;
            // Allow cursor to move freely
            Cursor.lockState = CursorLockMode.Confined;
    }

    public void ContinueGame()
    {
            PauseUI.SetActive(false);
            Time.timeScale = 1f;
            GamePaused = false;
            InputSystem.ResumeHaptics();
            if (firstPaused)
            {
            pausePromptAni.SetBool("FirstPaused", true);
            StartCoroutine(PlayIdleActiveAnimation());
                

            //pausePrompt.SetActive(false);

            // Hide the cursor
            Cursor.visible = false;
            // Lock the cursor to the centre of the screen
            Cursor.lockState = CursorLockMode.Locked;
            }
        firstPaused = false;

    }

    public void LoadMainMenu()
    {
        //SceneManager.LoadScene("MainMenuScene");
        Time.timeScale = 1f;
        GamePaused = false;
        Initiate.Fade(scene, loadToColor, 0.5f);

    }

    private IEnumerator PlayIdleActiveAnimation()
    {
        // Wait until the first animation finishes (assuming the first animation duration is known)
        yield return new WaitForSeconds(pausePromptAni.GetCurrentAnimatorStateInfo(0).length);

        //// pausePrompt.SetActive(false);
        //AnimatorStateInfo stateInfo = pausePromptAni.GetCurrentAnimatorStateInfo(0);

        //while (stateInfo.IsName("Close_Clip") && pausePromptAni.GetCurrentAnimatorStateInfo(0).normalizedTime < 1.0f)
        //{
        //    yield return null; // Wait for the frame
        //    stateInfo = pausePromptAni.GetCurrentAnimatorStateInfo(0); // Update the state info
        //}

        // Set the GameObject inactive after the animation completes
        pausePrompt.SetActive(false);

    }



}
