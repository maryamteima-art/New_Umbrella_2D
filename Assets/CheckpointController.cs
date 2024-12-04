using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CheckpointController : MonoBehaviour
{
    public Animator openFlagAnimator; // Animator for the "open flag" animation
    public GameObject flyingFlagSprite; // GameObject for the "flying flag" sprite
    private bool isActivated = false; // Track if the checkpoint is already activated
    //You can switch the animatior

    void Start()
    {
       
    }

    void Update()
    {
        

    }

    private void OnTriggerEnter2D(Collider2D checkpoint_collision)
    {
        
    }


    private void ActivateCheckpoint()
    {
        isActivated = true; // Mark the checkpoint as activated

        // Play the "open flag" animation
        openFlagAnimator.SetTrigger("OpenFlag");

        // After the animation, switch to the "flying flag" sprite
        Invoke("SwitchToFlyingFlag", 1f); // Adjust delay based on animation length
    }

    private void SwitchToFlyingFlag()
    {
        // Disable the open flag sprite and animation
        openFlagAnimator.gameObject.SetActive(false);

        // Enable the flying flag sprite
        flyingFlagSprite.SetActive(true);
    }
}
