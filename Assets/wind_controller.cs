using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// User specified directions
public enum WindDirection
{
    Up,
    Down,
    Left,
    Right
}

public class wind_controller : MonoBehaviour
{
    // Defaulys
    public WindDirection direction = WindDirection.Right;
    public float forceMagnitude = 10f;

    private Vector2 forceVector;

    void Start()
    {
        UpdateForceVector();
    }

    void UpdateForceVector()
    {
        // Apply directional force
        switch (direction)
        {
            case WindDirection.Up:
                forceVector = Vector2.up * forceMagnitude;
                break;
            case WindDirection.Down:
                forceVector = Vector2.down * forceMagnitude;
                break;
            case WindDirection.Left:
                forceVector = Vector2.left * forceMagnitude;
                break;
            case WindDirection.Right:
                forceVector = Vector2.right * forceMagnitude;
                break;
        }
    }

    void OnTriggerStay2D(Collider2D other)
    {
        Rigidbody2D rb = other.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.AddForce(forceVector, ForceMode2D.Force);
        }
    }

    void OnValidate()
    {
        UpdateForceVector();
    }
}
