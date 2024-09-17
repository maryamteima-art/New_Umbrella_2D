using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UmbrellaController : MonoBehaviour
{
    public Transform player;
    public float displacement = 1.0f; 
    public float forceMagnitude = 10f; 
    public float slowFallGravityScale = 0.1f; 
    public Color slowedColor = Color.blue; 
    public float slowFallSpeed = 1f; 
    private Vector3 closedSize = new Vector3(0.5f, 1.5f, 1f); 
    private Vector3 openSize = new Vector3(2f, 0.5f, 1f); 
    private bool wasM1 = false;
    private bool umbrellaOpen = false;
    private Rigidbody2D playerRb;
    private SpriteRenderer playerSpriteRenderer;
    private Color originalColor;
    private float originalGravityScale;
    private PlayerController playerController;
    public Color umbrellaDownColor = Color.green; 
    public LayerMask hazardLayer; 
    public float collisionForceMultiplier = 0.1f; 
    public float debugForceMagnitude = 1f; 
    private bool isUmbrellaFacingDown = false; 

    void Start()
    {
        transform.localScale = closedSize;
        playerRb = player.GetComponent<Rigidbody2D>();
        originalGravityScale = playerRb.gravityScale; 
        playerSpriteRenderer = player.GetComponent<SpriteRenderer>();
        originalColor = playerSpriteRenderer.color; 
        playerController = player.GetComponent<PlayerController>();
        playerRb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
    }

    void Update()
    {
        Vector3 direction = Vector3.zero;
        Vector3 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mousePosition.z = 0;
        direction = mousePosition - player.position;
        direction.Normalize();
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg - 90f;
        if (angle < 0)
        {
            angle += 360f;
        }
        transform.position = player.position + direction * displacement; 
        transform.rotation = Quaternion.Euler(0, 0, angle);
        if (Input.GetMouseButton(0)) 
        {
            transform.localScale = openSize;
            wasM1 = true;
            umbrellaOpen = true;

            // Umbrella float
            if (angle > 315f || angle < 45f)
            {
                playerRb.gravityScale = 0f;
                playerSpriteRenderer.color = slowedColor;
                isUmbrellaFacingDown = false;
            }
            // Umbrella bounce
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

            if (wasM1 && playerController.grounded)
            {
                playerRb.AddForce(new Vector2(direction.x, direction.y) * forceMagnitude, ForceMode2D.Impulse);
            }
            wasM1 = false;
        }
    }

    void OnTriggerStay2D(Collider2D other)
    {
        if (umbrellaOpen && isUmbrellaFacingDown && ((1 << other.gameObject.layer) & hazardLayer) != 0)
        {
            Vector2 collisionDirection = player.position - transform.position;
            collisionDirection.Normalize();
            Vector2 force = collisionDirection * debugForceMagnitude; 

            playerRb.AddForce(force, ForceMode2D.Impulse);

            Debug.DrawLine(transform.position, transform.position + (Vector3)force, Color.red, 2f);
        }
    }

    void FixedUpdate()
    {
        if (umbrellaOpen && playerRb.gravityScale == 0f)
        {
            playerRb.velocity = new Vector2(playerRb.velocity.x, -slowFallSpeed);
        }
    }

    void ResetGravityAndColor()
    {
        playerRb.gravityScale = originalGravityScale;
        playerSpriteRenderer.color = originalColor;
        Debug.Log("ResetGravityAndColor called. Color reset to original.");
    }
}