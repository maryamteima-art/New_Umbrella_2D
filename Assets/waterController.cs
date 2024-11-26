using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaterController : MonoBehaviour
{
    public float floatForce = 5f;
    private Rigidbody2D playerRb;
    private PlayerController player;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            player = other.GetComponent<PlayerController>();
            if (player != null && player.hasBubble)
            {
                playerRb = other.GetComponent<Rigidbody2D>();
            }
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerRb = null;
        }
    }

    private void FixedUpdate()
    {
        if (playerRb != null && player.hasBubble)
        {
            float adjustedForce = floatForce * 200f;
            playerRb.AddForce(Vector2.up * adjustedForce * Time.fixedDeltaTime, ForceMode2D.Force);

            // Clamp the vertical velocity
            float maxVerticalVelocity = 8f;
            if (playerRb.velocity.y > maxVerticalVelocity)
            {
                playerRb.velocity = new Vector2(playerRb.velocity.x, maxVerticalVelocity);
            }
        }
    }
}
