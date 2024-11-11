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
    private Rigidbody2D rb;
    public bool umbrellaActive = false;
    // for SoundFX
    public float windCollisionCooldown = 1f;
    private float lastCollisionTime = 0f;

    private PlayerInputActions inputActions;
    private Vector2 moveInput;
    private bool lastDirectionRight = true; // true for right, false for left

    // Determines the amount of time for the player to be sliding left / right, enabling purely horizontal launches
    public bool isLaunching = false;
    private float launchTime = 0f;
    private float launchDuration = 2.5f;

    private float shakeMagnitude = 0.1f; // Define shake magnitude

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
            if (grounded && !isLaunching)
            {
                // If on ground and not launching, apply ground force logic
                rb.AddForce(Vector2.right * moveInput.x * moveSpeed, ForceMode2D.Force);
            }
            else if (!grounded)
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
        if (grounded && !isLaunching)
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
        // Check if player touches a hazard
        if (collision.gameObject.layer == LayerMask.NameToLayer("Hazard"))
        {
            SoundFXManager.instance.PlayDeathClip(transform, 0.5f);
            HitStop(250, () => {
                // Find the spawner closest to the left of the player
                GameObject[] spawners = GameObject.FindGameObjectsWithTag("Player Spawn");
                GameObject closestSpawner = null;
                float playerX = transform.position.x;
                float closestX = float.MinValue;

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

        // Check if player is grounded
        if (!isLaunching && other.gameObject.layer == LayerMask.NameToLayer("Terrain"))
        {
            grounded = true;
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

        // Check if player is no longer grounded
        if (trigger.gameObject.layer == LayerMask.NameToLayer("Terrain"))
        {
            grounded = false;
        }
    }

    public void OnMove(InputAction.CallbackContext context)
    {
        moveInput = context.ReadValue<Vector2>();
    }

    public void HitStop(float milliseconds, System.Action onComplete = null)
    {
        StartCoroutine(HitStopCoroutine(milliseconds, onComplete));
    }

    private IEnumerator HitStopCoroutine(float milliseconds, System.Action onComplete)
    {
        float originalTimeScale = Time.timeScale;
        // Freeze game
        Time.timeScale = 0f;
        StartCoroutine(ScreenShakeCoroutine(milliseconds / 1000f));
        yield return new WaitForSecondsRealtime(milliseconds / 1000f);
        Time.timeScale = originalTimeScale;
        // Execute callback
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
}
