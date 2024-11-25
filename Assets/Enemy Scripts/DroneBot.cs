using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DroneBot : MonoBehaviour
{
    [Header("Respawn Settings")]
    [Tooltip("Tag used to identify respawn points in the scene.")]
    [SerializeField] private string respawnTag = "Player Spawn"; 

    [Tooltip("Range of movement for the patrol path.")]
    [Range(0.5f, 500f)] public float patrolRange = 10f;

    [Tooltip("Bot's Patrol Speed.")]
    [Range(0.5f, 100f)] public float movementSpeed = 2f;

    [Tooltip("Bot's Movement Pattern: Sine, Linear, or Radial.")]
    public MovementPattern movementPattern = MovementPattern.Vertical;

    [Header("Detection Settings")]
    [Tooltip("Distance the bot detects the player.")]
    [Range(0.5f, 300f)] public float detectionRange = 5f;

    [Header("Deflection Settings")]
    [Tooltip("ForceIncreasesNearExplosion: More force near end as robot is about to explode. ForceDecreasesNearExplosion: More force the earlier you hit the bot")]
    public DeflectionMode deflectionMode = DeflectionMode.ForceIncreasesNearExplosion;

    [Tooltip("Determines whether the bot explodes immediately or follows player before exploding.")]
    public DetectionType detectionType = DetectionType.ExplodeOnDetection;

    //Duration for following the player when in proximity
    public float followDuration = 3f;
    //Speed while following the player
    public float followSpeed = 2f; 

    [Header("Explosion Settings")]
    [Tooltip("Radius of the explosion effect.")]
    [Range(0.5f, 200f)] public float explosionRadius = 3f;
    [Tooltip("Time before the bot explodes after being triggered.")]
    [SerializeField] public float totalTimeToExplode = 5f;


    /// <summary>
    /// Blake here! Just added this- dunno what it does to your code- but it would not run without this addition.
    /// </summary>
    // Total time before the bot explodes
    [Tooltip("Total time before the bot explodes.")]
    public float totalTimeToExplode = 5f;

    // Force applied during deflection
    private float deflectionForce;

    //----- Internal Variables -------

    //Initial position of the bot
    private Vector3 startPosition;
    //Timer for radial movement
    private float timeElapsed;
    //Reference to the player object
    private Transform player;
    //Trigger for whether the bot has detected the player
    private bool isTriggered = false;
    //Deflection (swing/attack or open umbrella). Needs reference and rigidbody for angle calculations (for bounce-back/rebound)
    private UmbrellaController umbrellaController;
    //enemy rigidbody
    private Rigidbody2D rb;
    //playerrb
    private Rigidbody2D playerRb;

    //Movement pattern & explosion behaviour
    public enum MovementPattern {Vertical, Linear, Radial}
    public enum DetectionType {ExplodeOnDetection, FollowAndExplode}

    //Deflection/Launch behaviour
    //There's two since when user testing, I want the desigenr to simply toggle between the two options
    //ForceIncreasesNearExplosion: More force closer to explosion --> Players feel a sense of urgency to hit the bot before it explodes & Deflection force rewards precision under pressure.
    //ForceDecreasesNearExplosion: More force earlier --> Encourages players to act quickly and rewards faster reflexes & Punishes hesitation, which can add challenge.

    public enum DeflectionMode {ForceIncreasesNearExplosion, ForceDecreasesNearExplosion}
    //Tracks time since detection
    private float timeSinceTrigger = 0f;
    //Dynamically calculated based on the remaining time
    private float deflectionForce;
    [Tooltip("Multiplier applied to the calculated deflection force for added control.")]
    [Range(0.1f, 10f)] public float deflectionForceMultiplier = 1f;
    [Tooltip("Multiplier applied to the player's knockback force for fine-tuning.")]
    [Range(0.1f, 10f)] public float playerForceMultiplier = 1.5f;




    void Start()
    {
        //Cache initial position
        startPosition = transform.position;

        //Find player and umbrella controller
        player = GameObject.FindGameObjectWithTag("Player")?.transform;
        if (player == null)
        {
            Debug.LogError("Player object not found! Ensure the player has the 'Player' tag.");
        }

        umbrellaController = player?.GetComponentInChildren<UmbrellaController>();
        if (umbrellaController == null)
        {
            Debug.LogError("UmbrellaController not found on the player!");
        }

        
        //Create rigidbody for enemy if none is provided 
        rb = gameObject.GetComponent<Rigidbody2D>();
        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody2D>();
            //Disable gravity for 2D bots
            rb.gravityScale = 0; 
        }

        //Add or find CircleCollider2D and match it to sprite size
        CircleCollider2D collider = gameObject.GetComponent<CircleCollider2D>();
        if (collider == null)
        {
            collider = gameObject.AddComponent<CircleCollider2D>();
        }

        //Match collider size to the sprite
        SpriteRenderer spriteRenderer = gameObject.GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
        {
            //Radius = half the sprite's width
            collider.radius = spriteRenderer.bounds.size.x / 2f; 
        }


        playerRb = player?.GetComponent<Rigidbody2D>();
        if (playerRb == null) Debug.LogError("Player does not have Rigidbody2D!");
    }


    void Update()
    {
        if (isTriggered)
        {
            timeSinceTrigger += Time.deltaTime;

            //Behavior depends on the detection type chosen by level designer
            if (detectionType == DetectionType.ExplodeOnDetection)
            {
                //Explode after the configured total timer
                if (timeSinceTrigger >= totalTimeToExplode)
                {
                    Explode();
                }
            }
            else if (detectionType == DetectionType.FollowAndExplode)
            {
                //Explode after the follow duration ends
                if (timeSinceTrigger >= followDuration)
                {
                    Explode();
                }
            }
        }
        else
        {
            Patrol();
            DetectPlayer();
        }
    }

    //-------------------- PATROL MOVEMENT FUNCTIONS --------------------//
    
    //Handles the bot's movement based on the selected movement pattern by level designer
    void Patrol()
    {
        Vector3 hoverOffset = Vector3.up;

        switch (movementPattern)
        {
            //Wave-Like Pattern
            case MovementPattern.Vertical:
                transform.position = startPosition + hoverOffset +
                new Vector3(0, Mathf.Sin(Time.time * movementSpeed) * patrolRange, 0);
                break;
            //Standard back and forth movement
            case MovementPattern.Linear:
                transform.position = startPosition + hoverOffset +
                new Vector3(Mathf.PingPong(Time.time * movementSpeed, patrolRange) - (patrolRange / 2), 0, 0);
                break;
            //Circular (ferris-wheel)
            case MovementPattern.Radial:
                timeElapsed += Time.deltaTime * movementSpeed;
                transform.position = startPosition + hoverOffset +
                new Vector3(Mathf.Cos(timeElapsed) * patrolRange, Mathf.Sin(timeElapsed) * patrolRange, 0);
                break;
        }
    }

    //Detects the player within the bot's detection range and triggers correspinding explosion behavior specified by the level designer.
    void DetectPlayer()
    {
        if (player == null) return;

        if (Vector3.Distance(transform.position, player.position) <= detectionRange)
        {
            isTriggered = true;

            if (detectionType == DetectionType.ExplodeOnDetection)
            {
                StartCoroutine(HandleExplosionTimer(totalTimeToExplode));
            }
            else if (detectionType == DetectionType.FollowAndExplode)
            {
                StartCoroutine(FollowAndExplode());
            }
        }
    }
    //For ExplodeOnDetection
    //Plays timer before exploding. timer is configurable by level designer
    private IEnumerator HandleExplosionTimer(float duration)
    {
        yield return new WaitForSeconds(duration);
        if (isTriggered) Explode();
    }


    //-------------------- DEFLECTION METHODS --------------------//

    //Bounce-back launch which occurs opposite of the open umbrella's direction
    //Same direction but upwards, so it launches in opposite direction 
    private void OnCollisionEnter2D(Collision2D collision)
    {
        //Trigger deflection logic for collisions with the player
        if (collision.collider.CompareTag("Player"))
        {
            //Handle deflection each time a collision occurs
            HandleDeflection(collision); 
        }
    }

    //Function is seperate because if I want to make it public and accessible by other scripts for the same behaviours I can
    //OnCollision is locked/protected (can't be accessed)
    private void HandleDeflection(Collision2D collision)
    {
        //Variables to store deflection direction for the bot and knockback direction for the player
        Vector2 botDeflectionDirection;
        Vector2 playerKnockbackDirection;

        //Determine deflection behavior when the umbrella is open
        if (umbrellaController != null && umbrellaController.umbrellaOpen)
        {
            //Calculate umbrella deflection direction
            Vector2 umbrellaDirection = GetUmbrellaDeflectionDirection();
            //Bot gets deflected opposite to the umbrella's direction
            botDeflectionDirection = -umbrellaDirection;
            //Player gets knocked back in the same direction as the umbrella
            playerKnockbackDirection = umbrellaDirection;
        }
        else if (umbrellaController != null && umbrellaController.isSwinging)
        {
            //Bot moves away from collision
            botDeflectionDirection = (collision.transform.position - transform.position).normalized;
            //Player moves opposite to the bot
            playerKnockbackDirection = (transform.position - collision.transform.position).normalized;
        }
        else
        {
            //If no umbrella action is detected, exit this knockback function
            return;
        }

        //Calculate the remaining time for the bot before explosion
        float remainingTime = totalTimeToExplode - timeSinceTrigger;

        //Adjust deflection force based on the selected deflection mode and configurable multipliers (for bot it's deflectionForceMultiplier, for player it's playerForceMultiplier) 
        if (deflectionMode == DeflectionMode.ForceIncreasesNearExplosion)
        {
            deflectionForce = Mathf.Lerp(5f, 20f, 1 - (remainingTime / totalTimeToExplode)) * deflectionForceMultiplier;
        }
        else
        {
            deflectionForce = Mathf.Lerp(20f, 5f, 1 - (remainingTime / totalTimeToExplode)) * playerForceMultiplier;
        }

        //Scale the force applied to the player to ensure proportional knockback
        float botMass = rb.mass > 0 ? rb.mass:1f;
        float playerMass = playerRb != null && playerRb.mass > 0 ? playerRb.mass:1f;

        //Apply calculated deflection force to the bot
        rb.AddForce(botDeflectionDirection * deflectionForce, ForceMode2D.Impulse);

        // Apply knockback force to the player with proportional scaling
        if (playerRb != null)
        {
            float adjustedForce = deflectionForce * playerForceMultiplier * (botMass / playerMass);
            playerRb.AddForce(playerKnockbackDirection * adjustedForce, ForceMode2D.Impulse);
        }
    }

    //Obtains angle to do the launch
    private Vector2 GetUmbrellaDeflectionDirection()
    {
        //Calculate direction based on umbrella's angle
        float angleRad = umbrellaController.umbrellaAngle * Mathf.Deg2Rad;
        return new Vector2(Mathf.Cos(angleRad), Mathf.Sin(angleRad));
    }

    //Deflection Behaviour depending on teh option the level designer chooses
    private void DeflectBot(Vector2 deflectionDirection)
    {
        //Calculate remaining time
        float remainingTime = totalTimeToExplode - timeSinceTrigger;

        // djust deflection force based on the selected mode
        if (deflectionMode == DeflectionMode.ForceIncreasesNearExplosion)
        {
            deflectionForce = Mathf.Lerp(5f, 20f, 1 - (remainingTime/totalTimeToExplode));
        }
        else if (deflectionMode == DeflectionMode.ForceDecreasesNearExplosion)
        {
            deflectionForce = Mathf.Lerp(20f, 5f, 1 - (remainingTime/totalTimeToExplode));
        }

        //Apply deflection force
        rb.AddForce(deflectionDirection * deflectionForce, ForceMode2D.Impulse);

        //Destroy the bot after deflection
        Destroy(gameObject, 0.5f);

        //PLAY VFX:

        //PLAY SOUND:
    }

    //-------------------- EXPLOSION BEHAVIOURS --------------------//

    //Immediate Explosion: Causes the bot to explode in place immediately, affecting nearby objects within the explosion radius.
    void Explode()
    {
        Debug.Log("Bot is exploding!");

        //Check for objects within the explosion radius
        Collider2D[] affectedObjects = Physics2D.OverlapCircleAll(transform.position, explosionRadius);

        foreach (var obj in affectedObjects)
        {
            if (obj.CompareTag("Player"))
            {
                RespawnPlayer();
                break;
            }
        }

        //Destroy the bot's GameObject 
        Destroy(gameObject);
    }


    //FollowAndExplode: Follows the player for a set duration before exploding.
    IEnumerator FollowAndExplode()
    {
        float followTime = 0f;

        while (followTime < followDuration)
        {
            //Move towards the player
            transform.position = Vector3.MoveTowards(transform.position, player.position, followSpeed * Time.deltaTime);

            //Immediate explosion on collision
            if (Vector3.Distance(transform.position, player.position) <= 0.1f)
            {
                RespawnPlayer();
                Destroy(gameObject);
                yield break;
            }

            followTime += Time.deltaTime;
            //Increment the timer for consistency
            timeSinceTrigger += Time.deltaTime; 
            yield return null;
        }

        // Explode when the follow duration ends
        Explode();
    }

    //-------------------- RESPAWN POINT AND OTHER GAME-WORLD FUNCTIONS --------------------//
    //Finds the closest respawn point using the Player Spawn tag
    void RespawnPlayer()
    {
        GameObject[] respawnPoints = GameObject.FindGameObjectsWithTag(respawnTag);

        if (respawnPoints.Length == 0)
        {
            Debug.LogError("No respawn points found in the scene!");
            return;
        }

        GameObject closestRespawn = null;
        float closestDistance = Mathf.Infinity;
        Vector3 playerPosition = player.position;

        foreach (var respawn in respawnPoints)
        {
            float distance = Vector3.Distance(respawn.transform.position, playerPosition);
            if (distance < closestDistance)
            {
                closestRespawn = respawn;
                closestDistance = distance;
            }
        }

        if (closestRespawn != null)
        {
            Debug.Log("Player respawned at the nearest checkpoint.");
            player.position = closestRespawn.transform.position;
        }
    }

    

    //-------------------- UI & VISUAL FUNCTIONS --------------------//
    //Visualizing the bot's variables for debugging (color, shape-visual & label)
    void OnDrawGizmosSelected()
    {


        //Detection range
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, detectionRange);
        UnityEditor.Handles.Label(transform.position + Vector3.right * (detectionRange + 0.2f) + Vector3.up * -1.5f, "Detection Range");


        //Explosion range
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, explosionRadius);
        UnityEditor.Handles.Label(transform.position + Vector3.right * (explosionRadius - 0.5f) + Vector3.up * 2.5f, "Explosion Range");


        //Overall debug label
        UnityEditor.Handles.Label(transform.position + Vector3.up * +7f, "**Enemy Bot Variable View**");


        //Patrol range visualization
        Gizmos.color = Color.yellow;
        switch (movementPattern)
        {
            case MovementPattern.Vertical:
                // Vertical movement bounds
                Gizmos.DrawWireCube(transform.position, new Vector3(0.1f, patrolRange * 2, 0.1f));
                UnityEditor.Handles.Label(transform.position + Vector3.up * (patrolRange + 0.5f), "Patrol Range (Vertical)");
                break;

            case MovementPattern.Linear:
                //Show horizontal patrol bounds
                Gizmos.DrawWireCube(transform.position, new Vector3(patrolRange * 2, 0.1f, 0.1f));
                UnityEditor.Handles.Label(transform.position + Vector3.right * (patrolRange + 0.5f), "Patrol Range (Horizontal)");

                break;

            case MovementPattern.Radial:
                //Show a circular patrol area
                Gizmos.DrawWireSphere(transform.position, patrolRange);
                UnityEditor.Handles.Label(
                    transform.position + Vector3.right * (patrolRange + 0.5f) + Vector3.up * 0.5f,
                    "Patrol Range (Radial)");
                break;
        }
    }
}