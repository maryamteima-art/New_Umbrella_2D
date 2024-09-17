using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public float moveSpeed = 10f;
    public float damping = 0.95f;
    public float airMultiplier = 0.1f;
    public float maxAirSpeed = 5f;
    public bool grounded = false;
    private Rigidbody2D rb;
    public bool umbrellaActive = false;

    void Start()
    {
        // Get player and prevent tilting
        rb = GetComponent<Rigidbody2D>();
        rb.freezeRotation = true;
    }

    void Update()
    {
        // Get user input
        float moveInput = Input.GetAxis("Horizontal");
        // Apply forces
        if (!umbrellaActive)
        {
            if (grounded)
            {
                // If on the ground, apply ground force logic
                rb.AddForce(Vector2.right * moveInput * moveSpeed, ForceMode2D.Force);
            }
            else
            {
                // If in the air, work with air control (drift slower logic)
                float airControl = moveInput * moveSpeed * airMultiplier;
                rb.AddForce(Vector2.right * airControl, ForceMode2D.Force);
                // Calculate and apply aerial velocity
                Vector2 velocity = rb.velocity;
                velocity.x = Mathf.Clamp(velocity.x, -maxAirSpeed, maxAirSpeed);
                rb.velocity = velocity;
            }
        }

        // Dampen player on ground (fast stop while running)
        if (grounded)
        {
            Vector2 velocity = rb.velocity;
            velocity.x *= damping;
            rb.velocity = velocity;
        }
    }

    void OnCollisionStay2D(Collision2D collision)
    {
        // Check if player is touching the ground
        if (collision.gameObject.layer == LayerMask.NameToLayer("Terrain"))
        {
            grounded = true;
        }
    }

    void OnCollisionExit2D(Collision2D collision)
    {
        // Check if player is in the air
        if (collision.gameObject.layer == LayerMask.NameToLayer("Terrain"))
        {
            grounded = false;
        }
    }
}
