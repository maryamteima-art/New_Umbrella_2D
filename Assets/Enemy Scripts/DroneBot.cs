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

    [Tooltip("Determines whether the bot explodes immediately or follows player before exploding.")]
    public DetectionType detectionType = DetectionType.ExplodeOnDetection;

    //Duration for following the player when in proximity
    public float followDuration = 3f;
    //Speed while following the player
    public float followSpeed = 2f; 

    [Header("Explosion Settings")]
    [Tooltip("Radius of the explosion effect.")]
    [Range(0.5f, 200f)] public float explosionRadius = 3f;

    //----- Internal Variables -------
    
    //Initial position of the bot
    private Vector3 startPosition;
    //Timer for radial movement
    private float timeElapsed;
    //Reference to the player object
    private Transform player;
    //Trigger for whether the bot has detected the player
    private bool isTriggered = false; 

    public enum MovementPattern {Vertical, Linear, Radial}
    public enum DetectionType {ExplodeOnDetection, FollowAndExplode}


    void Start()
    {
        //Cache the initial position of the bot
        startPosition = transform.position;

        //Find the player by tag
        player = GameObject.FindGameObjectWithTag("Player")?.transform;
        if (player == null)
        {
            Debug.LogError("Player object not found! Ensure the player has the 'Player' tag.");
        }
    }


    void Update()
    {
        //If the bot has been triggered, no further actions needed
        if (isTriggered) return;

        Patrol(); 
        DetectPlayer();
    }

    //-------------------- PATROL MOVEMENT --------------------//
    // Handles the bot's movement based on the selected movement pattern by level designer
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

    //-------------------- PLAYER DETECTION --------------------//
    //Detects the player within the bot's detection range and triggers correspinding explosion behavior specified by the level designer.
    void DetectPlayer()
    {
        if (player == null) return;

        if (Vector3.Distance(transform.position, player.position) <= detectionRange)
        {
            isTriggered = true;

            if (detectionType == DetectionType.ExplodeOnDetection)
            {
                Explode();
            }
            else if (detectionType == DetectionType.FollowAndExplode)
            {
                StartCoroutine(FollowAndExplode());
            }
        }
    }

    //-------------------- IMMEDIATE EXPLOSION --------------------//
    //Causes the bot to explode in place immediately, affecting nearby objects within the explosion radius.
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

    //-------------------- FOLLOW AND EXPLODE --------------------//
    // Follows the player for a set duration before exploding.
    IEnumerator FollowAndExplode()
    {
        float followTime = 0f;

        while (followTime < followDuration)
        {
            transform.position = Vector3.MoveTowards(transform.position, player.position, followSpeed * Time.deltaTime);

            if (Vector3.Distance(transform.position, player.position) <= 0.1f)
            {
                RespawnPlayer();
                //Destroy the bot after respawning the player
                Destroy(gameObject); 
                yield break;
            }

            followTime += Time.deltaTime;
            yield return null;
        }
        //Call Explode() if follow duration completes without contact
        Explode(); 
    }

    //-------------------- FIND CLOSEST RESPAWN POINT --------------------//
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