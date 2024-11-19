using UnityEngine;

[CreateAssetMenu(fileName = "RSSVariables", menuName = "RSS/VariableContainer", order = 1)]
public class RSSVariableContainer : ScriptableObject
{
    //--------- MENU SETTINGS --------------//
    public TriggerType triggerMechanism;
    public enum TriggerType { Proximity, TimedButtonPress, Thrust, Swing }

    [Tooltip("Activation Radius: Distance within which the player triggers the mechanism.")]
    public float activationRadius = 5f;

    [Tooltip("Number of points players can interact with to influence the structure.")]
    public int interactablePoints = 1;
    
    //This is what will be used to determine the specifc activations (for each small/multiple points within the larger radius form)
    public List<float> interactionPointRadii; 


    [Tooltip("Key or button for timed press activation.")]
    public KeyCode timedPressKey = KeyCode.E;

    //--------- PATH OPTIONS --------------//
    //Defines the movement path-type 
    public PathOption pathOption;
    public enum PathOption {Linear, Rotational}

    //--------- THRUST SETTINGS --------------//
    [Header("Thrust Settings")]
    [Tooltip("Threshold for thrust-based activation.")]
    public float thrustActivationThreshold = 3f;

    [Tooltip("Maximum height for thrust activation.")]
    public float thrustProximityHeight = 2f;

    //--------- TIMER SETTINGS --------------//
    [Header("Timer Settings")]
    [Tooltip("Duration for timed press activation.")]
    public float timerDuration = 5f;

    //--------- ROTATION SECURITY SYSTEM --------------//
    [Tooltip("Speed of rotation.")]
    public float rotationSpeed = 30f;

    [Tooltip("Total angle to rotate (e.g., 90°, 180°, 360°).")]
    public float rotationAngle = 90f;

    [Tooltip("Axis of rotation.")]
    public Vector3 rotationAxis = Vector3.up;

    [Tooltip("Set true for continuous rotation; false for limited rotation.")]
    public bool isRotationalForever = false;

    //--------- LINEAR SECURITY SYSTEM --------------//
    [Tooltip("Direction of movement (e.g., left, right, up, down).")]
    public Vector3 linearDirection = Vector3.right;

    [Tooltip("Total distance to move.")]
    public float linearDistance = 5f;

    [Tooltip("Speed of movement.")]
    public float linearSpeed = 2f;

    [Tooltip("Set true for oscillating motion; false for one-way motion.")]
    public bool isLinearOscillating = false;

    //--------- OTHER SETTINGS --------------//
    public Color activeColor = Color.green;
    public Color resetColor = Color.red;
    //Default to white
    public Color proximityColor = Color.white; 

}