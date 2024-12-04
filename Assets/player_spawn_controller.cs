using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public class PlayerSpawnController : MonoBehaviour
{
    [SerializeField] private GameObject player; // Reference to the player object
    private Vector3 spawnPosition;

   

    private void Start()
    {
        // Store the initial position of the spawner
        spawnPosition = transform.position;

        // Move the player to the spawner's position at the start of the game
        if (player != null)
        {
            player.transform.position = spawnPosition;
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // Check if the player touches a hazard
        if (other.gameObject.layer == LayerMask.NameToLayer("Hazard"))
        {
            // Teleport the player back to the spawner's position
            if (player != null)
            {
                player.transform.position = spawnPosition;
                Debug.Log("Play animation");

                //PLAY VFX
                VFXManager.Instance.PlayVFX("stars", spawnPosition);
            }
        }
    }

    private void OnDrawGizmos()
    {
        // Draw a gizmo in the editor to visualize the spawner's position
        Gizmos.color = Color.red;
        Gizmos.DrawSphere(transform.position, 0.5f);
    }

 

}