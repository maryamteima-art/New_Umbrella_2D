using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ProgressBarController : MonoBehaviour
{
    public Transform player; 
    public Transform goal;
    public Slider progressBar; // Reference to the UI Slider
    private float maxDistance;
    // Start is called before the first frame update
    void Start()
    {
        maxDistance = Vector2.Distance(player.position, goal.position);
        progressBar.maxValue = maxDistance;
        progressBar.value = maxDistance;

    }

    // Update is called once per frame
    void Update()
    {
        // Calculate the current distance in 2D space
        float currentDistance = Vector2.Distance(player.position, goal.position);
        float progress = Mathf.Clamp(maxDistance - currentDistance, 0, maxDistance);

        // Update the progress bar
        //progressBar.value = maxDistance - currentDistance;
        progressBar.value = progress;
    }
}
