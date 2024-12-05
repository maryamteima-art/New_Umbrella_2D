using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DoubleJumperController : MonoBehaviour
{
    // Called when another collider enters the trigger
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            // Get the PlayerController component and set grounded to true
            var playerController = other.GetComponent<PlayerController>();
            if (playerController != null)
            {
                playerController.grounded = true;
            }
        }
    }

    // Called when another collider exits the trigger
    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            // Get the PlayerController component and set grounded to false
            var playerController = other.GetComponent<PlayerController>();
            if (playerController != null)
            {
                playerController.grounded = false;
            }
        }
    }
}
