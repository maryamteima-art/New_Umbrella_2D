using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class UmbrellaController : MonoBehaviour, UmbrellaInputActions.IUmbrellaActions
{
    public Transform player;
    public float displacement = 1.0f; 
    public float forceMagnitude = 100f;
    public float slowFallGravityScale = 0.1f; 
    public Color slowedColor = Color.blue; 
    public float slowFallSpeed = 1f; 
    public bool umbrellaOpen = false;
    public float umbrellaAngle = 0;
    private Vector3 closedSize = new Vector3(0.25f, 2f, 1f); 
    private Vector3 openSize = new Vector3(2.5f, 0.5f, 1f); 
    private bool wasM1 = false;
    private Rigidbody2D playerRb;
    private SpriteRenderer playerSpriteRenderer;
    private Color originalColor;
    private float originalGravityScale;
    private PlayerController playerController;
    public Color umbrellaDownColor = Color.green; 
    public LayerMask hazardLayer; 
    public float collisionForceMultiplier = 0.5f; 
    public float debugForceMagnitude = 10f; 
    public float collisionCooldown = 0.1f; 
    private bool isUmbrellaFacingDown = false; 
    private float lastCollisionTime = 0f; 

    private UmbrellaInputActions inputActions;
    private Vector2 orientInput;
    private Vector2 previousOrientInput;
    private float releaseThreshold = 0.4f;
    private float launchCooldown = 0.5f;
    private float lastLaunchTime = 0f; 

    void Awake()
    {
        inputActions = new UmbrellaInputActions();
        inputActions.Umbrella.SetCallbacks(this);
        playerRb = player.GetComponent<Rigidbody2D>();
        playerSpriteRenderer = player.GetComponent<SpriteRenderer>();
        playerController = player.GetComponent<PlayerController>();
    }

    void OnEnable()
    {
        inputActions.Umbrella.Enable();
    }

    void OnDisable()
    {
        inputActions.Umbrella.Disable();
    }

    void Start()
    {
        // Close umbrella on start
        transform.localScale = closedSize;
        originalGravityScale = playerRb.gravityScale; 
        originalColor = playerSpriteRenderer.color; 
        playerRb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
    }

    void Update()
    {
        // Calculate direction based on right joystick input
        Vector3 direction = new Vector3(orientInput.x, orientInput.y, 0).normalized;
        float joystickMagnitude = orientInput.magnitude;

        // Ignore input if below minimum threshold
        if (joystickMagnitude < 0.1f)
        {
            direction = Vector3.zero;
        }

        if (direction != Vector3.zero)
        {
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg - 90f;
            if (angle < 0)
            {
                angle += 360f;
            }
            transform.position = player.position + direction * displacement; 
            transform.rotation = Quaternion.Euler(0, 0, angle);

            // Interpolate between open and closed
            transform.localScale = Vector3.Lerp(closedSize, openSize, joystickMagnitude);

            // Set umbrellaOpen to true only if umbrella >25% 
            umbrellaOpen = joystickMagnitude >= 0.25f;

            umbrellaAngle = angle;

            // Check if player or umbrella is in an air element
            bool isInWind = playerController.inWind;

            // Umbrella air float
            if ((angle > 315f || angle < 45f) && !isInWind)
            {
                playerRb.gravityScale = 0f;
                playerSpriteRenderer.color = slowedColor;
                isUmbrellaFacingDown = false;
            }
            // Umbrella water float
            else if (angle > 135f && angle < 225f)
            {
                playerSpriteRenderer.color = umbrellaDownColor;
                isUmbrellaFacingDown = true;
            }
            // Default
            else
            {
                ResetGravityAndColor();
                isUmbrellaFacingDown = false;
            }
        }
        else
        {
            transform.localScale = closedSize;
            umbrellaOpen = false;
            ResetGravityAndColor();
            isUmbrellaFacingDown = false;

            // Check if joystick quickly released and player grounded
            if (previousOrientInput.magnitude > releaseThreshold && orientInput.magnitude <= releaseThreshold && playerController.grounded)
            {
                // Calculate launch force based on umbrella width
                float launchForceMultiplier = Mathf.Lerp(0.1f, 1.2f, previousOrientInput.magnitude);
                playerRb.AddForce(new Vector2(previousOrientInput.x, previousOrientInput.y) * forceMagnitude * launchForceMultiplier, ForceMode2D.Impulse);
                lastLaunchTime = Time.time;
            }

            wasM1 = false;
        }

        // Store the current orientInput for the next frame
        previousOrientInput = orientInput;
    }

    void OnTriggerStay2D(Collider2D other)
    {
        if (Time.time - lastCollisionTime >= collisionCooldown)
        {
            bool isHazard = ((1 << other.gameObject.layer) & hazardLayer) != 0;
            bool isWater = other.CompareTag("Water");

            if (umbrellaOpen && isHazard)
            {
                float forceMultiplier = 1f;
                if (isWater && !isUmbrellaFacingDown)
                {
                    // Fall through water if umbrella not facing down
                    return;
                } 
                // Altered logic so instead of "rebounding," the player "floats." This also adds "pseudo surface tension"
                else if (isWater && isUmbrellaFacingDown) {
                    forceMultiplier = 0.5f;
                }
                else {
                    forceMultiplier = 2f;
                }
                if (!isWater) {
                    playerRb.velocity = Vector2.zero;
                }
                Vector2 collisionDirection = player.position - transform.position;
                collisionDirection.Normalize();
                Vector2 force = collisionDirection * debugForceMagnitude * collisionForceMultiplier * forceMultiplier;
                playerRb.AddForce(force, ForceMode2D.Impulse);

                // Apply opposite force to hazards
                Rigidbody2D otherRb = other.GetComponent<Rigidbody2D>();
                if (otherRb != null)
                {
                    Vector2 oppositeForce = -force;
                    otherRb.AddForce(oppositeForce, ForceMode2D.Impulse);
                }

                lastCollisionTime = Time.time;
            }
        }
    }

    void FixedUpdate()
    {
        if (umbrellaOpen && playerRb.gravityScale == 0f && Time.time - lastLaunchTime > launchCooldown)
        {
            playerRb.velocity = new Vector2(playerRb.velocity.x, -slowFallSpeed);
        }
    }

    void ResetGravityAndColor()
    {
        playerRb.gravityScale = originalGravityScale;
        playerSpriteRenderer.color = originalColor;
    }

    public void OnOrient(InputAction.CallbackContext context)
    {
        orientInput = context.ReadValue<Vector2>();
    }
}