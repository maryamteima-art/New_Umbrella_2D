using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class UmbrellaController : MonoBehaviour, UmbrellaInputActions.IUmbrellaActions
{
    public Transform player;
    public Transform anchorPoint;
    public float displacement = 1.0f; 
    public float forceMagnitude = 100f;
    public float slowFallGravityScale = 0.1f;
    
    // Handle
    public Transform handle;
    private Vector3 handleClosedSize = new Vector3(0.05f, 0.005f, 1f);
    private Vector3 handleOpenSize = new Vector3(0.0075f, 0.05f, 1f);

    public Color slowedColor = Color.magenta; 
    
    public float slowFallSpeed = 1f; 
    public static bool umbrellaOpen = false;
    public static bool umbrellaDown = false;
    public float umbrellaAngle = 0;
    
    public Color umbrellaDownColor = Color.green; 
    
    public LayerMask hazardLayer; 
    public float collisionForceMultiplier = 0.5f; 
    public float debugForceMagnitude = 10f; 
    public float collisionCooldown = 0.1f; 
    // for water SoundFX
    public float waterCollisionCooldown = 0.25f; 

    private Vector3 closedSize = new Vector3(0.25f, 2f, 1f); 
    private Vector3 openSize = new Vector3(2.5f, 0.5f, 1f);
    public static float umbrella_size;
    private bool wasM1 = false;
    private Rigidbody2D playerRb;
    private SpriteRenderer playerSpriteRenderer;
    private Color originalColor;
    private float originalGravityScale;
    private PlayerController playerController;
    private bool isUmbrellaFacingDown = false; 
    private float lastCollisionTime = 0f;
    private UmbrellaInputActions inputActions;
    private Vector2 orientationInput;
    private Vector2 previousOrientationInput;
    private float releaseThreshold = 0.1f; 
    private float launchCooldown = 0.5f;
    private float lastLaunchTime = 0f; 
    public bool isSwinging = false;
    private float swingDuration = 0.25f; 
    private float swingStartTime;
    private float swingExtent; 
    public float currentSwingSpeed; 
    private Vector3 originalPosition; 
    private Quaternion originalRotation; 
    private bool isFacingRight = true; 
    private bool wasUmbrellaOpen = false;
    private float previousUmbrellaWidth = 0f;
    private float maxUmbrellaWidth = 0f;

    // umbrella flipped
    private bool umbrellaFlipX = false;

    // Time to wait before confirming the trigger is released
    private float debounceTime = 0.1f; 
    private Coroutine debounceCoroutine;

    private LineRenderer lineRenderer;

    /* Creates new instance of umbrella's controller inputs, registers swing methods, and stores player references */
    void Awake()
    {
        inputActions = new UmbrellaInputActions();
        inputActions.Umbrella.SetCallbacks(this);
        inputActions.Umbrella.Swing.performed += ctx => StartSwing(ctx.ReadValue<float>());
        inputActions.Umbrella.Swing.canceled += ctx => StopSwing();
        playerRb = player.GetComponent<Rigidbody2D>();
        playerSpriteRenderer = player.GetComponent<SpriteRenderer>();
        playerController = player.GetComponent<PlayerController>();

        // Initialise umbrella arm
        lineRenderer = gameObject.AddComponent<LineRenderer>();
        lineRenderer.startWidth = 0.1f;
        lineRenderer.endWidth = 0.1f;
        lineRenderer.material = new Material(Shader.Find("Sprites/Default"));
        lineRenderer.startColor = Color.black;
        lineRenderer.endColor = new Color(0.6f, 0.3f, 0.1f); // Brown colour
        lineRenderer.sortingOrder = -1;
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
        transform.localScale = closedSize;
        originalGravityScale = playerRb.gravityScale; 
        originalColor = playerSpriteRenderer.color; 
        playerRb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
    }

    /* Wait and check for umbrella rotations and swings  */
    void Update()
    {
        if (!PauseMenu.GamePaused)
        {
            HandleUmbrellaOrientation();
            HandleSwinging();
            previousOrientationInput = orientationInput;

            // Update umbrella arm
            lineRenderer.SetPosition(0, transform.position);
            lineRenderer.SetPosition(1, anchorPoint.position);
        }
    }

    /* Calculate and apply necessary umbrella orientation */
    void HandleUmbrellaOrientation()
    {
        Vector3 direction = new Vector3(orientationInput.x, orientationInput.y, 0).normalized;
        float joystickMagnitude = orientationInput.magnitude;

        if (joystickMagnitude < 0.1f)
        {
            direction = Vector3.zero;
        }

        if (direction != Vector3.zero)
        {
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg - 90f;
            if (angle < 0) angle += 360f;

            transform.position = anchorPoint.position + direction * displacement;
            transform.rotation = Quaternion.Euler(0, 0, angle);
            transform.localScale = Vector3.Lerp(closedSize, openSize, joystickMagnitude);

            //handle scale
            //handle.transform.localScale = Vector3.Lerp(closedSize, openSize, joystickMagnitude) * -1;
            //handle.transform.localScale = new Vector3(0.05f, 0.075f, 1);
            //handle.transform.localScale = Vector3.Lerp(closedSize/10f, openSize/4f, 1);
            handle.transform.localScale = Vector3.Lerp(handleClosedSize, handleOpenSize, joystickMagnitude);

            umbrellaOpen = joystickMagnitude >= 0.25f;
            umbrellaAngle = angle;

            bool isInWind = playerController.inWind;

            if (joystickMagnitude >= 0.25f && !wasUmbrellaOpen)
            {
                VFXManager.Instance.PlayVFX("Open", transform.position + new Vector3(0, 0.5f, 0));
                wasUmbrellaOpen = true;

                //SoundFX
                SoundFXManager.instance.PlayUmbrellaOpenClip(transform, 1f);
            }

            if ((angle > 315f || angle < 45f) && !isInWind)
            {
                playerRb.gravityScale = 0f;
                // playerSpriteRenderer.color = slowedColor;
                isUmbrellaFacingDown = false;
            }
            else if (angle > 135f && angle < 225f)
            {
                // playerSpriteRenderer.color = umbrellaDownColor;
                isUmbrellaFacingDown = true;
                umbrellaDown = true;
            }
            else
            {
                ResetGravityAndColor();
                isUmbrellaFacingDown = false;
                umbrellaDown = false;
            }
        }
        else
        {
            transform.localScale = closedSize;
            umbrellaOpen = false;
            ResetGravityAndColor();
            isUmbrellaFacingDown = false;

            if (wasUmbrellaOpen)
            {
                VFXManager.Instance.PlayVFX("Close_TextOnly", transform.position + new Vector3(0, 0.5f, 0));
                wasUmbrellaOpen = false;

                //SoundFX
                SoundFXManager.instance.PlayUmbrellaCloseClip(transform, 1f);
            }


            // Launch based on umbrella width
            if (orientationInput.magnitude < releaseThreshold && playerController.grounded && Time.time - lastLaunchTime > launchCooldown)
            {
                if (previousOrientationInput.magnitude > releaseThreshold)
                {
                    float launchForceMultiplier = maxUmbrellaWidth / 2;

                    Vector2 launchDirection = previousOrientationInput.normalized;
                    playerRb.AddForce(launchDirection * forceMagnitude * launchForceMultiplier, ForceMode2D.Impulse);
                    lastLaunchTime = Time.time;
                    VFXManager.Instance.PlayVFX("dashPoof", transform.position);
                }
            }

            wasM1 = false;
        }

        float currentUmbrellaWidth = transform.localScale.x;
        //Debug.Log("Umbrella width"+ currentUmbrellaWidth); //0.25 and 2.5
        
        // Update max umbrella width logic
        if (currentUmbrellaWidth > previousUmbrellaWidth)
        {
            maxUmbrellaWidth = currentUmbrellaWidth;
        }
        else if (currentUmbrellaWidth < previousUmbrellaWidth && currentUmbrellaWidth > maxUmbrellaWidth)
        {
            maxUmbrellaWidth = currentUmbrellaWidth;
        }

        previousUmbrellaWidth = currentUmbrellaWidth;
    }

    /* Perform swing if currently swinging */
    void HandleSwinging()
    {
        if (isSwinging)
        {
            PerformSwing();
        }
    }

    /* Prepare for swing */
    void StartSwing(float triggerDepth)
    {
        // Prepare for swing
        umbrellaOpen = false;
        transform.localScale = closedSize;
        playerRb.gravityScale = originalGravityScale;
        playerSpriteRenderer.color = originalColor;

        // Clamp the trigger depth to ensure it's between 0 and 1
        swingExtent = Mathf.Clamp(triggerDepth, 0f, 1f);

        // Only start swinging if the trigger depth greater than the threshold
        if (swingExtent > 0.1f)
        {
            isSwinging = true;
            currentSwingSpeed = Mathf.Lerp(0.5f, 2f, swingExtent);
            originalPosition = transform.position;
            originalRotation = transform.rotation;
        }
    }

    /* Immediately stop swinging and reset swing extent */
    void StopSwing()
    {
        // Immediately stop swinging
        isSwinging = false;
        // Reset swing extent   
        swingExtent = 0f;
        umbrellaOpen = true;
        swingStartTime = Time.time;
    }

    /* Perform swing if currently swinging */
    void PerformSwing()
    {
        if (isSwinging && swingExtent > 0.1f)
        {
            float swingAngle = Mathf.Lerp(90, -60, swingExtent);
            Debug.Log("Trigger Depth: " + swingExtent);

            if (!isFacingRight)
            {
                swingAngle = 180 - swingAngle;
            }

            Vector3 direction = new Vector3(Mathf.Cos(swingAngle * Mathf.Deg2Rad), Mathf.Sin(swingAngle * Mathf.Deg2Rad), 0);
            
            // Apply the same offset used in HandleUmbrellaOrientation
            Vector3 offset = new Vector3(0.888f, 0, 0);
            transform.localPosition = direction * displacement + offset;
            
            transform.localRotation = Quaternion.Euler(0, 0, swingAngle - 90f);

            if (swingExtent > 0.5f)
            {
                StartCoroutine(TriggerVibration(0.1f));
            }
        }
        else
        {
            // Ensure swinging stops if the trigger depth is below the threshold
            isSwinging = false;
        }
    }

    IEnumerator DebounceStopSwing()
    {
        yield return new WaitForSeconds(debounceTime);
        if (swingExtent <= 0.1f)
        {
            isSwinging = false;
        }
    }

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

    void OnTriggerEnter2D(Collider2D other) 
    {
        if (Time.time - lastCollisionTime >= waterCollisionCooldown) 
        {
            bool isWater = other.CompareTag("Water");

            if (isWater && isUmbrellaFacingDown)
            {
                //SoundFX
                SoundFXManager.instance.PlayLandWaterClip(transform, 1f);

                //PlayVfx
                VFXManager.Instance.PlayVFX("Pop", transform.position);
            }
        }
            

    }

    void OnTriggerStay2D(Collider2D other)
    {
        if (Time.time - lastCollisionTime >= collisionCooldown)
        {
            bool isHazard = ((1 << other.gameObject.layer) & hazardLayer) != 0;
            bool isWater = other.CompareTag("Water");
            bool isKillWall = other.CompareTag("Kill Wall");

            if ((umbrellaOpen || isSwinging) && isHazard && !isKillWall)
            {
                float forceMultiplier = 1f;
                if (isWater && !isUmbrellaFacingDown)
                {
                    return;
                } 
                else if (isWater && isUmbrellaFacingDown) 
                {
                    forceMultiplier = 0.8f;
                }
                else 
                {
                    forceMultiplier = 2f;
                }
                if (!isWater) 
                {
                    playerRb.velocity = Vector2.zero;
                    VFXManager.Instance.PlayVFX("LinesExplosion", transform.position + new Vector3(0, 0.5f, 0));
                    StartCoroutine(TriggerVibration(0.10f));

                    //SoundFX
                    SoundFXManager.instance.PlayHitHazardClip(transform, 1f);
                }
                Vector2 collisionDirection = player.position - transform.position;
                collisionDirection.Normalize();
                Vector2 force = collisionDirection * debugForceMagnitude * collisionForceMultiplier * forceMultiplier;
                playerRb.AddForce(force, ForceMode2D.Impulse);

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
            // Gradually reduce upward velocity to 30% of its current value when umbrella is open
            float targetUpwardVelocity = Mathf.Max(playerRb.velocity.y, 0) * 0.3f;
            float smoothedUpwardVelocity = Mathf.Lerp(playerRb.velocity.y, targetUpwardVelocity, Time.fixedDeltaTime * 5f);
            
            // Apply the smoothed velocity and slow fall speed
            playerRb.velocity = new Vector2(playerRb.velocity.x, smoothedUpwardVelocity - slowFallSpeed * 0.2f);
        }
    }

    void ResetGravityAndColor()
    {
        playerRb.gravityScale = originalGravityScale;
    }

    public void OnOrient(InputAction.CallbackContext context)
    {
        orientationInput = context.ReadValue<Vector2>();
        if (orientationInput.x > 0)
        {
            isFacingRight = true;
        }
        else if (orientationInput.x < 0)
        {
            isFacingRight = false;
        }
    }
    private IEnumerator TriggerVibration(float time)
    {
        // Set motor speeds to create the vibration effect
        Gamepad.current.SetMotorSpeeds(0.01f, 0.03f);

        // Wait for a short duration
        yield return new WaitForSeconds(time);

        // Stop the vibration
        Gamepad.current.SetMotorSpeeds(0f, 0f);
    }
}