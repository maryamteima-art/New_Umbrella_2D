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
    // for SoundFX
    public float windCollisionCooldown = 1f;
    private float lastCollisionTime = 0f;

    private PlayerInputActions inputActions;
    private Vector2 moveInput;
    private bool lastDirectionRight = true; // true for right, false for left

    void Awake()
    {
        inputActions = new PlayerInputActions();
        inputActions.Player.SetCallbacks(this);
        rb = GetComponent<Rigidbody2D>();
    }

    void OnEnable()
    {
        if (inputActions != null)
        {
            inputActions.Player.Enable();
        }
    }

    void OnDisable()
    {
        if (inputActions != null)
        {
            inputActions.Player.Disable();
        }
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

        // Update last direction based on move input
        if (moveInput.x > 0)
        {
            lastDirectionRight = true;
        }
        else if (moveInput.x < 0)
        {
            lastDirectionRight = false;
        }
    }

    void OnCollisionStay2D(Collision2D collision)
    {
        // Check if player is touching the ground
        if (collision.gameObject.layer == LayerMask.NameToLayer("Terrain"))
        {
            grounded = true;
        }

        // Check if player touches a hazard
        if (collision.gameObject.layer == LayerMask.NameToLayer("Hazard"))
        {
            // Find the spawner and teleport the player back to it
            GameObject spawner = GameObject.FindGameObjectWithTag("Player Spawn");
            if (spawner != null)
            {
                transform.position = spawner.transform.position;
            }
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

            if (Time.time - lastCollisionTime >= windCollisionCooldown) 
            {
                //SoundFX
                SoundFXManager.instance.PlayInWindClip(transform, 0.5f);
            }
               
        }
    }

    void OnTriggerExit2D(Collider2D trigger)
    {
        // Check if player exits wind
        if (trigger.CompareTag("Wind"))
        {
            inWind = false;
        }
        lastCollisionTime = Time.time;
    }

    public void OnMove(InputAction.CallbackContext context)
    {
        moveInput = context.ReadValue<Vector2>();
    }
}
