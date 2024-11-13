using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundFXManager : MonoBehaviour
{


    // this is a singleton
    public static SoundFXManager instance;

    [SerializeField] private AudioSource soundFXObject;

    // sound effects (can load here, or can load on gameObjects instead)
    [SerializeField] private AudioClip soundFX_umbrellaClose;
    [SerializeField] private AudioClip soundFX_umbrellaOpen;
    [SerializeField] private AudioClip soundFX_landWater;
    [SerializeField] private AudioClip soundFX_inWind;
    [SerializeField] private AudioClip soundFX_death;
    [SerializeField] private AudioClip soundFX_Land;
    [SerializeField] private AudioClip soundFX_Swing;
    [SerializeField] private AudioClip soundFX_Walk;



    private void Awake() 
    {
        if (instance == null)
        {
            instance = this;
        }
    }

    public void PlaySoundFXClip(AudioClip audioClip, Transform spawnTransform, float volume) 
    {
        // create gameObject
        AudioSource audioSource = Instantiate(soundFXObject, spawnTransform.position, Quaternion.identity);

        // assign audioclip
        audioSource.clip = audioClip;

        // vvvolume
        audioSource.volume = volume;

        // play the sound!!
        audioSource.Play();

        // length of clip
        float clipLength = audioSource.clip.length;

        // destroy after play
        Destroy(audioSource.gameObject, clipLength);
    }

    public void PlayUmbrellaCloseClip(Transform spawnTransform, float volume)
    {
        // create gameObject
        AudioSource audioSource = Instantiate(soundFXObject, spawnTransform.position, Quaternion.identity);

        // assign audioclip
        audioSource.clip = soundFX_umbrellaClose;

        // vvvolume
        audioSource.volume = volume;

        // play the sound!!
        audioSource.Play();

        // length of clip
        float clipLength = audioSource.clip.length;

        // destroy after play
        Destroy(audioSource.gameObject, clipLength);
    }

    public void PlayUmbrellaOpenClip(Transform spawnTransform, float volume)
    {
        // create gameObject
        AudioSource audioSource = Instantiate(soundFXObject, spawnTransform.position, Quaternion.identity);

        // assign audioclip
        audioSource.clip = soundFX_umbrellaOpen;

        // vvvolume
        audioSource.volume = volume;

        // play the sound!!
        audioSource.Play();

        // length of clip
        float clipLength = audioSource.clip.length;

        // destroy after play
        Destroy(audioSource.gameObject, clipLength);
    }

    public void PlayLandWaterClip(Transform spawnTransform, float volume)
    {
        // create gameObject
        AudioSource audioSource = Instantiate(soundFXObject, spawnTransform.position, Quaternion.identity);

        // assign audioclip
        audioSource.clip = soundFX_landWater;

        // vvvolume
        audioSource.volume = volume;

        // play the sound!!
        audioSource.Play();

        // length of clip
        float clipLength = audioSource.clip.length;

        // destroy after play
        Destroy(audioSource.gameObject, clipLength);
    }
    
    public void PlayLandClip(Transform spawnTransform, float volume)
    {
        // create gameObject
        AudioSource audioSource = Instantiate(soundFXObject, spawnTransform.position, Quaternion.identity);

        // assign audioclip
        audioSource.clip = soundFX_Land;

        // vvvolume
        audioSource.volume = volume;

        // play the sound!!
        audioSource.Play();

        // length of clip
        float clipLength = audioSource.clip.length;

        // destroy after play
        Destroy(audioSource.gameObject, clipLength);
    }

    public void PlayInWindClip(Transform spawnTransform, float volume)
    {
        // create gameObject
        AudioSource audioSource = Instantiate(soundFXObject, spawnTransform.position, Quaternion.identity);

        // assign audioclip
        audioSource.clip = soundFX_inWind;

        // vvvolume
        audioSource.volume = volume;

        // play the sound!!
        audioSource.Play();

        // length of clip
        float clipLength = audioSource.clip.length;

        // destroy after play
        Destroy(audioSource.gameObject, clipLength);
    }

    public void PlayDeathClip(Transform spawnTransform, float volume)
    {
        // create gameObject
        AudioSource audioSource = Instantiate(soundFXObject, spawnTransform.position, Quaternion.identity);

        // assign audioclip
        audioSource.clip = soundFX_death;

        // set volume
        audioSource.volume = volume;

        // play the sound
        audioSource.Play();

        // destroy after play
        Destroy(audioSource.gameObject, audioSource.clip.length);
    }
    
    public void PlaySwingClip(Transform spawnTransform, float volume)
    {
        // create gameObject
        AudioSource audioSource = Instantiate(soundFXObject, spawnTransform.position, Quaternion.identity);

        // assign audioclip
        audioSource.clip = soundFX_Swing;

        // vvvolume
        audioSource.volume = volume;

        // play the sound!!
        audioSource.Play();

        // length of clip
        float clipLength = audioSource.clip.length;

        // destroy after play
        Destroy(audioSource.gameObject, clipLength);
    }
    
    public void PlayWalkClip(Transform spawnTransform, float volume)
    {
        // create gameObject
        AudioSource audioSource = Instantiate(soundFXObject, spawnTransform.position, Quaternion.identity);

        // assign audioclip
        audioSource.clip = soundFX_Walk;

        // vvvolume
        audioSource.volume = volume;

        // play the sound!!
        audioSource.Play();

        // length of clip
        float clipLength = audioSource.clip.length;

        // destroy after play
        Destroy(audioSource.gameObject, clipLength);
    }
}
