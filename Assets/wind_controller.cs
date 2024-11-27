using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class wind_controller : MonoBehaviour
{
    // User specified directions
    public float windAngle = 0;
    public float windStrength = 100f;

    private Vector2 forceVector;

    void Start()
    {
        windAngle = transform.localRotation.eulerAngles.z;
        UpdateForceVector();
    }

    void UpdateForceVector()
    {
        // Convert to rad
        float windAngleRad = windAngle * Mathf.Deg2Rad;

        // Calc direction
        Vector2 direction = new Vector2(Mathf.Cos(windAngleRad), Mathf.Sin(windAngleRad)).normalized;

        forceVector = direction * windStrength;
    }

    void OnTriggerStay2D(Collider2D other)
    {
        Rigidbody2D rb = other.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.AddForce(forceVector, ForceMode2D.Force);
        }
        else if (other.CompareTag("Umbrella"))
        {
            // If umbrella (and it is open), apply force to player
            Rigidbody2D parentRb = other.transform.parent.GetComponent<Rigidbody2D>();
            UmbrellaController umbrella = other.GetComponent<UmbrellaController>();
            if (parentRb != null && umbrella != null && UmbrellaController.umbrellaOpen)
            {
                // Calc angle between wind and umbrella
                float umbrellaAngleRad = (umbrella.umbrellaAngle + 90f) * Mathf.Deg2Rad;
                Vector2 umbrellaDirection = new Vector2(Mathf.Cos(umbrellaAngleRad), Mathf.Sin(umbrellaAngleRad)).normalized;

                float angleDifference = Vector2.Angle(forceVector, umbrellaDirection);

                float forceMultiplier;

                if (angleDifference <= 45f)
                {
                    // Full force
                    forceMultiplier = 1.5f;
                }
                else if (angleDifference >= 135f)
                {
                    // Half force
                    forceMultiplier = 0.75f;
                }
                else
                {
                    // Scaled force from 0.5x to 0.1x
                    forceMultiplier = Mathf.Lerp(0.75f, 0.1f, (angleDifference - 45f) / 90f);
                }

                // Apply calculated force to player
                parentRb.AddForce(forceVector * forceMultiplier, ForceMode2D.Force);
            }
        }
    }

    void OnValidate()
    {
        UpdateForceVector();
    }
}
