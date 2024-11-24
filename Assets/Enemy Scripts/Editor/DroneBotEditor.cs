using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

//This is added because I needed variables (X time before exlposion & follow speed) to dynamically hide and unhide themselves depending on the trigger type selected (Explode or FollowAndExplode)
[CustomEditor(typeof(DroneBot))]
public class DroneBotEditor : Editor
{
    public override void OnInspectorGUI()
    {
        //Get the reference to the DroneBot script
        DroneBot bot = (DroneBot)target;

        //Draw Movement Settings
        EditorGUILayout.LabelField("Movement Settings", EditorStyles.boldLabel);
        bot.patrolRange = EditorGUILayout.Slider("Patrol Range", bot.patrolRange, 0.5f, 500f);
        bot.movementSpeed = EditorGUILayout.Slider("Patrol Speed", bot.movementSpeed, 0.5f, 100f);
        bot.movementPattern = (DroneBot.MovementPattern)EditorGUILayout.EnumPopup("Movement Pattern", bot.movementPattern);

        //Add spacing (readability purposes)
        EditorGUILayout.Space();

        //Draw Detection Settings
        EditorGUILayout.LabelField("Detection Settings", EditorStyles.boldLabel);
        bot.detectionRange = EditorGUILayout.Slider("Detection Range", bot.detectionRange, 0.5f, 300f);
        bot.detectionType = (DroneBot.DetectionType)EditorGUILayout.EnumPopup("Detection Type", bot.detectionType);

        //If followAndExplode selected, unhide the variables for it (X time before explosion & follow speed)
        //Otherwise they're always hidden
        if (bot.detectionType == DroneBot.DetectionType.FollowAndExplode)
        {
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Follow-And-Explode Settings", EditorStyles.boldLabel);
            bot.followDuration = EditorGUILayout.Slider("Follow Duration (sec)", bot.followDuration, 0.5f, 100f);
            bot.followSpeed = EditorGUILayout.Slider("Follow Speed", bot.followSpeed, 0.5f, 50f);
        }

        //Add space before Explosion Settings
        EditorGUILayout.Space();

        //Draw Explosion Settings
        EditorGUILayout.LabelField("Explosion Settings", EditorStyles.boldLabel);
        bot.explosionRadius = EditorGUILayout.Slider("Explosion Radius", bot.explosionRadius, 0.5f, 200f);

        //Apply changes/updates to the script
        if (GUI.changed)
        {
            EditorUtility.SetDirty(bot);
        }
    }
}