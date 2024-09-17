using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class camera_controller : MonoBehaviour
{
    // Refs
    public Transform player;
    public float smoothSpeed = 0.125f;
    public Vector3 offset;

    // Logic
    void LateUpdate()
    {
        // Calculate target location
        Vector3 desiredPosition = new Vector3(player.position.x + offset.x, player.position.y + offset.y, transform.position.z);
        // Lerp to smooth
        Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed);
        // Apply translation
        transform.position = smoothedPosition;

        // Prevent player from leaving the screen
        Vector3 viewPos = Camera.main.WorldToViewportPoint(player.position);
        if (viewPos.x < 0.1f || viewPos.x > 0.9f || viewPos.y < 0.1f || viewPos.y > 0.9f)
        {
            transform.position = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed * 2);
        }
    }
}
