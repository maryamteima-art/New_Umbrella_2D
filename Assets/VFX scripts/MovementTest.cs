using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovementTest : MonoBehaviour
{
    //------------- CUSTOM VARIABLES -------------
    public float speed = 5f;
    private Vector2 movement;

    // Update is called once per frame
    void Update()
    {
        // Get input for movement (WASD or Arrow keys)
        movement.x = Input.GetAxis("Horizontal");
        movement.y = Input.GetAxis("Vertical");

        // Move the player sprite
        transform.Translate(movement * speed * Time.deltaTime);

        // Trigger different VFX using key presses (Q, E, R, etc.)
        if (Input.GetKeyDown(KeyCode.J))
        {
            VFXManager.Instance.PlayVFX("ShapeExplosion", transform.position);
        }
        else if (Input.GetKeyDown(KeyCode.K))
        {
            VFXManager.Instance.PlayVFX("ChargeWithShapes", transform.position);
        }
        else if (Input.GetKeyDown(KeyCode.L))
        {
            VFXManager.Instance.PlayVFX("SpeedLines", transform.position);
        }
        else if (Input.GetKeyDown(KeyCode.U))
        {
            VFXManager.Instance.PlayVFX("StrokeExplosion", transform.position);
        }
        else if (Input.GetKeyDown(KeyCode.I))
        {
            VFXManager.Instance.PlayVFX("Open", transform.position);
        }
        else if (Input.GetKeyDown(KeyCode.O))
        {
            VFXManager.Instance.PlayVFX("Close", transform.position);
        }
    }

}

