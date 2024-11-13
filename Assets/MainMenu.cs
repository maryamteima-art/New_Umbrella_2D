using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    public string scene;
    public Color loadToColor = Color.white;

    public void LoadGame()
    {
        SceneManager.LoadScene(1);
    }
    void Update()
    {
        // Detect any button press on the joystick or keyboard
        if (Input.anyKeyDown || AnyAnalogButtonPressed())
        {
            // Load the scene
            // LoadGame();
            Initiate.Fade(scene, loadToColor, 0.5f);
            //Debug.Log("I EXIST");
        }
    }

    // Method to detect joystick button press
    bool AnyAnalogButtonPressed()
    {
        for (int i = 0; i < 20; i++) // https://discussions.unity.com/t/find-out-if-any-button-on-any-gamepad-has-been-pressed-and-which-one/65089
        {
            if (Input.GetKey("joystick button " + i))
            {
                return true;
            }
        }
        return false;

    }
}
