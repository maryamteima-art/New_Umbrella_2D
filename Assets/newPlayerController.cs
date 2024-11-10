using UnityEngine;
using UnityEngine.InputSystem;

public class newPlayerController : MonoBehaviour, PlayerInputActions.IPlayerActions
{
    public float moveSpeed = 6f;
    private Rigidbody2D rb;
    private PlayerInputActions inputActions;
    private Vector2 moveInput;
    public bool grounded = false;
    public Transform groundCheck;
    public float groundCheckRadius = 0.1f;
    public LayerMask groundLayer;

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

    void Update()
    {
        // Apply horizontal movement
        rb.AddForce(Vector2.right * moveInput.x * moveSpeed, ForceMode2D.Force);

        // Cap horizontal speed
        Vector2 velocity = rb.velocity;
        velocity.x = Mathf.Clamp(velocity.x, -moveSpeed, moveSpeed);
        rb.velocity = velocity;

        // Stop horizontal movement if grounded and no input
        if (grounded && moveInput.x == 0)
        {
            velocity.x = 0;
            rb.velocity = velocity;
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        // Check if GroundCheck is overlapping with terrain
        if (other.gameObject.layer == LayerMask.NameToLayer("Terrain"))
        {
            grounded = true;
            // Kill horizontal momentum
            Vector2 velocity = rb.velocity;
            velocity.x = 0;
            rb.velocity = velocity;
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        // Check if GroundCheck is no longer overlapping with terrain
        if (other.gameObject.layer == LayerMask.NameToLayer("Terrain"))
        {
            grounded = false;
        }
    }

    public void OnMove(InputAction.CallbackContext context)
    {
        moveInput = context.ReadValue<Vector2>();
    }
}
