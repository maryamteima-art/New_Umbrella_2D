using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BubbleController : MonoBehaviour
{
    private PlayerController playerController;
    private Collider2D bubbleCollider;
    private Vector3 initialPosition;

    private void Awake()
    {
        bubbleCollider = GetComponent<Collider2D>();
        initialPosition = transform.position;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerController = other.GetComponent<PlayerController>();
            if (playerController != null)
            {
                playerController.hasBubble = true;
                playerController.grounded = true;

                other.transform.position = transform.position;
            }

            transform.SetParent(other.transform);
            bubbleCollider.isTrigger = false;
        }
    }

    private void FixedUpdate()
    {
        // If no longer equipped on player, reset
        if (playerController != null && !playerController.hasBubble)
        {
            transform.SetParent(null);
            bubbleCollider.isTrigger = true;
            transform.position = initialPosition;
            playerController = null;
        }
    }
}
