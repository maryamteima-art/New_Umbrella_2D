using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

//Finds buttons and assigns the ButtonVFXHandler to each button.
//VFX can be gloablly edited here
public class ButtonVFXManager : MonoBehaviour
{
    //Path to the VFX prefab in Resources
    [HideInInspector] private string vfxPrefabPath = "Prefabs/VFX/DancingShapes";
    // Offset for positioning the VFX relative to the button
    [HideInInspector] private Vector3 offset = Vector3.zero;
    //Duration of the click animation effect
    [HideInInspector] private float clickDuration = 0.5f;
    public Button resumeButton;


    void Start()
    {
        // Find all buttons in the children of this GameObject
        Button[] buttons = GetComponentsInChildren<Button>();

        foreach (Button button in buttons)
        {
            //Add a VFX handler to each button
            ButtonVFXHandler handler = button.gameObject.AddComponent<ButtonVFXHandler>();
            handler.SetupVFX(vfxPrefabPath, offset, clickDuration);
        }
        if (resumeButton != null)
        {
            EventSystem.current.SetSelectedGameObject(resumeButton.gameObject);
            //Trigger the resumeButton
            var handler = resumeButton.GetComponent<ButtonVFXHandler>();
            if (handler != null)
            {
                handler.TriggerVFX();
            }


        }
    }
}