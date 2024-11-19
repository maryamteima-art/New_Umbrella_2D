using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine;
using UnityEngine.UI;

public class RSSController : MonoBehaviour
{
    public RSSVariableContainer rssVariables;

    //------ ACTIVATIONS VARIABLES ------
    //Tracks accumulated thrust (for jetpack activation)
    private float thrustMeter = 0f;
    //Tracks swing hits for "swing/attack activation"
    private int swingCounter = 0;
    //Tracks hold time for "timed press activation"
    private float holdDuration = 0f;

    //------ REFERENCE VARIABLES ------

    //Reference to the player
    private Transform player;
    //Reference to PlayerController
    private PlayerController playerController;
    //Reference to PlayerInputActions
    private PlayerInputActions inputActions;

    //------ GENERAL VARIABLES ------

    //UI for visual feedback
    private Text progressText;
    //Tracks if the RSS is activated
    private bool isActivated = false;
    //Tracks the accumulated rotation
    private float totalRotation = 0f;
    //Stores the initial position of the object
    private Vector3 initialPosition;
    //Stores the initial rotation of the object
    private Quaternion initialRotation;
    //Tracks progress for linear motion
    private float movementProgress = 0f; 


    void Start()
    {

        //Save the initial position and rotation of RSS
        initialPosition = transform.position;
        initialRotation = transform.rotation;

        //Setup visual marker and colliders
        SetupVisualMarker();
        SetupCollider();
        SetupInteractionPointColliders();
        SetupSprite();

        //Find the player and set up references
        GameObject playerObject = GameObject.FindWithTag("Player");
        if (playerObject != null)
        {
            player = playerObject.transform;
            playerController = playerObject.GetComponent<PlayerController>();
        }

        //Initialize input actions
        inputActions = new PlayerInputActions();
        inputActions.Enable();
    }

    //Handles unlocking of RSS based on activation type
    //Thrust, swing, and timedbuttons use oncollisionenter methods
    void Update()
    {
        // Continuously update the initial position to reflect object movement
        initialPosition = transform.position;

        // Clamp and validate input values
        rssVariables.activationRadius = Mathf.Clamp(rssVariables.activationRadius, 0, float.MaxValue);
        rssVariables.linearDistance = Mathf.Clamp(rssVariables.linearDistance, 0, float.MaxValue);
        rssVariables.interactablePoints = Mathf.Clamp(rssVariables.interactablePoints, 1, int.MaxValue);
        rssVariables.interactablePoints = Mathf.RoundToInt(rssVariables.interactablePoints); // Ensure whole numbers

        // Only run activation logic if not already activated
        if (!isActivated)
        {
            if (rssVariables.triggerMechanism == RSSVariableContainer.TriggerType.Proximity)
            {
                HandleProximityActivation(); // Continuous check for proximity
            }
        }
        else
        {
            // Movement logic (linear or rotation) after activation
            HandleMovement();
        }
    }

    //----------- GENERAL FUNCTIONS OF RSS --------------//

    //Creating Colliders
    private void SetupCollider()
    {
        //Clear existing colliders
        Collider[] existingColliders = GetComponents<Collider>();
        foreach (var collider in existingColliders)
        {
            Destroy(collider);
        }

        //Add main collider for proximity activation
        // Proximity doesn't need smaller colliders
        if (rssVariables.triggerMechanism == RSSVariableContainer.TriggerType.Proximity)
        {
            SphereCollider proximityCollider = gameObject.AddComponent<SphereCollider>();
            proximityCollider.radius = rssVariables.activationRadius;
            proximityCollider.isTrigger = true;
            return; 
        }

        //Create smaller colliders for interactive points
        //Default radius if none provided is 0.5
        for (int i = 0; i < rssVariables.interactablePoints; i++)
        {
            float pointRadius = rssVariables.interactionPointRadii != null && i < rssVariables.interactionPointRadii.Count
                ? rssVariables.interactionPointRadii[i]
                : 0.5f; 

            //Calculate position of the collider
            float angle = (i * Mathf.PI * 2) / rssVariables.interactablePoints;
            Vector3 offset = new Vector3(Mathf.Cos(angle), 0, Mathf.Sin(angle)) * rssVariables.activationRadius;
            Vector3 interactionPoint = transform.position + offset;

            //Add a collider at the interaction point
            Collider collider;

            switch (rssVariables.triggerMechanism)
            {
                //Swing gets box collider
                case RSSVariableContainer.TriggerType.Swing:
                    collider = gameObject.AddComponent<BoxCollider>();
                    ((BoxCollider)collider).size = Vector3.one * pointRadius; 
                    break;
                //Thrust gets circular collider
                case RSSVariableContainer.TriggerType.Thrust:
                case RSSVariableContainer.TriggerType.TimedButtonPress:
                    collider = gameObject.AddComponent<SphereCollider>();
                    ((SphereCollider)collider).radius = pointRadius; 
                    break;

                default:
                    Debug.LogWarning($"Unhandled trigger mechanism for collider setup: {rssVariables.triggerMechanism}");
                    continue;
            }

            collider.isTrigger = true;

            //Move the collider to the calculated position
            collider.transform.position = interactionPoint;
        }
    }

    private void SetupInteractionPointColliders()
    {
        //Clear existing child colliders
        foreach (Transform child in transform)
        {
            Destroy(child.gameObject);
        }

        //Create colliders for each interaction point
        for (int i = 0; i < rssVariables.interactablePoints; i++)
        {
            float pointRadius = rssVariables.interactionPointRadii != null && i < rssVariables.interactionPointRadii.Count
                ? rssVariables.interactionPointRadii[i]
                : 0.5f; // Default radius if none provided

            //Correctly calculate angle per point
            float angle = (i * Mathf.PI * 2) / rssVariables.interactablePoints;

            // Offset position based on angle
            Vector3 offset = new Vector3(Mathf.Cos(angle), 0, Mathf.Sin(angle)) * rssVariables.activationRadius;

            //Create a new GameObject for the interaction point
            GameObject point = new GameObject($"InteractionPoint_{i}");
            point.transform.SetParent(transform);
            point.transform.position = transform.position + offset;

            //Add a sphere collider to the interaction point
            SphereCollider collider = point.AddComponent<SphereCollider>();
            collider.radius = pointRadius;
            collider.isTrigger = true;

            //Add a visual marker for debugging (optional)
            point.AddComponent<DebugMarker>().SetRadius(pointRadius);
        }
    }

    //Creating PLaceholder Sprites (they look like the colored version of the colliders)
    private void SetupSprite()
    {
        //Check if the RSSVariables scriptable object is set
        if (rssVariables == null)
        {
            Debug.LogWarning("RSSVariables is not assigned. Cannot set up sprite.");
            return;
        }

        //Check if a SpriteRenderer already exists
        SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();
        if (!spriteRenderer)
        {
            //Add a SpriteRenderer component if not present
            spriteRenderer = gameObject.AddComponent<SpriteRenderer>();
        }

        //Adjust the sprite size and appearance to match the collider
        switch (rssVariables.triggerMechanism)
        {
            //For "Proximity Activation"
            case RSSVariableContainer.TriggerType.Proximity:
                //Match SphereCollider
                SphereCollider sphereCollider = GetComponent<SphereCollider>();
                if (sphereCollider != null)
                {
                    spriteRenderer.drawMode = SpriteDrawMode.Sliced;
                    //Match diameter
                    spriteRenderer.size = Vector2.one * sphereCollider.radius * 2; 
                }
                else
                {
                    Debug.LogWarning("No SphereCollider found for Proximity Activation.");
                }
                break;

            //For "Attack/Swing Activation"
            case RSSVariableContainer.TriggerType.Swing:
                //Match BoxCollider
                BoxCollider boxCollider = GetComponent<BoxCollider>();
                if (boxCollider != null)
                {
                    spriteRenderer.drawMode = SpriteDrawMode.Sliced;
                    //Match size
                    spriteRenderer.size = new Vector2(boxCollider.size.x, boxCollider.size.y);
                }
                else
                {
                    Debug.LogWarning("No BoxCollider found for Swing Activation.");
                }
                break;

            //For "Jetpack Activation" or "Timed Press"
            case RSSVariableContainer.TriggerType.TimedButtonPress:
            case RSSVariableContainer.TriggerType.Thrust:
                //Match CapsuleCollider
                CapsuleCollider capsuleCollider = GetComponent<CapsuleCollider>();
                if (capsuleCollider != null)
                {
                    spriteRenderer.drawMode = SpriteDrawMode.Sliced;
                    spriteRenderer.size = new Vector2(capsuleCollider.radius * 2, capsuleCollider.height);
                }
                else
                {
                    Debug.LogWarning("No CapsuleCollider found for Thrust/TimedButtonPress Activation.");
                }
                break;

            default:
                Debug.LogWarning($"Unhandled TriggerMechanism: {rssVariables.triggerMechanism}");
                break;
        }

        //Assign a default sprite color for better visibility
        if (spriteRenderer != null)
        {
            spriteRenderer.color = rssVariables.proximityColor;
        }
        else
        {
            Debug.LogWarning("SpriteRenderer is missing after setup.");
        }
    }

    //Handling Movement of RSS unlock (linear or rotating)
    private void HandleMovement()
    {
        //Can only select one "ocilating forvever linear, or "ocilating forever for Rotation". 
        //Backup Check: Ensures both aren't simultaneaously selected, if yes, exit function immediately
        if (rssVariables.isRotationalForever && rssVariables.isLinearOscillating)
        {
            Debug.LogError("Both rotational and linear forever motion cannot be enabled at the same time.");
            return; 
        }

        switch (rssVariables.pathOption)
        {
            case RSSVariableContainer.PathOption.Linear:
                HandleLinearMovement();
                break;
            case RSSVariableContainer.PathOption.Rotational:
                HandleRotationalMovement();
                break;
            default:
                Debug.LogWarning("Select path option in HandleMovement!");
                break;
        }
    }

    //--------- MOTIONS (LINEAR VS ROTATIONS) & (FOREVER VS ONETIME) ---------------

    //Linear Motion of RSS object back and forth along a specified direction and distance. (up, down, left, right, etc)
    private void HandleLinearMovement()
    {
        if (rssVariables.isLinearOscillating)
        {
            //Oscillating motion (back and forth forever, "activation trigger" would make it stop moving)
            movementProgress += Time.deltaTime * rssVariables.linearSpeed;
            //PingPong is what makes it increment or decrement by 1
            float progressFactor = Mathf.PingPong(movementProgress, 1f);
            Vector3 targetPosition = initialPosition + rssVariables.linearDirection.normalized * rssVariables.linearDistance * progressFactor;
            transform.position = targetPosition;
        }
        else
        {
            //One-way motion (back and forth once, "activation trigger" would make it move in reverse to unlock)
            if (movementProgress < 1f)
            {
                movementProgress += Time.deltaTime * (rssVariables.linearSpeed/rssVariables.linearDistance);
                movementProgress = Mathf.Clamp01(movementProgress);
                Vector3 targetPosition = Vector3.Lerp(initialPosition, initialPosition + rssVariables.linearDirection.normalized * rssVariables.linearDistance, movementProgress);
                transform.position = targetPosition;
            }
        }
    }

    //Rotating (Radial) along a specific axis (0, 90, 180, 360, etc)
    private void HandleRotationalMovement()
    {
        if (rssVariables.isRotationalForever)
        {
            //Continuous rotation (forever, activation trigger means it'll stop moving)
            float rotationStep = rssVariables.rotationSpeed * Time.deltaTime;
            transform.RotateAround(transform.position, rssVariables.rotationAxis.normalized, rotationStep);
        }
        else
        {
            //Limited rotation (one-time, activation trigger means unlock it, or have it rotate to the target position to open a new pathway)
            if (totalRotation < rssVariables.rotationAngle)
            {
                float rotationStep = rssVariables.rotationSpeed * Time.deltaTime;

                //Clamp rotation step to not exceed the remaining angle
                if (totalRotation + rotationStep > rssVariables.rotationAngle)
                {
                    rotationStep = rssVariables.rotationAngle - totalRotation;
                }

                //Perform rotation
                transform.RotateAround(transform.position, rssVariables.rotationAxis.normalized, rotationStep);

                //Update total rotation progress
                totalRotation += rotationStep;
            }
        }
    }

    //Drawing gizmos for rotational and linear movement for better visualizations
    void OnDrawGizmos()
    {
        if (rssVariables != null)
        {
            // Proximity activation visualization
            if (rssVariables.triggerMechanism == RSSVariableContainer.TriggerType.Proximity)
            {
                Gizmos.color = rssVariables.proximityColor;
                Gizmos.DrawWireSphere(transform.position, rssVariables.activationRadius);
            }
            else
            {
                // Interaction points for Swing, Thrust, and Timed Button Press
                for (int i = 0; i < rssVariables.interactablePoints; i++)
                {
                    // Ensure interaction point radii are valid
                    float pointRadius = rssVariables.interactionPointRadii != null && i < rssVariables.interactionPointRadii.Count
                        ? rssVariables.interactionPointRadii[i]
                        : 0.5f; // Default radius if none provided

                    // Calculate positions for interaction points
                    float angle = (i * Mathf.PI * 2) / rssVariables.interactablePoints;
                    Vector3 offset = new Vector3(Mathf.Cos(angle), 0, Mathf.Sin(angle)) * rssVariables.activationRadius;
                    Vector3 interactionPoint = transform.position + offset;

                    // Visualize based on the trigger type
                    switch (rssVariables.triggerMechanism)
                    {
                        case RSSVariableContainer.TriggerType.Swing:
                            Gizmos.color = Color.yellow;
                            //Scaled cube for swings interaction points
                            Gizmos.DrawWireCube(interactionPoint, Vector3.one * pointRadius); 
                            break;

                        case RSSVariableContainer.TriggerType.Thrust:
                            Gizmos.color = Color.cyan;
                            //Sphere for thrust trigger interaction points
                            Gizmos.DrawWireSphere(interactionPoint, pointRadius); 
                            break;
                        //Sphere for timedbutton interaction points
                        case RSSVariableContainer.TriggerType.TimedButtonPress:
                            Gizmos.color = Color.magenta;
                            Gizmos.DrawWireSphere(interactionPoint, pointRadius); 
                            break;

                        default:
                            Gizmos.color = Color.gray;
                            Gizmos.DrawWireSphere(interactionPoint, pointRadius);
                            break;
                    }
                }
            }

            // Linear Path Visualization
            if (rssVariables.pathOption == RSSVariableContainer.PathOption.Linear)
            {
                Vector3 start = transform.position;
                Vector3 end = start + rssVariables.linearDirection.normalized * rssVariables.linearDistance;

                // Line thickness configuration
                //Distance between each line
                float thicknessOffset = 0.01f;
                //number of stacked lines to simulate/fake thickness
                int lineThickness = 30;

                //Draw the stacked main line (central one)
                Gizmos.color = Color.green;

                //Performing the stacking of lines using loop
                for (int i = 0; i < lineThickness; i++)
                {
                    float offset = (i - lineThickness / 2) * thicknessOffset;
                    Vector3 offsetVector = new Vector3(0, offset, 0);
                    Gizmos.DrawLine(start + offsetVector, end + offsetVector);
                }

                //Draw direction triangle (to differentiate between negative and positive directions)
                float triangleSize = 0.5f;
                Vector3 triangleTip = end + rssVariables.linearDirection.normalized * triangleSize;
                Vector3 perpendicularDirection = Vector3.Cross(rssVariables.linearDirection.normalized, Vector3.up).normalized;
                Vector3 triangleBase1 = end + perpendicularDirection * (triangleSize / 2);
                Vector3 triangleBase2 = end - perpendicularDirection * (triangleSize / 2);

                Gizmos.color = Color.red;

                //Performing the stacking of triangles using loop
                for (int i = 0; i < lineThickness; i++)
                {
                    float offset = (i - lineThickness / 2) * thicknessOffset;
                    Vector3 offsetVector = new Vector3(0, offset, 0);
                    Gizmos.DrawLine(triangleBase1 + offsetVector, triangleTip + offsetVector);
                    Gizmos.DrawLine(triangleBase2 + offsetVector, triangleTip + offsetVector);
                    Gizmos.DrawLine(triangleBase1 + offsetVector, triangleBase2 + offsetVector);
                }
            }

        //Rotational Path, visualizing the axis
        if (rssVariables.pathOption == RSSVariableContainer.PathOption.Rotational)
            {
                Gizmos.color = Color.blue;
                //Draw a small sphere to represent the rotation axis marker
                Gizmos.DrawWireSphere(transform.position, 0.5f);

                // If continuous rotation is enabled (oscilating forever), draw a circular path
                if (rssVariables.isRotationalForever)
                {
                    Gizmos.color = Color.cyan;
                    float radius = 1f;
                    //Iterate through angles in steps of 10 degrees
                    for (float angle = 0; angle < 360; angle += 10)
                    {
                        //Calculate two consecutive points on the circular path
                        Vector3 point1 = transform.position + Quaternion.Euler(0, angle, 0) * rssVariables.rotationAxis.normalized * radius;
                        Vector3 point2 = transform.position + Quaternion.Euler(0, angle + 10, 0) * rssVariables.rotationAxis.normalized * radius;
                        //Draw a line segment between the two points
                        Gizmos.DrawLine(point1, point2);
                    }
                }
            }


            //Include the label drawing code only when the Unity Editor is active.
            //Prevents label from running in builds where UnityEditor classes are unavailable.
            #if UNITY_EDITOR
            //Display a text label above the object showing the current trigger mechanism type
            UnityEditor.Handles.Label(transform.position + Vector3.up * 2f, "RSS: " + rssVariables.triggerMechanism.ToString());
            #endif
        }

    }
    

    //Unlocking RSS
    private void ActivateRSS()
    {
        isActivated = true;

        //Trigger Visual Animation
        Animator animator = GetComponent<Animator>();
        if (animator != null)
        {
            animator.SetTrigger("Activate");
        }

        //Change marker color
        if (progressText != null)
        {
            progressText.color = rssVariables.activeColor;
            progressText.text = "Activated!";
        }

        //Inform in Debug
        Debug.Log("RSS Activated!");
    }

    //----------- UI --------------//

    //Setting up Visual Markers for RSS (Glows, Texts components, etc)
    private void SetupVisualMarker()
    {
        //Create a canvas for visual markers
        GameObject canvasObj = new GameObject("RSSCanvas");
        Canvas canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.WorldSpace;
        canvasObj.transform.SetParent(transform);

        //Text Box
        RectTransform rect = canvas.GetComponent<RectTransform>();
        rect.sizeDelta = new Vector2(2, 1);
        rect.localPosition = Vector3.up * 2;

        //Add a Text element/component
        GameObject textObj = new GameObject("ProgressText");
        textObj.transform.SetParent(canvasObj.transform);

        //Text settings (color, size, etc)
        progressText = textObj.AddComponent<Text>();
        progressText.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        progressText.fontSize = 20;
        progressText.alignment = TextAnchor.MiddleCenter;
        progressText.color = Color.white;

        RectTransform textRect = progressText.GetComponent<RectTransform>();
        textRect.sizeDelta = new Vector2(200, 50);
        textRect.localPosition = Vector3.zero;
    }

    //Handles edits to visuals like changing the numbers themselves, and colors depending on the type of activation
    private void UpdateVisualMarker()
    {
        if (progressText == null) return;

        switch (rssVariables.triggerMechanism)
        {
            //This is a meter (ex. 100/500 pressure)
            case RSSVariableContainer.TriggerType.Thrust:
                float thrustPercent = (thrustMeter / rssVariables.thrustActivationThreshold) * 100f;
                progressText.text = $"Thrust Progress: {thrustPercent:F1}%";
                break;
            //This is a counter (ex. 1/5 hits)
            case RSSVariableContainer.TriggerType.Swing:
                progressText.text = $"Swings: {swingCounter}/{rssVariables.interactablePoints}";
                break;
            //This is like a loading/progress (ex. 30%)
            case RSSVariableContainer.TriggerType.TimedButtonPress:
                float holdPercent = (holdDuration / rssVariables.timerDuration) * 100f;
                progressText.text = $"Hold Progress: {holdPercent:F1}%";
                break;
            //This is a numbered range in percentages similar to the one above
            case RSSVariableContainer.TriggerType.Proximity:
                float distance = Vector3.Distance(player.position, transform.position);
                float proximityPercent = Mathf.Clamp01(1 - (distance / rssVariables.activationRadius)) * 100f;
                progressText.text = $"Proximity: {proximityPercent:F1}%";
                break;
        }

        progressText.color = isActivated ? rssVariables.activeColor : rssVariables.proximityColor;
    }

    //----------- HANDLING ACTIVATIONS OF RSS --------------//


    //----------- PROXIMITY ACTIVATION (Update loop) --------------//
    
    //Handles activation based on proximity
    private void HandleProximityActivation()
    {
        //Distance calculation using Unity's distance calculator
        float distance = Vector3.Distance(player.position, transform.position);

        //Check if player is within the activation radius
        if (distance <= rssVariables.activationRadius && !isActivated)
        {
            ActivateRSS();
        }

        UpdateVisualMarker();
    }

    //----------- COLLISIONS WITH INTERACTION POINTS (Thrust, Swing, TimedButtonPress) -------------//

    private void OnTriggerEnter(Collider other)
    {
        if (isActivated) return; // Ignore interactions if already activated

        switch (rssVariables.triggerMechanism)
        {
            case RSSVariableContainer.TriggerType.Swing:
                swingCounter++;
                if (swingCounter >= rssVariables.interactablePoints)
                {
                    ActivateRSS();
                }
                break;

            case RSSVariableContainer.TriggerType.Thrust:
                thrustMeter += Time.deltaTime; // Increment thrust meter
                if (thrustMeter >= rssVariables.thrustActivationThreshold)
                {
                    ActivateRSS();
                }
                break;

            case RSSVariableContainer.TriggerType.TimedButtonPress:
                holdDuration += Time.deltaTime; // Increment hold duration
                if (holdDuration >= rssVariables.timerDuration)
                {
                    ActivateRSS();
                }
                break;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (isActivated) return;

        // Reset state when player leaves collider bounds
        if (rssVariables.triggerMechanism == RSSVariableContainer.TriggerType.Thrust)
        {
            thrustMeter = Mathf.Max(0, thrustMeter - Time.deltaTime);
        }
        else if (rssVariables.triggerMechanism == RSSVariableContainer.TriggerType.TimedButtonPress)
        {
            holdDuration = Mathf.Max(0, holdDuration - Time.deltaTime);
        }
    }

}