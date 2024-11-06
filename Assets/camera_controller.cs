using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    // Refs
    public Transform player;
    public float smoothTime = 0.3f;
    public Vector3 offset;

    private Vector3 velocity = Vector3.zero;

    // Logic
    void LateUpdate()
    {
        // Calculate target location
        Vector3 targetPosition = player.position + offset;
        // Smooth damp to target
        Vector3 smoothedPosition = Vector3.SmoothDamp(transform.position, targetPosition, ref velocity, smoothTime);
        // Keep Z constant
        smoothedPosition.z = transform.position.z;
        // Apply translation
        transform.position = smoothedPosition;
    }
}
