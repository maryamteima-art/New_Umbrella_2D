using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BGM_Manager : MonoBehaviour
{
    public AudioSource audioSource; // Reference to the AudioSource
    public AudioClip mountainClip;    // Sound for the forest
    public AudioClip caveClip;      // Sound for the cave
    public float fadeTime;

    private void OnTriggerEnter2D(Collider2D other) 
    {
        Debug.Log("Entered Trigger with: " + other.tag);
        if (other.CompareTag("CaveArea"))
        {
            PlaySound(caveClip);
        }
    }

    private void PlaySound(AudioClip clip)
    {
        if (audioSource.clip != clip) //Advoid
        {
            audioSource.clip = clip;
            //audioSource.Play();
            StartCoroutine(CrossfadeSound(clip, fadeTime));
            Debug.Log("HI I AM PLAYING");
        }
    }

    IEnumerator CrossfadeSound(AudioClip newClip, float fadeDuration)
    {
        float volume = audioSource.volume;

        //Fade out current sound
        for (float t = 0; t < fadeDuration; t += Time.deltaTime)
        {
            audioSource.volume = Mathf.Lerp(volume, 0, t / fadeDuration);
            yield return null;
        }

        //Switch clip and fade in
        audioSource.clip = newClip;
        audioSource.Play();
        for (float t = 0; t < fadeDuration; t += Time.deltaTime)
        {
            audioSource.volume = Mathf.Lerp(0, volume, t / fadeDuration);
            yield return null;
        }
    }
}
