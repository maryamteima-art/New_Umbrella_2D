using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ButtonVFXManager : MonoBehaviour
{
    [HideInInspector] private string vfxPrefabPath = "Prefabs/VFX/DancingShapes";
    [HideInInspector] private Vector3 offset = Vector3.zero;
    [HideInInspector] private float clickDuration = 0.5f;

    void Start()
    {
        // Find all buttons in the children of this GameObject
        Button[] buttons = GetComponentsInChildren<Button>();

        foreach (Button button in buttons)
        {
            // Add a VFX handler for each button
            ButtonVFXHandler handler = button.gameObject.AddComponent<ButtonVFXHandler>();
            handler.Initialize(vfxPrefabPath, offset, clickDuration);
        }
    }
}

public class ButtonVFXHandler : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    private string vfxPrefabPath;
    private GameObject currentVFXInstance;
    private Vector3 offset;
    private float clickDuration;
    private bool isClickAnimationActive = false;
    //Store the original scale of the VFX instance
    private Vector3 initialScale; 

    private Vector3 starsPos;

    public void Initialize(string prefabPath, Vector3 positionOffset, float duration)
    {
        vfxPrefabPath = prefabPath;
        offset = positionOffset;
        clickDuration = duration;

        //Load the VFX prefab from Resources
        GameObject vfxPrefab = Resources.Load<GameObject>(vfxPrefabPath);
        if (vfxPrefab == null)
        {
            Debug.LogError("VFX Prefab not found at: " + vfxPrefabPath);
            return;
        }

        //Instantiate the VFX prefab at the button's position + offset
        currentVFXInstance = Instantiate(vfxPrefab, transform.position + offset, Quaternion.identity);

        //Parent the VFX to the canvas, so it stays aligned with the button
        Canvas canvas = GetComponentInParent<Canvas>();
        if (canvas != null && currentVFXInstance != null)
        {
            currentVFXInstance.transform.SetParent(canvas.transform, false);
        }

        //Match the size of the VFX to the button
        RectTransform rectTransform = GetComponent<RectTransform>();
        if (rectTransform != null && currentVFXInstance != null)
        {
            currentVFXInstance.transform.position = new Vector3(rectTransform.position.x, rectTransform.position.y, -1) + offset;
            starsPos = currentVFXInstance.transform.position;

            //Maintain the original aspect ratio of the VFX
            float scaleFactor = Mathf.Min(rectTransform.rect.width, rectTransform.rect.height) * 0.1f;
            currentVFXInstance.transform.localScale = new Vector3(scaleFactor + 2f, scaleFactor, scaleFactor);

            //Store the initial scale for resetting later
            initialScale = currentVFXInstance.transform.localScale;
        }

        //Set the sorting layer and order for the particle system
        ParticleSystemRenderer psRenderer = currentVFXInstance.GetComponent<ParticleSystemRenderer>();
        if (psRenderer != null)
        {
            psRenderer.sortingLayerName = "Default";
            psRenderer.sortingOrder = 1;
        }

        //Initially hide the VFX
        if (currentVFXInstance != null)
        {
            currentVFXInstance.SetActive(false);
        }
    }

    //--------- BUTTON INTERACTION FUNCTIONS ------------\\
    public void OnPointerEnter(PointerEventData eventData)
    {
        //Show the VFX on hover, but only if a click animation is not active
        if (currentVFXInstance != null && !isClickAnimationActive)
        {
            currentVFXInstance.SetActive(true);
        }
    }


    public void OnPointerExit(PointerEventData eventData)
    {
        //Hide the VFX when the pointer exits, but only if no click animation is active
        if (currentVFXInstance != null && !isClickAnimationActive)
        {
            currentVFXInstance.SetActive(false);
        }
    }


    public void OnPointerClick(PointerEventData eventData)
    {
        //Perform a pingpong effect before hiding the VFX
        if (currentVFXInstance != null && !isClickAnimationActive)
        {
            StartCoroutine(PlayPingPongEffect());
            VFXManager.Instance.PlayVFX("stars", starsPos);
        }
    }
    //--------- ANIMATIONS & VISUALS FUNCTIONS ------------\\
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

            //Generate a pingpong effect
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
    //--------- CLEANUP & WORLD FUNCTIONS ------------\\
    void OnDestroy()
    {
        //Cleanup: Destroy the VFX when the button is destroyed
        if (currentVFXInstance != null)
        {
            Destroy(currentVFXInstance);
        }
    }
}