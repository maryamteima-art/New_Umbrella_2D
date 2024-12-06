using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameEndController : MonoBehaviour
{
    public string scene;
    public Color loadToColor = Color.white;
    public AudioSource audioSource;
    private void OnTriggerEnter2D(Collider2D other)
    {
        // Check if the player touches a hazard
        if (other.CompareTag("Player"))
        {
            SoundFXManager.instance.PlayCheckpointClip(transform, 0.5f);
            Debug.Log("Goal Reached! Loading Scene...");
            audioSource.Stop();
            Initiate.Fade(scene, loadToColor, 0.5f);
        }
    }
}
