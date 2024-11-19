using UnityEditor;
using UnityEngine;

//Editor to showcase the RSSVariableEditor within the RSS game object's menu, as opposed to double clicking the instance to get their settings

[CustomEditor(typeof(RSSController))]
public class RSSControllerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        RSSController controller = (RSSController)target;

        // Draw default Inspector for the RSSController
        DrawDefaultInspector();

        // If RSSVariableContainer is assigned, draw its properties
        if (controller.rssVariables != null)
        {
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("RSS Variable Settings", EditorStyles.boldLabel);

            // Create a sub-Inspector for the ScriptableObject
            Editor editor = CreateEditor(controller.rssVariables);
            editor.OnInspectorGUI();
        }
        else
        {
            EditorGUILayout.HelpBox("No RSS Variable Container assigned.", MessageType.Warning);
        }
    }
}
