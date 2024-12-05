using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DoubleJumperController : MonoBehaviour
{
    // Called when another collider enters the trigger
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player") || other.CompareTag("Umbrella"))
        {
            // Get the PlayerController component and set grounded to true
            var playerController = other.GetComponent<PlayerController>();
            if (playerController != null)
            {
                playerController.doubleJumper = true;
                playerController.grounded = true;
            }
        }
    }

    // Called when another collider exits the trigger
    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player") || other.CompareTag("Umbrella"))
        {
            // Get the PlayerController component and set grounded to false
            var playerController = other.GetComponent<PlayerController>();
            if (playerController != null)
            {
                playerController.doubleJumper = false;
                playerController.grounded = false;
            }
        }
    }
}
