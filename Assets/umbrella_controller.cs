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
    public Color umbrellaDownColor = Color.green; 
    public LayerMask hazardLayer; 
    public float collisionForceMultiplier = 0.5f; 
    public float debugForceMagnitude = 10f; 
    public float collisionCooldown = 0.1f; 

    private Vector3 closedSize = new Vector3(0.25f, 2f, 1f); 
    private Vector3 openSize = new Vector3(2.5f, 0.5f, 1f); 
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
        HandleUmbrellaOrientation();
        HandleSwinging();
        previousOrientationInput = orientationInput;
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

            transform.position = player.position + direction * displacement; 
            transform.rotation = Quaternion.Euler(0, 0, angle);
            transform.localScale = Vector3.Lerp(closedSize, openSize, joystickMagnitude);
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
                playerSpriteRenderer.color = slowedColor;
                isUmbrellaFacingDown = false;
            }
            else if (angle > 135f && angle < 225f)
            {
                playerSpriteRenderer.color = umbrellaDownColor;
                isUmbrellaFacingDown = true;
            }
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

            if (wasUmbrellaOpen)
            {
                VFXManager.Instance.PlayVFX("Close_TextOnly", transform.position + new Vector3(0, 0.5f, 0));
                wasUmbrellaOpen = false;

                //SoundFX
                SoundFXManager.instance.PlayUmbrellaCloseClip(transform, 1f);
            }

            if (previousOrientationInput.magnitude > releaseThreshold && orientationInput.magnitude < releaseThreshold && playerController.grounded)
            {
                float launchForceMultiplier = Mathf.Lerp(0.1f, 1.2f, previousOrientationInput.magnitude);
                playerRb.AddForce(new Vector2(previousOrientationInput.x, previousOrientationInput.y) * forceMagnitude * launchForceMultiplier, ForceMode2D.Impulse);
                lastLaunchTime = Time.time;
                VFXManager.Instance.PlayVFX("SpeedLines", transform.position);
            }

            wasM1 = false;
        }
    }

    void HandleSwinging()
    {
        if (isSwinging)
        {
            PerformSwing();
        }
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
        isSwinging = false;
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

            transform.position = player.position + direction * displacement;
            transform.rotation = Quaternion.Euler(0, 0, angle);
        }
    }

    void PerformSwing()
    {
        float elapsedTime = Time.time - swingStartTime;
        if (elapsedTime < swingDuration / currentSwingSpeed)
        {
            float swingProgress = elapsedTime / (swingDuration / currentSwingSpeed);
            float swingAngle = Mathf.Lerp(90, swingExtent, swingProgress);
            if (!isFacingRight)
            {
                swingAngle = 180 - swingAngle;
            }
            Vector3 direction = new Vector3(Mathf.Cos(swingAngle * Mathf.Deg2Rad), Mathf.Sin(swingAngle * Mathf.Deg2Rad), 0);
            transform.localPosition = direction * displacement;
            transform.localRotation = Quaternion.Euler(0, 0, swingAngle - 90f);
        }
        else
        {
            StopSwing();
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
                    return;
                } 
                else if (isWater && isUmbrellaFacingDown) 
                {
                    forceMultiplier = 0.5f;
                }
                else 
                {
                    forceMultiplier = 2f;
                }
                if (!isWater) 
                {
                    playerRb.velocity = Vector2.zero;
                    VFXManager.Instance.PlayVFX("LinesExplosion", transform.position + new Vector3(0, 0.5f, 0));
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
}