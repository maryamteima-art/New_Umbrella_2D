using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CheckpointController : MonoBehaviour
{
    public Animator flagAnimator;
    //public Animator idleFlagAnimator; // Animator for the "open flag" animation
    //public GameObject flyingFlagSprite; // GameObject for the "flying flag" sprite
    private bool isActivated = false;
    private bool isFirstTimeActive = false;// Track if the checkpoint is already activated
    //You can switch the animatior
    private Vector3 spawnPosition;
    public GameObject fireworkVFX;

    //Slider
    public Animator sliderAnimator;
    public GameObject sliderUI;

    private bool isPlayerInsideTrigger = false;


    void Start()
    {
        spawnPosition = transform.position;
        sliderAnimator.enabled = false;
        sliderUI.SetActive(false);
    }


    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player") && !isPlayerInsideTrigger)
        {
            ActivateCheckpoint();
            ShowProgress();
            Debug.Log("I am colliding with flag");
            isPlayerInsideTrigger = true;
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player") && isPlayerInsideTrigger)
        {
            HideProgress();
            isPlayerInsideTrigger = false;
            Debug.Log("Player left the collision flag");
        }
    }


    private void ShowProgress()
    {
        if (isActivated)
        {
            sliderUI.SetActive(true);
            sliderAnimator.enabled = true;
            sliderAnimator.SetBool("Show", true);
            sliderAnimator.SetBool("Idle", false);
        }
    }

    private void HideProgress()
    {
        if (isActivated)
        {
            
            StartCoroutine(WaitForExitAnimation());
        }
    }




    private void ActivateCheckpoint()
    {
        if (!isActivated)
        {
            //if it is not the first tiem active so it only runs once
            isActivated = true;
            isFirstTimeActive =true;
            //flagAnimator.enabled = true; // Enable the Animator
            if (isFirstTimeActive)
            {
                flagAnimator.SetBool("firstActive", true);
                SoundFXManager.instance.PlayCheckpointClip(transform, 0.5f);

                // Set the parameter to trigger the first animation

                //isFirstTimeActive = false;
                Debug.Log("I am playing animations");

                StartCoroutine(PlayIdleActiveAnimation());
                ActivateFirework();


            }

        }
    }
    private IEnumerator PlayIdleActiveAnimation()
    {
        // Wait until the first animation finishes (assuming the first animation duration is known)
        yield return new WaitForSeconds(flagAnimator.GetCurrentAnimatorStateInfo(0).length);

        // Set the flag's animation to idle/active state
        flagAnimator.SetBool("firstActive", false);  // Reset the firstActive trigger
        flagAnimator.SetBool("active", true);  // Activate the idle active animation
        Debug.Log("I am playing second");
    }

    private IEnumerator WaitForExitAnimation()
    {
        sliderAnimator.SetBool("Show", false);
        sliderAnimator.SetBool("Idle", true);
        // Wait until the first animation finishes (assuming the first animation duration is known)
        yield return new WaitForSeconds(sliderAnimator.GetCurrentAnimatorStateInfo(0).length);
        

        sliderUI.SetActive(false);

        Debug.Log("I am playing hiding slider");
    }

    void ActivateFirework()
    {

        // Instantiate the particle system at the spawn location
        GameObject particleInstance = Instantiate(
            fireworkVFX,
            transform.position,
            Quaternion.identity
        );

        //parent the particle system to this object
        particleInstance.transform.SetParent(transform);

        // Play the particle system
        ParticleSystem ps = particleInstance.GetComponent<ParticleSystem>();
        if (ps != null)
        {
            ps.Play();
        }

        // Destroy the particle system after it finishes
        Destroy(particleInstance, 10);


    }
}
