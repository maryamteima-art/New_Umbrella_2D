using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour, PlayerInputActions.IPlayerActions
{
    public float moveSpeed = 10f;
    public float damping = 0.95f;
    public float airMultiplier = 0.1f;
    public float maxAirSpeed = 5f;
    public bool inWind = false;
    public bool grounded = false;
    private Rigidbody2D rb;
    public bool umbrellaActive = false;

    private PlayerInputActions inputActions;
    private Vector2 moveInput;

    void Awake()
    {
        inputActions = new PlayerInputActions();
        inputActions.Player.SetCallbacks(this);
        rb = GetComponent<Rigidbody2D>();
    }

    void OnEnable()
    {
        inputActions.Player.Enable();
    }

    void OnDisable()
    {
        inputActions.Player.Disable();
    }

    void Start()
    {
        // Get player and prevent tilting
        rb.freezeRotation = true;
    }

    void Update()
    {
        // Apply forces
        if (!umbrellaActive)
        {
            if (grounded)
            {
                // If on ground, apply ground force logic
                rb.AddForce(Vector2.right * moveInput.x * moveSpeed, ForceMode2D.Force);
            }
            else
            {
                // If in air, work with air control (drift slower logic)
                float airControl = moveInput.x * moveSpeed * airMultiplier;
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

    void OnTriggerEnter2D(Collider2D other)
    {
        // Check if player enters wind
        if (other.CompareTag("Wind"))
        {
            inWind = true;
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        // Check if player exits wind
        if (other.CompareTag("Wind"))
        {
            inWind = false;
        }
    }

    public void OnMove(InputAction.CallbackContext context)
    {
        moveInput = context.ReadValue<Vector2>();
    }
}
