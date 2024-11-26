using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

//Manages the behavior, interactions and animations
//Keeping ButtonVFXHandler in its own script allows me to reuse it with other managers or in different contexts.
public class ButtonVFXHandler : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler, ISelectHandler, IDeselectHandler
{
    private string vfxPrefabPath;        
    private GameObject currentVFXInstance;
    //Position offset for the VFX
    private Vector3 offset;
    //Duration of the click animation effect
    private float clickDuration;        
    private bool isClickAnimationActive = false;
    //Original scale of the VFX instance
    private Vector3 initialScale;
    //Position for specific secondary effects (e.x. Stars)
    private Vector3 starsPos;         


    public void SetupVFX(string prefabPath, Vector3 positionOffset, float duration)
    {
        vfxPrefabPath = prefabPath;
        offset = positionOffset;
        clickDuration = duration;

        //Load the VFX prefab from the Resources folder
        GameObject vfxPrefab = Resources.Load<GameObject>(vfxPrefabPath);
        if (vfxPrefab == null)
        {
            Debug.LogError("VFX Prefab not found at: " + vfxPrefabPath);
            return;
        }

        //Instantiate the VFX prefab
        currentVFXInstance = Instantiate(vfxPrefab, transform.position + offset, Quaternion.identity);

        // Parent the VFX to the canvas to align it with the button
        Canvas canvas = GetComponentInParent<Canvas>();
        if (canvas != null && currentVFXInstance != null)
        {
            currentVFXInstance.transform.SetParent(canvas.transform, false);
        }

        //Adjust the position and scale of the VFX to match the button
        RectTransform rectTransform = GetComponent<RectTransform>();
        if (rectTransform != null && currentVFXInstance != null)
        {
            currentVFXInstance.transform.position = rectTransform.position + offset;
            starsPos = currentVFXInstance.transform.position;

            //Scale the VFX to fit the button size while maintaining the aspect ratio
            float scaleFactor = Mathf.Min(rectTransform.rect.width, rectTransform.rect.height) * 0.1f;
            currentVFXInstance.transform.localScale = new Vector3(scaleFactor + 5f, scaleFactor, scaleFactor);

            //Store the initial scale for resetting after animations
            initialScale = currentVFXInstance.transform.localScale;
        }

        //Set the sorting layer and order for the particle system
        ParticleSystemRenderer psRenderer = currentVFXInstance.GetComponent<ParticleSystemRenderer>();
        if (psRenderer != null)
        {
            psRenderer.sortingLayerName = "MenuUI";
            psRenderer.sortingOrder = 0;
        }

        //Set VFX to be active but invisible initially
        if (currentVFXInstance != null)
        {
            currentVFXInstance.SetActive(true);
            currentVFXInstance.GetComponent<Renderer>().enabled = false; 
        }
    }

    //-------------- MOUSE INTERACTIONS ----------------//
    //Hover
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (currentVFXInstance != null && !isClickAnimationActive)
        {
            //Show the VFX
            currentVFXInstance.GetComponent<Renderer>().enabled = true; 
        }
    }
    //Unhover
    public void OnPointerExit(PointerEventData eventData)
    {
        if (currentVFXInstance != null && !isClickAnimationActive)
        {
            //Hide the VFX
            currentVFXInstance.GetComponent<Renderer>().enabled = false; 
        }
    }
    //click
    public void OnPointerClick(PointerEventData eventData)
    {
        //trigger the pulsing click animation and play the stars
        if (currentVFXInstance != null && !isClickAnimationActive)
        {
            StartCoroutine(PlayPingPongEffect());
            VFXManager.Instance.PlayVFX("stars", starsPos);
        }
    }

    //-------------- JOYSTICK INTERACTIONS ----------------//

    //Hover
    public void OnSelect(BaseEventData eventData)
    {
        // Show the VFX when the button is selected via joystick
        if (currentVFXInstance != null && !isClickAnimationActive)
        {
            currentVFXInstance.SetActive(true);

            currentVFXInstance.GetComponent<Renderer>().enabled = true;
        }
    }
    //Unhover
    public void OnDeselect(BaseEventData eventData)
    {
        // Hide the VFX when the button is deselected via joystick
        if (currentVFXInstance != null)
        {
            currentVFXInstance.SetActive(false);

            currentVFXInstance.GetComponent<Renderer>().enabled = false;
        }
    }

    // -------------------- VISUALS AND ANIMATIONS ANIMATION -------------------------//
    //Pulsing effect on clicking
    private IEnumerator PlayPingPongEffect()
    {
        isClickAnimationActive = true;

        float elapsedTime = 0f;

        //Ensure the VFX is visible and reset the scale at the start
        currentVFXInstance.SetActive(true);
        currentVFXInstance.transform.localScale = initialScale;

        while (elapsedTime < clickDuration)
        {
            elapsedTime += Time.deltaTime;

            // Create a ping-pong scaling effect
            float progress = elapsedTime / clickDuration;
            float scaleMultiplier = Mathf.PingPong(progress * 4f, 1f);
            currentVFXInstance.transform.localScale = initialScale * (1f + scaleMultiplier * 0.8f);

            yield return null;
        }

        //Reset the scale and hide the VFX after the animation
        currentVFXInstance.transform.localScale = initialScale;
        currentVFXInstance.SetActive(false);

        isClickAnimationActive = false;
    }

    // ---------- CLEANUP & OTHER GAME WORLD FUNCTIONS --------------//
    void OnDestroy()
    {
        // Destroy the VFX instance when the button is destroyed
        if (currentVFXInstance != null)
        {
            Destroy(currentVFXInstance);
        }
    }
}