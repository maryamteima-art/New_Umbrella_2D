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
    private float releaseThreshold = 0.1f; // Lowered threshold for more leniency
    private float launchCooldown = 0.5f;
    private float lastLaunchTime = 0f; 
    private bool isSwinging = false;
    private float swingDuration = 0.25f; // Duration of the swing in seconds (halved for double speed)
    private float swingStartTime;
    private float swingExtent; // Maximum swing angle based on trigger pull depth
    private float currentSwingSpeed; // Current swing speed based on trigger depth
    private Vector3 originalPosition; // Store the original position of the umbrella
    private Quaternion originalRotation; // Store the original rotation of the umbrella
    private bool isFacingRight = true; // Store the player's last direction

    //--------- VARIABLES FOR VFX SYSTEM -----------
    //Track the previous state of the umbrella
    private bool wasUmbrellaOpen = false;

    void Awake()
    {
        inputActions = new UmbrellaInputActions();
        inputActions.Umbrella.SetCallbacks(this);
        inputActions.Umbrella.Swing.performed += ctx => StartSwing(ctx.ReadValue<float>());
        inputActions.Umbrella.Swing.canceled += ctx => StopSwing();
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

            //UMBRELLA OPEN VFX CHECKER//
            // Check if umbrella is open
            if (joystickMagnitude >= 0.25f && !wasUmbrellaOpen)
            {
                ////PLAY VFX for umbrella Open when transitioning from closed to open
                VFXManager.Instance.PlayVFX("Open", transform.position);
                wasUmbrellaOpen = true;
            }
            
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

           
            //Only play VFX when transitioning from open to closed
            if (wasUmbrellaOpen)
            {
                //PLAY VFX for umbrella close
                VFXManager.Instance.PlayVFX("Close", transform.position);
                wasUmbrellaOpen = false;
            }




            // Check if joystick quickly released and player grounded
            if (previousOrientInput.magnitude > releaseThreshold && orientInput.magnitude < releaseThreshold && playerController.grounded)
            {
                // Calculate launch force based on umbrella width
                float launchForceMultiplier = Mathf.Lerp(0.1f, 1.2f, previousOrientInput.magnitude);
                playerRb.AddForce(new Vector2(previousOrientInput.x, previousOrientInput.y) * forceMagnitude * launchForceMultiplier, ForceMode2D.Impulse);
                lastLaunchTime = Time.time;

                //PLAY VFX for dashing
                VFXManager.Instance.PlayVFX("SpeedLines", transform.position);
            }

            wasM1 = false;
        }

        if (isSwinging)
        {
            PerformSwing();
        }

        // Store previous input (for direction)
        previousOrientInput = orientInput;
    }

    void StartSwing(float triggerDepth)
    {
        isSwinging = true;
        swingStartTime = Time.time;
        umbrellaOpen = false;
        transform.localScale = closedSize;
        playerRb.gravityScale = originalGravityScale; 
        playerSpriteRenderer.color = originalColor; 
        swingExtent = Mathf.Lerp(90, -30, triggerDepth); 
        currentSwingSpeed = Mathf.Lerp(2f, 0.5f, triggerDepth); 
        originalPosition = transform.position; 
        originalRotation = transform.rotation; 
    }

    void StopSwing()
    {
        // Reset umbrella position (this is broken as hell and needs work)
        isSwinging = false;
        transform.position = originalPosition; 
        transform.rotation = originalRotation; 
    }

    void PerformSwing()
    {
        float elapsedTime = Time.time - swingStartTime;
        if (elapsedTime < swingDuration / currentSwingSpeed)
        {
            float swingProgress = elapsedTime / (swingDuration / currentSwingSpeed);
            // Swing "animation"
            float swingAngle = Mathf.Lerp(90, swingExtent, swingProgress);
            if (!isFacingRight)
            {
                // Inverse swing when facing left
                swingAngle = 180 - swingAngle;
            }
            Vector3 direction = new Vector3(Mathf.Cos(swingAngle * Mathf.Deg2Rad), Mathf.Sin(swingAngle * Mathf.Deg2Rad), 0);
            transform.position = player.position + direction * displacement;
            // Change rotation to match direction
            transform.rotation = Quaternion.Euler(0, 0, swingAngle - 90f);
        }
        else
        {
            StopSwing();
        }
    }

    // Swinging (this is busted and needs work)
    public void OnSwing(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            StartSwing(context.ReadValue<float>());
        }
        else if (context.canceled)
        {
            StopSwing();
        }
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
        if (orientInput.x > 0)
        {
            isFacingRight = true;
        }
        else if (orientInput.x < 0)
        {
            isFacingRight = false;
        }
    }
}