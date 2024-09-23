using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using UnityEngine;

//DESCRIPTION OF VFX MANAGER:
//Handles all the VFX, by coordinating when and where VFX oarticle systems are instantiated. 
//It triggers effects (particle system) based on game events and manages any specific timings or other behaviours.

public class VFXManager : MonoBehaviour
{
    //------------------ CUSTOM VARIABLES ------------------

    //Dictionary to store VFX references by name and/or Event Type
    private Dictionary<string, GameObject> vfxDictionary = new Dictionary<string, GameObject>();

    //Singleton Pattern: Only a single global instance of VFXManager is created throughout the whole game. 
    //Use this instance to access the manager in other scripts :) 
    public static VFXManager Instance;

    //------------------ UNITY'S METHODS (START/AWAKE/UPDATE) ------------------

    //Called as soon as the script instance is loaded, before anything else in the scene.
    //Since VFX Manager will be used amongst other scripts, we initializate it before other scripts can interact with the object.
    private void Awake()
    {
        //If no instance is created/exists, set this as the VFX Manager
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            //Otherwise destroy the duplicate
            Destroy(gameObject);
        }

        //CUSTOM FUNCTION: Load all VFX prefabs into the dictionary (They can be assigned manually too)
        LoadAllVFX();
    }

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    //------------------ CUSTOM FUNCTIONS ------------------

    //Load all VFX prefabs from the "Prefabs > VFX" folder 
    private void LoadAllVFX()
    {
        //Create a VFXPrefabs list (list of objects)
        GameObject[] vfxPrefabs = Resources.LoadAll<GameObject>("Prefabs/VFX");

        //For each vfx in the VFXprefabs list, create a gameObject (member) and use the name in prefabs folder as its name
        //Store this VFX object in the "VFX master dictionary" (which will be referenced by the VFX subclasses)
        foreach (GameObject vfx in vfxPrefabs)
        {
            vfxDictionary[vfx.name] = vfx;
        }
    }

    //Call this method to trigger VFX at a specific position and rotation
    //Ex. for openUmbrella action it would look like this: VFXManager.Instance.PlayVFX("OpenUmbrella", transform.position, transform.rotation);
    //"OpenUmbrella" is the name of the particleSystem object (in prefabs folder), and transform.position & transform.rotation are the entity's position and rotation values 
    //To add spacing, just do: VFXManager.Instance.PlayVFX("OpenUmbrella", transform.position + spacing_float_amount); 
    public void PlayVFX(string vfxName, Vector3 position)
    {
        //If the VFX object exists
        if (vfxDictionary.ContainsKey(vfxName))
        {
            //Create an instance and place it at designated/inputted values
            //Identity means it creates an instance at the specific position (without taking in rotation values)
            Instantiate(vfxDictionary[vfxName], position, Quaternion.identity);

            //Reobtain vfx instance (to avoid context issues)
            GameObject vfxInstance = Instantiate(vfxDictionary[vfxName], position, Quaternion.identity);

            // Detach the VFX instance from any parent to prevent it from inheriting the player's movement or scale
            vfxInstance.transform.SetParent(null);

            // Ensure the particle system scale is not skewed
            vfxInstance.transform.localScale = Vector3.one;

            // Get the ParticleSystem component
            ParticleSystem ps = vfxInstance.GetComponent<ParticleSystem>();

            //Force-Destroy particle system object (added because some systems don't stop at their specified duration)
            //If a ParticleSystem component exists
            if (ps != null)
            {
                //Calculate the total duration of the particle system (duration + startLifetime)
                float totalDuration = ps.main.duration + ps.main.startLifetime.constantMax;

                //Destroy the GameObject after the particle system finishes playing
                Destroy(vfxInstance, totalDuration);
            }

        }
        else
        {
            //Send a "VFX NOT FOUND" error message
            Debug.LogWarning("VFX NOT FOUND: " + vfxName);
        }
    }
}
