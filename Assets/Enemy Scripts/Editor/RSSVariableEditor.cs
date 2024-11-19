using UnityEngine;
using UnityEditor;

//Instance of the RSSVariableContainer. This is the editor itself customized with the flexible variables from the container class
[CustomEditor(typeof(RSSVariableContainer))]
public class RSSVariableEditor : Editor
{
    SerializedProperty triggerMechanism;
    SerializedProperty activationRadius, interactablePoints, timedPressKey;
    SerializedProperty rotationSpeed, rotationAngle, rotationAxis, isRotationalForever;
    SerializedProperty linearDirection, linearDistance, linearSpeed, isLinearOscillating;
    SerializedProperty proximityColor, activeColor, resetColor;

    private void OnEnable()
    {
        //Activation Settings
        triggerMechanism = serializedObject.FindProperty("triggerMechanism");
        activationRadius = serializedObject.FindProperty("activationRadius");
        interactablePoints = serializedObject.FindProperty("interactablePoints");
        timedPressKey = serializedObject.FindProperty("timedPressKey");

        //Rotation Security System
        rotationSpeed = serializedObject.FindProperty("rotationSpeed");
        rotationAngle = serializedObject.FindProperty("rotationAngle");
        rotationAxis = serializedObject.FindProperty("rotationAxis");
        isRotationalForever = serializedObject.FindProperty("isRotationalForever");

        //Linear Security System
        linearDirection = serializedObject.FindProperty("linearDirection");
        linearDistance = serializedObject.FindProperty("linearDistance");
        linearSpeed = serializedObject.FindProperty("linearSpeed");
        isLinearOscillating = serializedObject.FindProperty("isLinearOscillating");

        //Visual Settings
        proximityColor = serializedObject.FindProperty("proximityColor");
        activeColor = serializedObject.FindProperty("activeColor");
        resetColor = serializedObject.FindProperty("resetColor");
    }

    
    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        //Determine the activation title dynamically
        //This is toggled depending on if "forever oscillating" is checked = trigger is "stop"
        //If "forever oscillating" is unchecked = trigger is "unlock"
        string triggerLabelPrefix = "Unlock";
        if (serializedObject.FindProperty("isLinearOscillating").boolValue || serializedObject.FindProperty("isRotationalForever").boolValue)
        {
            triggerLabelPrefix = "Stop";
        }

        //Display the "Trigger Mechanism" name with dynamic label
        EditorGUILayout.PropertyField(triggerMechanism, new GUIContent($"{triggerLabelPrefix} Trigger Type"));

        //Path Options (Add pathOption below triggerMechanism)
        SerializedProperty pathOption = serializedObject.FindProperty("pathOption");
        EditorGUILayout.PropertyField(pathOption, new GUIContent("Path Option"));

        //Activation Radius
        EditorGUILayout.PropertyField(activationRadius, new GUIContent("Activation Radius"));

        //Show interactablePoints only for specific trigger types
        RSSVariableContainer.TriggerType triggerType = (RSSVariableContainer.TriggerType)triggerMechanism.enumValueIndex;
        if (triggerType == RSSVariableContainer.TriggerType.Swing ||
            triggerType == RSSVariableContainer.TriggerType.Thrust ||
            triggerType == RSSVariableContainer.TriggerType.TimedButtonPress)
        {
            EditorGUILayout.PropertyField(interactablePoints, new GUIContent("Interactable Points"));
        }

        //Special handling for "Timed-Press"
        if (triggerType == RSSVariableContainer.TriggerType.TimedButtonPress)
        {
            EditorGUILayout.HelpBox("Timed-Press: Requires a key press for activation. Adjust key below.", MessageType.Info);
            EditorGUILayout.PropertyField(timedPressKey, new GUIContent("Timed Press Key"));
        }
        //Thrust Specific
        if (triggerType == RSSVariableContainer.TriggerType.Thrust)
        {
            EditorGUILayout.HelpBox("Thrust: Requires jetpack or similar thrust-based interaction.", MessageType.Info);
        }
        //Swing Specific
        if (triggerType == RSSVariableContainer.TriggerType.Swing)
        {
            EditorGUILayout.HelpBox("Swing: Requires physical attacks or swings.", MessageType.Info);
        }

        // Proximity-Specific Visual Settings
        if (triggerType == RSSVariableContainer.TriggerType.Proximity)
        {
            EditorGUILayout.PropertyField(proximityColor, new GUIContent("Proximity Color"));
        }

        //Show Linear or Rotational settings based on pathOption
        RSSVariableContainer.PathOption selectedPathOption = (RSSVariableContainer.PathOption)pathOption.enumValueIndex;

        if (selectedPathOption == RSSVariableContainer.PathOption.Linear)
        {
            //Linear Security System
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Linear Security System", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(linearDirection);
            EditorGUILayout.PropertyField(linearDistance);
            EditorGUILayout.PropertyField(linearSpeed);
            EditorGUILayout.PropertyField(isLinearOscillating);
        }
        else if (selectedPathOption == RSSVariableContainer.PathOption.Rotational)
        {
            //Rotational Security System
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Rotation Security System", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(rotationSpeed);
            EditorGUILayout.PropertyField(rotationAngle);
            EditorGUILayout.PropertyField(rotationAxis);
            EditorGUILayout.PropertyField(isRotationalForever);
        }

        //Visual Settings
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Visual Settings", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(proximityColor);
        EditorGUILayout.PropertyField(activeColor);
        EditorGUILayout.PropertyField(resetColor);

        serializedObject.ApplyModifiedProperties();
    }
}