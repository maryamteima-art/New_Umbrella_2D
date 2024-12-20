using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

public class PlayerController : MonoBehaviour, PlayerInputActions.IPlayerActions
{
    public float moveSpeed = 10f;
    public float damping = 0.95f;
    public float airMultiplier = 0.1f;
    public float maxAirSpeed = 5f;
    public bool inWind = false;
    public bool grounded = false;
    public bool doubleJumper = false;
    private Rigidbody2D rb;
    public bool umbrellaActive = false;
    // Animator
    public Animator robotAnimator;
    public SpriteRenderer robotSpriteRenderer;
    
    // for SoundFX
    public float windCollisionCooldown = 2f;
    private float lastCollisionTime = 0f;
    
    public float walkCooldown = 0.35f;
    private float lastWalk = 0f;

    private PlayerInputActions inputActions;
    private Vector2 moveInput;
    private bool lastDirectionRight = true; // true for right, false for left

    // Determines the amount of time for the player to be sliding left / right, enabling purely horizontal launches
    public bool isLaunching = false;
    private float launchTime = 0f;
    private float launchDuration = 2.5f;

    private float shakeMagnitude = 0.1f;

    public GameObject chargeMeter;
    private float chargeMeterInitialHeight;

    private float chargeMeterDecreaseRate = 0f;

    //-------- FOR VFX ---------
    //Trail object attached to the player (for gliding)
    public TrailRenderer trailRenderer;
    //How quickly the trail shrinks (in width to give illusion of fading away, since the fade-out didn't work)
    public float trailShrinkSpeed = 5f;
    private float exitWindTimer = 0f;
    private bool exitingWind = false;

    //-------- END OF VFX-------

    // Add a reference to the GroundCheck object
    public Transform groundCheck;
    public float groundCheckRadius = 0.2f;
    public LayerMask terrainLayer;

    public bool hasBubble = false; // Indicates if player has a bubble

    private bool isHitStopActive = false;
     //UmbrellaController umbrella = other.GetComponent<UmbrellaController>();

    void Awake()
    {
        inputActions = new PlayerInputActions();
        inputActions.Player.SetCallbacks(this);
        rb = GetComponent<Rigidbody2D>();
        chargeMeterInitialHeight = chargeMeter.transform.localScale.y;
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
        // This should be in elsewhere, but I'm lazy
        // Show the cursor
        Cursor.visible = true;
        // Lock the cursor to the centre of the screen
        Cursor.lockState = CursorLockMode.Locked;

        // Get player and prevent tilting
        rb.freezeRotation = true;

        //VFX: Find Trail Object attached to the Player
        trailRenderer = GameObject.Find("Flight_Trail").GetComponent<TrailRenderer>();

        //Ensure the trail is disabled at the start
        if (trailRenderer != null)
        {
            trailRenderer.enabled = false;
            //Reset the trail to avoid residual effects
            trailRenderer.Clear();
        }
    }

    void Update()
    {
        if (!PauseMenu.GamePaused)
        {
            // Apply forces
            if (grounded && !isLaunching && !doubleJumper)
            {
                // Ground force logic
                rb.AddForce(Vector2.right * moveInput.x * moveSpeed, ForceMode2D.Force);
            }
            else
            {
                // Air control logic
                float airControl = moveInput.x * moveSpeed * airMultiplier;
                rb.AddForce(Vector2.right * airControl, ForceMode2D.Force);
                Vector2 velocity = rb.velocity;
                velocity.x = Mathf.Clamp(velocity.x, -maxAirSpeed, maxAirSpeed);
                rb.velocity = velocity;
            }

            // Animator updates
            robotAnimator.SetBool("Grounded", grounded && !doubleJumper);
            robotAnimator.SetFloat("Horizontal", moveInput.x);

            // Dampen player on ground
            if (grounded && !isLaunching && !doubleJumper)
            {
                Vector2 velocity = rb.velocity;
                velocity.x *= damping;
                rb.velocity = velocity;
            }

            // Debugging
            Debug.Log($"Velocity: {rb.velocity.x}, DoubleJumper: {doubleJumper}");

            // Update last direction based on move input
            if (moveInput.x > 0)
            {
                lastDirectionRight = true;

                //Animator
                //robotSpriteRenderer.flipX = false;

                if ((Time.time - lastWalk >= walkCooldown) && (grounded))
                {
                    //SoundFX
                    SoundFXManager.instance.PlayWalkClip(transform, 0.5f);

                    lastWalk = Time.time;
                }

            }
            else if (moveInput.x < 0)
            {
                lastDirectionRight = false;

                //Animator
                //robotSpriteRenderer.flipX = true;

                if ((Time.time - lastWalk >= walkCooldown) && (grounded))
                {
                    //SoundFX
                    SoundFXManager.instance.PlayWalkClip(transform, 0.5f);

                    lastWalk = Time.time;
                }
            }


            // Continuously update charge meter if the decrease rate is set
            if (chargeMeterDecreaseRate > 0)
            {
                UpdateChargeMeter();
            }

            //VFX: ENABLE & DISABLE TRAIL 
            if (grounded && !doubleJumper)
            {
                //Gradually reduce trail width and disable when done
                if (trailRenderer != null && trailRenderer.widthMultiplier > 0f)
                {
                    trailRenderer.widthMultiplier -= trailShrinkSpeed * Time.deltaTime;

                    //Fully hide the trail when the width reaches zero
                    if (trailRenderer.widthMultiplier <= 0f)
                    {
                        //Clamping
                        trailRenderer.widthMultiplier = 0f;
                        //Clear lingering trails
                        trailRenderer.Clear();
                        //Disable trail
                        trailRenderer.enabled = false;
                    }
                }
            }
            else if (inWind)
            {
                //Reset exitWindTimer and exitingWind flag
                exitWindTimer = 0f;
                exitingWind = false;

                //Gradually increase the trail width to its default value
                if (trailRenderer != null && !trailRenderer.enabled)
                {
                    //Re-enable the trail if disabled
                    trailRenderer.enabled = true;
                }

                if (trailRenderer != null && trailRenderer.widthMultiplier < 1f)
                {
                    trailRenderer.widthMultiplier += trailShrinkSpeed * Time.deltaTime;

                    //Clamp width to maximum (default)
                    if (trailRenderer.widthMultiplier > 1f)
                    {
                        trailRenderer.widthMultiplier = 1f;
                    }
                }
            }
            else
            {
                //If exiting the winded region, start the timer
                if (!exitingWind)
                {
                    exitingWind = true;
                    //Set the delay time (how long it stays on out-of-wind region)
                    exitWindTimer = 3f; 
                }

                //Countdown the exit timer
                if (exitingWind && exitWindTimer > 0f)
                {
                    exitWindTimer -= Time.deltaTime;
                }
                else
                {
                    // Gradually reduce the trail width after the delay
                    if (trailRenderer != null && trailRenderer.widthMultiplier > 0f)
                    {
                        trailRenderer.widthMultiplier -= trailShrinkSpeed * Time.deltaTime;

                        //Fully hide the trail when the width reaches zero (clamp, clear & disable)
                        if (trailRenderer.widthMultiplier <= 0f)
                        {
                            trailRenderer.widthMultiplier = 0f;
                            trailRenderer.Clear();
                            trailRenderer.enabled = false;
                        }
                    }
                }
            }
        }
            
        
    }

    public void UpdateChargeMeter()
    {
        // Calculate new height based on time and decrease rate
        float newHeight = chargeMeter.transform.localScale.y - (chargeMeterDecreaseRate * Time.deltaTime);
        newHeight = Mathf.Max(newHeight, 0); // Ensure it doesn't go below zero

        // Calculate the change in height
        float heightDifference = chargeMeter.transform.localScale.y - newHeight;

        // Apply new scale to the charge meter
        Vector3 newScale = chargeMeter.transform.localScale;
        newScale.y = newHeight;
        chargeMeter.transform.localScale = newScale;

        // Adjust position to decrease from the top
        Vector3 newPosition = chargeMeter.transform.localPosition;
        newPosition.y -= heightDifference / 2; 
        chargeMeter.transform.localPosition = newPosition;

        // Reset meter if grounded
        if (grounded)
        {
            chargeMeter.transform.localScale = new Vector3(newScale.x, chargeMeterInitialHeight, newScale.z);
            chargeMeter.transform.localPosition = new Vector3(newPosition.x, 0, newPosition.z);
        }

        // Don't push player up if meter empty
        if (chargeMeterDecreaseRate > 0 && newHeight > 0)
        {
            float forceMagnitude = Mathf.Lerp(100f, 1000f, chargeMeterDecreaseRate);
            rb.AddForce(Vector2.up * forceMagnitude * Time.deltaTime, ForceMode2D.Force);

        }
    }

    void OnCollisionStay2D(Collision2D collision)
    {
        // Check if player touches a hazard
        if (collision.gameObject.layer == LayerMask.NameToLayer("Hazard"))
        {
            if (!hasBubble) {
                SoundFXManager.instance.PlayDeathClip(transform, 0.5f);
                HitStop(250, () => {
                    // Find the spawner closest to the left of the player
                    GameObject[] spawners = GameObject.FindGameObjectsWithTag("Player Spawn");
                    GameObject closestSpawner = null;
                    float playerX = transform.position.x;
                    float closestX = float.MinValue;
                    //Gamepad.current.SetMotorSpeeds(0.25f, 0.30f);
                    //Gamepad.current.SetMotorSpeeds(0f, 0f);
                    StartCoroutine(TriggerVibration(0.15f));
                    foreach (GameObject spawner in spawners)
                    {
                        float spawnerX = spawner.transform.position.x;
                        if (spawnerX < playerX && spawnerX > closestX)
                        {
                            closestX = spawnerX;
                            closestSpawner = spawner;
                        }
                    }

                    if (closestSpawner != null)
                    {
                        transform.position = closestSpawner.transform.position;
                    }
                });
            }
            else {
                hasBubble = false;
                //PLAY VFX
                VFXManager.Instance.PlayVFX("PopBubble", transform.position);
            }
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
        //Gamepad.current.SetMotorSpeeds(0.25f, 0.30f);
        Vector2 input = context.ReadValue<Vector2>();

        // Dead zone margin
        float deadZone = 0.1f;

        // Apply dead zone
        if (input.magnitude < deadZone)
        {
            moveInput = Vector2.zero;
        }
        else
        {
            moveInput = input;
        }
    }

    public void HitStop(float milliseconds, System.Action onComplete = null)
    {
        if (isHitStopActive) return; // Prevent re-entry
        isHitStopActive = true;
        
        StartCoroutine(HitStopCoroutine(milliseconds, () =>
        {
            // Debug.Log("HitStop complete");
            isHitStopActive = false; // Reset flag
            onComplete?.Invoke();
        }));
    }

    private IEnumerator HitStopCoroutine(float milliseconds, System.Action onComplete)
    {
        float originalTimeScale = Time.timeScale;
        try
        {
            Time.timeScale = 0f;
            // Debug.Log("Time scale set to 0");
            StartCoroutine(ScreenShakeCoroutine(milliseconds / 1000f));
            yield return new WaitForSecondsRealtime(milliseconds / 1000f);
        }
        finally
        {
            Time.timeScale = originalTimeScale;
            // Debug.Log("Time scale reset to: " + Time.timeScale);
        }
        onComplete?.Invoke();
    }

    private IEnumerator ScreenShakeCoroutine(float duration)
    {
        Vector3 originalCameraPosition = Camera.main.transform.localPosition;
        float elapsed = 0.0f;

        while (elapsed < duration)
        {
            // Shake values
            float x = Random.Range(-1f, 1f) * shakeMagnitude;
            float y = Random.Range(-1f, 1f) * shakeMagnitude;

            // Keep current camera position while shaking
            Camera.main.transform.localPosition = new Vector3(
                originalCameraPosition.x + x, 
                originalCameraPosition.y + y, 
                originalCameraPosition.z
            );

            // Shake despite hit stop
            elapsed += Time.unscaledDeltaTime;
            yield return null;
        }

        Camera.main.transform.localPosition = originalCameraPosition;
    }

    public void OnJetpack(InputAction.CallbackContext context)
    {
        // Check if the trigger is pressed or released
        if (context.phase == InputActionPhase.Performed)
        {
            // Adjust decrease rate based on trigger input
            float triggerValue = context.ReadValue<float>();
            chargeMeterDecreaseRate = Mathf.Lerp(0.2f, 1f, triggerValue);

            //FOR VFX: THRUST CHECKER
            if (HasFuel()) 
            { 
                VFXManager.Instance.PlayVFX("NewThrust", transform.position + new Vector3(0, -0.5f, 0));

                // Animator
                robotAnimator.SetBool("Boosting", true);
                //robotAnimator.SetTrigger("Jetpack");
                robotAnimator.SetFloat("Horizontal", moveInput.x);
            }
            else 
            {
                // Animator
                robotAnimator.SetBool("Boosting", false);
                robotAnimator.SetFloat("Horizontal", moveInput.x);
            }
        }
        else if (context.phase == InputActionPhase.Canceled)
        {
            // Reset decrease rate when trigger is released
            chargeMeterDecreaseRate = 0f;

            // Animator
            robotAnimator.SetBool("Boosting", false);
            robotAnimator.SetFloat("Horizontal", moveInput.x);
        }
    }

// New ground detection logic is on the GroundCheck child using "groundchecker" script
    // void OnCollisionEnter2D(Collision2D collision)
    // {
    //     // Check if GroundCheck collides with Terrain
    //     if (collision.gameObject.layer == LayerMask.NameToLayer("Terrain"))
    //     {
    //         grounded = true;
    //         UpdateChargeMeter();

    //         //Animator
    //         robotAnimator.SetBool("Grounded", grounded);
            
    //         //SoundFX
    //         SoundFXManager.instance.PlayLandClip(transform, 0.5f);

    //         //VFX
    //         VFXManager.Instance.PlayVFX("dust", transform.position);
    //     }
    // }

    // void OnCollisionExit2D(Collision2D collision)
    // {
    //     // Check if GroundCheck exits collision with Terrain
    //     if (collision.gameObject.layer == LayerMask.NameToLayer("Terrain"))
    //     {
    //         grounded = false;
    //         UpdateChargeMeter();
            
    //         //Animator
    //         robotAnimator.SetBool("Grounded", grounded);
            
            
    //     }
    // }

    //FOR VFX:
    // Method to check if the charge meter has fuel
    private bool HasFuel()
    {
        return chargeMeter.transform.localScale.y > 0;
    }
    //Triggering Rumble feedback 
    //https://docs.unity3d.com/Packages/com.unity.inputsystem@0.9/manual/Gamepad.html

    private IEnumerator TriggerVibration(float time)
    {
        // Set motor speeds to create the vibration effect
        Gamepad.current.SetMotorSpeeds(0.25f, 0.30f);

        // Wait for a short duration
        yield return new WaitForSeconds(time);

        // Stop the vibration
        Gamepad.current.SetMotorSpeeds(0f, 0f);
    }
}
