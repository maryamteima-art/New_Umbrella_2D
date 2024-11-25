using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

//This is added because I needed variables (X time before explosion & follow speed)
//to dynamically hide and unhide themselves depending on the trigger type selected (Explode or FollowAndExplode)
[CustomEditor(typeof(DroneBot))]
public class DroneBotEditor : Editor
{
    public override void OnInspectorGUI()
    {
        //Get the reference to the DroneBot script
        DroneBot bot = (DroneBot)target;

        //Deflection Behaviour & Player Boost section
        EditorGUILayout.LabelField("Deflection Behaviour & Player Boost", EditorStyles.boldLabel);

        bot.deflectionMode = (DroneBot.DeflectionMode)EditorGUILayout.EnumPopup(new GUIContent("Deflection Mode", "Force Increases Near Explosion: More force the later you hit the bot (before it explodes).\n\nForce Decreases Near Explosion: More force the earlier you hit the bot."),bot.deflectionMode);
        
        bot.deflectionForceMultiplier = EditorGUILayout.Slider(new GUIContent("Enemy Force Multiplier", "Multiplier applied to the enemy for added control."),bot.deflectionForceMultiplier, 0.1f, 10f);
        
        bot.playerForceMultiplier = EditorGUILayout.Slider(new GUIContent("Player Force Multiplier", "Multiplier applied to the player for added control."),bot.playerForceMultiplier, 0.1f, 10f);
        
        //Add spacing (readability purposes)
        EditorGUILayout.Space();

        //Draw Movement Settings
        EditorGUILayout.LabelField("Movement Settings", EditorStyles.boldLabel);

        bot.patrolRange = EditorGUILayout.Slider(new GUIContent("Patrol Range", "Range of movement for the patrol path."),bot.patrolRange, 0.5f, 500f);

        bot.movementSpeed = EditorGUILayout.Slider(new GUIContent("Patrol Speed", "Bot's Patrol Speed."),bot.movementSpeed, 0.5f, 100f);

        bot.movementPattern = (DroneBot.MovementPattern)EditorGUILayout.EnumPopup(new GUIContent("Movement Pattern", "Bot's Movement Pattern: Sine, Linear, or Radial."),bot.movementPattern);

        //Add spacing (readability purposes)
        EditorGUILayout.Space();

        //Draw Detection Settings
        EditorGUILayout.LabelField("Detection Settings", EditorStyles.boldLabel);

        bot.detectionRange = EditorGUILayout.Slider(new GUIContent("Detection Range", "Distance the bot detects the player."),bot.detectionRange, 0.5f, 300f);

        bot.detectionType = (DroneBot.DetectionType)EditorGUILayout.EnumPopup(new GUIContent("Detection Type", "Determines whether the bot explodes immediately or follows player before exploding."),bot.detectionType);

        //Add space before showing specific settings for the selected detection type
        EditorGUILayout.Space();

        if (bot.detectionType == DroneBot.DetectionType.FollowAndExplode)
        {
            bot.followDuration = EditorGUILayout.Slider(new GUIContent("Follow Duration (sec)", "Duration for following the player when in proximity."), bot.followDuration, 0.5f, 100f);

            bot.followSpeed = EditorGUILayout.Slider(new GUIContent("Follow Speed", "Speed while following the player."), bot.followSpeed, 0.5f, 50f);
        }

        //Add space before Explosion Settings
        EditorGUILayout.Space();

        //Draw Explosion Settings
        EditorGUILayout.LabelField("Explosion Settings", EditorStyles.boldLabel);

        bot.explosionRadius = EditorGUILayout.Slider(new GUIContent("Explosion Radius", "Radius of the explosion effect."),bot.explosionRadius, 0.5f, 200f);

        if (bot.detectionType == DroneBot.DetectionType.ExplodeOnDetection)
        {
            bot.totalTimeToExplode = EditorGUILayout.Slider(new GUIContent("Time to Explode (sec)", "Time before the bot explodes after being triggered."),bot.totalTimeToExplode, 0.5f, 100f);
        }

        //Apply changes/updates to the script
        if (GUI.changed)
        {
            EditorUtility.SetDirty(bot);
        }
    }
}